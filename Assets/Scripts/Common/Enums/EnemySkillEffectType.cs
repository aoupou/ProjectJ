using UnityEngine;

// 적 스킬 효과
// 값 추가: ProjectJ → 데이터베이스 → enum 만들기 · 값 추가
// (순서 번호로 저장되니 항상 맨 아래에 추가. 중간에 끼우거나 순서를 바꾸면 기존 데이터 값이 바뀜)
public enum EnemySkillEffectType
{
    [InspectorName("없음")] None,
}
