using System.Collections.Generic;
using UnityEditor;

// ItemData의 id를 에셋 GUID로 자동 설정한다
// 새로 만들거나 복제(Ctrl+D)해도 알아서 고유 id가 들어감
public class ItemDataIdFixer : AssetPostprocessor
{
    [InitializeOnLoadMethod]
    private static void OnEditorLoad()
    {
        EditorApplication.delayCall += () =>
        {
            foreach (string guid in AssetDatabase.FindAssets("t:ItemData"))
                Fix(AssetDatabase.GUIDToAssetPath(guid));
        };
    }

    private static void OnPostprocessAllAssets(
        string[] imported, string[] deleted, string[] moved, string[] movedFrom)
    {
        List<string> paths = new List<string>();

        foreach (string path in imported)
        {
            if (path.EndsWith(".asset"))
                paths.Add(path);
        }

        if (paths.Count == 0)
            return;

        // 임포트 도중에 에셋을 저장하면 안 되므로 끝난 뒤에 처리
        EditorApplication.delayCall += () =>
        {
            foreach (string path in paths)
                Fix(path);
        };
    }

    private static void Fix(string path)
    {
        ItemData item = AssetDatabase.LoadAssetAtPath<ItemData>(path);

        if (item == null)
            return;

        string guid = AssetDatabase.AssetPathToGUID(path);

        if (item.id == guid)
            return;

        item.id = guid;
        EditorUtility.SetDirty(item);
        AssetDatabase.SaveAssetIfDirty(item);
    }
}
