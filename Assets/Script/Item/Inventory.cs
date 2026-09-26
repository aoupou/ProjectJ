using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[Serializable]
public class OwnedItem
{
    public string id;
    public int count;
}

[Serializable]
public class InventorySaveData
{
    public int gold;
    public List<OwnedItem> owned = new List<OwnedItem>();
    public List<string> unlocked = new List<string>();
    public string[] equipped = new string[Inventory.SlotCount];
}

// 골드 / 보유 아이템 / 장착 아이템 / 해금 아이템 관리 (씬이 바뀌어도 유지됨)
//
// 저장 규칙 (기획서 15번)
//  - 대기화면 / 상점에서 바뀐 내용(구매, 장착)은 바로 저장
//  - 스테이지 안에서 바뀐 내용(아이템 사용, 골드 획득, 해금)은
//    클리어하면 CommitStage(), 실패하면 RevertStage()로 되돌림
public static class Inventory
{
    public const int SlotCount = 3;

    // 인벤토리 내용이 바뀌면 호출됨 (UI 갱신용)
    public static event Action Changed;

    private static InventorySaveData data;

    // 이 프로젝트는 플레이 시작 시 도메인 리로드가 꺼져 있어서(Enter Play Mode Options)
    // static 값이 이전 플레이에서 그대로 남는다. 그래서 플레이 시작마다 직접 초기화
    // (안 하면 전투에서 쓰고 저장 안 한 아이템이 다음 플레이에도 남음)
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        data = null;
        Changed = null;
    }

    private static string SavePath =>
        Path.Combine(Application.persistentDataPath, "inventory.json");

    private static InventorySaveData Data
    {
        get
        {
            if (data == null)
                Load();

            return data;
        }
    }

    public static int Gold => Data.gold;

    public static int GetCount(ItemData item)
    {
        OwnedItem owned = Find(item.id);
        return owned == null ? 0 : owned.count;
    }

    public static bool IsUnlocked(ItemData item)
    {
        return item.unlockedAtStart || Data.unlocked.Contains(item.id);
    }

    public static ItemData GetEquipped(int slot)
    {
        return ItemDatabase.Get(Data.equipped[slot]);
    }

    public static List<ItemData> GetOwnedItems()
    {
        List<ItemData> result = new List<ItemData>();

        foreach (OwnedItem owned in Data.owned)
        {
            ItemData item = ItemDatabase.Get(owned.id);

            if (item != null)
                result.Add(item);
        }

        return result;
    }

    // ---------- 대기화면 / 상점 (바로 저장) ----------

    public static bool CanBuy(ItemData item)
    {
        return IsUnlocked(item) && Data.gold >= item.price;
    }

    public static bool TryBuy(ItemData item)
    {
        if (!CanBuy(item))
            return false;

        Data.gold -= item.price;
        AddItem(item, 1);

        Save();
        Notify();
        return true;
    }

    // 같은 아이템은 보유 개수만큼만 여러 슬롯에 장착 가능
    public static bool Equip(int slot, ItemData item)
    {
        int equippedElsewhere = 0;

        for (int i = 0; i < SlotCount; i++)
        {
            if (i != slot && Data.equipped[i] == item.id)
                equippedElsewhere++;
        }

        if (GetCount(item) <= equippedElsewhere)
            return false;

        Data.equipped[slot] = item.id;

        Save();
        Notify();
        return true;
    }

    public static void Unequip(int slot)
    {
        Data.equipped[slot] = "";

        Save();
        Notify();
    }

    // ---------- 스테이지 안 (클리어 전까지 저장 안 함) ----------

    public static void Consume(ItemData item)
    {
        OwnedItem owned = Find(item.id);

        if (owned == null)
            return;

        owned.count--;

        if (owned.count <= 0)
            Data.owned.Remove(owned);

        RemoveExtraEquipped(item);
        Notify();
    }

    public static void AddGold(int amount)
    {
        Data.gold += amount;
        Notify();
    }

    // 스테이지 첫 클리어 보상 해금 (기획서 13-2-3). 이미 해금돼 있으면 false
    public static bool Unlock(ItemData item)
    {
        if (IsUnlocked(item))
            return false;

        Data.unlocked.Add(item.id);
        Notify();
        return true;
    }

    public static void CommitStage()
    {
        Save();
    }

    public static void RevertStage()
    {
        Load();
        Notify();
    }

    // ---------- 내부 ----------

    private static OwnedItem Find(string id)
    {
        return Data.owned.Find(owned => owned.id == id);
    }

    private static void AddItem(ItemData item, int amount)
    {
        OwnedItem owned = Find(item.id);

        if (owned == null)
        {
            owned = new OwnedItem { id = item.id };
            Data.owned.Add(owned);
        }

        owned.count += amount;
    }

    // 다 써서 보유 개수보다 많이 장착된 슬롯은 비운다
    private static void RemoveExtraEquipped(ItemData item)
    {
        int remaining = GetCount(item);

        for (int i = 0; i < SlotCount; i++)
        {
            if (Data.equipped[i] != item.id)
                continue;

            if (remaining > 0)
                remaining--;
            else
                Data.equipped[i] = "";
        }
    }

    private static void Save()
    {
        File.WriteAllText(SavePath, JsonUtility.ToJson(Data, true));
    }

    private static void Load()
    {
        data = File.Exists(SavePath)
            ? JsonUtility.FromJson<InventorySaveData>(File.ReadAllText(SavePath))
            : new InventorySaveData();

        if (data.equipped == null || data.equipped.Length != SlotCount)
            Array.Resize(ref data.equipped, SlotCount);
    }

    // 세이브 초기화 (테스트용)
    public static void ResetSave()
    {
        if (File.Exists(SavePath))
            File.Delete(SavePath);

        Load();
        Notify();
    }

    private static void Notify()
    {
        Changed?.Invoke();
    }
}
