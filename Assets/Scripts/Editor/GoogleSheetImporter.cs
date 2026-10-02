using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

// 구글 시트 → ItemData / CharacterData / 새 데이터베이스 만들기로 만든 데이터 에셋 가져오기
// 상단 메뉴 ProjectJ → 구글 시트
//
// 시트 규칙
//  - 1행은 머리글 (key, itemName, price ...). 열 순서는 상관없음
//  - key = 에셋 파일 이름. 같은 key면 기존 에셋을 고치고, 없으면 새로 만든다
//  - key가 비었거나 #으로 시작하는 행은 건너뜀 (설명 / 메모용)
//  - 빈 칸은 값을 바꾸지 않음
//  - 시트에서 지운 행의 에셋은 지우지 않고 콘솔에 경고만 띄움
//  - 한 행에 오류가 있으면 그 행만 적용 안 함
public static class GoogleSheetImporter
{
    private const string ItemsFolder = "Assets/Resources/Items";
    private const string CharactersFolder = "Assets/Resources/Characters";

    private const string ItemsSheet = "아이템";
    private const string CharactersSheet = "캐릭터";

    // ───────── 메뉴 ─────────

    [MenuItem("ProjectJ/구글 시트/전부 가져오기", priority = 0)]
    private static void ImportAll()
    {
        GoogleSheetSettings settings = GoogleSheetSettings.GetOrCreate();
        List<string> report = new List<string>();

        if (!string.IsNullOrWhiteSpace(settings.itemsUrl))
            report.Add(ImportItems(settings.itemsUrl));

        if (!string.IsNullOrWhiteSpace(settings.charactersUrl))
            report.Add(ImportCharacters(settings.charactersUrl));

        foreach (GoogleSheetSettings.SheetLink link in settings.otherSheets)
        {
            if (!string.IsNullOrWhiteSpace(link.url))
                report.Add(ImportOther(link));
        }

        if (report.Count == 0)
        {
            AskForUrl(settings);
            return;
        }

        EditorUtility.DisplayDialog("구글 시트 가져오기", string.Join("\n\n", report), "확인");
    }

    [MenuItem("ProjectJ/구글 시트/아이템 가져오기", priority = 1)]
    private static void ImportItemsMenu()
    {
        GoogleSheetSettings settings = GoogleSheetSettings.GetOrCreate();

        if (string.IsNullOrWhiteSpace(settings.itemsUrl))
        {
            AskForUrl(settings);
            return;
        }

        EditorUtility.DisplayDialog("구글 시트 가져오기", ImportItems(settings.itemsUrl), "확인");
    }

    [MenuItem("ProjectJ/구글 시트/캐릭터 가져오기", priority = 2)]
    private static void ImportCharactersMenu()
    {
        GoogleSheetSettings settings = GoogleSheetSettings.GetOrCreate();

        if (string.IsNullOrWhiteSpace(settings.charactersUrl))
        {
            AskForUrl(settings);
            return;
        }

        EditorUtility.DisplayDialog("구글 시트 가져오기", ImportCharacters(settings.charactersUrl), "확인");
    }

    [MenuItem("ProjectJ/구글 시트/링크 설정", priority = 20)]
    private static void SelectSettings()
    {
        Selection.activeObject = GoogleSheetSettings.GetOrCreate();
    }

    [MenuItem("ProjectJ/구글 시트/시트 양식 CSV 내보내기 (현재 데이터)", priority = 21)]
    private static void ExportTemplates()
    {
        string folder = EditorUtility.SaveFolderPanel("CSV 저장할 폴더", "", "");

        if (string.IsNullOrEmpty(folder))
            return;

        File.WriteAllText(Path.Combine(folder, ItemsSheet + ".csv"), BuildItemsCsv(), new UTF8Encoding(true));
        File.WriteAllText(Path.Combine(folder, CharactersSheet + ".csv"), BuildCharactersCsv(), new UTF8Encoding(true));

        List<string> files = new List<string> { ItemsSheet + ".csv", CharactersSheet + ".csv" };

        foreach (Type type in OtherDataTypes())
        {
            string sheetName = Info(type).DisplayName;
            File.WriteAllText(Path.Combine(folder, sheetName + ".csv"), BuildOtherCsv(type), new UTF8Encoding(true));
            files.Add(sheetName + ".csv");
        }

        EditorUtility.DisplayDialog("시트 양식 CSV 내보내기",
            $"{string.Join(", ", files)} 저장 완료\n\n" +
            "구글 시트에서 탭마다 파일 → 가져오기 → 업로드 →\n\"현재 시트 바꾸기\"로 넣으면 됩니다.", "확인");

        EditorUtility.RevealInFinder(Path.Combine(folder, ItemsSheet + ".csv"));
    }

