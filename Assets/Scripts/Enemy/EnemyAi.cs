using UnityEngine;
using System.Collections.Generic;

public class EnemyAi : MonoBehaviour
{
    [SerializeField] CharacterData characterData;
    [SerializeField] Map map;
    [SerializeField] GameObject enemy;

    int moveRangePower;
    int aggression;
    int distance;
    
    int playerX;
    int playerY;

    int enemyX;
    int enemyY;

    int epDistanceX;
    int epDistanceY;
    List <EnemySkillData> enemySkills;
    List<EnemySkillData> ableEnemySkills = new List<EnemySkillData>();
    private void Start()
    {
        moveRangePower = characterData.moveRange;
        aggression = characterData.aggression;
        distance = characterData.distance;
        enemySkills = EnemySkillDatabase.OfOwner(characterData);
    }

    public Vector2Int enemyTile() // 나중에 map으로 옮기기? 
    {
        float floatEnemyTileX = (enemy.transform.position.x - map.MapStartX) / map.TileWidthSize;
        float floatEnemyTileY = (enemy.transform.position.y - map.MapStartY) / map.TileHeightSize;

        int enemyTileX = (int)floatEnemyTileX;
        int enemyTileY = (int)floatEnemyTileY;

        return new Vector2Int (enemyTileX, enemyTileY);
    }
    Vector2Int Rotate (int x, int y, int direction) // 어디론가 나중에 옮기긴 해야함 ㅇㅇ 
    {
        switch(direction) 
        {
            case 0: // up
                return new Vector2Int (x, y); 
            case 1: // down
                return new Vector2Int (-x, -y); 
            case 2: // right 
                return new Vector2Int(y, -x); 
            case 3: // left  
                return new Vector2Int (-y, x); 
        }
        return new Vector2Int(0, 0);
    }

    void Active()
    {
        playerX = map.PlayerTileX;
        playerY = map.PlayerTileY;

        enemyX = enemyTile().x;
        enemyY = enemyTile().y;
    }

    void FindAbleSkill()
    {
        ableEnemySkills.Clear();
        foreach (EnemySkillData skillTiles in enemySkills)
        {
            for (int i =0; i < 4;  i++)
            {
                foreach (AttackCell attackCell in skillTiles.skillRD)
                {
                    Vector2Int outAttackCell = Rotate(attackCell.cell.x, attackCell.cell.y, i);
                    if (outAttackCell.x == (playerX - enemyX)  && outAttackCell.y == (playerY - enemyY))
                    {
                        if (!ableEnemySkills.Contains(skillTiles))
                        {
                            ableEnemySkills.Add(skillTiles);
                            break;
                        }
                    }
                }
            }
        }
    }

    bool AggressionTest()
    {
        int RandomInt = Random.Range(0, 100);

        if (aggression <= RandomInt) 
            return false;

        else //aggression > RandomInt
            return true;
    }

    Vector2Int EPDirection()
    {
        epDistanceX = playerX - enemyX;
        epDistanceY = playerY - enemyY;
        Vector2Int distanceTile = new Vector2Int(epDistanceX, epDistanceY);

        if (Mathf.Abs(distanceTile.x) <= Mathf.Abs(distanceTile.y))
        {
            if (distanceTile.y > 0)// up

                return new Vector2Int (0, 1 );
            else //(distanceTile.y < 0)
                return new Vector2Int(0, - 1); // down
        }
        else //(Mathf.Abs(distanceTile.x) > Mathf.Abs(distanceTile.y))
        {
            if (distanceTile.x > 0)
                return new Vector2Int(1,0); // right 
            else
                return new Vector2Int(-1, 0); ; // left
        }
    }

    Vector2Int DistanceMove()
    {
        for (int i = 0; i < moveRangePower; i++)
        {
            Vector2Int direction = EPDirection();
            if (distance > Mathf.Abs(epDistanceX) + Mathf.Abs(epDistanceY))
            {
                enemyX -= direction.x;
                enemyY -= direction.y;
            }
            else if (distance < Mathf.Abs(epDistanceX) + Mathf.Abs(epDistanceY))
            {
                enemyX += direction.x;
                enemyY += direction.y;
            }
            else
                break;
        }
        return new Vector2Int(enemyX, enemyY);
    }

    EnemySkillData AtpTest()
    {
        return enemySkills[0];
    }
}
