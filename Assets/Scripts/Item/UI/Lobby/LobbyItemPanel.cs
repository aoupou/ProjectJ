using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

// 대기화면 아이템 영역 (기획서 7-2)
// 팀원이 배치한 '아이템 슬롯 1~3' 스프라이트에 장착 기능을,
// '아이템 이미지' 스프라이트들에 보유 아이템 표시 / 드래그 기능을 붙인다
public class LobbyItemPanel : MonoBehaviour
{
    private readonly List<EquipSlotUI> equipSlots = new List<EquipSlotUI>();
    private readonly List<OwnedItemUI> ownedCells = new List<OwnedItemUI>();

    private void Start()
    {
        // 스프라이트(월드 오브젝트)도 마우스 이벤트를 받을 수 있게 카메라에 레이캐스터 추가
        Camera cam = Camera.main;

        if (cam != null && cam.GetComponent<Physics2DRaycaster>() == null)
            cam.gameObject.AddComponent<Physics2DRaycaster>();

        for (int i = 0; i < Inventory.SlotCount; i++)
        {
            SpriteRenderer slot = FindSprite($"아이템 슬롯 {i + 1}_0");

            if (slot == null)
            {
                Debug.LogWarning($"[아이템] 대기화면에서 '아이템 슬롯 {i + 1}_0'을 못 찾음");
                continue;
            }

            equipSlots.Add(EquipSlotUI.Attach(slot, i));
        }

        // 보유 아이템 칸: 왼쪽 위부터 오른쪽으로, 줄 단위 순서
        List<SpriteRenderer> cells = gameObject.scene.GetRootGameObjects()
            .Where(obj => obj.name.StartsWith("아이템 이미지"))
            .Select(obj => obj.GetComponent<SpriteRenderer>())
            .Where(sr => sr != null && sr.sprite != null)
            .OrderByDescending(sr => Mathf.Round(sr.bounds.center.y * 2))
            .ThenBy(sr => sr.bounds.center.x)
            .ToList();

        foreach (SpriteRenderer cell in cells)
            ownedCells.Add(OwnedItemUI.Attach(cell));

        ItemTooltip.Create(GetComponentInParent<Canvas>());

        Inventory.Changed += Refresh;
        Refresh();
    }

    private void OnDestroy()
    {
        Inventory.Changed -= Refresh;
    }

    private SpriteRenderer FindSprite(string name)
    {
        GameObject obj = ItemUI.Find(gameObject.scene, name);
        return obj != null ? obj.GetComponent<SpriteRenderer>() : null;
    }

    private void Refresh()
    {
        foreach (EquipSlotUI slot in equipSlots)
            slot.Refresh();

        List<ItemData> owned = Inventory.GetOwnedItems();

        for (int i = 0; i < ownedCells.Count; i++)
            ownedCells[i].Set(i < owned.Count ? owned[i] : null);

        if (owned.Count > ownedCells.Count)
            Debug.LogWarning($"[아이템] 보유 아이템({owned.Count}개)이 칸({ownedCells.Count}개)보다 많아서 일부가 안 보임");
    }
}
