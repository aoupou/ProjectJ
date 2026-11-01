using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class BattlePreview : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private LayerMask tileLayer;

    public Battle battle;
    public Map map;

    private SpriteRenderer[,] tileFill;
    private TMP_Text[,] tileText;
    private Color[,] originalTileColors;

    private int lastTileX;
    private int lastTileY;
    private bool hasLastTile;

    public Color movePreviewColor;

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
        if (Mouse.current == null || mainCamera == null || map == null)
            return;

        Vector2 mousePosition = Mouse.current.position.ReadValue();
        Ray ray = mainCamera.ScreenPointToRay(mousePosition);

        RaycastHit2D hit =
            Physics2D.GetRayIntersection(ray, Mathf.Infinity, tileLayer);

        if (hit.collider == null)
        {
            hasLastTile = false;
            return;
        }

        if (map.TryGetTileCoordinate(hit.collider, out int tileX, out int tileY))
        {
            if (!hasLastTile || tileX != lastTileX || tileY != lastTileY)
            {
                lastTileX = tileX;
                lastTileY = tileY;
                hasLastTile = true;
            }
        }
        if(Mouse.current.leftButton.wasPressedThisFrame && map.TryGetTileCoordinate(hit.collider, out int x, out int y))
        {
            if (battle.CurrentStep != Battle.Battle_Step.SelectDirection) return;
            battle.Move_Player(x,y);
        }
    }
    public void PlayerAttackPreview()
    {
        ResetPreviewColors();

    }
    public void PlayerMovePreview()
    {
        if (map == null)
            return;

        ResetPreviewColors();
        if (!map.HasPlayerTile)
            return;

        var PlayerMovePower = battle.Moverange();

        Vector2Int playerTile = new Vector2Int(map.PlayerTileX, map.PlayerTileY);

        if (!map.TryGetTile(playerTile.x, playerTile.y, out _))
            return;

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

                if (!tile.Visitable || tile.CanPlayerGo || tile.OnEnemy)
                    continue;
                isVisited[neighborTile.x, neighborTile.y] = true;

                distance[neighborTile.x, neighborTile.y] = currentDistance + 1;

                Paint(neighborTile.x, neighborTile.y);
                queue.Enqueue(neighborTile);
            }
        }
        //오늘의 목표 
        //1. 플레이어 좌표값 받아오기) 
        //2. Queue 자료구조 만들기 ( Queue.Enqueue (playerTile) ) 
        //3. bool 형으로 isVisited[tilex , tiley] 리스트 만들기 - 방문했는지 체크용 
        //4. int 형으로  distance[tilex,tiley] 리스트 만들기 - 멘헤튼 거리 계산용(최대 거리 계산용)  

        // 플레이어 타일 vs 새롭게 탐색할 타일 distance if 형 
        // moverange 보다 작을 경우? -> 탐색 시작 
        // isVisited -> false -> 탐색 시작 
        // playertile을 새롭게 탐색할 타일로 교체 -> 위 행동 반복 
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
    private void Paint(int x, int y)
    {
        if (x < 0 || x >= map.Width ||
            y < 0 || y >= map.Height)
        {
            return;
        }

        if (tileFill[x, y] == null)
            return;

        tileFill[x, y].color = movePreviewColor;
    }

    // 이전 이동 미리보기 색을 원래 타일 색으로 복원
    private void ResetPreviewColors()
    {
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