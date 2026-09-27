using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEngine;

public class SpriteAnimationExtractor : EditorWindow
{
    private class SheetSettings
    {
        public float fps;
        public bool loop;
    }

    // 새로 드롭된 시트에 들어갈 기본값
    private float framesPerSecond = 12f;
    private bool loop = true;
    private readonly Dictionary<string, SheetSettings> settings = new Dictionary<string, SheetSettings>();
    private bool overwrite = true;
    private Vector2 scroll;
    private readonly List<string> sheetPaths = new List<string>();
    private int selected = -1;
    // 저장하지 않는 미리보기 전용 인스턴스
    private readonly Dictionary<string, SpriteAnimationData> previews = new Dictionary<string, SpriteAnimationData>();
    private int cellWidth = 32;
    private int cellHeight = 32;
    private bool skipEmptyCells = true;
    // 기준 시트(예: 걷기)의 캐릭터와 같은 크기가 되도록 PPU / pivot 자동 설정
    private bool matchReference;
    private Texture2D referenceSheet;
    private bool alignFeet = true;
    private const string ReferencePrefKey = "SpriteAnimationExtractor.Reference";
    private const byte AlphaThreshold = 8; // 이 값 이하 알파는 빈 픽셀로 취급 (크기 측정용)

    private void OnEnable()
    {
        EditorApplication.update += Repaint;
        matchReference = EditorPrefs.GetBool(ReferencePrefKey + ".On", false);
        alignFeet = EditorPrefs.GetBool(ReferencePrefKey + ".Feet", true);
        string refPath = EditorPrefs.GetString(ReferencePrefKey, "");
        if (!string.IsNullOrEmpty(refPath)) referenceSheet = AssetDatabase.LoadAssetAtPath<Texture2D>(refPath);
    }

    private void OnDisable()
    {
        EditorApplication.update -= Repaint;
        ClearPreviews();
    }

    [MenuItem("Tools/Sprite Animation Extractor")]
    private static void Open() => GetWindow<SpriteAnimationExtractor>("Sprite Anim Extractor");

    private void OnGUI()
    {
        framesPerSecond = EditorGUILayout.FloatField("기본 Frames Per Second", framesPerSecond);
        loop = EditorGUILayout.Toggle("기본 Loop", loop);
        overwrite = EditorGUILayout.Toggle("Overwrite Existing", overwrite);

        EditorGUILayout.Space();
        DrawGridSlicer();

        EditorGUILayout.Space();
        Rect drop = GUILayoutUtility.GetRect(0, 100, GUILayout.ExpandWidth(true));
        GUI.Box(drop, "스프라이트 시트(Texture)를 여기에 드래그\n(여러 개 가능)", EditorStyles.helpBox);

        Event e = Event.current;
        if ((e.type == EventType.DragUpdated || e.type == EventType.DragPerform) && drop.Contains(e.mousePosition))
        {
            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
            if (e.type == EventType.DragPerform)
            {
                DragAndDrop.AcceptDrag();
                // 같은 시트가 여러 번 들어와도(시트 + 하위 스프라이트) 한 번만 처리
                var paths = DragAndDrop.objectReferences
                    .Select(AssetDatabase.GetAssetPath)
                    .Where(p => !string.IsNullOrEmpty(p))
                    .Distinct()
                    .OrderBy(p => p, System.StringComparer.OrdinalIgnoreCase);
                sheetPaths.Clear();
                sheetPaths.AddRange(paths);
                settings.Clear();
                foreach (string sp in sheetPaths) GetSettings(sp);
                ClearPreviews();
                selected = sheetPaths.Count > 0 ? 0 : -1;
            }
            e.Use();
        }

        DrawSheetList();
        DrawPreview();
    }

    private SheetSettings GetSettings(string path)
    {
        if (!settings.TryGetValue(path, out var st))
            settings[path] = st = new SheetSettings { fps = framesPerSecond, loop = loop };
        return st;
    }

