using UnityEditor;

// Assets/Resources/ItemUISettings 에셋이 없으면 기본값으로 만들어준다
public static class ItemUISettingsCreator
{
    private const string AssetPath = "Assets/Resources/" + ItemUISettings.ResourcePath + ".asset";

    [InitializeOnLoadMethod]
    private static void OnEditorLoad()
    {
        EditorApplication.delayCall += () =>
        {
            if (AssetDatabase.LoadAssetAtPath<ItemUISettings>(AssetPath) != null)
                return;

            AssetDatabase.CreateAsset(UnityEngine.ScriptableObject.CreateInstance<ItemUISettings>(), AssetPath);
            AssetDatabase.SaveAssets();
        };
    }

    [MenuItem("ProjectJ/아이템 UI 설정 열기")]
    private static void Open()
    {
        Selection.activeObject = AssetDatabase.LoadAssetAtPath<ItemUISettings>(AssetPath);
    }
}
