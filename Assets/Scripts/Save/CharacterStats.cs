using UnityEngine;
using System;

public abstract class CharacterStats
{
    public int MaxHp { get; private set; }
    public int CurrentHp { get; private set; }
    public int MoveRange { get; }


    bool IsDead => CurrentHp == 0;

    protected CharacterStats(int startingHp, int maxHp, int moveRange)
    {
        if (maxHp <= 0)
            throw new ArgumentOutOfRangeException(nameof(maxHp));

        if (startingHp < 0 || startingHp > maxHp)
            throw new ArgumentOutOfRangeException(nameof(startingHp));

        if (moveRange < 0)
            throw new ArgumentOutOfRangeException(nameof(moveRange));

        MaxHp = maxHp;
        CurrentHp = startingHp;
        MoveRange = moveRange;
    }
    public void TakeDamage(int damage)
    {
        if (damage < 0)
            throw new ArgumentOutOfRangeException(nameof(damage));

        // 남은 HP까지만
        CurrentHp -= Math.Min(damage, CurrentHp);
    }
    public void Heal(int amount)
    {
        if (amount < 0)
            throw new ArgumentOutOfRangeException(nameof(amount));

        // 예외처리
        if (IsDead)
            return;

        // 최대 Hp이상으로는 X.
        CurrentHp += amount;
        if(CurrentHp >= MaxHp)
            CurrentHp = MaxHp;
    }
}
