using UnityEngine;
using TMPro;
using System.Collections.Generic;

[DefaultExecutionOrder(-100)]
public class Map : MonoBehaviour
{
    [SerializeField] private MapData mapDataAsset;

    private TileData[,] mapData;

    public GameObject MapSize;
    public GameObject TilePrefab;

    public float TileWidthSize;
    public float TileHeightSize;

    public float MapStartX;
    public float MapStartY;

    private int playerTileX = -1;
    private int playerTileY = -1;
    private bool hasPlayerTile;

    public int PlayerTileX => playerTileX;
    public int PlayerTileY => playerTileY;
    public bool HasPlayerTile => hasPlayerTile;

    private BattlePreview tileColor;

    // 타일 콜라이더로 해당 타일의 데이터를 찾음
    private Dictionary<Collider2D, TileData> tileDataByCollider =
        new Dictionary<Collider2D, TileData>();

    public Color CheckerColor = new Color(0.25f, 0.25f, 0.3f, 0.07f);
    public Color FrameColor = new Color(0.2f, 0.2f, 0.25f, 0.9f);
    public float FrameThickness = 0.08f;

    public int Width => mapDataAsset.width;
    public int Height => mapDataAsset.height;

    private void Awake()
    {
        if (mapDataAsset == null || MapSize == null)
        {
            Debug.LogError("MapData 또는 MapSize가 지정되지 않았습니다.");
            enabled = false;
            return;
        }

        int width = mapDataAsset.width;
        int height = mapDataAsset.height;

        if (width <= 0 || height <= 0)
        {
            Debug.LogError("맵의 가로와 세로 크기는 1 이상이어야 합니다.");
            enabled = false;
            return;
        }

        TileWidthSize = MapSize.transform.localScale.x / width;
        TileHeightSize = MapSize.transform.localScale.y / height;

        MapStartX =
            MapSize.transform.position.x - MapSize.transform.localScale.x / 2f;

        MapStartY =
            MapSize.transform.position.y - MapSize.transform.localScale.y / 2f;

        mapData = new TileData[width, height];

        // MapData 에셋의 타일 데이터를 런타임 배열로 복사
        if (mapDataAsset.tiles != null)
        {
            foreach (TileData source in mapDataAsset.tiles)
            {
                if (source == null)
                    continue;

                if (source.x < 0 || source.x >= width ||
                    source.y < 0 || source.y >= height)
                {
                    Debug.LogWarning(
                        $"잘못된 타일 좌표입니다: ({source.x}, {source.y})"
                    );
                    continue;
                }

                mapData[source.x, source.y] = new TileData
                {
                    x = source.x,
                    y = source.y,

                    // 플레이어 시작 위치는 아래에서 무작위로 지정
                    OnPlayer = false,

                    // 기존 적 위치 정보는 유지
                    OnEnemy = source.OnEnemy
                };
            }
        }

        // 플레이어가 배치될 타일의 x, y를 무작위로 선택
        if (TryGetRandomPlayerTile(out int startX, out int startY))
        {
            SetPlayerTile(startX, startY);
        }
        else
        {
            Debug.LogError("플레이어를 배치할 수 있는 타일이 없습니다.");
        }

        // 맵 크기 지정용 오브젝트를 게임 중 숨김
        SpriteRenderer mapSizeRenderer =
            MapSize.GetComponent<SpriteRenderer>();

        if (mapSizeRenderer != null)
            mapSizeRenderer.enabled = false;
    }

    private void Start()
    {
        if (TilePrefab == null)
        {
            Debug.LogError("TilePrefab이 지정되지 않았습니다.");
            return;
        }

        tileColor = FindObjectOfType<BattlePreview>();

        CreateTiles();
        CreateFrame();
    }

    // 실제 배열에 존재하는 타일 중 하나를 무작위로 선택
    private bool TryGetRandomPlayerTile(out int x, out int y)
    {
        x = -1;
        y = -1;

        List<TileData> availableTiles = new List<TileData>();

        for (int tileX = 0; tileX < Width; tileX++)
        {
            for (int tileY = 0; tileY < Height; tileY++)
            {
                if (mapData[tileX, tileY] != null)
                    availableTiles.Add(mapData[tileX, tileY]);
            }
        }

        if (availableTiles.Count == 0)
            return false;

        TileData selectedTile =
            availableTiles[Random.Range(0, availableTiles.Count)];

        x = selectedTile.x;
        y = selectedTile.y;
        return true;
    }

    // 플레이어 타일 좌표와 OnPlayer 표시를 함께 갱신
    public bool SetPlayerTile(int newX, int newY)
    {
        if (!IsValidTile(newX, newY))
            return false;

        if (hasPlayerTile)
            mapData[playerTileX, playerTileY].OnPlayer = false;

        playerTileX = newX;
        playerTileY = newY;

        mapData[playerTileX, playerTileY].OnPlayer = true;
        hasPlayerTile = true;

        return true;
    }

