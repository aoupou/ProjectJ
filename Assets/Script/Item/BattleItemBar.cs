using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// 전투 화면 아이템 사용 (기획서 10번). 팀원이 만든 'Item' 오브젝트에 붙는다
//  1/2/3 또는 클릭 → 선택 상태 (다른 아이템은 비활성)
//  같은 키/클릭 한 번 더 → 사용 (취소 불가)
//  선택 상태에서 ESC → 선택 취소
public class BattleItemBar : MonoBehaviour
{
    private readonly List<BattleItemSlotUI> slots = new List<BattleItemSlotUI>();

    // 이동 애니메이션 중 등 아이템을 쓰면 안 될 때 턴 시스템에서 false로 바꾸면 됨
    public bool Interactable = true;

    private readonly ItemData[] items = new ItemData[Inventory.SlotCount];
    private readonly bool[] used = new bool[Inventory.SlotCount];
    private int selected = -1;

    // 턴이 바뀌었는지 확인용 (Battle.currentTurn이 바뀌면 턴 종료)
    private Battle battle;
    private int lastTurn;

    private void Start()
    {
        ItemEffects.ResetBattle();

        battle = FindAnyObjectByType<Battle>();
        lastTurn = battle != null ? battle.currentTurn : 0;

        // 자식 중 Item_1, Item_2, Item_3 순서로 슬롯 연결
        for (int i = 0; i < Inventory.SlotCount; i++)
        {
            Transform slotObj = transform.Find($"Item_{i + 1}");

            if (slotObj == null)
            {
                Debug.LogWarning($"[아이템] 전투 화면에서 'Item_{i + 1}' 슬롯을 못 찾음");
                break;
            }

            slots.Add(BattleItemSlotUI.Create(slotObj.gameObject, i + 1));
        }

        for (int i = 0; i < slots.Count; i++)
        {
            items[i] = Inventory.GetEquipped(i);

            int index = i;
            slots[i].Init(items[i], () => Press(index));
        }

        Refresh();

        HpHealPreview.Create(gameObject.scene);
    }

    private void Update()
    {
        // 턴이 넘어가면 이번 턴 효과(이동 +N 등) 초기화
        // (이동이 끝날 때도, 공격이 끝날 때도 Battle.currentTurn이 올라감)
        if (battle != null && battle.currentTurn != lastTurn)
        {
            lastTurn = battle.currentTurn;
            ItemEffects.OnTurnEnd();
        }

        Keyboard keyboard = Keyboard.current;

        if (keyboard == null)
            return;

        if (keyboard.digit1Key.wasPressedThisFrame) Press(0);
        if (keyboard.digit2Key.wasPressedThisFrame) Press(1);
        if (keyboard.digit3Key.wasPressedThisFrame) Press(2);

        if (keyboard.escapeKey.wasPressedThisFrame) Cancel();
    }

    public void Press(int index)
    {
        if (!Interactable || index >= slots.Count)
            return;

        if (items[index] == null || used[index])
            return;

        if (selected == -1)
        {
            selected = index;
            ItemEffects.StartPreview(items[index]);
            Refresh();
        }
        else if (selected == index)
        {
            Use(index);
        }
        // 다른 아이템이 선택된 상태면 무시
    }

    public void Cancel()
    {
        if (selected == -1)
            return;

        selected = -1;
        ItemEffects.EndPreview();
        Refresh();
    }

    private void Use(int index)
    {
        ItemData item = items[index];

        used[index] = true;
        selected = -1;

        ItemEffects.EndPreview();
        Inventory.Consume(item);
        ItemEffects.Apply(item);

        Refresh();
    }

    private void Refresh()
    {
        for (int i = 0; i < slots.Count; i++)
        {
            ItemSlotState state;

            if (items[i] == null)
                state = ItemSlotState.Empty;
            else if (used[i])
                state = ItemSlotState.Used;
            else if (selected == -1)
                state = ItemSlotState.Normal;
            else if (selected == i)
                state = ItemSlotState.Selected;
            else
                state = ItemSlotState.Disabled;

            slots[i].SetState(state);
        }
    }
}
