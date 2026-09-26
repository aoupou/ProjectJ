using UnityEditor;

// 상단 메뉴 ProjectJ에서 테스트용으로 아이템 / 골드 지급 (플레이 중에도 사용 가능)
public static class ItemDebugMenu
{
    [MenuItem("ProjectJ/아이템 테스트/골드 +1000")]
    private static void AddGold()
    {
        Inventory.AddGold(1000);
        Inventory.CommitStage();
    }

    [MenuItem("ProjectJ/아이템 테스트/모든 아이템 1개씩 구매")]
    private static void BuyAll()
    {
        foreach (ItemData item in ItemDatabase.All)
        {
            Inventory.AddGold(item.price);
            Inventory.Unlock(item);
            Inventory.TryBuy(item);
        }
    }

    private const string TestGoldMenu = "ProjectJ/아이템 테스트/플레이 시 골드 +100";

    [MenuItem(TestGoldMenu)]
    private static void ToggleTestGold()
    {
        TestGold.Enabled = !TestGold.Enabled;
    }

    [MenuItem(TestGoldMenu, true)]
    private static bool ToggleTestGoldValidate()
    {
        Menu.SetChecked(TestGoldMenu, TestGold.Enabled); // 켜져 있으면 체크 표시
        return true;
    }

    [MenuItem("ProjectJ/아이템 테스트/세이브 초기화")]
    private static void ResetSave()
    {
        Inventory.ResetSave();
    }
}