    private bool IsValidTile(int x, int y)
    {
        return mapData != null &&
               x >= 0 && x < Width &&
               y >= 0 && y < Height &&
               mapData[x, y] != null;
    }

    private void CreateTiles()
    {
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                if (mapData[x, y] != null)
                    CreateTile(x, y);
            }
        }
    }

    private void CreateTile(int x, int y)
    {
        GameObject tile = Instantiate(TilePrefab);

        SetTileTransform(tile.transform, x, y);
        RegisterTileCoordinate(tile, x, y);
        SetupTileVisuals(tile, x, y);
    }

    // 타일을 배치하면서 월드 좌표를 해당 TileData에 저장
    private void SetTileTransform(Transform tileTransform, int x, int y)
    {
        tileTransform.localScale = new Vector3(
            TileWidthSize,
            TileHeightSize,
            1f
        );

        Vector3 worldPosition = new Vector3(
            MapStartX + TileWidthSize * (x + 0.5f),
            MapStartY + TileHeightSize * (y + 0.5f),
            1f
        );

        tileTransform.position = worldPosition;
        mapData[x, y].worldPosition = worldPosition;
    }

    private void RegisterTileCoordinate(GameObject tile, int x, int y)
    {
        Collider2D tileCollider = tile.GetComponent<Collider2D>();

        if (tileCollider == null)
        {
            Debug.LogError($"타일 ({x}, {y})에 Collider2D가 없습니다.");
            return;
        }

        tileDataByCollider[tileCollider] = mapData[x, y];
    }

    private void SetupTileVisuals(GameObject tile, int x, int y)
    {
        SpriteRenderer fill =
            tile.transform.Find("Fill").GetComponent<SpriteRenderer>();

        TMP_Text damageText =
            tile.transform.Find("DamageText").GetComponent<TMP_Text>();

        fill.color = (x + y) % 2 == 0
            ? CheckerColor
            : Color.clear;

        if (tileColor != null)
            tileColor.AddTile(x, y, fill, damageText);
    }

    // 콜라이더에 해당하는 타일의 x, y를 반환
    public bool TryGetTileCoordinate(
        Collider2D tileCollider,
        out int x,
        out int y)
    {
        x = -1;
        y = -1;

        if (!tileDataByCollider.TryGetValue(tileCollider, out TileData tile))
            return false;

        x = tile.x;
        y = tile.y;
        return true;
    }

    // 저장된 타일 월드 좌표를 가져옴
    public bool TryGetTileWorldPosition(
        int x,
        int y,
        out Vector3 worldPosition)
    {
        worldPosition = Vector3.zero;

        if (!IsValidTile(x, y))
            return false;

        worldPosition = mapData[x, y].worldPosition;
        return true;
    }

    // 플레이어 타일의 월드 좌표를 가져옴
    public bool TryGetPlayerWorldPosition(out Vector3 worldPosition)
    {
        return TryGetTileWorldPosition(
            playerTileX,
            playerTileY,
            out worldPosition
        );
    }

    private void CreateFrame()
    {
        SpriteRenderer fill =
            TilePrefab.transform.Find("Fill").GetComponent<SpriteRenderer>();

        float width = MapSize.transform.localScale.x;
        float height = MapSize.transform.localScale.y;

        float centerX = MapStartX + width / 2f;
        float centerY = MapStartY + height / 2f;

        CreateFrameLine(
            fill,
            new Vector2(centerX, MapStartY),
            new Vector2(width + FrameThickness, FrameThickness)
        );

        CreateFrameLine(
            fill,
            new Vector2(centerX, MapStartY + height),
            new Vector2(width + FrameThickness, FrameThickness)
        );

        CreateFrameLine(
            fill,
            new Vector2(MapStartX, centerY),
            new Vector2(FrameThickness, height)
        );

        CreateFrameLine(
            fill,
            new Vector2(MapStartX + width, centerY),
            new Vector2(FrameThickness, height)
        );
    }

    private void CreateFrameLine(
        SpriteRenderer fill,
        Vector2 position,
        Vector2 size)
    {
        GameObject line = new GameObject("BoardFrame");

        line.transform.position =
            new Vector3(position.x, position.y, 1f);

        line.transform.localScale =
            new Vector3(size.x, size.y, 1f);

        SpriteRenderer renderer = line.AddComponent<SpriteRenderer>();
        renderer.sprite = fill.sprite;
        renderer.sharedMaterial = fill.sharedMaterial;
        renderer.color = FrameColor;
        renderer.sortingOrder = 2;
    }
}