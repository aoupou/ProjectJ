using UnityEngine;

public class enemy_movement : MonoBehaviour
{
    [SerializeField] private Battle battle;
    [SerializeField] private Map map;
    [SerializeField] private float ESize;
    [SerializeField] private Player player;
    [SerializeField, Range(0f, 1f)]
    private float attackChance = 0.3f;
    private Vector3 playerPosition;

    private void Start()
    {
        FindPlayer();

        SetPosition();
        SetSize();
    }

    private void FindPlayer()
    {
        playerPosition = player.transform.position;
    }

    public void SelectAction()
    {
        FindPlayer();

        if (IsPlayerNearby())
        {
            float random = Random.value;

            if (random < attackChance)
            {
                Attack();
                return;
            }
        }

        Vector2 direction = ChasePlayer();

        Move(direction);
    }
    private void Attack()
    {
        // TODO: 새 Battle에 적 공격 들어오면 연결 (예전 코드는 Heritage/Old_Battle.cs의 EnemyAttack)
        // battle.EnemyAttack();
    }
    private Vector2 ChasePlayer()
    {
        float xDistance =
            playerPosition.x - transform.position.x;

        float yDistance =
            playerPosition.y - transform.position.y;

        Vector2 direction;

        if (Mathf.Abs(xDistance) > Mathf.Abs(yDistance))
        {
            if (xDistance > 0)
                direction = Vector2.right;
            else
                direction = Vector2.left;
        }
        else
        {
            if (yDistance > 0)
                direction = Vector2.up;
            else
                direction = Vector2.down;
        }

        return direction;
    }

    private void Move(Vector2 direction)
    {
        Vector3 targetPosition;

        if (direction == Vector2.left ||
            direction == Vector2.right)
        {
            targetPosition =
                transform.position +
                new Vector3(
                    direction.x * map.TileWidthSize,
                    0,
                    0
                );
        }
        else
        {
            targetPosition =
                transform.position +
                new Vector3(
                    0,
                    direction.y * map.TileHeightSize,
                    0
                );
        }

        // 이동할 위치가 플레이어와 겹치면
        // 플레이어 방향을 제외하고 무작위 방향 선택
        if (targetPosition == player.transform.position)
        {
            direction = RandomDirection();
        }

        if (direction == Vector2.left ||
            direction == Vector2.right)
        {
            transform.position +=
                new Vector3(
                    direction.x * map.TileWidthSize,
                    0,
                    0
                );
        }
        else
        {
            transform.position +=
                new Vector3(
                    0,
                    direction.y * map.TileHeightSize,
                    0
                );
        }
    }

    private Vector2 RandomDirection()
    {
        Vector2 direction;

        do
        {
            int random = Random.Range(0, 4);

            switch (random)
            {
                case 0:
                    direction = Vector2.up;
                    break;

                case 1:
                    direction = Vector2.down;
                    break;

                case 2:
                    direction = Vector2.left;
                    break;

                default:
                    direction = Vector2.right;
                    break;
            }

        } while (IsPlayerDirection(direction));

        return direction;
    }

    private bool IsPlayerDirection(Vector2 direction)
    {
        Vector3 targetPosition;

        if (direction == Vector2.left ||
            direction == Vector2.right)
        {
            targetPosition =
                transform.position +
                new Vector3(
                    direction.x * map.TileWidthSize,
                    0,
                    0
                );
        }
        else
        {
            targetPosition =
                transform.position +
                new Vector3(
                    0,
                    direction.y * map.TileHeightSize,
                    0
                );
        }

        return targetPosition == player.transform.position;
    }

    private void SetPosition()
    {
        int random_x;
        int random_y;

        float enemy_x;
        float enemy_y;

        int tryCount = 0;

        do
        {
            random_x = Random.Range(0, map.Width);
            random_y = Random.Range(0, map.Height);

            enemy_x =
                map.MapStartX
                + (map.TileWidthSize / 2)
                + (map.TileWidthSize * random_x);

            enemy_y =
                map.MapStartY
                + (map.TileHeightSize / 2)
                + (map.TileHeightSize * random_y);

            tryCount++;

        } while (IsNearPlayer(enemy_x, enemy_y) && tryCount < 100);

        transform.position =
            new Vector3(enemy_x, enemy_y, 1);
    }

    private bool IsNearPlayer(float enemy_x, float enemy_y)
    {
        return Mathf.Abs(enemy_x - playerPosition.x) <= map.TileWidthSize &&
               Mathf.Abs(enemy_y - playerPosition.y) <= map.TileHeightSize;
    }

    private void SetSize()
    {
        transform.localScale = new Vector3(
            map.TileWidthSize * ESize,
            map.TileHeightSize * ESize,
            1
        );
    }
    private bool IsPlayerNearby()
    {
        float xDistance =
            Mathf.Abs(player.transform.position.x - transform.position.x);

        float yDistance =
            Mathf.Abs(player.transform.position.y - transform.position.y);

        return xDistance <= map.TileWidthSize &&
               yDistance <= map.TileHeightSize;
    }
}