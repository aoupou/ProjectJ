using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class BattlePreview : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private LayerMask tileLayer;

    public Battle battle;
    public Map map;

    private SpriteRenderer[,] tileFill;
    private TMP_Text[,] tileText;
    private Color[,] originalTileColors;

    private bool[,] reachableTiles;

    public Color movePreviewColor;
    public Color attackPreviewColor;

    // 공격 미리보기용: 지금 고른 무기, 마지막으로 칠한 방향, 마우스가 올라간 칸
    private WeaponData currentWeapon;
    private Vector2Int lastDirection;
    private int lastTileX;
    private int lastTileY;

    private void Awake()
    {
        if (map == null)
        {
            Debug.LogError("BattlePreview에 Map이 지정되지 않았습니다.");
            enabled = false;
            return;
        }

        tileFill = new SpriteRenderer[map.Width, map.Height];
        tileText = new TMP_Text[map.Width, map.Height];
        originalTileColors = new Color[map.Width, map.Height];
    }

    private void Update()
    {
        if (Mouse.current == null || mainCamera == null || map == null || battle == null)
            return;

        RefreshAttackPreview();

        if (!Mouse.current.leftButton.wasPressedThisFrame ||
            battle.CurrentStep != Battle.Battle_Step.SelectDirection)
            return;

        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        RaycastHit2D hit = Physics2D.GetRayIntersection(ray, Mathf.Infinity, tileLayer);

        if (hit.collider == null ||
            !map.TryGetTileCoordinate(hit.collider, out int x, out int y))
            return;

        // Recalculate so changed obstacles and movement power are respected.
        PlayerMovePreview();
        if (!CanMoveTo(x, y))
            return;

        battle.Move_Player(x, y);
    }

    private Vector2Int Rotate(Vector2Int cell, int direction)
    {
        if (direction == 0) 
        {
            Vector2Int directionChangeCell = new Vector2Int(cell.x, cell.y);
            return directionChangeCell;
        }
        else if (direction == 1)
        {
            Vector2Int directionChangeCell = new Vector2Int(-cell.x, -cell.y);
            return directionChangeCell;
        }
        else if (direction == 2)
        {
            Vector2Int directionChangeCell = new Vector2Int(cell.y, -cell.x);
            return directionChangeCell;
        }
        else // (direction == 3)
        {
            Vector2Int directionChangeCell = new Vector2Int(-cell.y, cell.x);
            return directionChangeCell;
        }
        
    }
    private Vector2Int GetMouseDirection(Vector2Int targetTile)
    {
        Vector2Int playerTile = new Vector2Int(map.PlayerTileX, map.PlayerTileY);

        int dx = lastTileX - playerTile.x ;
        int dy = lastTileY - playerTile.y ;

        if (Mathf.Abs(dy) > Mathf.Abs(dx))
        {
            if (dy > 0) { Vector2Int RotateTile = Rotate(targetTile, 0); return RotateTile; }
            else { Vector2Int RotateTile = Rotate(targetTile, 1); return RotateTile; }
        }
        else // dx < dy 
        {
            if (dx > 0) { Vector2Int RotateTile = Rotate(targetTile, 2); return RotateTile; }
            else { Vector2Int RotateTile = Rotate(targetTile, 3); return RotateTile; }
        }
    }

    public void PlayerAttackPreview(WeaponData weapon)
    {
        currentWeapon = weapon;

        ClearMovePreview();

        Vector2Int playerTile = new Vector2Int(map.PlayerTileX, map.PlayerTileY);
        
        if (weapon.weaponType == WeaponType.Magic)
        {
            foreach (AttackCell attackCell in weapon.weaponRD)
            {
                Vector2Int targetTile = attackCell.cell;
                // 채호님이 알아서... 
                Paint(targetTile.x, targetTile.y, attackPreviewColor);
            }
            
        }

        else
        {
            foreach (AttackCell attackCell in weapon.weaponRD)
            {
                Vector2Int targetTile = attackCell.cell;
                Vector2Int getMouseDirection = playerTile + GetMouseDirection(targetTile);

                Paint(getMouseDirection.x, getMouseDirection.y, attackPreviewColor);
            }
        }
    }

    // 마우스가 올라간 칸 갱신
    private void UpdateMouseTile()
    {
        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        RaycastHit2D hit = Physics2D.GetRayIntersection(ray, Mathf.Infinity, tileLayer);

        if (hit.collider != null && map.TryGetTileCoordinate(hit.collider, out int x, out int y))
        {
            lastTileX = x;
            lastTileY = y;
        }
    }

    private void RefreshAttackPreview()
    {
        UpdateMouseTile();

        if (battle.CurrentStep != Battle.Battle_Step.SelectAttack)
            return;
        if (currentWeapon == null)
            return;

        Vector2Int direction = GetMouseDirection(Vector2Int.up);
        if (direction == lastDirection)
            return;

        lastDirection = direction;
        PlayerAttackPreview(currentWeapon);
    }


    public void PlayerMovePreview()
    {
        reachableTiles = CalculateMoveRange();
        ShowMovePreview(reachableTiles);
    }

    public bool CanMoveTo(int x, int y)
    {
        return reachableTiles != null &&
               x >= 0 && x < reachableTiles.GetLength(0) &&
               y >= 0 && y < reachableTiles.GetLength(1) &&
               reachableTiles[x, y] &&
               map != null && map.TryGetTile(x, y, out TileData tile) &&
               tile.Visitable && tile.CanPlayerGo && !tile.OnEnemy;
    }

    public void ClearMovePreview()
    {
        reachableTiles = null;
        ResetPreviewColors();
    }

    // Calculate reachability without changing any visuals.
    private bool[,] CalculateMoveRange()
    {
        if (map == null || battle == null || !map.HasPlayerTile)
            return null;

        var PlayerMovePower = battle.Moverange();
        Vector2Int playerTile = new Vector2Int(map.PlayerTileX, map.PlayerTileY);

        if (!map.TryGetTile(playerTile.x, playerTile.y, out _))
            return null;

        Vector2Int[] directions =
        {
            new Vector2Int (0,1),
            new Vector2Int (0,-1), 
            new Vector2Int (-1,0), 
            new Vector2Int (1,0), 
        };

        int[,] distance = new int[map.Width, map.Height];
        bool[,] isVisited = new bool [map.Width, map.Height];


        Queue<Vector2Int> queue = new Queue<Vector2Int>();

        distance[playerTile.x, playerTile.y] = 0;
        isVisited[playerTile.x, playerTile.y] = true;
        queue.Enqueue(playerTile); // eu1 

        while (queue.Count > 0)
        {

            Vector2Int startTile = queue.Dequeue();
            int currentDistance = distance[startTile.x, startTile.y];

            // 이동력을 모두 사용한 타일에서는 더 탐색하지 않음.
            if (currentDistance >= PlayerMovePower)
                continue;

            for (int i = 0; i < directions.Length; i++)
            {
                Vector2Int neighborTile = startTile + directions[i];

                if(!map.TryGetTile(neighborTile.x, neighborTile.y, out TileData tile))
                {
                    continue;
                }

                if (isVisited[neighborTile.x, neighborTile.y])
                    continue;

                // 적은 탐색을 막지 않는다. 지형의 이동 불가 설정만 검사한다.
                if (!tile.Visitable || !tile.CanPlayerGo)
                    continue;
                isVisited[neighborTile.x, neighborTile.y] = true;

                distance[neighborTile.x, neighborTile.y] = currentDistance + 1;


                queue.Enqueue(neighborTile);
            }
        }
        // The starting tile is visited during BFS but is not a destination.
        isVisited[playerTile.x, playerTile.y] = false;
        return isVisited;
    }

    // Visuals only: display the calculated movement range.
    private void ShowMovePreview(bool[,] moveRange)
    {
        ResetPreviewColors();
        if (moveRange == null)
            return;

        for (int x = 0; x < moveRange.GetLength(0); x++)
        {
            for (int y = 0; y < moveRange.GetLength(1); y++)
            {
                // 적 타일은 탐색에 포함하지만 색칠과 이동 목적지에서는 제외한다.
                if (moveRange[x, y] && CanMoveTo(x, y))
                    Paint(x, y, movePreviewColor);
            }
        }
    }

    public void AddTile(
        int x,
        int y,
        SpriteRenderer fill,
        TMP_Text tileDamage)
    {
        tileFill[x, y] = fill;
        tileText[x, y] = tileDamage;
        originalTileColors[x, y] = fill.color;
    }

    // 배열 범위 안에 있는 타일만 색칠
    private void Paint(int x, int y, Color color)
    {
        if (x < 0 || x >= map.Width ||
            y < 0 || y >= map.Height)
        {
            return;
        }

        if (tileFill[x, y] == null)
            return;

        tileFill[x, y].color = color;
    }

    // 이전 이동 미리보기 색을 원래 타일 색으로 복원
    private void ResetPreviewColors()
    {
        if (tileFill == null)
            return;

        for (int x = 0; x < map.Width; x++)
        {
            for (int y = 0; y < map.Height; y++)
            {
                if (tileFill[x, y] != null)
                    tileFill[x, y].color = originalTileColors[x, y];
            }
        }
    }
}
