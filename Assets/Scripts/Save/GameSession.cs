using UnityEngine;

public static class GameSession 
{
    private static BeadWallet wallet;

    // 씬을 이동해도 같은 실행 세션에서는 잔액을 계속 공유
    private static BeadWallet Wallet =>
        wallet ??= new BeadWallet(SaveManager.Load().beads);

    public static int Beads => Wallet.Balance;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetSession()
    {
        // 새 실행 새션에서 처음 접근할 때 저장 파일을 다시 읽음
        wallet = null;
    }
    public static void AddBeads(int amount)
    {
        var updatedWalle = new BeadWallet(Beads);
        updatedWalle.Add(amount);
        SaveWallet(updatedWalle);
    }
    public static bool TrySpendBeads(int amount)
    {
        var updatedWallet = new BeadWallet(Beads);

        if (!updatedWallet.TrySpend(amount))
            return false;

        if (amount > 0)
            SaveWallet(updatedWallet);

        return true;
    }
    public static void SaveWallet(BeadWallet updatedWallet)
    {
        // 저장에 성공한 뒤 메모리의 잔액을 바꿔요.
        SaveManager.Save(new SaveData { beads = updatedWallet.Balance });
        wallet = updatedWallet;
    }
}
