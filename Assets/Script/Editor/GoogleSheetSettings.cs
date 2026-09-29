using UnityEditor;
using UnityEngine;

// 구글 시트 링크 저장용. 이 에셋은 git에 올라가므로 팀원 모두 같은 링크를 씀
// 링크는 시트에서 해당 탭을 연 상태의 주소창 주소를 그대로 붙여넣으면 된다
// (공유 → "링크가 있는 모든 사용자: 뷰어" 필요. "웹에 게시" CSV 링크도 가능)
public class GoogleSheetSettings : ScriptableObject
{
    private const string AssetPath = "Assets/Settings/GoogleSheetSettings.asset";

    [Tooltip("아이템 탭 주소 (예: https://docs.google.com/spreadsheets/d/.../edit#gid=0)")]
    public string itemsUrl;

    [Tooltip("캐릭터 탭 주소 (플레이어 / 적 스탯)")]
    public string charactersUrl;

    public static GoogleSheetSettings GetOrCreate()
    {
        GoogleSheetSettings settings = AssetDatabase.LoadAssetAtPath<GoogleSheetSettings>(AssetPath);

        if (settings == null)
        {
            settings = CreateInstance<GoogleSheetSettings>();
            AssetDatabase.CreateAsset(settings, AssetPath);
            AssetDatabase.SaveAssets();
        }

        return settings;
    }
}
