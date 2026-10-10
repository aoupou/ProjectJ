using UnityEngine;

public class Battle : MonoBehaviour
{
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Map map;
    [SerializeField] private GameObject ActionUI;
    [SerializeField] private GameObject WeaponUI;
    public enum Battle_Step
    {
        SelectAction = 0, // 행동 선택
        SelectDirection, // 이동 위치 또는 방향 선택
        SelectWeapon, // 무기 선택
        SelectAttack, // 공격 방식 선택
        TurnEnd // 턴 종료
    }
    [SerializeField] private Battle_Step currentStep = Battle_Step.SelectAction;
    public Battle_Step CurrentStep => currentStep;
    private void Start()
    {
        ChangeStep(Battle_Step .SelectAction);
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
        ChangeStep(Battle_Step .SelectDirection);
        // 배틀 프리뷰의 이동위치 보는 코드와 연결
    }
    public void Move_Player(int direction) // 배틀 프리뷰에서 이동위치 1,2,3,4 로 받음
    {
        if (CurrentStep != Battle_Step.SelectDirection)
            return;
        // 배틀 프리뷰에서 받은 목적지 확인
        // 이동가능 여부 판단 (장애물 생기면)
        switch (direction)
        {
            case 1: // 위
                Move_Up();
                break;

            case 2: // 아래
                Move_Down();
                break;

            case 3: // 오른쪽
                Move_Right();
                break;

            case 4: // 왼쪽
                Move_Left();
                break;

            default:
                // 잘못된 이동 명령
                return;
        }

        // 배틀 쇼의 애니메이션 재생 코드와 연결
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
}
