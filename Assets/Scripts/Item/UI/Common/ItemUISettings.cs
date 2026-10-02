using System;
using TMPro;
using UnityEngine;

// 아이템 UI 배치 설정. Assets/Resources/ItemUISettings 에셋을 선택해서 인스펙터에서 조정
// 플레이 중에 값을 바꾸면 바로 화면에 반영된다 (플레이가 끝나도 값은 유지됨)
[CreateAssetMenu(fileName = "ItemUISettings", menuName = "ProjectJ/Item UI Settings")]
public class ItemUISettings : ScriptableObject
{
    [Serializable]
    public class TextStyle
    {
        [Tooltip("버튼 가운데 기준 위치 (캔버스 픽셀)")]
        public Vector2 position;
        [Tooltip("글자 영역 크기 (캔버스 픽셀)")]
        public Vector2 size = new Vector2(250, 30);
        public float fontSize = 22;
        public Color color = new Color(0.1f, 0.1f, 0.1f);
        [Tooltip("자간 (음수면 좁아짐)")]
        public float characterSpacing;
        public FontStyles fontStyle = FontStyles.Normal;
        public TextAlignmentOptions alignment = TextAlignmentOptions.Center;

        public void Apply(TMP_Text text)
        {
            ItemUISettings.Place(text.rectTransform, position, size);

            text.enableAutoSizing = false;
            text.fontSize = fontSize;
            text.color = color;
            text.characterSpacing = characterSpacing;
            text.fontStyle = fontStyle;
            text.alignment = alignment;
            text.margin = Vector4.zero;
        }
    }

    [Header("상점 아이템 버튼 - 아이콘 (버튼 가운데 기준, 캔버스 픽셀)")]
    public Vector2 shopIconPosition = new Vector2(0, 31);
    public Vector2 shopIconSize = new Vector2(120, 120);

    [Header("상점 아이템 버튼 - 이름")]
    public TextStyle shopName = new TextStyle
    {
        position = new Vector2(0, -100),
        size = new Vector2(250, 28),
        fontSize = 24,
        fontStyle = FontStyles.Bold,
    };

    [Header("상점 아이템 버튼 - 가격")]
    public TextStyle shopPrice = new TextStyle
    {
        position = new Vector2(0, -124),
        size = new Vector2(250, 26),
        fontSize = 22,
    };

    [Tooltip("{0} 자리에 가격이 들어감")]
    public string priceFormat = "{0} G";

    // ---------- 불러오기 ----------

    public const string ResourcePath = "ItemUISettings";

    public static event Action Changed;

    private static ItemUISettings instance;

    public static ItemUISettings Instance
    {
        get
        {
            if (instance == null)
                instance = Resources.Load<ItemUISettings>(ResourcePath);

            // 에셋이 없으면 기본값으로 동작
            if (instance == null)
                instance = CreateInstance<ItemUISettings>();

            return instance;
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        instance = null;
        Changed = null;
    }

    // 인스펙터에서 값을 바꿀 때마다 호출됨
    private void OnValidate()
    {
        Changed?.Invoke();
    }

    // 부모 가운데 기준으로 위치 / 크기 지정
    public static void Place(RectTransform rt, Vector2 position, Vector2 size)
    {
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = position;
        rt.sizeDelta = size;
    }
}