    private void ClearPreviews()
    {
        foreach (var d in previews.Values) if (d != null) DestroyImmediate(d);
        previews.Clear();
    }

    private void CreateAllAssets()
    {
        int count = 0;
        foreach (string p in sheetPaths)
            if (Extract(p) != null) count++;
        AssetDatabase.SaveAssets();
        Debug.Log($"[SpriteAnimationExtractor] 애니메이션 에셋 {count}개 처리 완료");
    }

    private void DrawGridSlicer()
    {
        EditorGUILayout.LabelField("Grid 일괄 자르기", EditorStyles.boldLabel);
        cellWidth = Mathf.Max(1, EditorGUILayout.IntField("Cell Width (px)", cellWidth));
        cellHeight = Mathf.Max(1, EditorGUILayout.IntField("Cell Height (px)", cellHeight));
        skipEmptyCells = EditorGUILayout.Toggle("빈 칸 건너뛰기", skipEmptyCells);

        EditorGUI.BeginChangeCheck();
        matchReference = EditorGUILayout.Toggle("기준 시트에 크기 맞추기", matchReference);
        if (matchReference)
        {
            EditorGUI.indentLevel++;
            referenceSheet = (Texture2D)EditorGUILayout.ObjectField("기준 시트", referenceSheet, typeof(Texture2D), false);
            alignFeet = EditorGUILayout.Toggle("발 위치 맞추기", alignFeet);
            EditorGUI.indentLevel--;
            if (referenceSheet == null)
                EditorGUILayout.HelpBox("크기를 맞출 기준 시트(이미 슬라이스된 것, 예: 걷기)를 넣으세요.", MessageType.Info);
        }
        if (EditorGUI.EndChangeCheck())
        {
            EditorPrefs.SetBool(ReferencePrefKey + ".On", matchReference);
            EditorPrefs.SetBool(ReferencePrefKey + ".Feet", alignFeet);
            EditorPrefs.SetString(ReferencePrefKey, referenceSheet != null ? AssetDatabase.GetAssetPath(referenceSheet) : "");
        }

        using (new EditorGUI.DisabledScope(sheetPaths.Count == 0))
        {
            if (GUILayout.Button($"Grid 일괄 적용 ({sheetPaths.Count}개 시트)"))
                ApplyGridSlice();
            using (new EditorGUI.DisabledScope(!matchReference || referenceSheet == null))
            {
                // 이미 잘라둔 시트는 다시 자르지 않고(스프라이트 참조 유지) 크기만 맞춘다
                if (GUILayout.Button($"크기만 맞추기 ({sheetPaths.Count}개 시트)"))
                    ApplyFitOnly();
            }
            if (GUILayout.Button($"애니메이션 에셋 제작 ({sheetPaths.Count}개 시트)"))
                CreateAllAssets();
        }
    }

    private void ApplyGridSlice()
    {
        foreach (string path in sheetPaths) SliceSheet(path);
        AssetDatabase.Refresh();
        ClearPreviews();
    }

    private void SliceSheet(string path)
    {
        var importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null) return;

