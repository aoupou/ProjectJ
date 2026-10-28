using UnityEngine;

public class Battle : MonoBehaviour
{
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Map map;
    [SerializeField] private GameObject ActionUI;
    [SerializeField] private GameObject WeaponUI;
    [SerializeField] private Transform mapsponerTransform;
    [SerializeField] private BattlePreview battlePreview;
    [SerializeField] private Battle_Step currentStep = Battle_Step.SelectAction;
    public Battle_Step CurrentStep => currentStep;
    public int moverange;
    CharacterData playerRangeData;
    public enum Battle_Step
    {
        SelectAction = 0, // 행동 선택
        SelectDirection, // 이동 위치 또는 방향 선택
        SelectWeapon, // 무기 선택
        SelectAttack, // 공격 방식 선택
        EnemySellect, //  AI 행동 선택
        TurnEnd // 턴 종료
    }
    public void Awake()
    {
        Moverange();
    }
    private void Start()
    {
        ChangeStep(Battle_Step.SelectAction);
        SelectAction();
    }
    private void SelectAction() // 행동 UI 활성화
    {
        if (CurrentStep != Battle_Step.SelectAction)
            return;
        ShowActionUI();
    }
    public void Choose_Move()
    {
        HideActionUI();
        ChangeStep(Battle_Step.SelectDirection);
        battlePreview.PlayerMovePreview();
        // 배틀 프리뷰의 이동위치 보는 코드와 연결
    }
    public void Move_Player(int x, int y) // 배틀 프리뷰에서 이동위치 1,2,3,4 로 받음
    {
        if (CurrentStep != Battle_Step.SelectDirection)
            return;
        // 배틀 프리뷰에서 받은 목적지 확인
        // 이동가능 여부 판단 (장애물 생기면)

        if(!map.TryGetTileWorldPosition(x,y, out Vector3 worldPosition))
        {
            Debug.LogWarning("월드 포지션 없음 ㅅㄱ");
        }
        playerTransform.position = worldPosition;


        // 배틀 쇼의 애니메이션 재생 코드와 연결
        Finish_Anim(); // 임시로 턴 종료 시키지만, 나중에 적 행동과 연결지어야 함
    }
    public void Finish_Anim() // 애니메이션 재생 종료 후 실행
    {
        ChangeStep(Battle_Step.TurnEnd);
        End_Turn();
    }
    private void End_Turn() // 배틀 쇼의 애니메이션 종료 후 실행
    {
        if (CurrentStep != Battle_Step.TurnEnd)
            return;

        ChangeStep(Battle_Step.SelectAction);
        SelectAction();
    }
    private void ShowActionUI()
    {
        ActionUI.SetActive(true);
    }
    private void HideActionUI()
    {
        ActionUI.SetActive(false);
    }
    private void ShowWeaponUI()
    {
        WeaponUI.SetActive(true);
    }
    private void HideWeaponUI()
    {
        WeaponUI.SetActive(false);
    }
    private void Move_Up()
    {
        playerTransform.position += Vector3.up * map.TileHeightSize;
    }
    private void Move_Down()
    {
        playerTransform.position += Vector3.down * map.TileHeightSize;
    }
    private void Move_Right()
    {
        playerTransform.position += Vector3.right * map.TileWidthSize;
    }
    private void Move_Left()
    {
        playerTransform.position += Vector3.left * map.TileWidthSize;
    }
    private void ChangeStep(Battle_Step nextStep)
    {
        currentStep = nextStep;
    }
    public int Moverange()
    {
        playerRangeData = CharacterDatabase.Get(CharacterDatabase.PlayerKey);
        moverange = playerRangeData.moveRange;
        return moverange;
    }

}
