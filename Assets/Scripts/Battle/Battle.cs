using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

public class Battle : MonoBehaviour
{
    [SerializeField] private Transform playerTransform;
    [SerializeField] private SpriteAnimator spriteAnimator; 
    [SerializeField] private Map map;
    [SerializeField] private GameObject ActionUI;
    [SerializeField] private GameObject WeaponUI;
    [SerializeField] private Transform mapsponerTransform;
    [SerializeField] private BattlePreview battlePreview;
    [SerializeField] private BattleShow battleShow; // 공격 연출 담당 (비워두면 같은 오브젝트에서 찾거나 붙임)
    [SerializeField] private Battle_Step currentStep = Battle_Step.SelectAction;

    public Battle_Step CurrentStep => currentStep;
    public Map BattleMap => map;                     // BattleShow가 연출 위치 계산에 씀
    public Transform PlayerTransform => playerTransform;
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

        if (battleShow == null)
            battleShow = GetComponent<BattleShow>();
        if (battleShow == null)
            battleShow = gameObject.AddComponent<BattleShow>();
    }
    private void Start()
    {
        ChangeStep(Battle_Step.SelectAction);
        SelectAction();
    }
    private void Update()
    {
        // 공격 방향 고르는 중에 클릭하면 공격 확정
        if (CurrentStep != Battle_Step.SelectAttack)
            return;
        if (Mouse.current == null || !Mouse.current.leftButton.wasPressedThisFrame)
            return;

        Attack_Player();
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
    }
    public void Choose_Attack()
    {
        HideActionUI();
        ShowWeaponUI();
        ChangeStep(Battle_Step.SelectWeapon);
    }
    public void Move_Player(int x, int y) 
    {
        if (CurrentStep != Battle_Step.SelectDirection)
            return;
        if (map == null || playerTransform == null || battlePreview == null)
            return;
        if (!battlePreview.CanMoveTo(x, y))
            return;

        Player player = playerTransform.GetComponent<Player>();
        if (player == null)
        {
            Debug.LogError("playerTransform에 Player 컴포넌트가 없습니다.");
            return;
        }

        if(!map.TryGetTileWorldPosition(x,y, out Vector3 worldPosition))
        {
            Debug.LogWarning("월드 포지션 없음 ㅅㄱ");
            return;
        }
        if (!map.SetPlayerTile(x, y))
            return;

        MoveAni(player.GetPositionOnTile(worldPosition));

        battlePreview.ClearMovePreview();

    }
    public void MoveAni(Vector3 moveTile)
    {
        StartCoroutine(CoroutineMove(moveTile));
    }
    private IEnumerator CoroutineMove(Vector3 moveTile)
    {
        spriteAnimator.SetState("WalkingOn", true);
        yield return new WaitForSeconds(spriteAnimator.Duration);

        playerTransform.position = moveTile;
         
        spriteAnimator.SetState("WalkingOff", true);
        yield return new WaitForSeconds(spriteAnimator.Duration);

        spriteAnimator.SetState("Idle", true);
        Finish_Anim();
    }
    public void Attack_Player()
    {
        if (CurrentStep != Battle_Step.SelectAttack)
            return;

        WeaponData weapon = battlePreview.CurrentWeapon;
        if (weapon == null)
            return;

        Vector2Int direction = battlePreview.AttackDirection;
        List<Vector2Int> attackTiles = battlePreview.GetAttackTiles();

        ChangeStep(Battle_Step.TurnEnd); // 연출 중 재클릭 방지
        battlePreview.ClearMovePreview();

        // TODO: 데미지 처리

        battleShow.AttackAni(weapon.weaponType, direction, attackTiles); // 끝나면 BattleShow가 Finish_Anim() 호출
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
    public void ChooseSword()
    {
        if (CurrentStep != Battle_Step.SelectWeapon)
            return;
        WeaponData weapon = GetWeapon(0, WeaponType.Sword); // 추후 강화 시스템 나오면 레벨 수정 
        battlePreview.PlayerAttackPreview(weapon);
        
        HideWeaponUI();
        ChangeStep(Battle_Step.SelectAttack);
    }
    public void ChooseSpear()
    {
        if (CurrentStep != Battle_Step.SelectWeapon)
            return;

        WeaponData weapon = GetWeapon(0, WeaponType.Spear); // 추후 강화 시스템 나오면 레벨 수정 
        battlePreview.PlayerAttackPreview(weapon);

        HideWeaponUI();
        ChangeStep(Battle_Step.SelectAttack);
    }
    public void ChooseGun()
    {
        if (CurrentStep != Battle_Step.SelectWeapon)
            return;
        WeaponData weapon = GetWeapon(0, WeaponType.Gun);// 추후 강화 시스템 나오면 레벨 수정
        battlePreview.PlayerAttackPreview(weapon);

        HideWeaponUI();
        ChangeStep(Battle_Step.SelectAttack);
    }
    public void ChooseMagic()
    {
        if (CurrentStep != Battle_Step.SelectWeapon)
            return;
        WeaponData weapon = GetWeapon(0, WeaponType.Magic);// 추후 강화 시스템 나오면 레벨 수정
        battlePreview.PlayerAttackPreview(weapon);

        HideWeaponUI();
        ChangeStep(Battle_Step.SelectAttack);
    }
    private WeaponData GetWeapon(int level ,WeaponType type)
    {
        switch (type)
        {
            case WeaponType.Sword:
                WeaponData Sword = WeaponDatabase.Get("Sword_" + level);
                return Sword;

            case WeaponType.Spear:
                WeaponData Spear = WeaponDatabase.Get("Spear_" + level);
                return Spear;
            
            case WeaponType.Gun:
                WeaponData Gun = WeaponDatabase.Get("Gun_" + level);
                return Gun;
            
            case WeaponType.Magic:
                WeaponData Magic = WeaponDatabase.Get("Magic_" + level);
                return Magic;
        }
        
        return null;
    }
}
