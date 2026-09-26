using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

// 대기화면의 보유 아이템 칸 하나 ('아이템 이미지' 스프라이트에 붙음)
// 드래그해서 장착 슬롯에 놓으면 장착 (기획서 7-2-1)
public class OwnedItemUI : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler,
    IPointerEnterHandler, IPointerExitHandler
{
    // 칸 그림 안쪽 ('아이템 이미지' 글자가 있는 곳)
    private static readonly Rect ImageArea = ItemUI.Area(0.12f, 0.18f, 0.87f, 0.82f);
    private static readonly Rect CountArea = ItemUI.Area(0.55f, 0.18f, 0.87f, 0.45f);

    private SpriteRenderer cellSprite;
    private SpriteRenderer icon;
    private TextMeshPro countText;

    public ItemData Item { get; private set; }

    private SpriteRenderer ghost; // 드래그 중 마우스를 따라다니는 아이콘

    public static OwnedItemUI Attach(SpriteRenderer cellSprite)
    {
        if (cellSprite.GetComponent<Collider2D>() == null)
            cellSprite.gameObject.AddComponent<BoxCollider2D>(); // 마우스 판정용

        OwnedItemUI cell = cellSprite.gameObject.AddComponent<OwnedItemUI>();
        cell.cellSprite = cellSprite;

        // '아이템 이미지' 임시 글자는 항상 가림 (빈 칸은 빈 칸으로 보이게)
        ItemUI.CreateWorldCover(cellSprite, ImageArea, ItemUI.Green);
        cell.icon = ItemUI.CreateWorldIcon(cellSprite, ImageArea);
        cell.countText = ItemUI.CreateWorldText(cellSprite, CountArea, 5);
        cell.countText.alignment = TextAlignmentOptions.BottomRight;

        return cell;
    }

    public void Set(ItemData item)
    {
        Item = item;

        ItemUI.SetWorldIcon(icon, item != null ? item.icon : null, cellSprite, ImageArea);

        int count = item != null ? Inventory.GetCount(item) : 0;
        countText.text = count > 1 ? $"x{count}" : "";
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (Item == null)
        {
            eventData.pointerDrag = null; // 빈 칸은 드래그 안 됨
            return;
        }

        ItemTooltip.Hide();

        ghost = Instantiate(icon);
        ghost.name = "DragGhost";
        ghost.transform.localScale = icon.transform.lossyScale;
        ghost.sortingOrder += 10;

        OnDrag(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (ghost == null || Camera.main == null)
            return;

        Vector3 world = Camera.main.ScreenToWorldPoint(eventData.position);
        world.z = icon.transform.position.z - 0.1f;
        Vector3 offset = ghost.sprite != null ? Vector3.Scale(ghost.sprite.bounds.center, ghost.transform.localScale) : Vector3.zero;
        ghost.transform.position = world - offset;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (ghost != null)
            Destroy(ghost.gameObject);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!eventData.dragging)
            ItemTooltip.Show(Item);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ItemTooltip.Hide();
    }
}
