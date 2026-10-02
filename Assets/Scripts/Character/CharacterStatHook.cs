using UnityEngine;
using UnityEngine.SceneManagement;

// 전투 씬이 열리면 CharacterData(구글 시트 스탯)를 플레이어 / 적에게 넣어준다
// 씬 파일(.unity)은 건드리지 않음 (ItemSceneHook과 같은 방식)
//
//  Player가 붙은 character_HP → "player" 스탯 (HP, 공격력)
//  그 외 character_HP         → "enemy" 스탯 (HP)
//
// sceneLoaded는 Awake 다음, Start 전에 불리므로 하트 UI가 처음부터 시트 HP로 그려진다
// 시트에 해당 key가 없으면 씬에 적힌 값을 그대로 쓴다
public static class CharacterStatHook
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Init()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Battle battle = FindInScene<Battle>(scene);

        // 타이틀 / 상점 등에도 character_HP가 있지만 전투 씬에만 적용
        if (battle == null)
            return;

        CharacterData player = CharacterDatabase.Get(CharacterDatabase.PlayerKey);
        CharacterData enemy = CharacterDatabase.Get(CharacterDatabase.EnemyKey);

        if (player != null)
            battle.SetAttackDamage(player.attack);

        foreach (GameObject root in scene.GetRootGameObjects())
        {
            foreach (character_HP hp in root.GetComponentsInChildren<character_HP>(true))
            {
                CharacterData data = hp.GetComponent<Player>() != null ? player : enemy;

                if (data != null)
                    hp.SetHP(data.hp);
            }
        }
    }

    private static T FindInScene<T>(Scene scene) where T : Component
    {
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            T found = root.GetComponentInChildren<T>(true);

            if (found != null)
                return found;
        }

        return null;
    }
}
