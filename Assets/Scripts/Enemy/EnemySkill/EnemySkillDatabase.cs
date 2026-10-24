using System.Collections.Generic;
using UnityEngine;

// Resources/EnemySkills 폴더의 EnemySkillData를 전부 불러와서 key(에셋 파일 이름)로 찾아준다
// 사용: EnemySkillData data = EnemySkillDatabase.Get("key");
//       foreach (EnemySkillData data in EnemySkillDatabase.All) { ... }
//       foreach (EnemySkillData data in EnemySkillDatabase.OfOwner(enemyData)) { ... }
public static class EnemySkillDatabase
{
    private static Dictionary<string, EnemySkillData> all;

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

        all = new Dictionary<string, EnemySkillData>();

        foreach (EnemySkillData data in Resources.LoadAll<EnemySkillData>("EnemySkills"))
            all[data.name] = data;
    }

    public static EnemySkillData Get(string key)
    {
        Load();

        if (string.IsNullOrEmpty(key))
            return null;

        all.TryGetValue(key, out EnemySkillData data);
        return data;
    }

    public static IEnumerable<EnemySkillData> All
    {
        get
        {
            Load();
            return all.Values;
        }
    }

    // 이 적(owner)이 쓰는 스킬만 모아서 돌려준다
    public static List<EnemySkillData> OfOwner(CharacterData owner)
    {
        Load();

        List<EnemySkillData> result = new List<EnemySkillData>();

        foreach (EnemySkillData data in all.Values)
        {
            if (data.owner == owner)
                result.Add(data);
        }

        return result;
    }
}