    private static void AskForUrl(GoogleSheetSettings settings)
    {
        Selection.activeObject = settings;
        EditorUtility.DisplayDialog("구글 시트 링크 없음",
            "Inspector에 선택된 GoogleSheetSettings에 시트 탭 주소를 붙여넣어 주세요.\n\n" +
            "시트는 공유 → \"링크가 있는 모든 사용자: 뷰어\"로 되어 있어야 합니다.", "확인");
    }

    // ───────── 아이템 ─────────

    private static string ImportItems(string url)
    {
        if (!TryDownloadTable(url, ItemsSheet, out Table table, out string error))
            return error;

        if (!table.Require(out error, "key", "itemName"))
            return error;

        SpriteFinder sprites = new SpriteFinder();
        AssetFinder assets = new AssetFinder();
        List<FieldInfo> extraFields = ItemExtraFields();

        return ImportRows<ItemData>(table, ItemsSheet, ItemsFolder, (row, item) =>
        {
            row.Text("itemName", v => item.itemName = v);
            row.Text("concept", v => item.concept = v);
            row.Text("effectText", v => item.effectText = v);
            row.Int("price", v => item.price = v);
            row.UnlockStage(row.Has("unlock") ? "unlock" : "unlockedAtStart", v => item.unlockStage = v); // 예전 열 이름도 인식
            row.Enum<ItemEffectType>("effectType", v => item.effectType = v);
            row.Int("value", v => item.value = v);
            row.Text("icon", v =>
            {
                Sprite sprite = sprites.Find(v, out string spriteError);

                if (sprite == null)
                    row.Error("icon", spriteError);
                else
                    item.icon = sprite;
            });

            // 위에서 따로 처리하지 않은 칸 (칸 추가 툴로 넣은 것)은 자동으로
            ApplyFields(row, item, extraFields, assets);
        });
    }

    // 아이템 시트에서 위에서 직접 처리하는 칸 (나머지는 자동)
    private static readonly HashSet<string> ItemCustomFields = new HashSet<string>
    {
        "itemName", "icon", "concept", "effectText", "price", "unlockStage", "effectType", "value",
    };

    private static List<FieldInfo> ItemExtraFields()
    {
        return SheetFields(typeof(ItemData)).Where(f => !ItemCustomFields.Contains(f.Name)).ToList();
    }

    private static string BuildItemsCsv()
    {
        StringBuilder csv = new StringBuilder();
        List<FieldInfo> extraFields = ItemExtraFields();

        AppendCsvLine(csv, new[] { "key", "itemName", "icon", "price", "unlock", "effectType", "value", "concept", "effectText" }
            .Concat(extraFields.Select(f => f.Name)).ToArray());
        AppendCsvLine(csv, new[] { "#설명: 파일 이름(영문)", "이름", "아이콘 스프라이트 이름", "가격", "상점 해금 (드롭다운)",
                "효과 (드롭다운)", "효과 수치", "콘셉트 설명", "효과 설명" }
            .Concat(extraFields.Select(Description)).ToArray());

        foreach (string path in AssetsInFolder<ItemData>(ItemsFolder))
        {
            ItemData item = AssetDatabase.LoadAssetAtPath<ItemData>(path);
            AppendCsvLine(csv, new[] { item.name, item.itemName, item.icon != null ? item.icon.name : "", item.price.ToString(),
                    UnlockLabel(item.unlockStage), EnumLabel(item.effectType), item.value.ToString(),
                    item.concept, item.effectText }
                .Concat(extraFields.Select(f => CellText(f.GetValue(item)))).ToArray());
        }

        return csv.ToString();
    }

    // ───────── 캐릭터 (플레이어 / 적 공통) ─────────
    // CharacterData의 칸을 전부 자동으로 읽음 (칸 추가 툴로 넣은 칸도 열 이름 = 변수 이름)

    private static string ImportCharacters(string url)
    {
        if (!TryDownloadTable(url, CharactersSheet, out Table table, out string error))
            return error;

        if (!table.Require(out error, "key"))
            return error;

        return ImportByFields(table, CharactersSheet, CharactersFolder, typeof(CharacterData));
    }

    private static string BuildCharactersCsv()
    {
        List<CharacterData> defaults = new List<CharacterData>();

        // 아직 가져온 적 없으면 기본값으로 플레이어 / 적 양식 채움
        if (AssetsInFolder<CharacterData>(CharactersFolder).Count == 0)
        {
            foreach ((string key, string label) in new[] { (CharacterDatabase.PlayerKey, "플레이어"), (CharacterDatabase.EnemyKey, "적") })
            {
                CharacterData data = ScriptableObject.CreateInstance<CharacterData>();
                data.name = key;
                data.displayName = label;
                defaults.Add(data);
            }
        }

        string csv = BuildFieldsCsv(typeof(CharacterData), CharactersFolder, "#설명: player = 플레이어 / enemy = 적", defaults);

        foreach (CharacterData data in defaults)
            UnityEngine.Object.DestroyImmediate(data);

        return csv;
    }

    // ───────── 공통: 행 → 에셋 ─────────

