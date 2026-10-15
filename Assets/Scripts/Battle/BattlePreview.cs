using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

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
    }

    // 플레이어 기준 맨해튼 거리 1인 타일 네 칸을 칠함
    public void PlayerMove()
    {
        if (map == null || !map.HasPlayerTile)
            return;

        ResetPreviewColors();

        int playerX = map.PlayerTileX;
        int playerY = map.PlayerTileY;

        Paint(playerX + 1, playerY); // 오른쪽
        Paint(playerX - 1, playerY); // 왼쪽
        Paint(playerX, playerY + 1); // 위쪽
        Paint(playerX, playerY - 1); // 아래쪽
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