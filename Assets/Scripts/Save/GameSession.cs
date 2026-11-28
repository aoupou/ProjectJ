using UnityEngine;
using System;

public static class GameSession 
{
    private const int UpgradeTypeCount = (int)UpgradeType.MoveRange + 1;
    private static SaveData data;
    private static SaveData Data => data ??= SaveManager.Load();
    public static int Beads => Data.beads;
    public static event Action<int> BeadsChanged;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetSession()
    {
        // 새 실행 새션에서 처음 접근할 때 저장 파일을 다시 읽음
        data = null;
        BeadsChanged = null;
    }
    public static int GetUpgradeLevel(UpgradeType type)
    {
        int index = (int)type;
        int[] levels = Data.upgradeLevels;


        if (levels == null || index < 0 || index >= levels.Length)
            return 0;

        return Math.Max(0, levels[index]);
    }
    public static void AddBeads(int amount)
    {
        var wallet = new BeadWallet(Beads);
        wallet.Add(amount);
        SaveWallet(wallet);
    }
    public static bool TrySpendBeads(int amount)
    {
        var wallet = new BeadWallet(Beads);
        if (!wallet.TrySpend(amount))
            return false;

        if (amount > 0)
            SaveWallet(wallet);

        return true;
    }
    public static void SaveWallet(BeadWallet updatedWallet)
    {
        SaveAndApply(new SaveData
        {
            beads = updatedWallet.Balance,
            upgradeLevels = Data.upgradeLevels
        });
    }
    public static void SaveUpgradeResult(
        UpgradeType type, int remainingBeads, int level)
    {
        int[] previousLevels = Data.upgradeLevels;
        var updatedLevels = new int[Math.Max(
            UpgradeTypeCount, previousLevels?.Length ?? 0)];

        if (previousLevels != null)
            Array.Copy(previousLevels, updatedLevels, previousLevels.Length);

        updatedLevels[(int)type] = level;

        SaveAndApply(new SaveData
        {
            beads = remainingBeads,
            upgradeLevels = updatedLevels
        });
    }
    private static void SaveAndApply(SaveData updatedData)
    {
        SaveManager.Save(updatedData);
        data = updatedData;
        BeadsChanged?.Invoke(updatedData.beads);
    }
}
