using TMPro;
using UnityEditor;
using UnityEngine;

// 게임 전체 폰트(개구) 설정
//  1. Resources/Fonts/Gaegu-Regular.ttf 로 TMP 폰트 에셋(Gaegu-Regular SDF)을 만든다
//  2. TMP 기본 폰트로 지정한다 (앞으로 새로 만드는 글자에도 개구가 적용됨)
// 에디터가 열릴 때 한 번 자동으로 실행되고, 메뉴 ProjectJ → 게임 폰트 다시 설정 으로도 실행 가능
public static class GameFontSetup
{
    private const string FontFilePath = "Assets/Resources/" + GameFont.SourcePath + ".ttf";
    private const string FontAssetPath = "Assets/Resources/" + GameFont.AssetPath + ".asset";

    [InitializeOnLoadMethod]
    private static void OnEditorLoad()
    {
        EditorApplication.delayCall += () =>
        {
            TMP_FontAsset fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontAssetPath);

            if (fontAsset == null || TMP_Settings.defaultFontAsset != fontAsset)
                Setup();
        };
    }

    [MenuItem("ProjectJ/게임 폰트 다시 설정")]
    private static void Setup()
    {
        Font font = AssetDatabase.LoadAssetAtPath<Font>(FontFilePath);

        if (font == null)
        {
            Debug.LogWarning($"[폰트] {FontFilePath} 가 없음");
            return;
        }

        TMP_FontAsset fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontAssetPath);

        // TMP 기본 메뉴(폰트 우클릭 → Create → TextMeshPro → Font Asset → SDF)로 폰트 에셋 생성
        if (fontAsset == null)
        {
            Object[] oldSelection = Selection.objects;
            Selection.objects = new Object[] { font };
            EditorApplication.ExecuteMenuItem("Assets/Create/TextMeshPro/Font Asset/SDF");
            Selection.objects = oldSelection;

            fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontAssetPath);

            if (fontAsset == null)
            {
                Debug.LogWarning("[폰트] 개구 TMP 폰트 에셋을 만들지 못함. Gaegu-Regular.ttf 우클릭 → Create → TextMeshPro → Font Asset → SDF 로 직접 만들어 주세요");
                return;
            }
        }

        if (TMP_Settings.instance == null)
            return;

        TMP_Settings.defaultFontAsset = fontAsset;
        EditorUtility.SetDirty(TMP_Settings.instance);
        AssetDatabase.SaveAssets();

        Debug.Log("[폰트] TMP 기본 폰트를 개구(Gaegu)로 설정함");
    }
}
