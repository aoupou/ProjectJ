using UnityEngine;

public class enemy_movement : MonoBehaviour
{
    [SerializeField] private Battle battle;
    [SerializeField] private Map map;
    [SerializeField] private Vector2 positionInTile = new Vector2(0.5f, 0.5f);
    [SerializeField] private float ESize;
    [SerializeField] private Player player;
    [SerializeField, Range(0f, 1f)]
    private float attackChance = 0.3f;
    private Vector3 playerPosition;

    private void Start()
    {
        SetSize();
        SetPosition();

    }



    private void SetPosition()
    {
        if (!map.HasEnemyTile)
        {
            Debug.LogError("Map에 적 시작 타일이 없습니다.");
            return;
        }

        if (!map.TryGetEnemyWorldPosition(out Vector3 worldPosition))
        {
            Debug.LogError("적 타일의 월드 좌표를 가져오지 못했습니다.");
            return;
        }

        worldPosition += new Vector3(positionInTile.x, positionInTile.y, 0f);
        transform.position = worldPosition;
    }

    private void SetSize()
    {
        transform.localScale = new Vector3(
            map.TileWidthSize * ESize,
            map.TileHeightSize * ESize,
            1
        );
    }
}