using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

// 데이터 하나를 새로 만드는 창. 상단 메뉴 ProjectJ → 데이터베이스 → 새 데이터 만들기
// 종류를 고르고 key(파일 이름)를 적으면 맞는 Resources 폴더에 만들어준다 → 만드는 순간 데이터베이스에 들어감
// 목록에 나오는 종류 = [GameDatabase]가 붙은 데이터 (아이템, 캐릭터, 새 데이터베이스 만들기로 만든 것)
public class DataCreatorWindow : EditorWindow
{
    private Type[] types = new Type[0];
    private string[] labels = new string[0];
    private string selectedType; // 목록이 바뀌어도 고른 종류 유지
    private string key = "";
    private List<string> existing = new List<string>();
    private Vector2 scroll;

    [MenuItem("ProjectJ/데이터베이스/새 데이터 만들기", priority = 0)]
    private static void Open()
    {
        GetWindow<DataCreatorWindow>("새 데이터");
    }

    private void OnEnable()
    {
        Refresh();
    }

    // 새 데이터베이스를 만들거나 에셋을 지웠을 때 목록 갱신
    private void OnProjectChange()
    {
        Refresh();
        Repaint();
    }

    private void Refresh()
    {
        types = TypeCache.GetTypesWithAttribute<GameDatabaseAttribute>()
            .Where(t => typeof(ScriptableObject).IsAssignableFrom(t) && !t.IsAbstract)
            .OrderBy(t => Info(t).DisplayName)
            .ToArray();
        labels = types.Select(t => Info(t).DisplayName).ToArray();

        if (types.Length > 0 && Array.FindIndex(types, t => t.Name == selectedType) < 0)
            selectedType = types[0].Name;

        RefreshExisting();
    }

    private void RefreshExisting()
    {
        existing.Clear();
        Type type = SelectedType;

        if (type == null || !AssetDatabase.IsValidFolder(Folder(type)))
            return;

        foreach (string guid in AssetDatabase.FindAssets("t:" + type.Name, new[] { Folder(type) }))
            existing.Add(AssetDatabase.GUIDToAssetPath(guid));

        existing.Sort(StringComparer.Ordinal);
    }

    private Type SelectedType => types.FirstOrDefault(t => t.Name == selectedType);

    private static GameDatabaseAttribute Info(Type type)
    {
        return (GameDatabaseAttribute)Attribute.GetCustomAttribute(type, typeof(GameDatabaseAttribute));
    }

    private static string Folder(Type type)
    {
        return "Assets/Resources/" + Info(type).Folder;
    }

    private void OnGUI()
    {
        if (types.Length == 0)
        {
            EditorGUILayout.HelpBox("데이터 종류가 없어요. 먼저 데이터베이스를 만들어주세요.", MessageType.Info);

            if (GUILayout.Button("새 데이터베이스 만들기..."))
                DatabaseCreatorWindow.Open();

            return;
        }

        int index = Array.FindIndex(types, t => t.Name == selectedType);
        int newIndex = EditorGUILayout.Popup("종류", index, labels);

        if (newIndex != index)
        {
            selectedType = types[newIndex].Name;
            RefreshExisting();
        }

        Type type = types[newIndex];
        string folder = Folder(type);

        key = EditorGUILayout.TextField(
            new GUIContent("key (파일 이름)", "영문 추천. 게임에서 이 이름으로 찾음 (예: sword)"), key).Trim();

        string error = KeyError(folder);

        if (error != null && key.Length > 0)
            EditorGUILayout.HelpBox(error, MessageType.Warning);

        using (new EditorGUI.DisabledScope(error != null))
        {
            if (GUILayout.Button("만들기", GUILayout.Height(28)))
                Create(type, folder);
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField($"{labels[newIndex]} 목록  ({folder})", EditorStyles.boldLabel);

        scroll = EditorGUILayout.BeginScrollView(scroll);

        if (existing.Count == 0)
            EditorGUILayout.LabelField("  아직 없음", EditorStyles.miniLabel);

        foreach (string path in existing)
        {
            if (GUILayout.Button("  " + Path.GetFileNameWithoutExtension(path), EditorStyles.label))
            {
                Selection.activeObject = AssetDatabase.LoadMainAssetAtPath(path);
                EditorGUIUtility.PingObject(Selection.activeObject);
            }
        }

        EditorGUILayout.EndScrollView();

        EditorGUILayout.Space();

        if (GUILayout.Button("데이터베이스 종류 새로 만들기..."))
            DatabaseCreatorWindow.Open();
    }

    private string KeyError(string folder)
    {
        if (key.Length == 0)
            return "key를 적어주세요";

        if (key.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0 || key.StartsWith("#"))
            return "key에 파일 이름에 못 쓰는 문자가 있어요";

        if (File.Exists($"{folder}/{key}.asset"))
            return $"'{key}'은(는) 이미 있어요";

        return null;
    }

    private void Create(Type type, string folder)
    {
        if (!AssetDatabase.IsValidFolder(folder))
            AssetDatabase.CreateFolder("Assets/Resources", Info(type).Folder);

        ScriptableObject data = CreateInstance(type);
        AssetDatabase.CreateAsset(data, $"{folder}/{key}.asset");
        AssetDatabase.SaveAssets();

        // 바로 Inspector에서 값을 채울 수 있게 선택
        Selection.activeObject = data;
        EditorGUIUtility.PingObject(data);

        key = "";
        GUI.FocusControl(null);
        RefreshExisting();
    }
}
