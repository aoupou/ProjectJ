using System.Collections.Generic;
using UnityEngine;

// Resources/Items 폴더의 ItemData를 전부 불러와서 key(에셋 파일 이름)로 찾아준다
// 다른 스크립트는 ItemDatabase.Get("Bandage"), ItemDatabase.All 처럼 바로 쓰면 됨 (씬에 붙일 필요 없음)
public static class ItemDatabase
{
    private static Dictionary<string, ItemData> items;
    private static List<ItemData> itemList;

    // 도메인 리로드가 꺼져 있어서 플레이 시작마다 직접 초기화 (CharacterDatabase와 같은 이유)
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        items = null;
        itemList = null;
    }

    private static void Load()
    {
        if (items != null)
            return;

        items = new Dictionary<string, ItemData>();
        itemList = new List<ItemData>();

        foreach (ItemData data in Resources.LoadAll<ItemData>("Items"))
        {
            items[data.name] = data;
            itemList.Add(data);
        }
    }

    // key(에셋 파일 이름)로 아이템 하나 찾기. 없으면 null
    public static ItemData Get(string key)
    {
        Load();

        if (string.IsNullOrEmpty(key))
            return null;

        items.TryGetValue(key, out ItemData data);
        return data;
    }

    // 전체 아이템 목록 (상점, 정보창에서 씀)
    public static IReadOnlyList<ItemData> All
    {
        get
        {
            Load();
            return itemList;
        }
    }

    // clearedStage까지 클리어했을 때 상점에 나오는 아이템만
    public static List<ItemData> Unlocked(int clearedStage)
    {
        Load();

        List<ItemData> result = new List<ItemData>();

        foreach (ItemData data in itemList)
        {
            if (data.unlockStage <= clearedStage)
                result.Add(data);
        }

        return result;
    }
}
