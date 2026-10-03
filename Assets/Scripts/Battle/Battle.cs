using UnityEngine;
using static UnityEngine.CullingGroup;

public class Battle : MonoBehaviour
{
    [SerializeField] private GameObject Action; // 이동
    [SerializeField] private GameObject moveDirection; // 이동 방향 UI
    [SerializeField] private GameObject attackDirection;
    [SerializeField] private Transform player;
    [SerializeField] private character_HP playerHP;
    [SerializeField] private Stage_change stageChange;
    [SerializeField] private character_HP enemyHP;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private int attackDamage = 1; // 임시
    [SerializeField] private enemy_movement enemy;

    public Turn_UI turnUI;
    private Player playerScript;
    public int currentTurn = 1;

    public enum Battle_Step
    {
        SelectAction = 0,
        SelectDirection,
        SelectWeapon,
        SelectAttack,
        ExecuteAction,
        TurnEnd,
    }

    public Battle_Step Step;

    private Battle_Step playerAction = Battle_Step.SelectAction;
    private Battle_Step enemyAction = Battle_Step.SelectAction;

    // 구글 시트 스탯으로 플레이어 공격력 설정 (CharacterStatHook에서 호출)
    public void SetAttackDamage(int damage)
    {
        attackDamage = damage;
    }

    void Start()
    {
        playerScript = player.GetComponent<Player>();
        Start_Turn();
    }

    void Start_Turn()
    {
        playerAction = Battle_Step.SelectAction;
        enemyAction = Battle_Step.SelectAction;

        CheckHP();

        turnUI.SetTurn(currentTurn);

        Sellect_Action(); // 버튼 UI띄워주는 코드
    }

    void Sellect_Action()
    {
        Vector3 worldPosition =
            player.position + new Vector3(1f, -2f, 0);

        Vector2 screenPosition =
            mainCamera.WorldToScreenPoint(worldPosition);

        RectTransform canvasRect =
            Action.GetComponentInParent<Canvas>().GetComponent<RectTransform>();

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            screenPosition,
            mainCamera,
            out Vector2 localPosition
        );

        Action.GetComponent<RectTransform>().localPosition = localPosition;

