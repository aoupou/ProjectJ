using UnityEngine;

public sealed class EnemyStats : CharacterStats
{
    // 적마다 객체를 하나씩 만들어 현재 HP를 따로 관리
    public EnemyStats(int startingHp, int maxHp, int moveRange)
        : base(startingHp, maxHp, moveRange)
    {
    }
}
