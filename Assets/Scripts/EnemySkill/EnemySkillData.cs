using System.Collections.Generic;
using UnityEngine;

// 적 스킬 하나의 정보
// 추가: ProjectJ → 데이터베이스 → 새 데이터 만들기 → 적 스킬
//   또는 구글 시트 "적 스킬" 탭에 한 줄 추가 → ProjectJ → 구글 시트 → 전부 가져오기
// 저장 위치: Assets/Resources/EnemySkills (에셋 파일 이름 = key). 게임에서 찾기: EnemySkillDatabase.Get("key")
// 칸을 늘리려면 아래에 public 변수를 추가하면 된다 (구글 시트 열 이름 = 변수 이름)
[GameDatabase("적 스킬", "EnemySkills")]
public class EnemySkillData : ScriptableObject
{
    [Tooltip("스킬 이름")]
    public string displayName;
    [Tooltip("이 스킬을 쓰는 적 (캐릭터 표의 key)")]
    public CharacterData owner;
    [Tooltip("공격 칸과 데미지. 적이 위쪽을 볼 때 기준 (x+ 오른쪽, y+ 위). 공격할 때 플레이어 방향으로 돌림 (무기 범위와 같은 방식)")]
    public List<AttackCell> skillRD = new List<AttackCell>();
    [Tooltip("공격 선호도. 닿는 공격이 2개 이상일 때 이 값의 비율로 고름")]
    public int ATP;
    [Tooltip("명중률")]
    public int hitP;
    [Tooltip("스킬 효과")]
    public EnemySkillEffectType effectType;
    [Tooltip("효과 수치")]
    public int effectValue;
    [Tooltip("스킬 설명")]
    public string skillText;
}