        Action.SetActive(true);
    }

    public void Sellect_Move()
    {
        Action.SetActive(false);

        Sellect_direction();
    }

    public void Sellect_Attack()
    {
        Action.SetActive(false);

        Sellect_Attack_direction();
    }

    void Attack(Vector2 direction)
    {
        Vector2 playerPosition = player.position;

        float tileWidth = playerScript.map.TileWidthSize;
        float tileHeight = playerScript.map.TileHeightSize;

        Vector2 targetCenter;

        // 위 / 아래 공격
        if (direction == Vector2.up || direction == Vector2.down)
        {
            targetCenter =
                playerPosition + direction * tileHeight;
        }
        // 왼쪽 / 오른쪽 공격
        else
        {
            targetCenter =
                playerPosition + direction * tileWidth;
        }

        Vector2 target1;
        Vector2 target2;
        Vector2 target3;

        // 위 / 아래 → 가로로 3칸
        if (direction == Vector2.up || direction == Vector2.down)
        {
            target1 =
                targetCenter + Vector2.left * tileWidth;

            target2 =
                targetCenter;

            target3 =
                targetCenter + Vector2.right * tileWidth;
        }

        // 왼쪽 / 오른쪽 → 세로로 3칸
        else
        {
            target1 =
                targetCenter + Vector2.up * tileHeight;

            target2 =
                targetCenter;

            target3 =
                targetCenter + Vector2.down * tileHeight;
        }

        enemy.SelectAction();
        
        // 3칸 공격
        CheckAttackPosition(target1);
        CheckAttackPosition(target2);
        CheckAttackPosition(target3);

        // 공격 방향 UI 끄기
        attackDirection.SetActive(false);

        // 턴 종료
        EndTurn();
    }

    public void Attack_Up()
    {
        Attack(Vector2.up);
    }

    public void Attack_Down()
    {
        Attack(Vector2.down);
    }

    public void Attack_Left()
    {
        Attack(Vector2.left);
    }

    public void Attack_Right()
    {
        Attack(Vector2.right);
    }

    void CheckAttackPosition(Vector2 position)
    {
        Vector2 enemyPosition = enemy.transform.position;

        if (Vector2.Distance(enemyPosition, position) <= 0.01f)
        {
            character_HP hp = enemy.GetComponent<character_HP>();

            if (hp != null)
            {
                hp.TakeDamage(attackDamage);
            }
        }
    }

    void Sellect_direction()
    {
        Vector3 worldPosition =
            player.position + new Vector3(0, 0, 0);

        Vector2 screenPosition =
            mainCamera.WorldToScreenPoint(worldPosition);

        RectTransform canvasRect =
            Action.GetComponentInParent<Canvas>().GetComponent<RectTransform>();

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            screenPosition,
            mainCamera,
            out Vector2 localPosition
        );

        moveDirection.GetComponent<RectTransform>().localPosition = localPosition;

        moveDirection.SetActive(true);// 플레이어 방향선택 UI 등장

        Step = Battle_Step.SelectDirection;
    }

    void Sellect_Attack_direction()
    {
        Vector3 worldPosition =
            player.position + new Vector3(0, 0, 0);

        Vector2 screenPosition =
            mainCamera.WorldToScreenPoint(worldPosition);

        RectTransform canvasRect =
            Action.GetComponentInParent<Canvas>().GetComponent<RectTransform>();

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            screenPosition,
            mainCamera,
            out Vector2 localPosition
        );

        attackDirection.GetComponent<RectTransform>().localPosition = localPosition;

        attackDirection.SetActive(true);// 공격 방향선택 UI 등장

        Step = Battle_Step.SelectAttack;
    }

    public void ExecuteAction()
    {
        moveDirection.SetActive(false);
    }

    private void OnEnable()
    {
        Player.movingOut += AfterPlayerMove;

        Player.moveSellecting += ExecuteAction;
    }

    private void OnDisable()
    {
        Player.movingOut -= AfterPlayerMove;

        Player.moveSellecting -= ExecuteAction;
    }

    void AfterPlayerMove()
    {
        Step = Battle_Step.TurnEnd;

        // 플레이어 이동이 끝났으므로 적 행동 선택
        enemy.SelectAction();

        EndTurn();
    }

    void EndTurn()
    {
        currentTurn++;

        Start_Turn();
    }

    void CheckHP()
    {
        if (playerHP.CurrentHP <= 0)
        {
            stageChange.GameOver();
            return;
        }

        if (enemyHP.CurrentHP <= 0)
        {
            stageChange.GameClear();
            return;
        }
    }
    public void EnemyAttack()
    {
        Vector2 direction = GetEnemyAttackDirection();

        Vector2 enemyPosition = enemy.transform.position;

        float tileWidth = playerScript.map.TileWidthSize;
        float tileHeight = playerScript.map.TileHeightSize;

        // 적 바로 앞의 칸
        Vector2 targetCenter;

        if (direction == Vector2.left ||
            direction == Vector2.right)
        {
            targetCenter =
                enemyPosition + direction * tileWidth;
        }
        else
        {
            targetCenter =
                enemyPosition + direction * tileHeight;
        }

        Vector2 target1;
        Vector2 target2;
        Vector2 target3;

        // 세로 방향 공격
        if (direction == Vector2.up ||
            direction == Vector2.down)
        {
            target1 =
                targetCenter + Vector2.left * tileWidth;

            target2 =
                targetCenter;

            target3 =
                targetCenter + Vector2.right * tileWidth;
        }
        // 가로 방향 공격
        else
        {
            target1 =
                targetCenter + Vector2.up * tileHeight;

            target2 =
                targetCenter;

            target3 =
                targetCenter + Vector2.down * tileHeight;
        }

        CheckEnemyAttackPosition(target1);
        CheckEnemyAttackPosition(target2);
        CheckEnemyAttackPosition(target3);
    }
    private Vector2 GetEnemyAttackDirection()
    {
        float xDistance =
            player.position.x - enemy.transform.position.x;

        float yDistance =
            player.position.y - enemy.transform.position.y;

        if (Mathf.Abs(xDistance) > Mathf.Abs(yDistance))
        {
            if (xDistance > 0)
                return Vector2.right;
            else
                return Vector2.left;
        }
        else
        {
            if (yDistance > 0)
                return Vector2.up;
            else
                return Vector2.down;
        }
    }
    private void CheckEnemyAttackPosition(Vector2 position)
    {
        Vector2 playerPosition = player.position;

        if (Vector2.Distance(playerPosition, position) <= 0.01f)
        {

            if (playerHP != null)
            {
                playerHP.TakeDamage(attackDamage);
            }
        }
    }
}