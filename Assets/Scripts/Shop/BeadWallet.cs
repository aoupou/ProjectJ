using UnityEngine;
using System;

public sealed class BeadWallet
{
    // 잔액 변경은 Add()와 TrySpend()를 통해서
    public int Balance {  get; private set; }
    public BeadWallet(int startingBeads = 0)
    {
        //예외처리
        if (startingBeads < 0)
            throw new ArgumentOutOfRangeException(nameof(startingBeads));

        Balance = startingBeads;
    }
    public void Add(int amount)
    {
        //예외처리
        if (amount <= 0)
            throw new ArgumentOutOfRangeException(nameof(amount));

        // 더한 값이 int의 범위를 넘으면 예외 발생
        Balance = checked(Balance + amount);
    }
    public bool TrySpend(int amount)
    {
        //예외처리
        if (amount < 0)
            throw new ArgumentOutOfRangeException(nameof(amount));

        // 잔액이 부족하면 차감하기 전에 종료
        if (Balance < amount)
            return false;

        Balance -= amount;
        return true;
    }

}
