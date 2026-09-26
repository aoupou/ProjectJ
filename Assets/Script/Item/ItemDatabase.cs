using System.Collections.Generic;
using UnityEngine;

// Resources/Items 폴더의 ItemData를 전부 불러와서 id로 찾아준다
public static class ItemDatabase
{
    private static Dictionary<string, ItemData> items;

    // 이 프로젝트는 플레이 시작 시 도메인 리로드가 꺼져 있어서(Enter Play Mode Options)
    // static 값이 이전 플레이에서 그대로 남는다. 그래서 플레이 시작마다 직접 초기화
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        items = null;
    }

    private static void Load()
    {
        if (items != null)
            return;

        items = new Dictionary<string, ItemData>();

        foreach (ItemData item in Resources.LoadAll<ItemData>("Items"))
        {
            if (string.IsNullOrEmpty(item.id) || items.ContainsKey(item.id))
            {
                Debug.LogError($"아이템 id가 비어있거나 중복됨: {item.name}");
                continue;
            }

            items.Add(item.id, item);
        }
    }

    public static ItemData Get(string id)
    {
        Load();

        if (string.IsNullOrEmpty(id))
            return null;

        items.TryGetValue(id, out ItemData item);
        return item;
    }

    public static IEnumerable<ItemData> All
    {
        get
        {
            Load();
            return items.Values;
        }
    }
}
