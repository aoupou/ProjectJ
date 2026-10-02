using UnityEngine;

// 캐릭터 하나의 스탯. 플레이어와 적이 같은 형식을 쓴다
// 구글 시트의 "캐릭터" 탭에서 가져와서 Assets/Resources/Characters에 자동으로 만들어짐
// (ProjectJ → 구글 시트 → 캐릭터 가져오기). 에셋 파일 이름 = 시트의 key
[CreateAssetMenu(fileName = "Character_", menuName = "ProjectJ/Character")]
[GameDatabase("캐릭터", "Characters")]
public class CharacterData : ScriptableObject
{
    public string displayName;
    public int hp = 10;    // 시작 HP (1 = 반 칸, 10 = 하트 5칸)

    [Tooltip("최대 생명력")]
    public int maxHp;
    [Tooltip("이동 칸 수")]
    public int moveRange;
    [Tooltip("얼굴 아이콘 들어갈 자리")]
    public Sprite icon;
    [Tooltip("회피 확률 (0 ~ 1)")]
    public float missP;
}
