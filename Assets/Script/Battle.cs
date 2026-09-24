using UnityEngine;

public class Battle : MonoBehaviour
{
    [SerializeField] private GameObject moveButton; // 이동
    [SerializeField] private GameObject attackButton; // 공격
    [SerializeField] private GameObject moveDirection; // 이동 방향 UI
    public int currentTurn = 1; 
    private enum Battle_Step
    {
        SelectAction = 0,
        SelectMove,
        SelectWeapon,
        SelectAttack,
        ExecuteAction,
        TurnEnd,
    }
    private Battle_Step Step;
    private Battle_Step playerAction = Battle_Step.SelectAction;
    private Battle_Step enemyAction = Battle_Step.SelectAction;

    void Start()
    {
        Start_Turn();
    }
    void Start_Turn()
    {
        playerAction = Battle_Step.SelectAction;
        enemyAction = Battle_Step.SelectAction;

        Debug.Log("현재 턴 : " + currentTurn);
        Sellect_Action(); // 버튼 UI띄워주는 코드
    }
    void Sellect_Action()
    {
        moveButton.SetActive(true);
        attackButton.SetActive(true);  // 이건 내일 하자
    }    
    public void Sellect_Move()
    {
        moveButton.SetActive(false);
        attackButton.SetActive(false);
        Sellect_direction();
    }
    void Sellect_Attack()
    {
        Step = Battle_Step.SelectWeapon;
    }


    void Sellect_direction()
    {
        moveDirection.SetActive(true);// 플레이어 방향선택 UI 등장 / 버튼을 플레이어 코드에 연계시킬 예정 / 입력으로 받는건 나중에 
    }

    public void ExecuteAction()
    {
        moveDirection.SetActive(false);
    }
    private void OnEnable()
    {
        Player.turn += AfterPlayerMove;
    }

    private void OnDisable()
    {
        Player.turn -= AfterPlayerMove;
    }
    void AfterPlayerMove()
    { 
        Step = Battle_Step.TurnEnd;

        EndTurn();
    }


    void EndTurn()
    {
        currentTurn++;

        Start_Turn();
    }
    

}
