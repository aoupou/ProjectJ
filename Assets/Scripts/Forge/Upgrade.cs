using UnityEngine;
using System;
using System.IO;

public enum UpgradeType
{
    Sword = 0,
    Spear = 1,
    Gun = 2,
    Magic = 3,
    Health = 4,
    MoveRange = 5,
}
public enum UpgradeResult
{
    Success,
    Failed,
    NotEnoughBeads,
    MaxLevel,
    InvalidRule,
    SaveFailed
}
public sealed class Upgrade : MonoBehaviour
{
    [Serializable]
    private struct UpgradeRule
    {
        [Min(1)]
        public int costBeads;

        [Range(0, 100)]
        public int successPercent;
        public UpgradeRule(int costBeads, int successPercent)
        {
            this.costBeads = costBeads;
            this.successPercent = successPercent;
        }
    }

    // 배열 인덱스는 현재 강화 단계
    // Element 0은 0강 → 1강, Element 1은 1강 → 2강에 사용하는 규칙
    // 1번째는 소모 재화 2번째는 성공확률
    [Header("무기 강화")]
    [SerializeField]
    private UpgradeRule[] swordRules =
    {
        new UpgradeRule(20, 100),
        new UpgradeRule(35, 80),
        new UpgradeRule(50, 60),
        new UpgradeRule(80, 40)
    };
    [SerializeField]
    private UpgradeRule[] spearRules =
    {
        new UpgradeRule(20, 100),
        new UpgradeRule(35, 80),
        new UpgradeRule(50, 60)
    };
    [SerializeField]
    private UpgradeRule[] gunRules =
    {
        new UpgradeRule(20, 100),
        new UpgradeRule(35, 80)
    };

    [SerializeField]
    private UpgradeRule[] magicRules =
    {
        new UpgradeRule(20, 100)
    };

    [Header("플레이어 스탯 강화")]
    [SerializeField]
    private UpgradeRule[] healthRules =
    {
        new UpgradeRule(20, 100),
        new UpgradeRule(35, 80),
        new UpgradeRule(50, 60)
    };

    [SerializeField]
    private UpgradeRule[] moveRangeRules =
    {
        new UpgradeRule(30, 100)
    };
    public UpgradeResult TryUpgrade(UpgradeType type)
    {
        UpgradeRule[] rules = GetRules(type);
        if (rules == null || rules.Length == 0)
            return UpgradeResult.InvalidRule;

        try
        {
            // 현재 단계에 해당하는 강화 규칙을 확인
            int level = GameSession.GetUpgradeLevel(type);
            if (level >= rules.Length)
                return UpgradeResult.MaxLevel;

            UpgradeRule rule = rules[level];

            if (rule.costBeads <= 0 ||
                rule.successPercent < 0 ||
                rule.successPercent > 100)
            {
                return UpgradeResult.InvalidRule;
            }

            int balance = GameSession.Beads;
            if (balance < rule.costBeads)
                return UpgradeResult.NotEnoughBeads;

            // 0~99 까지
            bool succeeded = UnityEngine.Random.Range(0, 100) < rule.successPercent;
            int nextLevel = succeeded ? level + 1 : level;

            // 성공과 실패 모두 비용을 지불 성공한다면 단계 증가
            GameSession.SaveUpgradeResult(type, balance - rule.costBeads, nextLevel);

            return succeeded ? UpgradeResult.Success : UpgradeResult.Failed;
        }
        catch (Exception exception) when (
            exception is IOException ||
            exception is UnauthorizedAccessException ||
            exception is ArgumentException)
        {
            Debug.LogException(exception, this);
            return UpgradeResult.SaveFailed;
        }
    }

    private UpgradeRule[] GetRules(UpgradeType type)
    {
        switch (type)
        {
            case UpgradeType.Sword: return swordRules;
            case UpgradeType.Spear: return spearRules;
            case UpgradeType.Gun: return gunRules;
            case UpgradeType.Magic: return magicRules;
            case UpgradeType.Health: return healthRules;
            case UpgradeType.MoveRange: return moveRangeRules;
            default: return null;
        }
    }
}
