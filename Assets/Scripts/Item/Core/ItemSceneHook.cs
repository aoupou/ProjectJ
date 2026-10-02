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
//
// 스테이지 저장 규칙 (기획서 15번)
//  전투 씬 → 클리어 화면(stg_CLR)으로 가면 저장 (CommitStage)
//  전투 씬 → 그 외(게임오버, 타이틀 등)로 가면 스테이지 안에서 쓴 아이템 / 골드 되돌림 (RevertStage)
public static class ItemSceneHook
{
    private const string ClearSceneName = "stg_CLR";

    private static bool inStage; // 방금까지 전투 씬이었는지

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        inStage = false;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Init()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        bool isBattle = HasBattle(scene);

        // 전투가 끝나고 다른 씬으로 넘어온 경우
        if (inStage && !isBattle)
        {
            if (scene.name == ClearSceneName)
                Inventory.CommitStage();
            else
                Inventory.RevertStage();
        }

        inStage = isBattle;

        if (scene.name == "shop")
            Attach<ShopItemUI>(scene, "canvas");
        else if (scene.name == "stage_choice")
            Attach<LobbyItemPanel>(scene, "Canvas");
        else if (isBattle)
            Attach<BattleItemBar>(scene, "Item");
    }

    private static bool HasBattle(Scene scene)
    {
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            if (root.GetComponentInChildren<Battle>(true) != null)
                return true;
        }

        return false;
    }

    private static void Attach<T>(Scene scene, string objectName) where T : Component
    {
        GameObject target = ItemUI.Find(scene, objectName);

        if (target == null)
        {
            Debug.LogWarning($"[아이템] {scene.name} 씬에서 '{objectName}' 오브젝트를 못 찾아서 {typeof(T).Name}을 붙이지 못함");
            return;
        }

        if (target.GetComponent<T>() == null)
            target.AddComponent<T>();
    }
}
