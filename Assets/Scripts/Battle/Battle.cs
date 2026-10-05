using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using static UnityEngine.CullingGroup;


public class Battle : MonoBehaviour
{
    [SerializeField] private GameObject Action; // 이동
    [SerializeField] private GameObject moveDirection; // 이동 방향 UI
    [SerializeField] private GameObject weaponSelect;
    [SerializeField] private Transform player;
    [SerializeField] private character_HP playerHP;
    [SerializeField] private Stage_change stageChange;
    [SerializeField] private character_HP enemyHP;
    [SerializeField] private Camera mainCamera; 
    [SerializeField] private int attackDamage = 1; // 임시
    [SerializeField] private WeaponType selectweapon;
    [SerializeField] private enemy_movement enemy;
    public Turn_UI turnUI;
    private Player playerScript;

    private change_tile_color tileColor;

    private bool isAiming = false;
    private Vector2 aimDirection;


    public int currentTurn = 1;
    // 공격 범위: 플레이어 기준 칸 → 그 칸 데미지
    private Dictionary<Vector2Int, int> cells = new Dictionary<Vector2Int, int>();

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

    void Start()
    {
        playerScript = player.GetComponent<Player>();
        Start_Turn();
        tileColor = GetComponent<change_tile_color>();
    }

    private void Update()
    {
        if (!isAiming) return;

        Vector2 direction = GetMouseDirection();
        if (direction != aimDirection)
        {
            aimDirection = direction;
            SetCells(direction);
        }

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            isAiming = false;
            AttackCells();
            tileColor.ResetColor();
            enemy.SelectAction();
            EndTurn();
        }
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
        Sellect_weapon();
    }

    //void Attack(Vector2 direction)
    //{
    //    Vector2 playerPosition = player.position;

    //    float tileWidth = playerScript.map.TileWidthSize;
    //    float tileHeight = playerScript.map.TileHeightSize;

    //    Vector2 targetCenter;

    //    // 위 / 아래 공격
    //    if (direction == Vector2.up || direction == Vector2.down)
    //    {
    //        targetCenter =
    //            playerPosition + direction * tileHeight;
    //    }
    //    // 왼쪽 / 오른쪽 공격
    //    else
    //    {
    //        targetCenter =
    //            playerPosition + direction * tileWidth;
    //    }

    //    Vector2 target1;
    //    Vector2 target2;
    //    Vector2 target3;

    //    // 위 / 아래 → 가로로 3칸
    //    if (direction == Vector2.up || direction == Vector2.down)
    //    {
    //        target1 =
    //            targetCenter + Vector2.left * tileWidth;

    //        target2 =
    //            targetCenter;

    //        target3 =
    //            targetCenter + Vector2.right * tileWidth;
    //    }

    //    // 왼쪽 / 오른쪽 → 세로로 3칸
    //    else
    //    {
    //        target1 =
    //            targetCenter + Vector2.up * tileHeight;

    //        target2 =
    //            targetCenter;

    //        target3 =
    //            targetCenter + Vector2.down * tileHeight;
    //    }

    //    // 3칸 공격
    //    CheckAttackPosition(target1);
    //    CheckAttackPosition(target2);
    //    CheckAttackPosition(target3);

    //    // 공격 방향 UI 끄기
    //    //weaponSelect.SetActive(false);

    //    // 턴 종료
    //    EndTurn();
    //}
    //public void Attack_Up()
    //{
    //    Attack(Vector2.up);
    //}
    //public void Attack_Down()
    //{
    //    Attack(Vector2.down);
    //}
    //public void Attack_Left()
    //{
    //    Attack(Vector2.left);
    //}
    //public void Attack_Right()
    //{
    //    Attack(Vector2.right);
    //}
    //void CheckAttackPosition(Vector2 position)
    //{
    //    Collider2D hit = Physics2D.OverlapPoint(position);

    //    if (hit == null)
    //        return;

    //    character_HP hp =
    //        hit.GetComponent<character_HP>();

    //    if (hp != null)
    //    {
    //        hp.TakeDamage(attackDamage);
    //    }
    //}
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
    void Sellect_weapon()
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

        weaponSelect.GetComponent<RectTransform>().localPosition = localPosition;

        weaponSelect.SetActive(true);

        Step = Battle_Step.SelectAttack;
    }
    void AttackCells()
    {
        Vector3 enemyPoisiotn = enemy.transform.position;
        Vector3 playerPoisiotn = player.position;
        
        Vector2Int enemyCells = tileColor.WorldTocell(enemyPoisiotn);
        Vector2Int playerCells = tileColor.WorldTocell(playerPoisiotn);

        Vector2Int enemy_player = enemyCells - playerCells;

        if (cells.TryGetValue(enemy_player, out int damage))
        {
            enemyHP.TakeDamage(damage);
        }

    }


    Vector2 GetMouseDirection()
    {
        Vector2 mouseScreen = Mouse.current.position.ReadValue();
        Vector3 mouseWorld = mainCamera.ScreenToWorldPoint(mouseScreen);

        Vector2 diff = mouseWorld - player.position;

        if (Mathf.Abs(diff.x) > Mathf.Abs(diff.y))
        {
            if (diff.x > 0)
                return Vector2.right;
            else
                return Vector2.left;
        }

        else if (Mathf.Abs(diff.y) > Mathf.Abs(diff.x))
        {
            if (diff.y > 0)
                return
                    Vector2.up;
            else
                return Vector2.down;
        }

        return Vector2.up;
    }
    public Vector2Int Rotate(Vector2Int cell, Vector2 direction)
    {
        if (direction == Vector2.up) { return cell; }
        else if (direction == Vector2.right) { return new Vector2Int(cell.y,-cell.x); }
        else if (direction == Vector2.down) { return new Vector2Int(-cell.x,-cell.y); }
        else if (direction == Vector2.left) { return new Vector2Int(-cell.y,cell.x); }
        return cell;
    }
    void SetCells(Vector2 direction)
    {
        cells.Clear();
        foreach (WeaponData weapon in WeaponDatabase.OfType(selectweapon))
        {
            foreach (Vector2Int cell in weapon.range)
            {
                Vector2Int rotated = Rotate(cell, direction);

                // 같은 종류 무기끼리 범위가 겹치면 안 됨 → 무기 데이터 range 다시 설정
                if (cells.ContainsKey(rotated))
                    Debug.LogWarning($"[무기] {selectweapon} 범위 겹침: {weapon.name}의 {cell}");

                cells[rotated] = weapon.damage;
            }
        }
        tileColor.ShowRange(player.position, cells);
    }
    void StartAiming(WeaponType type)
    {
        weaponSelect.SetActive(false);
        selectweapon = type;
        isAiming = true;

        aimDirection = GetMouseDirection();
        SetCells(aimDirection);

    }
    
    public void Magic()
    {
        StartAiming(WeaponType.Magic);
    }
    public void Sword()
    {
        StartAiming(WeaponType.Sword);
    }
    public void Gun()
    {
        StartAiming(WeaponType.Gun);
    }
    public void Spear()
    {
        StartAiming(WeaponType.Spear);
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
