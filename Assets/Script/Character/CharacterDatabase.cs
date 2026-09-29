using System.Collections.Generic;
using UnityEngine;

// Resources/Characters 폴더의 CharacterData를 전부 불러와서 key(에셋 이름)로 찾아준다
public static class CharacterDatabase
{
    public const string PlayerKey = "player";
    public const string EnemyKey = "enemy";

    private static Dictionary<string, CharacterData> characters;

    // 도메인 리로드가 꺼져 있어서 플레이 시작마다 직접 초기화 (ItemDatabase와 같은 이유)
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        characters = null;
    }

    private static void Load()
    {
        if (characters != null)
            return;

        characters = new Dictionary<string, CharacterData>();

        foreach (CharacterData data in Resources.LoadAll<CharacterData>("Characters"))
            characters[data.name] = data;
    }

    public static CharacterData Get(string key)
    {
        Load();

        if (string.IsNullOrEmpty(key))
            return null;

        characters.TryGetValue(key, out CharacterData data);
        return data;
    }
}
