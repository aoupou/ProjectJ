using UnityEngine;
using System.Collections.Generic;

public class EnemyAi : MonoBehaviour
{
    [SerializeField] CharacterData characterData;
    [SerializeField] Map map;
    [SerializeField] GameObject enemy;
    [SerializeField] Battle battle;

    int moveRangePower;
    int aggression;
    int distance;
    
    int playerX;
    int playerY;

    int enemyX;
    int enemyY;

    int epDistanceX;
    int epDistanceY;

    bool enemyCanMove = true;
    EnemySkillData attackskill;
    List <EnemySkillData> enemySkills;
    List<EnemySkillData> ableEnemySkills = new List<EnemySkillData>();
    private readonly Dictionary<EnemySkillData, (int direction, AttackCell cell)> skillHits 
        = new Dictionary<EnemySkillData, (int direction, AttackCell cell)> ();
    private void Start()
    {
        moveRangePower = characterData.moveRange;
        aggression = characterData.aggression;
        distance = characterData.distance;
        enemySkills = EnemySkillDatabase.OfOwner(characterData);
    }
    public void Active()
    {
        playerX = map.PlayerTileX;
        playerY = map.PlayerTileY;

        enemyX = enemyTile().x;
        enemyY = enemyTile().y;

        attackskill = null;
        enemyCanMove = true;

        FindAbleSkill(); // b  
        if (ableEnemySkills.Count > 0) //b-1 
        {
            bool aggaressTest = AggressionTest();

            if (aggaressTest == true && ableEnemySkills.Count == 1) //c-2
            {
                attackskill = ableEnemySkills[0];
            }
            
            else if (aggaressTest == true) //c-1 
            {
                attackskill = AtpTest(ableEnemySkills);
            }

            else //aggressionTest == false  //c-3
            {
                enemyCanMove = TryMoveEnemy(DistanceMove());

                if (!enemyCanMove)
                {
                    attackskill = AtpTest(ableEnemySkills);
                }
            }
        }

        else // (ableEnemySkills.Count == 0) //b-2 
        {
            enemyCanMove = TryMoveEnemy(DistanceMove());
        }
        if (attackskill == null || !skillHits.TryGetValue(attackskill, out var hit))
            return;

        int targetX = map.PlayerTileX - enemyX;
        int targetY = map.PlayerTileY - enemyY;

        foreach (AttackCell attackCell in attackskill.skillRD)
        {
            Vector2Int rotatedCell = Rotate(
                attackCell.cell.x, attackCell.cell.y, hit.direction);

            if (rotatedCell.x != targetX || rotatedCell.y != targetY)
                continue;

            battle.AttackEnemy(attackCell.damage);
            return;
        }

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

    void FindAbleSkill()
    {
        ableEnemySkills.Clear();
        skillHits.Clear();

        int targetX = playerX - enemyX;
        int targetY = playerY - enemyY;

        foreach (EnemySkillData skill in enemySkills)
        {
            for (int direction = 0; direction < 4; direction++)
            {
                foreach (AttackCell attackCell in skill.skillRD)
                {
                    Vector2Int rotatedCell = Rotate(
                        attackCell.cell.x, attackCell.cell.y, direction);

                    if (rotatedCell.x != targetX || rotatedCell.y != targetY)
                        continue;

                    ableEnemySkills.Add(skill);
                    skillHits[skill] = (direction, attackCell);
                    break;
                }

                if (skillHits.ContainsKey(skill))
                    break;
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

    //Vector2Int EPDirection()
    //{
    //    epDistanceX = playerX - enemyX;
    //    epDistanceY = playerY - enemyY;
    //    Vector2Int distanceTile = new Vector2Int(epDistanceX, epDistanceY);

    //    if (Mathf.Abs(distanceTile.x) <= Mathf.Abs(distanceTile.y))
    //    {
    //        if (distanceTile.y > 0)// up
    //            return new Vector2Int (0, 1 );

    //        else //(distanceTile.y < 0)
    //            return new Vector2Int(0, - 1); // down
    //    }
    //    else //(Mathf.Abs(distanceTile.x) > Mathf.Abs(distanceTile.y))
    //    {
    //        if (distanceTile.x > 0)
    //            return new Vector2Int(1,0); // right 
            
    //        else
    //            return new Vector2Int(-1, 0); ; // left
    //    }
    //}

    //Vector2Int moveTest()
    //{
    //    Vector2Int enemyTile = DistanceMove();

    //    if (map.CanMove(enemyTile.x, enemyTile.y) == true)
    //        return new Vector2Int(enemyTile.x, enemyTile.y);

    //    else
    //        return new Vector2Int(enemyTile.x, enemyTile.y);
    //}

    Vector2Int DistanceMove()
    {
        List <Vector2Int> ableMoveTile = new List <Vector2Int>();

        Vector2Int enemyTile = new Vector2Int(enemyX, enemyY);
        ableMoveTile.Add(enemyTile);

        int neighborTileX = 0;
        int neighborTileY = 0;
        Vector2Int neighborTile = new Vector2Int(neighborTileX, neighborTileY);

        Vector2Int[] directions =
        {
            new Vector2Int (0,1),
            new Vector2Int (0,-1),
            new Vector2Int (-1,0),
            new Vector2Int (1,0),
        };

        int[,] distance1 = new int[map.Width, map.Height];

        bool[,] isVisited = new bool[map.Width, map.Height];
        isVisited[enemyTile.x, enemyTile.y] = true;

        Queue<Vector2Int> queue = new Queue<Vector2Int>();

        queue.Enqueue(enemyTile); // eu1 
        while (queue.Count > 0)
        {
            Vector2Int startTile = queue.Dequeue();
            distance1[enemyTile.x, enemyTile.y] = 0;

            for (int directionsCount = 0; directionsCount < directions.Length; directionsCount++)
            {
                neighborTile = startTile + directions[directionsCount];
                
                if (!map.TryGetTile(neighborTile.x, neighborTile.y, out TileData tile))
                    continue;

                if (!tile.Visitable || !tile.CanEnemyGo || tile.OnPlayer)
                    continue;

                if (!isVisited[neighborTile.x, neighborTile.y]
                    && distance1[startTile.x, startTile.y] + 1 <= moveRangePower)
                {
                    distance1[neighborTile.x, neighborTile.y] = distance1[startTile.x, startTile.y] + 1;
                    isVisited[neighborTile.x, neighborTile.y] = true;
                    queue.Enqueue(neighborTile);
                    ableMoveTile.Add(neighborTile);
                }
            }
        }

        List<Vector2Int> bestTiles = new List<Vector2Int>();
        int bestDifference = int.MaxValue;

        foreach (Vector2Int tile in ableMoveTile)
        {
            int distanceToPlayer = Mathf.Abs(playerX - tile.x)
                                 + Mathf.Abs(playerY - tile.y);
            int difference = Mathf.Abs(distanceToPlayer - distance);

            if (difference > bestDifference)
                continue;

            if (difference < bestDifference)
            {
                bestDifference = difference;
                bestTiles.Clear();
            }

            bestTiles.Add(tile);
        }

        return bestTiles[Random.Range(0, bestTiles.Count)];

        //{
        //    for (int i = 0; i < moveRangePower; i++)
        //    {
        //        Vector2Int direction = EPDirection();

        //        if (distance > Mathf.Abs(epDistanceX) + Mathf.Abs(epDistanceY))
        //        {
        //            enemyX -= direction.x;
        //            enemyY -= direction.y;
        //        }
        //        else if (distance < Mathf.Abs(epDistanceX) + Mathf.Abs(epDistanceY))
        //        {
        //            enemyX += direction.x;
        //            enemyY += direction.y;
        //        }

        //        else if (distance == Mathf.Abs(epDistanceX) + Mathf.Abs(epDistanceY))
        //        {
        //            if (distance <= 2)
        //            {
        //                enemyX += direction.x;
        //                enemyY += direction.y;
        //            }

        //            else if (distance >= 3)
        //            {
        //                enemyX -= direction.x;
        //                enemyY -= direction.y;
        //            }
        //            break;
        //        }
        //        else
        //            break;
        //    }
        //    return new Vector2Int(enemyX, enemyY);  
    }

    EnemySkillData AtpTest(List <EnemySkillData> list)
    {
        int fullAtp = 0;
        int realAtp = 0;

        for ( int i = 0;  i < list.Count;i++) 
        {
            fullAtp += list[i].ATP; 
        }
        
        int randomint = Random.Range(0,fullAtp);

        for (int i = 0; i < list.Count; i++)
        {
            realAtp += list[i].ATP;
            
            if (randomint < realAtp)
            {
                return list[i];
            }
        }
        return list[0];
    }
    private bool TryMoveEnemy(Vector2Int destination)
    {
        int startX = map.EnemyTileX;
        int startY = map.EnemyTileY;

        if (destination.x == enemyX && destination.y == enemyY)
            return false;
       
        battle.MoveEnemy(destination.x, destination.y);

        enemyX = map.EnemyTileX;
        enemyY = map.EnemyTileY;

        return enemyX != startX || enemyY != startY;
    }
}
