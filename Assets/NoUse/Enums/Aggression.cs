using UnityEngine;

// 공격성
// 값 추가: ProjectJ → 데이터베이스 → enum 만들기 · 값 추가
// (순서 번호로 저장되니 항상 맨 아래에 추가. 중간에 끼우거나 순서를 바꾸면 기존 데이터 값이 바뀜)
public enum Aggression
{
    [InspectorName("무조건 공격")] Mustattack, // 100
    [InspectorName("높음")] High, // 80
    [InspectorName("보통")] Middle, // 60
    [InspectorName("낮음")] Low, // 40
    [InspectorName("매우낮음")] VeryLow, // 20
    [InspectorName("공격안함")] PeaceFull, // 0
}
