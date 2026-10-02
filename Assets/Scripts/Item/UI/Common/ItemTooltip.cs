using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// 아이템 위에 마우스를 올리면 이름 / 효과 표시 (기획서 7-2-2)
// 다른 스크립트는 ItemTooltip.Show / Hide로 사용
public class ItemTooltip : MonoBehaviour
{
    private static ItemTooltip instance;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        instance = null;
    }

    private Canvas canvas;
    private RectTransform panel;
    private TMP_Text nameText;
    private TMP_Text effectText;

    private static readonly Vector2 Offset = new Vector2(16, -16);

    public static void Create(Canvas canvas)
    {
        canvas = canvas.rootCanvas;

        Image background = ItemUI.CreateImage(canvas.transform, "ItemTooltip", new Color(1f, 0.98f, 0.85f));
        RectTransform panel = background.rectTransform;
        panel.anchorMin = panel.anchorMax = new Vector2(0.5f, 0.5f); // 캔버스 전체로 늘어나지 않게
        panel.pivot = new Vector2(0, 1); // 마우스 오른쪽 아래에 표시
        panel.sizeDelta = new Vector2(260, 90);

        // 대기화면 스프라이트(아이템 칸)보다 항상 위에 그려지도록 정렬 순서를 따로 줌
        Canvas tooltipCanvas = panel.gameObject.AddComponent<Canvas>();
        tooltipCanvas.overrideSorting = true;
        tooltipCanvas.sortingOrder = 100;

        // 툴팁이 마우스를 가리면 PointerExit가 계속 발생해서 깜빡이므로 클릭 판정 끔
        CanvasGroup group = panel.gameObject.AddComponent<CanvasGroup>();
        group.blocksRaycasts = false;

        ItemTooltip tooltip = panel.gameObject.AddComponent<ItemTooltip>();
        tooltip.canvas = canvas;
        tooltip.panel = panel;

        tooltip.nameText = ItemUI.CreateText(panel, "Name", 28, TextAlignmentOptions.Left);
        tooltip.nameText.fontStyle = FontStyles.Bold;
        ItemUI.SetAnchors(tooltip.nameText.rectTransform, 0.04f, 0.58f, 0.96f, 0.96f);

        tooltip.effectText = ItemUI.CreateText(panel, "Effect", 22, TextAlignmentOptions.TopLeft);
        ItemUI.SetAnchors(tooltip.effectText.rectTransform, 0.04f, 0.04f, 0.96f, 0.58f);

        // 자간 좁게
        tooltip.nameText.characterSpacing = -4;
        tooltip.effectText.characterSpacing = -4;

        instance = tooltip;
        panel.gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        if (instance == this)
            instance = null;
    }

    public static void Show(ItemData item)
    {
        if (instance == null || item == null)
            return;

        instance.nameText.text = item.itemName;
        instance.effectText.text = item.effectText;
        instance.panel.SetAsLastSibling();
        instance.panel.gameObject.SetActive(true);
        instance.FollowMouse();
    }

    public static void Hide()
    {
        if (instance != null)
            instance.panel.gameObject.SetActive(false);
    }

    private void Update()
    {
        FollowMouse();
    }

    private void FollowMouse()
    {
        if (Mouse.current == null)
            return;

        Vector2 screen = Mouse.current.position.ReadValue() + Offset;
        Camera cam = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;

        RectTransformUtility.ScreenPointToWorldPointInRectangle(
            (RectTransform)canvas.transform, screen, cam, out Vector3 world);

        panel.position = world;
    }
}
