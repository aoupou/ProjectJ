using UnityEngine;

// 아이템 효과 종류 (기획서 2-2의 8종)
// 숫자는 에셋에 저장되는 값이라 순서를 바꾸면 기존 에셋 효과가 바뀜 → 새 효과는 맨 뒤에 추가
// InspectorName = Inspector와 구글 시트 드롭다운에 보이는 이름 (시트는 영문 이름도 인식)
public enum ItemEffectType
{
    [InspectorName("HP 회복")] HealHP = 0,              // 반창고: value = 회복량 (1 = 반 칸), 최대 HP는 안 넘음
    [InspectorName("아머 획득")] Armor = 1,             // 딸기 맛 사탕: value = 아머 수치 (1 = 반 칸)
    [InspectorName("명중률 증가")] HitUp = 2,           // 안경: value = 증가량(%), duration 턴 동안
    [InspectorName("함정 설치")] Trap = 3,              // 폭발형 연필깎이: 고른 칸에 설치, value = 피해량, duration 턴 동안 유지
    [InspectorName("무적")] Invincible = 4,             // 책받침: duration 턴 동안 무적 + 넉백 면역
    [InspectorName("플레이어 순간이동")] TeleportPlayer = 5, // 순간이동 종이비행기: 고른 칸으로 플레이어 이동
    [InspectorName("적 순간이동")] TeleportEnemy = 6,   // 차원 지우개: 고른 칸으로 적 이동
    [InspectorName("임시 타일")] TempTile = 7,          // 포스트잇: 붕괴된 칸에 duration 턴 동안 임시 타일
}

// 아이템 하나의 정보. Assets/Resources/Items 폴더에 만들어두면 ItemDatabase가 자동으로 불러온다
// 만들기: Project 창 우클릭 → Create → ProjectJ → Item (또는 구글 시트 "아이템" 탭에서 가져오기)
// 아이템을 찾을 때 key = 에셋 파일 이름 (CharacterData와 같은 방식)
[CreateAssetMenu(fileName = "Item_", menuName = "ProjectJ/Item")]
[GameDatabase("아이템", "Items")]
public class ItemData : ScriptableObject
{
    // 예전 세이브용 고유 번호. 지금은 채워주는 코드 없음 (기존 에셋 값 보존용으로만 남김)
    [HideInInspector] public string id;

    public string itemName;
    public Sprite icon;
    [TextArea] public string concept;    // 상점 상세 정보의 콘셉트 설명
    [TextArea] public string effectText; // 툴팁 / 상세 정보의 효과 설명
    public int price;
    // 상점에 나오는 조건. 0 = 처음부터, N = 스테이지 N 클리어 후
    // (기획서 "N 스테이지 진입 전" = 여기서는 N-1)
    [Min(0)] public int unlockStage;

    public ItemEffectType effectType;
    [Tooltip("효과 수치 (회복량, 아머, 명중률 %, 함정 피해량). 수치가 없는 효과는 0")]
    public int value;
    [Tooltip("효과가 유지되는 턴 수. 바로 끝나는 효과는 0")]
    [Min(0)] public int duration;

    // 사용할 때 칸을 골라야 하는 아이템인지 (TileSelector가 필요한 것)
    public bool NeedsTile =>
        effectType == ItemEffectType.Trap ||
        effectType == ItemEffectType.TeleportPlayer ||
        effectType == ItemEffectType.TeleportEnemy ||
        effectType == ItemEffectType.TempTile;
}