        // 임포트 설정(maxSize 등)과 무관하게 원본 크기/픽셀을 읽는다
        var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        try
        {
            if (!ImageConversion.LoadImage(tex, File.ReadAllBytes(path)))
            {
                Debug.LogWarning($"[SpriteAnimationExtractor] 이미지를 읽을 수 없음: {path}");
                return;
            }

            int cols = tex.width / cellWidth;
            int rows = tex.height / cellHeight;
            if (cols == 0 || rows == 0)
            {
                Debug.LogWarning($"[SpriteAnimationExtractor] '{path}' ({tex.width}x{tex.height}) 가 셀 크기보다 작음");
                return;
            }

            bool fit = matchReference && referenceSheet != null;
            Color32[] px = skipEmptyCells || fit ? tex.GetPixels32() : null;
            string baseName = Path.GetFileNameWithoutExtension(path);
            var rects = new List<SpriteRect>();
            int index = 0;
            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < cols; col++)
                {
                    int x = col * cellWidth;
                    int y = tex.height - (row + 1) * cellHeight; // 좌상단부터, Unity는 좌하단 원점
                    if (skipEmptyCells && IsEmpty(px, tex.width, x, y)) continue;
                    rects.Add(new SpriteRect
                    {
                        name = $"{baseName}_{index++}",
                        spriteID = GUID.Generate(),
                        rect = new Rect(x, y, cellWidth, cellHeight),
                        alignment = SpriteAlignment.Center,
                        pivot = new Vector2(0.5f, 0.5f)
                    });
                }
            }

            if (fit) FitToReference(path, importer, px, tex.width, rects);

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.SaveAndReimport(); // Multiple 모드를 먼저 확정

            var factory = new SpriteDataProviderFactories();
            factory.Init();
            var provider = factory.GetSpriteEditorDataProviderFromObject(importer);
            provider.InitSpriteEditorDataProvider();
            provider.SetSpriteRects(rects.ToArray());
            // 이름 <-> ID 매핑 갱신 (재슬라이스 시 참조 유지/새 이름 등록)
            var nameIds = provider.GetDataProvider<ISpriteNameFileIdDataProvider>();
            if (nameIds != null)
                nameIds.SetNameFileIdPairs(rects.Select(r => new SpriteNameFileIdPair(r.name, r.spriteID)));
            provider.Apply();
            importer.SaveAndReimport();
            Debug.Log($"[SpriteAnimationExtractor] {path}: {rects.Count} 프레임 ({cols}x{rows} grid, {cellWidth}x{cellHeight})");
        }
        finally
        {
            DestroyImmediate(tex);
        }
    }

    private void ApplyFitOnly()
    {
        foreach (string path in sheetPaths)
        {
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) continue;
            Color32[] px = LoadPixels(path, out int texWidth, out _);
            if (px == null) continue;

            var provider = GetDataProvider(importer);
            var rects = provider.GetSpriteRects().ToList();
            if (rects.Count == 0)
            {
                Debug.LogWarning($"[SpriteAnimationExtractor] '{path}' 에 슬라이스된 스프라이트가 없습니다.");
                continue;
            }
            if (!FitToReference(path, importer, px, texWidth, rects)) continue;
            provider.SetSpriteRects(rects.ToArray());
            provider.Apply();
            importer.SaveAndReimport();
        }
        ClearPreviews();
    }

    // 기준 시트 캐릭터와 키가 같아지도록 PPU를, 발 높이가 같아지도록 pivot을 설정
    // 모든 프레임의 캐릭터 영역을 합친 범위로 재서 프레임마다 크기가 튀지 않게 한다
    private bool FitToReference(string path, TextureImporter importer, Color32[] px, int texWidth, List<SpriteRect> rects)
    {
        string refPath = AssetDatabase.GetAssetPath(referenceSheet);
        if (refPath == path)
        {
            Debug.LogWarning($"[SpriteAnimationExtractor] '{path}' 는 기준 시트라서 크기 맞추기를 건너뜀");
            return false;
        }
        if (!TryMeasureReference(refPath, out float refBottom, out float refTop))
            return false;

        int minY = int.MaxValue, maxY = int.MinValue;
        foreach (SpriteRect r in rects)
        {
            if (!TryMeasure(px, texWidth, r.rect, out int lo, out int hi)) continue;
            minY = Mathf.Min(minY, lo);
            maxY = Mathf.Max(maxY, hi);
        }
        if (minY > maxY)
        {
            Debug.LogWarning($"[SpriteAnimationExtractor] '{path}' 에서 캐릭터 픽셀을 찾지 못함");
            return false;
        }

        float ppu = (maxY - minY + 1) / (refTop - refBottom);
        importer.spritePixelsPerUnit = ppu;
        if (alignFeet)
        {
            foreach (SpriteRect r in rects)
            {
                r.alignment = SpriteAlignment.Custom;
                r.pivot = new Vector2(0.5f, (minY - refBottom * ppu) / r.rect.height);
            }
        }
        Debug.Log($"[SpriteAnimationExtractor] {path}: '{Path.GetFileName(refPath)}' 기준으로 PPU {ppu:0.#}" +
                  (alignFeet ? ", 발 위치 맞춤" : ""));
        return true;
    }

    // 기준 시트 캐릭터의 아래/위 끝 (pivot 기준, 유닛 단위)
    private static bool TryMeasureReference(string refPath, out float bottom, out float top)
    {
        bottom = float.MaxValue;
        top = float.MinValue;
        var importer = AssetImporter.GetAtPath(refPath) as TextureImporter;
        Color32[] px = importer != null ? LoadPixels(refPath, out int texWidth, out _) : null;
        if (px == null)
        {
            Debug.LogWarning($"[SpriteAnimationExtractor] 기준 시트를 읽을 수 없음: {refPath}");
            return false;
        }

        float ppu = importer.spritePixelsPerUnit;
        foreach (SpriteRect r in GetDataProvider(importer).GetSpriteRects())
        {
            if (!TryMeasure(px, texWidth, r.rect, out int lo, out int hi)) continue;
            float pivotY = PivotY(r.alignment, r.pivot) * r.rect.height;
            bottom = Mathf.Min(bottom, (lo - pivotY) / ppu);
            top = Mathf.Max(top, (hi + 1 - pivotY) / ppu);
        }
        if (bottom >= top)
        {
            Debug.LogWarning($"[SpriteAnimationExtractor] 기준 시트 '{refPath}' 에 슬라이스된 스프라이트가 없습니다.");
            return false;
        }
        return true;
    }

    // 칸 안에서 보이는 픽셀의 가장 아래/위 행 (칸 아래 기준)
    private static bool TryMeasure(Color32[] px, int texWidth, Rect rect, out int lo, out int hi)
    {
        int x0 = Mathf.RoundToInt(rect.x), y0 = Mathf.RoundToInt(rect.y);
        int w = Mathf.RoundToInt(rect.width), h = Mathf.RoundToInt(rect.height);
        lo = int.MaxValue;
        hi = int.MinValue;
        for (int y = 0; y < h; y++)
        {
            int row = (y0 + y) * texWidth;
            for (int x = x0; x < x0 + w; x++)
            {
                if (px[row + x].a <= AlphaThreshold) continue;
                lo = Mathf.Min(lo, y);
                hi = y;
                break;
            }
        }
        return lo <= hi;
    }

    private static float PivotY(SpriteAlignment alignment, Vector2 pivot)
    {
        switch (alignment)
        {
            case SpriteAlignment.TopLeft:
            case SpriteAlignment.TopCenter:
            case SpriteAlignment.TopRight: return 1f;
            case SpriteAlignment.BottomLeft:
            case SpriteAlignment.BottomCenter:
            case SpriteAlignment.BottomRight: return 0f;
            case SpriteAlignment.Custom: return pivot.y;
            default: return 0.5f;
        }
    }

    // 임포트 설정(maxSize 등)과 무관하게 원본 픽셀을 읽는다
    private static Color32[] LoadPixels(string path, out int width, out int height)
    {
        var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        try
        {
            width = height = 0;
            if (!ImageConversion.LoadImage(tex, File.ReadAllBytes(path))) return null;
            width = tex.width;
            height = tex.height;
            return tex.GetPixels32();
        }
        finally
        {
            DestroyImmediate(tex);
        }
    }

    private static ISpriteEditorDataProvider GetDataProvider(TextureImporter importer)
    {
        var factory = new SpriteDataProviderFactories();
        factory.Init();
        var provider = factory.GetSpriteEditorDataProviderFromObject(importer);
        provider.InitSpriteEditorDataProvider();
        return provider;
    }

    private bool IsEmpty(Color32[] px, int texWidth, int x0, int y0)
    {
        for (int y = y0; y < y0 + cellHeight; y++)
            for (int x = x0; x < x0 + cellWidth; x++)
                if (px[y * texWidth + x].a > 0) return false;
        return true;
    }

    private void DrawSheetList()
    {
        if (sheetPaths.Count == 0) return;
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("시트 목록 (클릭해서 미리보기)", EditorStyles.boldLabel);
        scroll = EditorGUILayout.BeginScrollView(scroll, GUILayout.MaxHeight(140));
        for (int i = 0; i < sheetPaths.Count; i++)
        {
            bool on = GUILayout.Toggle(selected == i, Path.GetFileName(sheetPaths[i]), EditorStyles.miniButton);
            if (on && selected != i) selected = i;
        }
        EditorGUILayout.EndScrollView();
    }

    private void DrawPreview()
    {
        if (selected < 0 || selected >= sheetPaths.Count) return;
        string path = sheetPaths[selected];

        if (!previews.TryGetValue(path, out var data))
        {
            Sprite[] sprites = LoadSprites(path);
            if (sprites.Length > 0)
            {
                data = CreateInstance<SpriteAnimationData>();
                data.hideFlags = HideFlags.HideAndDontSave;
                data.sprites = sprites;
            }
            previews[path] = data;
        }

        if (data == null)
        {
            EditorGUILayout.HelpBox("슬라이스된 스프라이트가 없습니다. Grid를 적용하세요.", MessageType.Info);
            return;
        }
        SheetSettings st = GetSettings(path);
        st.fps = Mathf.Max(0.01f, EditorGUILayout.FloatField("Frames Per Second", st.fps));
        st.loop = EditorGUILayout.Toggle("Loop", st.loop);
        if (GUILayout.Button("이 값을 모든 시트에 적용"))
            foreach (string sp in sheetPaths) { var o = GetSettings(sp); o.fps = st.fps; o.loop = st.loop; }
        data.framesPerSecond = st.fps;
        data.loop = st.loop;

        EditorGUILayout.LabelField($"{Path.GetFileName(path)}  ({data.sprites.Length} frames)", EditorStyles.miniLabel);
        Rect r = GUILayoutUtility.GetRect(200, 200, GUILayout.ExpandWidth(true));
        SpriteAnimationPreview.Draw(r, data);
    }

    private static Sprite[] LoadSprites(string path)
    {
        if (!(AssetDatabase.LoadMainAssetAtPath(path) is Texture2D)) return new Sprite[0];
        return NaturalSort(AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>());
    }

    private SpriteAnimationData Extract(string path)
    {
        Sprite[] sprites = LoadSprites(path);

        if (sprites.Length == 0)
        {
            Debug.LogWarning($"[SpriteAnimationExtractor] '{path}' 에 슬라이스된 스프라이트가 없습니다.");
            return null;
        }

        string assetPath = Path.ChangeExtension(path, null) + "_Anim.asset";
        var existing = AssetDatabase.LoadAssetAtPath<SpriteAnimationData>(assetPath);
        if (existing != null && !overwrite)
        {
            Debug.LogWarning($"[SpriteAnimationExtractor] 이미 존재: {assetPath}");
            return existing;
        }

        SpriteAnimationData data = existing;
        if (data == null)
        {
            data = CreateInstance<SpriteAnimationData>();
            AssetDatabase.CreateAsset(data, assetPath);
        }
        data.sprites = sprites;
        SheetSettings st = GetSettings(path);
        data.framesPerSecond = st.fps;
        data.loop = st.loop;
        EditorUtility.SetDirty(data);
        Debug.Log($"[SpriteAnimationExtractor] {assetPath} ({sprites.Length} frames)", data);
        return data;
    }

    // 이름 끝 숫자 기준 정렬 (Idle_0, Idle_1, ... Idle_10)
    private static Sprite[] NaturalSort(IEnumerable<Sprite> sprites)
    {
        return sprites.OrderBy(s => TrailingNumber(s.name)).ThenBy(s => s.name).ToArray();
    }

    private static int TrailingNumber(string name)
    {
        Match m = Regex.Match(name, @"(\d+)$");
        return m.Success ? int.Parse(m.Value) : 0;
    }
}
