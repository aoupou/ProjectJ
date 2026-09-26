#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

// 테스트용: 에디터에서 플레이할 때마다 골드 +100 (빌드에는 포함 안 됨)
// 끄기 / 켜기: 메뉴 ProjectJ → 아이템 테스트 → 플레이 시 골드 +100
public static class TestGold
{
    public const int Amount = 100;
    private const string PrefKey = "ProjectJ.TestGoldOnPlay";

    public static bool Enabled
    {
        get => EditorPrefs.GetBool(PrefKey, true);
        set => EditorPrefs.SetBool(PrefKey, value);
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void OnPlay()
    {
        if (!Enabled)
            return;

        Inventory.AddGold(Amount);
        Inventory.CommitStage(); // 바로 저장

        Debug.Log($"[테스트] 골드 +{Amount} (현재 {Inventory.Gold} G)");
    }
}
#endif
