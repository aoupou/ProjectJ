using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// 상점 화면 (기획서 8번)
// 팀원이 만든 Item_1~6 버튼, item_info(상세 정보), goods(골드)에 기능을 붙인다
public class ShopItemUI : MonoBehaviour
{
    // 아이템 버튼 그림(아이템_이미지_이름_가격)에서 예시 글자가 있는 칸 → 같은 색으로 덮어서 지움
    private static readonly Rect CardImageArea = ItemUI.Area(0.09f, 0.29f, 0.91f, 0.92f);
    private static readonly Rect CardTextArea = ItemUI.Area(0.09f, 0.03f, 0.91f, 0.2f);

    // 아이템 버튼 하나에 표시되는 것들 (위치 / 크기는 ItemUISettings 에셋에서 조정)
    private class Card
    {
        public ItemData item;
        public Image icon;
        public TMP_Text nameText;
        public TMP_Text priceText;
    }

    private readonly List<Card> cards = new List<Card>();

    // ---- 상세 정보 그림(아이템_정보_UI) 안의 칸 위치 ----
    private static readonly Rect InfoImageArea = ItemUI.Area(0.07f, 0.7f, 0.42f, 0.92f);
    private static readonly Rect InfoNameArea = ItemUI.Area(0.525f, 0.86f, 0.89f, 0.928f);
    private static readonly Rect InfoConceptArea = ItemUI.Area(0.525f, 0.69f, 0.89f, 0.804f);
    private static readonly Rect InfoEffectArea = ItemUI.Area(0.087f, 0.404f, 0.88f, 0.633f);
    private static readonly Rect InfoPriceArea = ItemUI.Area(0.082f, 0.248f, 0.894f, 0.328f);
    private static readonly Rect InfoBuyArea = ItemUI.Area(0.059f, 0.088f, 0.908f, 0.187f);

    private TMP_Text goldText;

    private GameObject detailRoot; // item_info (팀원 씬에서는 처음에 꺼져 있음)
    private Image detailIcon;
    private TMP_Text detailName;
    private TMP_Text detailConcept;
    private TMP_Text detailEffect;
    private TMP_Text detailPrice;
    private Button buyButton;

    private ItemData selected;

    private void Start()
    {
        // 해금된 아이템을 가격순으로 Item_1, Item_2 ... 버튼에 배치
        List<ItemData> items = ItemDatabase.All
            .Where(Inventory.IsUnlocked)
            .OrderBy(item => item.price)
            .ToList();

        int buttonCount = 0;

        while (true)
        {
            GameObject button = ItemUI.Find(gameObject.scene, $"Item_{buttonCount + 1}");

            if (button == null)
                break;

            SetupItemButton(button, buttonCount < items.Count ? items[buttonCount] : null);
            buttonCount++;
        }

        if (items.Count > buttonCount)
            Debug.LogWarning($"[아이템] 상점 버튼({buttonCount}개)보다 아이템이 많아서 일부가 안 보임");

        SetupGold();
        SetupDetail();

        Inventory.Changed += Refresh;
        ItemUISettings.Changed += ApplyCardLayout;
        Select(null);
    }

    private void OnDestroy()
    {
        Inventory.Changed -= Refresh;
        ItemUISettings.Changed -= ApplyCardLayout;
    }

    // 아이템 버튼: 예시 글자를 덮고 아이콘 / 이름 / 가격 표시
    private void SetupItemButton(GameObject buttonObj, ItemData item)
    {
        // 아이템이 모자라면 남는 버튼은 숨김
        if (item == null)
        {
            buttonObj.SetActive(false);
            return;
        }

        ItemUI.HideOriginalTexts(buttonObj);

        RectTransform rt = (RectTransform)buttonObj.transform;

        // 예시 글자('아이템 이미지', '아이템 이름/가격') 덮기
        // 덮개도 버튼 클릭 시 같이 어두워지게 버튼 색을 따라감
        Button button = buttonObj.GetComponent<Button>();

        foreach (Rect area in new[] { CardImageArea, CardTextArea })
        {
            RectTransform coverArea = ItemUI.CreateRegion(rt, "Cover", area);
            Image cover = ItemUI.CreateImage(coverArea, "Cover", ItemUI.Blue);
            ItemTintFollow.Add(cover, button.targetGraphic);
        }

        // 버튼 전체를 덮는 영역 (버튼 scale이 찌그러져 있어도 캔버스 픽셀 기준으로 배치되게)
        RectTransform content = ItemUI.CreateRegion(rt, "ItemContent", ItemUI.Area(0, 0, 1, 1));

        Card card = new Card { item = item };

        card.icon = ItemUI.CreateIcon(content);
        card.icon.sprite = item.icon;
        card.icon.enabled = item.icon != null;
        ItemTintFollow.Add(card.icon, button.targetGraphic);

        card.nameText = ItemUI.CreateText(content, "Name", 22);
        card.nameText.text = item.itemName;

        card.priceText = ItemUI.CreateText(content, "Price", 20);

        cards.Add(card);
        ApplyCardLayout(card);

        button.onClick.AddListener(() => Select(item));
    }

