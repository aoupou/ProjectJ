using System;
using System.Collections.Generic;
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

    [Serializable]
    public class SheetLink
    {
        [Tooltip("데이터 종류 (코드 이름, 예: WeaponData)")]
        public string dataType;

        [Tooltip("그 데이터 탭 주소")]
        public string url;
    }

    [Tooltip("새 데이터베이스 만들기로 만든 데이터 종류의 탭 주소 (만들 때 자동으로 칸이 추가됨)")]
    public List<SheetLink> otherSheets = new List<SheetLink>();

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

    // 새 데이터베이스 만들기에서 호출. 주소는 비워두고 칸만 만든다
    public void AddOtherSheet(string dataType)
    {
        if (otherSheets.Exists(s => s.dataType == dataType))
            return;

        otherSheets.Add(new SheetLink { dataType = dataType });
        EditorUtility.SetDirty(this);
        AssetDatabase.SaveAssets();
    }
}
