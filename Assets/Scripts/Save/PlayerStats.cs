using UnityEngine;

public sealed class PlayerStats : CharacterStats
{
    // Battle에서 이번 전투에 적용할 체력과 이동력을 넘겨줌
    public PlayerStats(int startingHp, int maxHp, int moveRange)
        : base(startingHp, maxHp, moveRange)
    {
    }
}