    private void ApplyCardLayout()
    {
        foreach (Card card in cards)
            ApplyCardLayout(card);
    }

    private void ApplyCardLayout(Card card)
    {
        ItemUISettings settings = ItemUISettings.Instance;

        ItemUISettings.Place(card.icon.rectTransform, settings.shopIconPosition, settings.shopIconSize);
        settings.shopName.Apply(card.nameText);
        settings.shopPrice.Apply(card.priceText);
        card.priceText.text = string.Format(settings.priceFormat, card.item.price);
    }

    // goods: 현재 골드 표시
    private void SetupGold()
    {
        GameObject goods = ItemUI.Find(gameObject.scene, "goods");

        if (goods == null)
            return;

        ItemUI.HideOriginalTexts(goods);

        RectTransform area = ItemUI.CreateRegion((RectTransform)goods.transform, "Gold", ItemUI.Area(0.05f, 0.05f, 0.95f, 0.95f));
        goldText = ItemUI.CreateText(area, "GoldText", 62);
    }

    // item_info: 이미지 / 이름 / 콘셉트 / 효과 / 가격 칸 채우기 + 구매 칸을 버튼으로 (기획서 8-1, 8-2)
    private void SetupDetail()
    {
        GameObject info = ItemUI.Find(gameObject.scene, "item_info");

        if (info == null)
        {
            Debug.LogWarning("[아이템] 상점에서 item_info를 못 찾음");
            return;
        }

        detailRoot = info;
        RectTransform rt = (RectTransform)info.transform;

        RectTransform imageArea = ItemUI.CreateRegion(rt, "ItemImage", InfoImageArea);
        ItemUI.CreateImage(imageArea, "Cover", ItemUI.Green);
        detailIcon = ItemUI.CreateIcon(imageArea);

        detailName = CreateInfoText(rt, "ItemName", InfoNameArea, 42);
        detailName.fontStyle = FontStyles.Bold;
        detailConcept = CreateInfoText(rt, "ItemConcept", InfoConceptArea, 30);
        detailEffect = CreateInfoText(rt, "ItemEffect", InfoEffectArea, 36);
        detailPrice = CreateInfoText(rt, "ItemPrice", InfoPriceArea, 46);

        // 그림의 '구매' 칸 위에 투명 버튼. 못 사면 회색으로 덮임
        RectTransform buyArea = ItemUI.CreateRegion(rt, "BuyButton", InfoBuyArea);
        Image buyImage = ItemUI.CreateImage(buyArea, "Hit", Color.white, true);
        ItemUI.Stretch(buyImage.rectTransform);

        buyButton = buyImage.gameObject.AddComponent<Button>();
        buyButton.targetGraphic = buyImage;

        ColorBlock colors = buyButton.colors;
        colors.normalColor = new Color(1, 1, 1, 0);
        colors.highlightedColor = new Color(0, 0, 0, 0.08f);
        colors.pressedColor = new Color(0, 0, 0, 0.18f);
        colors.selectedColor = new Color(1, 1, 1, 0);
        colors.disabledColor = new Color(0.35f, 0.35f, 0.35f, 0.6f);
        buyButton.colors = colors;

        buyButton.onClick.AddListener(Buy);
    }

    private TMP_Text CreateInfoText(RectTransform parent, string name, Rect area, float maxSize)
    {
        RectTransform region = ItemUI.CreateRegion(parent, name, area);
        ItemUI.CreateImage(region, "Cover", ItemUI.Green);
        return ItemUI.CreateText(region, "Text", maxSize);
    }

    private void Select(ItemData item)
    {
        selected = item;
        Refresh();
    }

    private void Buy()
    {
        EventSystem.current.SetSelectedGameObject(null);

        if (selected != null)
            Inventory.TryBuy(selected);
    }

    private void Refresh()
    {
        if (goldText != null)
            goldText.text = $"{Inventory.Gold} G";

        if (detailRoot == null)
            return;

        // 어떤 아이템을 골라도 상세 정보가 켜지게
        if (selected != null)
            detailRoot.SetActive(true);

        // 미선택이면 칸만 비워둠 (기획서 8-1)
        bool has = selected != null;

        detailIcon.enabled = has && selected.icon != null;
        detailIcon.sprite = has ? selected.icon : null;
        detailName.text = has ? selected.itemName : "";
        detailConcept.text = has ? selected.concept : "";
        detailEffect.text = has ? selected.effectText : "";
        detailPrice.text = has ? $"{selected.price} G   (보유 {Inventory.GetCount(selected)}개)" : "";

        // 골드가 모자라면 회색 + 클릭 안 됨 (기획서 8-2)
        buyButton.interactable = has && Inventory.CanBuy(selected);
    }
}
