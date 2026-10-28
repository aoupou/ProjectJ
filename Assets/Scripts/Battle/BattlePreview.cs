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
    public void PlayerMovePreview()
    {
        if (map == null || !map.HasPlayerTile)
            return;
        ResetPreviewColors();
        //var PlayerMovePower = battle.Moverange(); // 나중에 바꾸기 
        int PlayerMovePower = 5;

        var playerX = map.PlayerTileX;
        var playerY = map.PlayerTileY;
        Vector2Int playerTile = new Vector2Int(playerX, playerY);

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

        int[,] distance = new int[map.Width, map.Height];


        bool[,] isVisited = new bool [map.Width, map.Height];
        isVisited[playerTile.x, playerTile.y] = true;


        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        
        queue.Enqueue(playerTile); // eu1 
        while (queue.Count > 0)
        {

            Vector2Int startTile = queue.Dequeue();
            distance[playerTile.x, playerTile.y] = 0;

            for (int directionsCount=0; directionsCount < directions.Length; directionsCount++)
            {
                neighborTile = startTile + directions[directionsCount];
                distance[neighborTile.x, neighborTile.y] = distance[startTile.x, startTile.y]  + 1;

                if ( distance[neighborTile.x,neighborTile.y] <= PlayerMovePower 
                    && isVisited[neighborTile.x,neighborTile.y] == false 
                    && map.[neighborTile.x,neighborTile.y] == true)
                {
                    isVisited[neighborTile.x,neighborTile.y] = true;
                    Paint(neighborTile.x, neighborTile.y);
                    queue.Enqueue(neighborTile);
                }
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