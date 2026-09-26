using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

// 대기화면의 아이템 장착 슬롯 ('아이템 슬롯 1~3' 스프라이트에 붙음)
// 보유 아이템을 드롭하면 장착, 우클릭하면 해제
public class EquipSlotUI : MonoBehaviour,
    IDropHandler, IPointerClickHandler,
    IPointerEnterHandler, IPointerExitHandler
{
    // 슬롯 그림 안의 칸 위치: 위쪽 + 표시 칸, 아래쪽 '아이템 슬롯 N' 글자 칸
    private static readonly Rect ImageArea = ItemUI.Area(0.1f, 0.26f, 0.9f, 0.9f);
    private static readonly Rect LabelArea = ItemUI.Area(0.1f, 0.05f, 0.9f, 0.19f);

    private int slotIndex;
    private SpriteRenderer slotSprite;
    private SpriteRenderer imageCover;
    private SpriteRenderer labelCover;
    private SpriteRenderer icon;
    private TextMeshPro nameText;

    public static EquipSlotUI Attach(SpriteRenderer slotSprite, int slotIndex)
    {
        if (slotSprite.GetComponent<Collider2D>() == null)
            slotSprite.gameObject.AddComponent<BoxCollider2D>(); // 마우스 판정용

        EquipSlotUI slot = slotSprite.gameObject.AddComponent<EquipSlotUI>();
        slot.slotIndex = slotIndex;
        slot.slotSprite = slotSprite;

        slot.imageCover = ItemUI.CreateWorldCover(slotSprite, ImageArea, ItemUI.Green);
        slot.icon = ItemUI.CreateWorldIcon(slotSprite, ImageArea);
        slot.labelCover = ItemUI.CreateWorldCover(slotSprite, LabelArea, ItemUI.Green);
        slot.nameText = ItemUI.CreateWorldText(slotSprite, LabelArea, 6);

        return slot;
    }

    // 비어있으면 원래 그림(+ 표시, '아이템 슬롯 N')을 그대로 보여줌
    public void Refresh()
    {
        ItemData item = Inventory.GetEquipped(slotIndex);
        bool has = item != null;

        imageCover.enabled = has;
        labelCover.enabled = has;
        nameText.text = has ? item.itemName : "";
        ItemUI.SetWorldIcon(icon, has ? item.icon : null, slotSprite, ImageArea);
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag == null)
            return;

        OwnedItemUI owned = eventData.pointerDrag.GetComponent<OwnedItemUI>();

        if (owned != null && owned.Item != null)
            Inventory.Equip(slotIndex, owned.Item);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            ItemTooltip.Hide();
            Inventory.Unequip(slotIndex);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!eventData.dragging)
            ItemTooltip.Show(Inventory.GetEquipped(slotIndex));
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ItemTooltip.Hide();
    }
}
