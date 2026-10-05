using System.Collections.Generic;
using UnityEngine;

// Resources/Weapons 폴더의 WeaponData를 전부 불러와서 key(에셋 파일 이름)로 찾아준다
// 사용: WeaponData data = WeaponDatabase.Get("key");
//       foreach (WeaponData data in WeaponDatabase.All) { ... }
//       foreach (WeaponData data in WeaponDatabase.OfType(WeaponType.Magic)) { ... }
public static class WeaponDatabase
{
    private static Dictionary<string, WeaponData> all;

    // 도메인 리로드가 꺼져 있어서 플레이 시작마다 직접 초기화 (ItemDatabase와 같은 이유)
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        all = null;
    }

    private static void Load()
    {
        if (all != null)
            return;

        all = new Dictionary<string, WeaponData>();

        foreach (WeaponData data in Resources.LoadAll<WeaponData>("Weapons"))
            all[data.name] = data;
    }

    public static WeaponData Get(string key)
    {
        Load();

        if (string.IsNullOrEmpty(key))
            return null;

        all.TryGetValue(key, out WeaponData data);
        return data;
    }

    public static IEnumerable<WeaponData> All
    {
        get
        {
            Load();
            return all.Values;
        }
    }

    // 같은 종류(weaponType)인 무기만 모아서 돌려준다 (예: Magic → Magic_1, Magic_2)
    public static List<WeaponData> OfType(WeaponType type)
    {
        Load();

        List<WeaponData> result = new List<WeaponData>();

        foreach (WeaponData data in all.Values)
        {
            if (data.weaponType == type)
                result.Add(data);
        }

        return result;
    }
}
