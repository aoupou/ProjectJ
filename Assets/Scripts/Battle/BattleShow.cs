using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BattleShow : MonoBehaviour
{
    [SerializeField] SpriteAnimator spriteAnimator;
    [SerializeField] Battle battle;

    // 무기 공격 이펙트: 상태 이름 = WeaponType 이름(Sword/Spear/Gun/Magic)
    // 비워두면 Resources/Animations/WeaponEffectCollection 을 자동으로 불러옴
    [SerializeField] private SpriteAnimationCollection weaponEffectCollection;
    // 이펙트 크기 배율 (1 = 공격 범위 크기에 맞춤). 무기마다 따로 조절
    // 공격할 때마다 읽어서, 플레이 중에 바꿔도 다음 공격부터 바로 반영됨
    [Header("무기 이펙트 크기 배율 (1 = 공격 범위에 딱 맞춤)")]
    [SerializeField, Range(0.5f, 1.5f)] private float swordEffectSize = 1f;
    [SerializeField, Range(0.5f, 1.5f)] private float spearEffectSize = 1f;
    [SerializeField, Range(0.5f, 1.5f)] private float gunEffectSize = 1f;
    [SerializeField, Range(0.5f, 1.5f)] private float magicEffectSize = 0.8f;
    [SerializeField] private int weaponEffectSortingOrder = 100; // 이펙트 그리는 순서 (클수록 위)
    private SpriteAnimator weaponEffect;

    private Map map => battle.BattleMap;
    private Transform playerTransform => battle.PlayerTransform;

    private void Awake()
    {
        if (battle == null)
            battle = GetComponent<Battle>();
    }

    private IEnumerator moveRoutineOn()
    {
        spriteAnimator.SetState("WalkingOn", true);
        yield return new WaitForSeconds(spriteAnimator.Duration);

        spriteAnimator.SetState("Idle", true);
        battle.Finish_Anim();
    }
    private IEnumerator moveRoutineOff()
    {
        spriteAnimator.SetState("WalkingOff", true);
        yield return new WaitForSeconds(spriteAnimator.Duration);

        spriteAnimator.SetState("Idle", true);
        battle.Finish_Anim();
    }
    public void MoveAniOn()
    {
        StartCoroutine(moveRoutineOn());
    }
    public void MoveAniOff()
    {
        StartCoroutine(moveRoutineOff());
    }
    // 공격 연출: Battle.Attack_Player()가 부름. 끝나면 battle.Finish_Anim()
    public void AttackAni(WeaponType type, Vector2Int direction, List<Vector2Int> attackTiles)
    {
        StartCoroutine(CoroutineAttack(type, direction, attackTiles));
    }
    private IEnumerator CoroutineAttack(WeaponType type, Vector2Int direction, List<Vector2Int> attackTiles)
    {
        SpriteAnimationData anim = GetWeaponEffect(type);
        if (anim == null)
        {
            battle.Finish_Anim();
            yield break;
        }

        SpriteAnimator effect = GetWeaponEffectObject();

        // 공격 범위(빨간 칸) 한가운데에 놓음. 범위가 없으면 플레이어 앞 한 칸
        if (attackTiles == null || attackTiles.Count == 0)
            attackTiles = new List<Vector2Int> { new Vector2Int(map.PlayerTileX, map.PlayerTileY) + direction };

        Vector2 min = attackTiles[0];
        Vector2 max = attackTiles[0];
        foreach (Vector2Int tile in attackTiles)
        {
            min = Vector2.Min(min, tile);
            max = Vector2.Max(max, tile);
        }
        Vector2 center = (min + max) * 0.5f;
        float rangeWidth = max.x - min.x + 1f;  // 범위 가로 칸 수
        float rangeHeight = max.y - min.y + 1f; // 범위 세로 칸 수

        effect.transform.position = new Vector3(
            map.MapStartX + map.TileWidthSize * (center.x + 0.5f),
            map.MapStartY + map.TileHeightSize * (center.y + 0.5f),
            playerTransform.position.z);

        // 칼/창/총은 공격 방향으로 회전 (그림이 위쪽 기준이라고 가정), 마법은 그대로
        float angle = 0f;
        if (type != WeaponType.Magic)
            angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        effect.transform.rotation = Quaternion.Euler(0f, 0f, angle);

        // 옆으로 돌린 경우 그림의 가로/세로가 범위의 세로/가로에 대응
        if (angle != 0f && direction.x != 0)
        {
            float temp = rangeWidth;
            rangeWidth = rangeHeight;
            rangeHeight = temp;
        }

        // 시트마다 해상도/PPU가 달라서 첫 프레임 기준으로 범위 크기에 맞춤
        effect.transform.localScale = Vector3.one;
        if (anim.sprites.Length > 0 && anim.sprites[0] != null)
        {
            // 그림이 범위 상자(가로 x 세로) 안에 들어가도록 둘 중 작은 배율 사용
            Vector2 size = anim.sprites[0].bounds.size;
            if (size.x > 0f && size.y > 0f)
            {
                float fitX = rangeWidth * map.TileWidthSize / size.x;
                float fitY = rangeHeight * map.TileHeightSize / size.y;
                effect.transform.localScale = Vector3.one * (GetWeaponEffectSize(type) * Mathf.Min(fitX, fitY));
            }
        }

        effect.gameObject.SetActive(true);
        effect.Play(anim, type.ToString());
        yield return new WaitForSeconds(effect.Duration);

        effect.gameObject.SetActive(false);
        battle.Finish_Anim();
    }
    private float GetWeaponEffectSize(WeaponType type)
    {
        switch (type)
        {
            case WeaponType.Sword: return swordEffectSize;
            case WeaponType.Spear: return spearEffectSize;
            case WeaponType.Gun: return gunEffectSize;
            case WeaponType.Magic: return magicEffectSize;
        }
        return 1f;
    }
    private SpriteAnimationData GetWeaponEffect(WeaponType type)
    {
        if (weaponEffectCollection == null)
            weaponEffectCollection = Resources.Load<SpriteAnimationCollection>("Animations/WeaponEffectCollection");
        if (weaponEffectCollection == null)
        {
            Debug.LogWarning("무기 이펙트 컬렉션이 없습니다.");
            return null;
        }

        SpriteAnimationData anim = weaponEffectCollection.Get(type.ToString());
        if (anim == null || anim.sprites == null || anim.sprites.Length == 0)
        {
            Debug.LogWarning(type + " 이펙트 애니메이션이 컬렉션에 없습니다.");
            return null;
        }
        return anim;
    }
    // 이펙트 오브젝트는 하나만 만들어 두고 켜고 끄면서 재사용
    private SpriteAnimator GetWeaponEffectObject()
    {
        if (weaponEffect != null)
            return weaponEffect;

        GameObject obj = new GameObject("WeaponEffect");
        SpriteRenderer effectRenderer = obj.AddComponent<SpriteRenderer>();
        weaponEffect = obj.AddComponent<SpriteAnimator>();

        // 플레이어/적/타일보다 위에 그려지게 (데미지 숫자는 ui 레이어라 이것보다 위에 남음)
        effectRenderer.sortingLayerName = "Default";
        effectRenderer.sortingOrder = weaponEffectSortingOrder;

        obj.SetActive(false);
        return weaponEffect;
    }
}
