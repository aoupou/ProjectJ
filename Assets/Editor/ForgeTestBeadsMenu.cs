using System.IO;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class ForgeTestBeadsMenu
{
    private const int TestBalance = 999;
    private const string PendingGrant = "Library/ForgeTestBeads999.pending";
    private const string MenuPath = "ProjectJ/Forge Test/Set Beads to 999";

    static ForgeTestBeadsMenu()
    {
        EditorApplication.playModeStateChanged += ApplyPendingGrant;
    }

    private static void ApplyPendingGrant(PlayModeStateChange state)
    {
        if (state != PlayModeStateChange.EnteredPlayMode || !File.Exists(PendingGrant))
            return;

        SetBeads();
        File.Delete(PendingGrant);
    }

    [MenuItem(MenuPath, true)]
    private static bool CanSetBeads() => EditorApplication.isPlaying;

    [MenuItem(MenuPath)]
    private static void SetBeads()
    {
        GameSession.SaveWallet(new BeadWallet(TestBalance));
        Debug.Log($"테스트 비즈 잔액: {GameSession.Beads}");
    }
}