    private static string ImportRows<T>(Table table, string sheetName, string folder, Action<Row, T> apply)
        where T : ScriptableObject
    {
        return ImportRows(table, sheetName, folder, typeof(T), (row, asset) => apply(row, (T)asset));
    }

    private static string ImportRows(Table table, string sheetName, string folder, Type type, Action<Row, ScriptableObject> apply)
    {
        if (!AssetDatabase.IsValidFolder(folder))
            AssetDatabase.CreateFolder(Path.GetDirectoryName(folder).Replace('\\', '/'), Path.GetFileName(folder));

        int created = 0, changed = 0, same = 0;
        List<string> errors = new List<string>();
        HashSet<string> keys = new HashSet<string>();

        AssetDatabase.StartAssetEditing();

        try
        {
            foreach (Row row in table.Rows)
            {
                string key = row.Get("key");

                if (string.IsNullOrEmpty(key) || key.StartsWith("#"))
                    continue;

                if (key.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
                {
                    errors.Add($"{row.Line}행: key '{key}'에 파일 이름에 못 쓰는 문자가 있음");
                    continue;
                }

                if (!keys.Add(key))
                {
                    errors.Add($"{row.Line}행: key '{key}'가 중복됨");
                    continue;
                }

                string path = $"{folder}/{key}.asset";
                ScriptableObject asset = AssetDatabase.LoadAssetAtPath(path, type) as ScriptableObject;

                // 복사본에 먼저 적용해보고 오류가 없을 때만 실제 에셋에 반영
                ScriptableObject copy = asset != null ? UnityEngine.Object.Instantiate(asset) : ScriptableObject.CreateInstance(type);
                copy.name = key;

                apply(row, copy);

                if (row.Errors.Count > 0)
                {
                    errors.AddRange(row.Errors);
                    UnityEngine.Object.DestroyImmediate(copy);
                    continue;
                }

                if (asset == null)
                {
                    AssetDatabase.CreateAsset(copy, path);
                    created++;
                    continue;
                }

                if (EditorJsonUtility.ToJson(copy) == EditorJsonUtility.ToJson(asset))
                {
                    same++;
                }
                else
                {
                    Undo.RecordObject(asset, "구글 시트 가져오기");
                    EditorUtility.CopySerialized(copy, asset);
                    EditorUtility.SetDirty(asset);
                    changed++;
                }

                UnityEngine.Object.DestroyImmediate(copy);
            }
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
            AssetDatabase.SaveAssets();
        }

        // 시트에서 빠진 에셋은 실수일 수도 있으니 지우지 않고 알려주기만 함
        List<string> missing = new List<string>();

        foreach (string path in AssetsInFolder(folder, type))
        {
            string name = Path.GetFileNameWithoutExtension(path);

            if (!keys.Contains(name))
                missing.Add(name);
        }

        StringBuilder report = new StringBuilder();
        report.Append($"[{sheetName}] 새로 만듦 {created} / 바뀜 {changed} / 그대로 {same}");

        if (missing.Count > 0)
        {
            report.Append($"\n시트에 없는 에셋 {missing.Count}개 (삭제 안 함): {string.Join(", ", missing)}");
            Debug.LogWarning($"[구글 시트] {sheetName} 시트에 없는 에셋 (삭제하려면 직접 지우세요): {string.Join(", ", missing)}");
        }

        if (errors.Count > 0)
        {
            report.Append($"\n오류 {errors.Count}개 — 해당 행은 적용 안 됨 (콘솔 확인)");

            foreach (string error in errors)
                Debug.LogError($"[구글 시트] {sheetName} {error}");
        }

        Debug.Log($"[구글 시트] {report}");
        return report.ToString();
    }

    private static List<string> AssetsInFolder<T>(string folder) where T : UnityEngine.Object
    {
        return AssetsInFolder(folder, typeof(T));
    }

    private static List<string> AssetsInFolder(string folder, Type type)
    {
        List<string> paths = new List<string>();

        if (!AssetDatabase.IsValidFolder(folder))
            return paths;

        foreach (string guid in AssetDatabase.FindAssets("t:" + type.Name, new[] { folder }))
            paths.Add(AssetDatabase.GUIDToAssetPath(guid));

        paths.Sort(StringComparer.Ordinal);
        return paths;
    }

    // ───────── 다른 데이터 (새 데이터베이스 만들기로 만든 것) ─────────
    // 시트 열 이름 = 변수 이름. 아래 형식을 자동으로 읽는다
    //  string / int / long / float / double / bool (TRUE·FALSE, O·X, 1·0)
    //  Vector2 / Vector3 / Vector2Int / Vector3Int ("1, 2" 처럼), Color ("#FF8800" 또는 "#FF880080")
    //  enum (코드 이름 또는 드롭다운 이름), 에셋 / 다른 데이터 (이름으로 찾음)
    //  List / 배열은 칸 하나에 | 로 구분 ("sword | bow")

    private static GameDatabaseAttribute Info(Type type)
    {
        return (GameDatabaseAttribute)Attribute.GetCustomAttribute(type, typeof(GameDatabaseAttribute));
    }

    // 아이템 / 캐릭터는 위에서 따로 처리하므로 제외
    private static IEnumerable<Type> OtherDataTypes()
    {
        return TypeCache.GetTypesWithAttribute<GameDatabaseAttribute>()
            .Where(t => typeof(ScriptableObject).IsAssignableFrom(t) && !t.IsAbstract
                        && t != typeof(ItemData) && t != typeof(CharacterData))
            .OrderBy(t => t.Name);
    }

    private static readonly Regex Number = new Regex(@"-?\d+(\.\d+)?([eE][-+]?\d+)?");

    // 칸 하나에 들어가는 형식
    private static bool IsCellType(Type type)
    {
        return type == typeof(string) || type == typeof(int) || type == typeof(long) || type == typeof(float)
               || type == typeof(double) || type == typeof(bool)
               || type == typeof(Vector2) || type == typeof(Vector3) || type == typeof(Vector2Int) || type == typeof(Vector3Int)
               || type == typeof(Color) || type.IsEnum || typeof(UnityEngine.Object).IsAssignableFrom(type);
    }

    // List<T> / T[]면 T, 아니면 null
    private static Type ElementType(Type type)
    {
        if (type.IsArray)
            return type.GetElementType();

        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
            return type.GetGenericArguments()[0];

        return null;
    }

    // 시트 열로 쓰는 칸: Inspector에 보이는 변수 중 위 형식인 것 (AnimationCurve 같은 건 Inspector에서만)
    private static List<FieldInfo> SheetFields(Type type)
    {
        List<FieldInfo> fields = new List<FieldInfo>();

        foreach (FieldInfo field in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
        {
            bool serialized = field.IsPublic
                ? !field.IsDefined(typeof(NonSerializedAttribute), true)
                : field.IsDefined(typeof(SerializeField), true);

            if (!serialized || field.IsDefined(typeof(HideInInspector), true))
                continue;

            Type element = ElementType(field.FieldType);

            if (IsCellType(field.FieldType) || (element != null && IsCellType(element)))
                fields.Add(field);
        }

        return fields;
    }

    private static string ImportOther(GoogleSheetSettings.SheetLink link)
    {
        Type type = OtherDataTypes().FirstOrDefault(t => t.Name == link.dataType);

        if (type == null)
            return $"[{link.dataType}] 이런 데이터 종류가 없음. 링크 설정의 Other Sheets 이름을 확인해 주세요.";

        string sheetName = Info(type).DisplayName;

        if (!TryDownloadTable(link.url, sheetName, out Table table, out string error))
            return error;

        if (!table.Require(out error, "key"))
            return error;

        return ImportByFields(table, sheetName, "Assets/Resources/" + Info(type).Folder, type);
    }

    // 칸을 전부 자동으로 읽어서 가져오기
    private static string ImportByFields(Table table, string sheetName, string folder, Type type)
    {
        AssetFinder assets = new AssetFinder();
        List<FieldInfo> fields = SheetFields(type);

        // 시트 열 이름은 대소문자를 안 가려서 hp / Hp 같은 칸이 같이 있으면 구분이 안 됨
        string[] clashes = fields.GroupBy(f => f.Name, StringComparer.OrdinalIgnoreCase)
            .Where(g => g.Count() > 1)
            .Select(g => string.Join(" / ", g.Select(f => f.Name)))
            .ToArray();

        if (clashes.Length > 0)
            return $"[{sheetName}] 대소문자만 다른 칸이 있어서 시트 열을 구분할 수 없음: {string.Join(", ", clashes)}. 칸 이름을 바꿔주세요.";

        return ImportRows(table, sheetName, folder, type, (row, asset) => ApplyFields(row, asset, fields, assets));
    }

    // 행의 값을 칸마다 넣기 (빈 칸은 그대로)
    private static void ApplyFields(Row row, UnityEngine.Object asset, List<FieldInfo> fields, AssetFinder assets)
    {
        foreach (FieldInfo field in fields)
        {
            string cell = row.Get(field.Name);

            if (cell.Length == 0)
                continue; // 빈 칸은 그대로

            Type element = ElementType(field.FieldType);

            if (element == null)
            {
                if (TryParseCell(field.FieldType, cell, assets, out object value, out string cellError))
                    field.SetValue(asset, value);
                else
                    row.Error(field.Name, cellError);

                continue;
            }

            List<object> values = new List<object>();
            bool ok = true;

            foreach (string part in cell.Split('|'))
            {
                if (part.Trim().Length == 0)
                    continue;

                if (!TryParseCell(element, part, assets, out object value, out string cellError))
                {
                    row.Error(field.Name, cellError);
                    ok = false;
                    break;
                }

                values.Add(value);
            }

            if (!ok)
                continue;

            if (field.FieldType.IsArray)
            {
                Array array = Array.CreateInstance(element, values.Count);

                for (int i = 0; i < values.Count; i++)
                    array.SetValue(values[i], i);

                field.SetValue(asset, array);
            }
            else
            {
                System.Collections.IList list =
                    (System.Collections.IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(element));

                foreach (object value in values)
                    list.Add(value);

                field.SetValue(asset, list);
            }
        }
    }

    // 시트 글자 하나 → 값
    private static bool TryParseCell(Type type, string text, AssetFinder assets, out object value, out string error)
    {
        string s = text.Trim();
        CultureInfo inv = CultureInfo.InvariantCulture;
        value = null;
        error = null;

        if (type == typeof(string))
        {
            value = s;
            return true;
        }

        if (type == typeof(int) && int.TryParse(s.Replace(",", ""), NumberStyles.Integer, inv, out int i))
        {
            value = i;
            return true;
        }

        if (type == typeof(long) && long.TryParse(s.Replace(",", ""), NumberStyles.Integer, inv, out long l))
        {
            value = l;
            return true;
        }

        if (type == typeof(float) && float.TryParse(s.Replace(",", ""), NumberStyles.Float, inv, out float f))
        {
            value = f;
            return true;
        }

        if (type == typeof(double) && double.TryParse(s.Replace(",", ""), NumberStyles.Float, inv, out double d))
        {
            value = d;
            return true;
        }

        if (type == typeof(bool))
        {
            switch (s.ToUpperInvariant())
            {
                case "TRUE": case "O": case "1": case "켜기": value = true; return true;
                case "FALSE": case "X": case "0": case "끄기": value = false; return true;
            }

            error = $"'{s}'는 TRUE / FALSE가 아님";
            return false;
        }

        if (type == typeof(Vector2) || type == typeof(Vector3) || type == typeof(Vector2Int) || type == typeof(Vector3Int))
        {
            float[] n = Number.Matches(s).Cast<Match>().Select(m => float.Parse(m.Value, inv)).ToArray();
            bool three = type == typeof(Vector3) || type == typeof(Vector3Int);

            if (n.Length == (three ? 3 : 2))
            {
                if (type == typeof(Vector2)) value = new Vector2(n[0], n[1]);
                else if (type == typeof(Vector3)) value = new Vector3(n[0], n[1], n[2]);
                else if (type == typeof(Vector2Int)) value = new Vector2Int(Mathf.RoundToInt(n[0]), Mathf.RoundToInt(n[1]));
                else value = new Vector3Int(Mathf.RoundToInt(n[0]), Mathf.RoundToInt(n[1]), Mathf.RoundToInt(n[2]));
                return true;
            }

            error = $"'{s}'는 {type.Name}가 아님. \"{(three ? "1, 2, 3" : "1, 2")}\"처럼 적어주세요";
            return false;
        }

        if (type == typeof(Color))
        {
            if (ColorUtility.TryParseHtmlString(s.StartsWith("#") ? s : "#" + s, out Color color))
            {
                value = color;
                return true;
            }

            error = $"'{s}'는 색이 아님. \"#FF8800\"처럼 적어주세요";
            return false;
        }

        if (type.IsEnum)
        {
            List<string> labels = new List<string>();

            // 드롭다운 이름(HP 회복) 또는 코드 이름(HealHP)
            foreach (System.Enum option in System.Enum.GetValues(type))
            {
                string label = EnumLabel(option);
                labels.Add(label);

                if (s == label || s.Equals(option.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    value = option;
                    return true;
                }
            }

            error = $"'{s}'는 없는 값. 가능한 값: {string.Join(", ", labels)}";
            return false;
        }

        if (typeof(UnityEngine.Object).IsAssignableFrom(type))
        {
            value = assets.Find(type, s, out error);
            return value != null;
        }

        if (error == null)
            error = $"'{s}'는 {type.Name}(으)로 못 읽음";

        return false;
    }

    private static string BuildOtherCsv(Type type)
    {
        return BuildFieldsCsv(type, "Assets/Resources/" + Info(type).Folder, "#설명: 파일 이름(영문)", null);
    }

    // 칸 목록으로 CSV 만들기. extraRows = 폴더에 없지만 양식에 넣을 예시 데이터
    private static string BuildFieldsCsv(Type type, string folder, string keyDescription, IEnumerable<UnityEngine.Object> extraRows)
    {
        List<FieldInfo> fields = SheetFields(type);
        StringBuilder csv = new StringBuilder();

        AppendCsvLine(csv, new[] { "key" }.Concat(fields.Select(f => f.Name)).ToArray());
        AppendCsvLine(csv, new[] { keyDescription }.Concat(fields.Select(Description)).ToArray());

        IEnumerable<UnityEngine.Object> rows = AssetsInFolder(folder, type).Select(path => AssetDatabase.LoadAssetAtPath(path, type));

        foreach (UnityEngine.Object asset in extraRows != null ? extraRows.Concat(rows) : rows)
            AppendCsvLine(csv, new[] { asset.name }.Concat(fields.Select(f => CellText(f.GetValue(asset)))).ToArray());

        return csv.ToString();
    }

    // 설명 행: 툴팁 + 적는 법
    private static string Description(FieldInfo field)
    {
        string tooltip = field.GetCustomAttribute<TooltipAttribute>()?.tooltip ?? "";
        Type element = ElementType(field.FieldType);
        string hint = FormatHint(element ?? field.FieldType);

        if (element != null)
            hint = (hint.Length > 0 ? hint : "값") + " | 로 여러 개";

        if (hint.Length == 0)
            return tooltip;

        return tooltip.Length > 0 ? $"{tooltip} ({hint})" : hint;
    }

    private static string FormatHint(Type type)
    {
        if (type == typeof(Vector2) || type == typeof(Vector2Int)) return "x, y";
        if (type == typeof(Vector3) || type == typeof(Vector3Int)) return "x, y, z";
        if (type == typeof(Color)) return "#RRGGBB";
        if (type == typeof(bool)) return "TRUE / FALSE";
        if (type.IsEnum) return string.Join(" / ", System.Enum.GetValues(type).Cast<System.Enum>().Select(EnumLabel));
        if (typeof(UnityEngine.Object).IsAssignableFrom(type)) return type.Name + " 이름";
        return "";
    }

    private static string CellText(object value)
    {
        CultureInfo inv = CultureInfo.InvariantCulture;

        switch (value)
        {
            case null: return "";
            case UnityEngine.Object obj: return obj != null ? obj.name : "";
            case string text: return text;
            case bool b: return b ? "TRUE" : "FALSE";
            case float f: return f.ToString(inv);
            case double d: return d.ToString(inv);
            case Vector2 v: return $"{v.x.ToString(inv)}, {v.y.ToString(inv)}";
            case Vector3 v: return $"{v.x.ToString(inv)}, {v.y.ToString(inv)}, {v.z.ToString(inv)}";
            case Vector2Int v: return $"{v.x}, {v.y}";
            case Vector3Int v: return $"{v.x}, {v.y}, {v.z}";
            case Color c: return "#" + (c.a < 1f ? ColorUtility.ToHtmlStringRGBA(c) : ColorUtility.ToHtmlStringRGB(c));
            case System.Enum e: return EnumLabel(e);
            case System.Collections.IList list: return string.Join(" | ", list.Cast<object>().Select(CellText));
            default: return Convert.ToString(value, inv);
        }
    }

    // ───────── 드롭다운 표시 이름 ─────────

    private const string UnlockAtStartLabel = "처음부터";

    private static string UnlockLabel(int stage)
    {
        return stage == 0 ? UnlockAtStartLabel : $"스테이지 {stage} 클리어";
    }

    // enum 값의 [InspectorName] (없으면 영문 이름)
    private static string EnumLabel(System.Enum value)
    {
        FieldInfo field = value.GetType().GetField(value.ToString());
        InspectorNameAttribute attribute = field?.GetCustomAttribute<InspectorNameAttribute>();
        return attribute != null ? attribute.displayName : value.ToString();
    }

    // ───────── 다운로드 ─────────

    private static bool TryDownloadTable(string url, string sheetName, out Table table, out string error)
    {
        table = null;
        string csvUrl = ToCsvUrl(url.Trim());

        using (UnityWebRequest request = UnityWebRequest.Get(csvUrl))
        {
            UnityWebRequestAsyncOperation op = request.SendWebRequest();

            try
            {
                while (!op.isDone)
                {
                    if (EditorUtility.DisplayCancelableProgressBar("구글 시트", $"{sheetName} 시트 받는 중...", request.downloadProgress))
                    {
                        request.Abort();
                        error = $"[{sheetName}] 취소함";
                        return false;
                    }
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }

            if (request.result != UnityWebRequest.Result.Success)
            {
                error = $"[{sheetName}] 다운로드 실패: {request.error}\n링크를 확인해 주세요.";
                return false;
            }

            string text = request.downloadHandler.text;

            // 비공개 시트면 CSV 대신 구글 로그인 페이지(HTML)가 온다
            if (text.TrimStart().StartsWith("<"))
            {
                error = $"[{sheetName}] CSV가 아니라 웹페이지가 받아짐.\n시트 공유를 \"링크가 있는 모든 사용자: 뷰어\"로 바꿔주세요.";
                return false;
            }

            table = new Table(ParseCsv(text));
            error = null;
            return true;
        }
    }

    // 브라우저 주소창 주소를 CSV 다운로드 주소로 바꿈
    //  .../d/<id>/edit#gid=123       → .../d/<id>/export?format=csv&gid=123  (공유 링크, 바로 반영)
    //  .../d/e/<id>/pubhtml?gid=123  → .../d/e/<id>/pub?gid=123&output=csv  (웹에 게시, 반영에 몇 분 걸림)
    private static string ToCsvUrl(string url)
    {
        if (url.Contains("/d/e/"))
        {
            url = url.Replace("/pubhtml", "/pub");

            if (!url.Contains("output=csv"))
                url += (url.Contains("?") ? "&" : "?") + "output=csv";

            return url;
        }

        Match id = Regex.Match(url, @"/spreadsheets/d/([a-zA-Z0-9_-]+)");

        if (!id.Success)
            return url; // 그 외 CSV 주소는 그대로 사용

        Match gid = Regex.Match(url, @"[#&?]gid=(\d+)");
        return $"https://docs.google.com/spreadsheets/d/{id.Groups[1].Value}/export?format=csv&gid={(gid.Success ? gid.Groups[1].Value : "0")}";
    }

    // ───────── CSV ─────────

    // 따옴표 안의 쉼표 / 줄바꿈 / "" 처리 (RFC 4180)
    private static List<string[]> ParseCsv(string text)
    {
        List<string[]> rows = new List<string[]>();
        List<string> cells = new List<string>();
        StringBuilder cell = new StringBuilder();
        bool quoted = false;

        if (text.Length > 0 && text[0] == '\uFEFF')
            text = text.Substring(1);

        for (int i = 0; i < text.Length; i++)
        {
            char c = text[i];

            if (quoted)
            {
                if (c == '"' && i + 1 < text.Length && text[i + 1] == '"')
                {
                    cell.Append('"');
                    i++;
                }
                else if (c == '"')
                {
                    quoted = false;
                }
                else if (c != '\r')
                {
                    cell.Append(c);
                }
            }
            else if (c == '"')
            {
                quoted = true;
            }
            else if (c == ',')
            {
                cells.Add(cell.ToString());
                cell.Clear();
            }
            else if (c == '\n')
            {
                cells.Add(cell.ToString());
                cell.Clear();
                rows.Add(cells.ToArray());
                cells.Clear();
            }
            else if (c != '\r')
            {
                cell.Append(c);
            }
        }

        if (cell.Length > 0 || cells.Count > 0)
        {
            cells.Add(cell.ToString());
            rows.Add(cells.ToArray());
        }

        return rows;
    }

    private static void AppendCsvLine(StringBuilder csv, params string[] cells)
    {
        for (int i = 0; i < cells.Length; i++)
        {
            if (i > 0)
                csv.Append(',');

            string cell = cells[i] ?? "";

            if (cell.IndexOfAny(new[] { ',', '"', '\n', '\r' }) >= 0)
                cell = "\"" + cell.Replace("\"", "\"\"") + "\"";

            csv.Append(cell);
        }

        csv.Append("\r\n");
    }

    private class Table
    {
        private readonly Dictionary<string, int> columns = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        public readonly List<Row> Rows = new List<Row>();

        public Table(List<string[]> lines)
        {
            if (lines.Count == 0)
                return;

            for (int i = 0; i < lines[0].Length; i++)
            {
                string name = lines[0][i].Trim();

                if (name.Length > 0 && !columns.ContainsKey(name))
                    columns.Add(name, i);
            }

            for (int i = 1; i < lines.Count; i++)
                Rows.Add(new Row(columns, lines[i], i + 1));
        }

        public bool Require(out string error, params string[] names)
        {
            List<string> missing = new List<string>();

            foreach (string name in names)
            {
                if (!columns.ContainsKey(name))
                    missing.Add(name);
            }

            error = missing.Count > 0
                ? $"1행(머리글)에 {string.Join(", ", missing)} 열이 없음. 시트 링크가 맞는 탭인지 확인해 주세요."
                : null;

            return missing.Count == 0;
        }
    }

    private class Row
    {
        private readonly Dictionary<string, int> columns;
        private readonly string[] cells;

        public readonly int Line; // 시트에서의 행 번호
        public readonly List<string> Errors = new List<string>();

        public Row(Dictionary<string, int> columns, string[] cells, int line)
        {
            this.columns = columns;
            this.cells = cells;
            Line = line;
        }

        public bool Has(string column)
        {
            return columns.ContainsKey(column);
        }

        public string Get(string column)
        {
            if (!columns.TryGetValue(column, out int index) || index >= cells.Length)
                return "";

            return cells[index].Trim();
        }

        public void Error(string column, string message)
        {
            Errors.Add($"{Line}행 {column}: {message}");
        }

        // 아래 함수들은 빈 칸이면 아무것도 안 함 (기존 값 유지)

        public void Text(string column, Action<string> set)
        {
            string value = Get(column);

            if (value.Length > 0)
                set(value);
        }

        public void Int(string column, Action<int> set)
        {
            string value = Get(column).Replace(",", ""); // 시트에서 1,000처럼 표시될 때

            if (value.Length == 0)
                return;

            if (int.TryParse(value, out int result))
                set(result);
            else
                Error(column, $"'{value}'는 정수가 아님");
        }

        // "처음부터" / "스테이지 N 클리어" (숫자만 써도 됨. 예전 TRUE도 처음부터로 인식)
        public void UnlockStage(string column, Action<int> set)
        {
            string value = Get(column);

            if (value.Length == 0)
                return;

            if (value == UnlockAtStartLabel || value.Equals("TRUE", StringComparison.OrdinalIgnoreCase))
            {
                set(0);
                return;
            }

            Match number = Regex.Match(value, @"\d+");

            if (number.Success)
                set(int.Parse(number.Value));
            else
                Error(column, $"'{value}'는 해금 조건이 아님. \"{UnlockAtStartLabel}\" 또는 \"스테이지 1 클리어\"처럼 적어주세요");
        }

        public void Enum<T>(string column, Action<T> set) where T : struct, System.Enum
        {
            string value = Get(column);

            if (value.Length == 0)
                return;

            List<string> labels = new List<string>();

            // 드롭다운 이름(HP 회복) 또는 영문 이름(HealHP)
            foreach (T option in System.Enum.GetValues(typeof(T)))
            {
                string label = EnumLabel(option);
                labels.Add(label);

                if (value == label || value.Equals(option.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    set(option);
                    return;
                }
            }

            Error(column, $"'{value}'는 없는 값. 가능한 값: {string.Join(", ", labels)}");
        }
    }

    // 이름으로 에셋 찾기 (형식 상관없이. 스프라이트처럼 한 파일 안에 여러 개 있는 것도 포함)
    // 형식마다 처음 쓸 때 한 번만 전체 검색
    private class AssetFinder
    {
        private readonly Dictionary<Type, Dictionary<string, List<UnityEngine.Object>>> cache =
            new Dictionary<Type, Dictionary<string, List<UnityEngine.Object>>>();

        public UnityEngine.Object Find(Type type, string name, out string error)
        {
            if (!cache.TryGetValue(type, out Dictionary<string, List<UnityEngine.Object>> byName))
                cache.Add(type, byName = Build(type));

            error = null;

            if (!byName.TryGetValue(name, out List<UnityEngine.Object> found))
            {
                error = $"'{name}' 이름의 {type.Name}이(가) 없음";
                return null;
            }

            if (found.Count > 1)
                Debug.LogWarning($"[구글 시트] '{name}' {type.Name}이(가) {found.Count}개 있어서 첫 번째 사용: {AssetDatabase.GetAssetPath(found[0])}");

            return found[0];
        }

        private static Dictionary<string, List<UnityEngine.Object>> Build(Type type)
        {
            Dictionary<string, List<UnityEngine.Object>> byName = new Dictionary<string, List<UnityEngine.Object>>();
            HashSet<string> paths = new HashSet<string>();

            foreach (string guid in AssetDatabase.FindAssets("t:" + type.Name, new[] { "Assets" }))
                paths.Add(AssetDatabase.GUIDToAssetPath(guid));

            foreach (string path in paths)
            {
                // 프리팹은 파일 자체만 (안에 든 자식 오브젝트는 제외)
                UnityEngine.Object[] assets = type == typeof(GameObject)
                    ? new[] { AssetDatabase.LoadMainAssetAtPath(path) }
                    : AssetDatabase.LoadAllAssetsAtPath(path);

                foreach (UnityEngine.Object asset in assets)
                {
                    if (asset == null || !type.IsInstanceOfType(asset))
                        continue;

                    if (!byName.TryGetValue(asset.name, out List<UnityEngine.Object> list))
                        byName.Add(asset.name, list = new List<UnityEngine.Object>());

                    list.Add(asset);
                }
            }

            return byName;
        }
    }

    // 스프라이트 이름으로 찾기 (시트 여러 장으로 잘린 스프라이트도 포함). 처음 쓸 때 한 번만 전체 검색
    private class SpriteFinder
    {
        private Dictionary<string, List<Sprite>> sprites;

        public Sprite Find(string name, out string error)
        {
            if (sprites == null)
                Build();

            error = null;

            if (!sprites.TryGetValue(name, out List<Sprite> found))
            {
                error = $"'{name}' 이름의 스프라이트가 없음";
                return null;
            }

            if (found.Count > 1)
                Debug.LogWarning($"[구글 시트] '{name}' 스프라이트가 {found.Count}개 있어서 첫 번째 사용: {AssetDatabase.GetAssetPath(found[0])}");

            return found[0];
        }

        private void Build()
        {
            sprites = new Dictionary<string, List<Sprite>>();
            HashSet<string> paths = new HashSet<string>();

            foreach (string guid in AssetDatabase.FindAssets("t:Sprite", new[] { "Assets" }))
                paths.Add(AssetDatabase.GUIDToAssetPath(guid));

            foreach (string path in paths)
            {
                foreach (UnityEngine.Object asset in AssetDatabase.LoadAllAssetsAtPath(path))
                {
                    if (!(asset is Sprite sprite))
                        continue;

                    if (!sprites.TryGetValue(sprite.name, out List<Sprite> list))
                        sprites.Add(sprite.name, list = new List<Sprite>());

                    list.Add(sprite);
                }
            }
        }
    }
}
