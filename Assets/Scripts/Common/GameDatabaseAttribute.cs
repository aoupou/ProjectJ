using System;

// 데이터 종류 표시. 이게 붙은 ScriptableObject는
//  - ProjectJ → 데이터베이스 → 새 데이터 만들기 목록에 나오고
//  - Assets/Resources/<folder> 에 저장된다
// 새 데이터베이스 만들기로 만든 코드에는 자동으로 붙어 있음
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class GameDatabaseAttribute : Attribute
{
    public readonly string DisplayName; // 메뉴와 구글 시트 탭에 보이는 이름 (예: 무기)
    public readonly string Folder;      // Resources 안의 폴더 이름 (예: Weapons)

    public GameDatabaseAttribute(string displayName, string folder)
    {
        DisplayName = displayName;
        Folder = folder;
    }
}
