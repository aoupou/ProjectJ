using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public enum ItemSlotState
{
    Empty,
    Normal,
    Selected,
    Disabled, // 다른 아이템이 선택돼서 비활성 (기획서 10-2-3)
    Used,
}

// 전투 화면 아이템 슬롯 하나 (팀원이 만든 Item_1~3 이미지에 붙음)
// 슬롯 이미지를 장착한 아이템 아이콘으로 바꾸고, 클릭하면 선택 / 사용
public class BattleItemSlotUI : MonoBehaviour
{
    private static readonly Color SelectedColor = new Color(1f, 0.85f, 0.3f);
    private const float DimAlpha = 0.35f;

    private Image image;
    private Button button;
    private CanvasGroup canvasGroup;
    private TMP_Text nameText;

    public static BattleItemSlotUI Create(GameObject slotObj, int keyNumber)
    {
        BattleItemSlotUI slot = slotObj.AddComponent<BattleItemSlotUI>();

        slot.image = slotObj.GetComponent<Image>();
        slot.image.preserveAspect = true;

        slot.button = slotObj.GetComponent<Button>();
        if (slot.button == null)
            slot.button = slotObj.AddComponent<Button>();

        slot.button.targetGraphic = slot.image;
        slot.button.transition = Selectable.Transition.None; // 색은 SetState에서 직접 바꿈

        slot.canvasGroup = slotObj.AddComponent<CanvasGroup>();

        // 이름은 슬롯 오른쪽에 표시 (기획서 10-1)
        RectTransform rt = (RectTransform)slotObj.transform;
        RectTransform nameArea = ItemUI.CreateRegion(rt, "ItemName", ItemUI.Area(1.05f, 0.2f, 2.6f, 0.8f));
        slot.nameText = ItemUI.CreateText(nameArea, "Text", 38, TextAlignmentOptions.Left);

        RectTransform keyArea = ItemUI.CreateRegion(rt, "Key", ItemUI.Area(-0.3f, 0.65f, 0f, 1f));
        TMP_Text keyText = ItemUI.CreateText(keyArea, "Text", 30);
        keyText.text = keyNumber.ToString();

        return slot;
    }

    public void Init(ItemData item, Action onClick)
    {
        // 장착한 아이템이 있으면 슬롯 이미지를 아이템 아이콘으로 교체
        if (item != null && item.icon != null)
            image.sprite = item.icon;

        nameText.text = item != null ? item.itemName : "";

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() =>
        {
            // 클릭 후 버튼이 선택된 채로 남으면 스페이스바를 눌렀을 때 또 클릭되므로 해제
            EventSystem.current.SetSelectedGameObject(null);
            onClick();
        });
    }

    public void SetState(ItemSlotState state)
    {
        image.color = state == ItemSlotState.Selected ? SelectedColor : Color.white;

        button.interactable =
            state == ItemSlotState.Normal || state == ItemSlotState.Selected;

        // 빈 슬롯 / 다 쓴 슬롯 / 비활성 슬롯은 흐리게
        bool dim = state != ItemSlotState.Normal && state != ItemSlotState.Selected;
        canvasGroup.alpha = dim ? DimAlpha : 1f;
    }
}
