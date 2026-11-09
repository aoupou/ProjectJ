using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BattleShow : MonoBehaviour
{
    [SerializeField] SpriteAnimator spriteAnimator;
    [SerializeField] Battle battle;
    [SerializeField] Map map;
    [SerializeField] GameObject WeaponEffect;
    [SerializeField] GameObject Player;

    // 무기별 이펙트 크기 (1 = 씬에 놓은 크기 그대로, 2 = 두 배)
    [SerializeField] float swordSize = 1f;
    [SerializeField] float spearSize = 1f;
    [SerializeField] float gunSize = 1f;
    [SerializeField] float magicSize = 1f;

    Vector3 baseScale; // 씬에 놓은 이펙트 원래 크기

    private void Start()
    {
        baseScale = WeaponEffect.transform.localScale;
    }

    public void effectOn(WeaponData weapon, List<Vector2Int> attackTiles, Vector2Int direction)
    {
        Vector3 sum = Vector3.zero;
        int count = 0;

        foreach (Vector2Int tile in attackTiles)
        {
            if (map.TryGetTileWorldPosition(tile.x, tile.y, out Vector3 tilePosition))
            {
                sum += tilePosition;
                count++;
            }
        }

        Vector3 center = Player.transform.position;
        if (count > 0)
        {
            center = sum / count;
        }

        WeaponEffect.transform.position = new Vector3(center.x, center.y, WeaponEffect.transform.position.z);

        float distance = 0f;
        if (weapon.weaponType != WeaponType.Magic)
        {
            if (direction == Vector2Int.down)
            {
                distance = 180f;
            }
            else if (direction == Vector2Int.right)
            {
                distance = -90f;
            }
            else if (direction == Vector2Int.left)
            {
                distance = 90f;
            }
        }
        WeaponEffect.transform.rotation = Quaternion.Euler(0f, 0f, distance);

        float size = GetSize(weapon.weaponType);
        WeaponEffect.transform.localScale = baseScale * size;

        StartCoroutine(WeaponeffectOn(weapon));
    }

    private IEnumerator WeaponeffectOn(WeaponData weapon)
    {
        WeaponEffect.gameObject.SetActive(true);

        string newWeapon = weapon.weaponType.ToString();

        spriteAnimator.SetState(newWeapon, true);
        yield return new WaitForSeconds(spriteAnimator.Duration);

        WeaponEffect.gameObject.SetActive(false);

        battle.FinishAnim(); 
    }

    private float GetSize(WeaponType type)
    {
        if (type == WeaponType.Sword)
        {
            return swordSize;
        }
        if (type == WeaponType.Spear)
        {
            return spearSize;
        }
        if (type == WeaponType.Gun)
        {
            return gunSize;
        }
        return magicSize;
    }
}
