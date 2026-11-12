using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class Battle : MonoBehaviour
{
    [SerializeField] private Stage_change stgch;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private SpriteAnimator spriteAnimator; 
    [SerializeField] private BattleShow battleShow;
    [SerializeField] private Map map;
    [SerializeField] private GameObject ActionUI;
    [SerializeField] private GameObject WeaponUI;
    [SerializeField] private Transform mapsponerTransform;
    [SerializeField] private BattlePreview battlePreview;
    [SerializeField] private Battle_Step currentStep = Battle_Step.SelectAction;
    [SerializeField] private TMP_Text PHpTxt;
    [SerializeField] private TMP_Text EHpTxt;
    [SerializeField] private int PlayerHp = 10;
    [SerializeField] private int EnemyHp = 10;
    private bool isplayerdead = false;
    private bool isenemydead = false;


    public WeaponData selectWeapon;

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
    public void ChooseMove()
    {
        HideActionUI();
        ChangeStep(Battle_Step.SelectDirection);
        battlePreview.PlayerMovePreview();
    }
    public void ChooseAttack()
    {
        HideActionUI();
        ShowWeaponUI();
        ChangeStep(Battle_Step.SelectWeapon);
    }
    public void AttackPlayer()
    {
        if (CurrentStep != Battle_Step.SelectAttack)
            return;

        ChangeStep(Battle_Step.TurnEnd);

        List<Vector2Int> attackTiles = battlePreview.AttackTiles;
        
        if (map.HasEnemyTile)
        {
            Vector2Int enemyTile = new Vector2Int(map.EnemyTileX, map.EnemyTileY);

            for (int i = 0; i < attackTiles.Count; i++)
            {
                if (attackTiles[i] != enemyTile)
                    continue;

                int damage = selectWeapon.weaponRD[i].damage;
                SetEnenmyHp(EnemyHp - damage);
                Debug.Log($"적의 남은 체력: {EnemyHp}");
                break;
            }
        }

        battleShow.effectOn(selectWeapon, attackTiles, battlePreview.AttackDirection);

        battlePreview.CleanPreviewColor();
    }

    public void MovePlayer(int x, int y) 
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

        battlePreview.CleanPreviewColor();

    }
    public void MoveAni(Vector3 moveTile)
    {
        StartCoroutine(CoroutineMove(moveTile));
    }
    private IEnumerator CoroutineMove(Vector3 moveTile)
    {
        spriteAnimator.SetState("WalkingOff", true);
        yield return new WaitForSeconds(spriteAnimator.Duration);

        playerTransform.position = moveTile;
         
        spriteAnimator.SetState("WalkingOn", true);
        yield return new WaitForSeconds(spriteAnimator.Duration);

        spriteAnimator.SetState("Idle", true);
        FinishAnim();
    }
    public void FinishAnim() // 애니메이션 재생 종료 후 실행
    {
        ChangeStep(Battle_Step.TurnEnd);
        EndTurn();
    }
    private void EndTurn() // 배틀 쇼의 애니메이션 종료 후 실행
    {
        if (CurrentStep != Battle_Step.TurnEnd)
            return;

        if (isplayerdead && !isenemydead)
            stgch.ChangeStageTo(Stage_change.Stg.Game_Over);
        else if (isenemydead)
            stgch.ChangeStageTo(Stage_change.Stg.stg_CLR);
        
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
        
        selectWeapon = weapon;
        
        HideWeaponUI();
        ChangeStep(Battle_Step.SelectAttack);
    }
    public void ChooseSpear()
    {
        if (CurrentStep != Battle_Step.SelectWeapon)
            return;

        WeaponData weapon = GetWeapon(0, WeaponType.Spear); // 추후 강화 시스템 나오면 레벨 수정 
        battlePreview.PlayerAttackPreview(weapon);

        selectWeapon = weapon;

        HideWeaponUI();
        ChangeStep(Battle_Step.SelectAttack);
    }
    public void ChooseGun()
    {
        if (CurrentStep != Battle_Step.SelectWeapon)
            return;
        WeaponData weapon = GetWeapon(0, WeaponType.Gun);// 추후 강화 시스템 나오면 레벨 수정
        battlePreview.PlayerAttackPreview(weapon);

        selectWeapon = weapon;

        HideWeaponUI();
        ChangeStep(Battle_Step.SelectAttack);
    }
    public void ChooseMagic()
    {
        if (CurrentStep != Battle_Step.SelectWeapon)
            return;
        WeaponData weapon = GetWeapon(0, WeaponType.Magic);// 추후 강화 시스템 나오면 레벨 수정
        battlePreview.PlayerAttackPreview(weapon);

        selectWeapon = weapon;

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
    
    private void ShowPlayerHP()
    {
        PHpTxt.text = $"HP = {PlayerHp}";
    }
    private void ShowEnemyHP()
    {
        EHpTxt.text = $"HP = {EnemyHp}";
    }
    private int SetEnenmyHp(int HP)
    {
        EnemyHp = HP;
        if (EnemyHp < 0)
        {
            EnemyHp = 0;
            isenemydead = true;
        }
            ShowEnemyHP();

        return EnemyHp;
        
    }
    private int SetPlayerHp(int HP)
    {
        PlayerHp = HP;
        if (PlayerHp < 0)
        {
            PlayerHp = 0;
            isplayerdead = true;
        }
        ShowPlayerHP();
        return PlayerHp;
        
    }
}
