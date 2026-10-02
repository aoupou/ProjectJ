using UnityEditor;
using UnityEngine;

// 애니메이션 컬렉션은 어느 폴더에서 만들든 항상 Assets/Animations/Collections 에 만든다
public static class AnimationCollectionCreator
{
    private const string Folder = "Assets/Animations/Collections";

    [MenuItem("Assets/Create/Animation/Animation Collection")]
    private static void Create()
    {
        if (!AssetDatabase.IsValidFolder(Folder))
            AssetDatabase.CreateFolder("Assets/Animations", "Collections");

        string path = AssetDatabase.GenerateUniqueAssetPath(Folder + "/NewAnimationCollection.asset");
        var collection = ScriptableObject.CreateInstance<SpriteAnimationCollection>();
        AssetDatabase.CreateAsset(collection, path);
        AssetDatabase.SaveAssets();

        Selection.activeObject = collection;
        EditorGUIUtility.PingObject(collection);
    }
}
