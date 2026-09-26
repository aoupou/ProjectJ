using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// 아이템 UI를 코드로 만들 때 쓰는 공용 함수들
//
// 팀원이 만든 이미지에는 '아이템 이미지', '아이템 이름' 같은 임시 글자가 그림에 박혀 있어서
// 그 칸을 같은 색으로 덮고 그 위에 실제 아이템 내용을 표시한다
// 칸 위치는 스프라이트 크기 기준 비율(0~1, 왼쪽 아래가 0,0)로 적는다
public static class ItemUI
{
    public static readonly Color Blue = new Color32(0xDC, 0xEA, 0xF7, 0xFF);  // 상점 아이템 버튼 칸 색
    public static readonly Color Green = new Color32(0xD9, 0xF2, 0xD0, 0xFF); // 상세 정보 / 대기화면 칸 색
    public static readonly Color TextColor = new Color(0.1f, 0.1f, 0.1f);

    private static Sprite whiteSprite;

    // 이 프로젝트는 플레이 시작 시 도메인 리로드가 꺼져 있어서(Enter Play Mode Options)
    // static 값이 이전 플레이에서 그대로 남는다. 그래서 플레이 시작마다 직접 초기화
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        whiteSprite = null;
    }

    // 게임 전체 폰트(개구)를 그대로 사용 (GameFont.cs)
    public static TMP_FontAsset Font => GameFont.Font;

    // 1x1 크기의 흰색 스프라이트 (칸 덮기용)
    private static Sprite WhiteSprite
    {
        get
        {
            if (whiteSprite == null)
            {
                Texture2D tex = Texture2D.whiteTexture;
                whiteSprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), tex.width);
            }

            return whiteSprite;
        }
    }

    public static Rect Area(float xMin, float yMin, float xMax, float yMax)
    {
        return Rect.MinMaxRect(xMin, yMin, xMax, yMax);
    }

    // 씬 안에서 이름으로 오브젝트 찾기 (꺼져있는 것도 포함)
    public static GameObject Find(Scene scene, string name)
    {
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
            {
                if (t.name == name)
                    return t.gameObject;
            }
        }

        return null;
    }

    // 팀원 UI의 원래 글자(TMP) 숨기기
    public static void HideOriginalTexts(GameObject obj)
    {
        foreach (TMP_Text text in obj.GetComponentsInChildren<TMP_Text>(true))
            text.gameObject.SetActive(false);
    }

    // ================= UI (Image / Button) =================

    public static RectTransform CreateRect(Transform parent, string name)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform));
        obj.transform.SetParent(parent, false);
        return (RectTransform)obj.transform;
    }

    // parent 안의 area 영역에 RectTransform을 만든다
    // 팀원 UI는 scale이 가로세로 다르게 늘어나 있어서, 그대로 자식을 넣으면 글자가 찌그러진다
    // 그래서 자식 scale을 반대로 줄여서, 캔버스 기준 1:1 크기로 보이게 맞춘다
    // (글자 크기도 부모 scale과 상관없이 캔버스 픽셀 기준이 됨)
    public static RectTransform CreateRegion(RectTransform parent, string name, Rect area)
    {
        RectTransform rt = CreateRect(parent, name);

        Vector3 s = parent.lossyScale;
        // parent가 꺼져 있어도 찾을 수 있게 true
        float k = parent.GetComponentInParent<Canvas>(true).rootCanvas.transform.lossyScale.x;

        rt.localScale = new Vector3(k / s.x, k / s.y, 1);
        rt.anchorMin = rt.anchorMax = area.center;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;

        Vector2 size = Vector2.Scale(parent.rect.size, area.size);
        rt.sizeDelta = new Vector2(size.x * s.x / k, size.y * s.y / k);

        return rt;
    }

    public static Image CreateImage(Transform parent, string name, Color color, bool raycast = false)
    {
        RectTransform rt = CreateRect(parent, name);
        Image image = rt.gameObject.AddComponent<Image>();
        image.color = color;
        image.raycastTarget = raycast;
        Stretch(rt);
        return image;
    }

    public static Image CreateIcon(Transform parent)
    {
        Image icon = CreateImage(parent, "Icon", Color.white);
        icon.preserveAspect = true;
        icon.enabled = false;
        return icon;
    }

    public static TMP_Text CreateText(Transform parent, string name, float maxSize,
        TextAlignmentOptions alignment = TextAlignmentOptions.Center)
    {
        RectTransform rt = CreateRect(parent, name);
        TextMeshProUGUI text = rt.gameObject.AddComponent<TextMeshProUGUI>();
        text.font = Font;
        text.color = TextColor;
        text.alignment = alignment;
        text.raycastTarget = false;
        text.enableAutoSizing = true;
        text.fontSizeMin = 8;
        text.fontSizeMax = maxSize;
        text.margin = new Vector4(6, 4, 6, 4);
        Stretch(rt);
        return text;
    }

    public static void Stretch(RectTransform rt)
    {
        SetAnchors(rt, 0, 0, 1, 1);
    }

    // 부모 기준 비율(0~1)로 위치/크기 지정
    public static void SetAnchors(RectTransform rt, float xMin, float yMin, float xMax, float yMax)
    {
        rt.anchorMin = new Vector2(xMin, yMin);
        rt.anchorMax = new Vector2(xMax, yMax);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    // ================= 월드 스프라이트 (대기화면) =================

    // 스프라이트 안의 area 영역 중심 (로컬 좌표)
    private static Vector2 LocalCenter(SpriteRenderer parent, Rect area)
    {
        Bounds b = parent.sprite.bounds;
        return (Vector2)b.min + Vector2.Scale(area.center, b.size);
    }

    private static Vector2 LocalSize(SpriteRenderer parent, Rect area)
    {
        return Vector2.Scale(area.size, parent.sprite.bounds.size);
    }

    private static Transform CreateChild(SpriteRenderer parent, string name, Rect area)
    {
        Transform t = new GameObject(name).transform;
        t.SetParent(parent.transform, false);
        t.localPosition = (Vector3)LocalCenter(parent, area) + new Vector3(0, 0, -0.01f);
        return t;
    }

    // 칸을 단색으로 덮는 사각형
    public static SpriteRenderer CreateWorldCover(SpriteRenderer parent, Rect area, Color color, int order = 1)
    {
        Transform t = CreateChild(parent, "Cover", area);
        t.localScale = LocalSize(parent, area);

        SpriteRenderer sr = t.gameObject.AddComponent<SpriteRenderer>();
        sr.sprite = WhiteSprite;
        sr.color = color;
        sr.sortingLayerID = parent.sortingLayerID;
        sr.sortingOrder = parent.sortingOrder + order;
        return sr;
    }

    public static SpriteRenderer CreateWorldIcon(SpriteRenderer parent, Rect area, int order = 2)
    {
        Transform t = CreateChild(parent, "Icon", area);

        SpriteRenderer sr = t.gameObject.AddComponent<SpriteRenderer>();
        sr.sortingLayerID = parent.sortingLayerID;
        sr.sortingOrder = parent.sortingOrder + order;
        return sr;
    }

    // 아이콘을 영역 안에 비율 유지해서 맞추기
    public static void SetWorldIcon(SpriteRenderer icon, Sprite sprite, SpriteRenderer parent, Rect area)
    {
        icon.sprite = sprite;
        icon.enabled = sprite != null;

        if (sprite == null)
            return;

        Vector3 s = parent.transform.lossyScale;
        Vector2 worldArea = Vector2.Scale(LocalSize(parent, area), s);
        Vector2 iconSize = sprite.bounds.size;
        float fit = Mathf.Min(worldArea.x / iconSize.x, worldArea.y / iconSize.y);

        icon.transform.localScale = new Vector3(fit / s.x, fit / s.y, 1);
        icon.transform.localPosition =
            (Vector3)(LocalCenter(parent, area) - Vector2.Scale(sprite.bounds.center, new Vector2(fit / s.x, fit / s.y)))
            + new Vector3(0, 0, -0.02f);
    }

    public static TextMeshPro CreateWorldText(SpriteRenderer parent, Rect area, float maxSize, int order = 3)
    {
        Transform t = CreateChild(parent, "Text", area);

        TextMeshPro text = t.gameObject.AddComponent<TextMeshPro>();

        // TextMeshPro를 붙이면 Transform이 RectTransform으로 교체되면서 기존 t는 파괴되므로 다시 가져옴
        t = text.transform;

        // 부모 scale을 되돌려서 1:1 비율로 보이게
        Vector3 s = parent.transform.lossyScale;
        t.localScale = new Vector3(1 / s.x, 1 / s.y, 1);
        text.rectTransform.sizeDelta = Vector2.Scale(LocalSize(parent, area), s);
        t.localPosition = (Vector3)LocalCenter(parent, area) + new Vector3(0, 0, -0.03f);

        text.font = Font;
        text.color = TextColor;
        text.alignment = TextAlignmentOptions.Center;
        text.enableAutoSizing = true;
        text.fontSizeMin = 0.5f;
        text.fontSizeMax = maxSize;
        text.sortingLayerID = parent.sortingLayerID;
        text.sortingOrder = parent.sortingOrder + order;
        return text;
    }
}
