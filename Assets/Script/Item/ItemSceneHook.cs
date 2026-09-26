using UnityEngine;
using UnityEngine.SceneManagement;

// 씬이 열릴 때 팀원이 만든 UI를 찾아서 아이템 기능을 붙인다
// 씬 파일(.unity)은 건드리지 않으므로 팀원과 씬 충돌이 안 난다
//
//  상점 (shop)             → canvas에 ShopItemUI
//  대기화면 (stage_choice) → Canvas에 LobbyItemPanel
//  전투 (Battle이 있는 씬) → Item에 BattleItemBar
//
// 씬에 직접 컴포넌트를 붙여두면 그걸 쓰고 여기서는 건너뜀
public static class ItemSceneHook
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Init()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "shop")
            Attach<ShopItemUI>(scene, "canvas");
        else if (scene.name == "stage_choice")
            Attach<LobbyItemPanel>(scene, "Canvas");
        else if (Object.FindAnyObjectByType<Battle>() != null)
            Attach<BattleItemBar>(scene, "Item");
    }

    private static void Attach<T>(Scene scene, string objectName) where T : Component
    {
        if (Object.FindAnyObjectByType<T>() != null)
            return;

        GameObject target = ItemUI.Find(scene, objectName);

        if (target == null)
        {
            Debug.LogWarning($"[아이템] {scene.name} 씬에서 '{objectName}' 오브젝트를 못 찾아서 {typeof(T).Name}을 붙이지 못함");
            return;
        }

        target.AddComponent<T>();
    }
}
