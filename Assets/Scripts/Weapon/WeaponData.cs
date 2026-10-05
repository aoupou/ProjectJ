using System.Collections.Generic;
using UnityEngine;

// 무기 하나의 정보 (ProjectJ → 데이터베이스 → 새 데이터베이스 만들기로 생성)
// 추가: ProjectJ → 데이터베이스 → 새 데이터 만들기 → 무기
//   또는 구글 시트 "무기" 탭에 한 줄 추가 → ProjectJ → 구글 시트 → 전부 가져오기
// 저장 위치: Assets/Resources/Weapons (에셋 파일 이름 = key). 게임에서 찾기: WeaponDatabase.Get("key")
// 칸을 늘리려면 아래에 public 변수를 추가하면 된다 (구글 시트 열 이름 = 변수 이름)
[GameDatabase("무기", "Weapons")]
public class WeaponData : ScriptableObject
{
    [Tooltip("무기 이름")]
    public string displayName;
    [Tooltip("무기 종류")]
    public WeaponType weaponType;
    [Tooltip("공격 데미지")]
    public int damage;
    [Tooltip("공격 범위 칸")]
    public List<Vector2Int> range = new List<Vector2Int>();
}
