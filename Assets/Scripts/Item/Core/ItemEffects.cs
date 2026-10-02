using System;
using UnityEngine;

// 전투 중 아이템 효과 적용 + 다른 시스템에 알려주는 곳
// 다른 사람 코드는 여기 값/이벤트를 읽기만 하면 된다
public static class ItemEffects
{
    // 이번 턴 추가 이동 칸 수 (이동 범위 계산할 때 더해서 쓰면 됨)
    public static int BonusMoveRange { get; private set; }

    // 선택 상태인 아이템 (없으면 null). 이동 범위 표시에서 추가 칸을 깜빡이게 할 때 사용
    public static ItemData PreviewItem { get; private set; }

    public static event Action<ItemData> PreviewStarted; // 아이템 선택 상태 진입 (기획서 10-2-2)
    public static event Action PreviewEnded;
    public static event Action<ItemData> Used;

    // 이 프로젝트는 플레이 시작 시 도메인 리로드가 꺼져 있어서(Enter Play Mode Options)
    // static 값이 이전 플레이에서 그대로 남는다. 그래서 플레이 시작마다 직접 초기화
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        ResetBattle();
        PreviewStarted = null;
        PreviewEnded = null;
        Used = null;
    }

    public static void StartPreview(ItemData item)
    {
        PreviewItem = item;
        PreviewStarted?.Invoke(item);
    }

    public static void EndPreview()
    {
        PreviewItem = null;
        PreviewEnded?.Invoke();
    }

    public static void Apply(ItemData item)
    {
        switch (item.effectType)
        {
            case ItemEffectType.HealHP:
                {
                    // character_HP는 플레이어와 적 둘 다 붙어 있으므로 반드시 플레이어 것을 찾음
                    Player player = UnityEngine.Object.FindAnyObjectByType<Player>();
                    character_HP hp = player != null ? player.GetComponent<character_HP>() : null;

                    // character_HP에 회복 함수가 없어서 음수 데미지로 회복 (Heal()이 생기면 교체)
                    if (hp != null)
                        hp.TakeDamage(-item.value);

                    break;
                }
            case ItemEffectType.MoveRangeUp:
                {
                    BonusMoveRange += item.value;
                    break;
                }
        }

        Used?.Invoke(item);
    }

    // 턴이 끝나면 이번 턴 효과 초기화
    public static void OnTurnEnd()
    {
        BonusMoveRange = 0;
    }

    // 전투 시작 시 초기화
    public static void ResetBattle()
    {
        BonusMoveRange = 0;
        PreviewItem = null;
    }
}
