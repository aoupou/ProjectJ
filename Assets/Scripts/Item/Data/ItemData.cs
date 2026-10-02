using UnityEngine;

public enum ItemEffectType
{
    // InspectorName = Inspector와 구글 시트 드롭다운에 보이는 이름 (시트는 영문 이름도 인식)
    [InspectorName("HP 회복")] HealHP,          // HP 회복 (value = 회복량, character_HP와 같은 단위: 1 = 반 칸)
    [InspectorName("이동 칸 증가")] MoveRangeUp, // 이동 가능 칸 수 증가 (value = 추가 칸 수, 이번 턴만)
}

// 아이템 하나의 정보. Assets/Resources/Items 폴더에 만들어두면 자동으로 불러온다
// 만들기: Project 창 우클릭 → Create → ProjectJ → Item
[CreateAssetMenu(fileName = "Item_", menuName = "ProjectJ/Item")]
[GameDatabase("아이템", "Items")]
public class ItemData : ScriptableObject
{
    // 세이브용 고유 번호. 에디터가 에셋 GUID로 자동으로 채움 (Editor/ItemDataIdFixer.cs)
    // 그래서 파일 이름 / 아이템 이름을 마음대로 바꿔도 세이브가 유지된다
    [HideInInspector] public string id;

    public string itemName;
    public Sprite icon;
    [TextArea] public string concept;    // 상점 상세 정보의 콘셉트 설명
    [TextArea] public string effectText; // 툴팁 / 상세 정보의 효과 설명
    public int price;
    // 상점에 나오는 조건. 0 = 처음부터, N = 스테이지 N 클리어 보상으로 해금 (Inventory.UnlockStageRewards)
    [Min(0)] public int unlockStage;

    public ItemEffectType effectType;
    public int value;
}
