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
    [Tooltip("무기 강화 단계")]
    public int level;
    [Tooltip("명중률 (0~100)")]
    public int hitP;
    [Tooltip("공격 칸 / 칸마다 데미지")]
    public List<AttackCell> weaponRD = new List<AttackCell>();
    [Tooltip("강화 성공 확률 (0~100)")]
    public int upgradeP;
    [Tooltip("강화 소모 골드")]
    public int upgradeGold;
    [Tooltip("무기 설명")]
    public string weaponText;
    [Tooltip("무기 효과 / 수치")]
    public List<WeaponEffect> weaponEffects = new List<WeaponEffect>();
    [Tooltip("공격 이펙트")]
    public GameObject vfx;
    [Tooltip("무기 아이콘")]
    public Sprite icon;
}
