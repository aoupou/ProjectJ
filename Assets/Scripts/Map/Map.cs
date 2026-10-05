using UnityEngine;
using TMPro;

// 타일 크기를 Awake에서 계산하니까 Player 등 다른 스크립트 Awake보다 먼저 돌게 함
[DefaultExecutionOrder(-100)]
public class Map : MonoBehaviour
{
    [SerializeField] private MapData mapDataAsset;
    private TileData[,] mapData; 
    public GameObject MapSize;
    public int Width => mapDataAsset.width;
    public int Height => mapDataAsset.height;

    public float TileWidthSize;
    public float TileHeightSize;

    public float MapStartX = 0;
    public float MapStartY = 0;

    public GameObject TilePrefab;

    // 체스판 무늬 색 (한 칸 건너 연하게 칠함). 범위 표시를 끌 때도 이 색으로 돌려놓기
    public Color CheckerColor = new Color(0.25f, 0.25f, 0.3f, 0.07f);

    // 게임판 바깥 테두리 (안쪽 선보다 굵고 진하게)
    public Color FrameColor = new Color(0.2f, 0.2f, 0.25f, 0.9f);
    public float FrameThickness = 0.08f;

    private void Awake()
    {
        int width = mapDataAsset.width; // 맵 데이터에서 너비와 높이를 가져옴
        int height = mapDataAsset.height;

        TileWidthSize = MapSize.transform.localScale.x / width;
        TileHeightSize = MapSize.transform.localScale.y/ height;

        MapStartX = MapSize.transform.position.x
            - MapSize.transform.localScale.x / 2;
        
        MapStartY = MapSize.transform.position.y
            - MapSize.transform.localScale.y / 2;

        mapData = new TileData[width, height];
       
        foreach (TileData tileData in mapDataAsset.tiles) // 타일데이터에 타일 좌표들을 저장
        {
            mapData[tileData.x, tileData.y] = tileData;
        } 

        // 스포너는 에디터에서 범위 잡는 용도라 게임 시작하면 안 보이게 (위치·크기 값은 그대로 씀)
        SpriteRenderer spawnerRenderer = MapSize.GetComponent<SpriteRenderer>();
        if (spawnerRenderer != null)
            spawnerRenderer.enabled = false;
    }

    void Start()
    {
        change_tile_color tileColor = FindObjectOfType<change_tile_color>();

        for (int x = 0; x < mapDataAsset.width; x++)
        {
            for (int y = 0; y < mapDataAsset.height; y++)
            {
                GameObject Tile = Instantiate(TilePrefab);

                Tile.transform.localScale = new Vector3(
                    TileWidthSize,
                    TileHeightSize,
                    1
                );

                Tile.transform.position = new Vector3(
                    MapStartX
                    + (TileWidthSize / 2)
                    + (TileWidthSize * x),

                    MapStartY
                    + (TileHeightSize / 2)
                    + (TileHeightSize * y),

                    1
                );

                SpriteRenderer fill = Tile.transform.Find("Fill").GetComponent<SpriteRenderer>();
                TMP_Text damageText = Tile.transform.Find("DamageText").GetComponent<TMP_Text>();

                fill.color = (x + y) % 2 == 0 ? CheckerColor : Color.clear;

                if (tileColor != null)
                    tileColor.AddTile(x, y, fill, damageText);
                    
            }
        }

        CreateFrame();
    }

    // 게임판 바깥에 굵은 선 4개(위·아래·왼쪽·오른쪽)를 그림
    // 타일의 Fill(흰 사각형) 그림을 가져다가 길쭉하게 늘려서 씀
    void CreateFrame()
    {
        SpriteRenderer fill = TilePrefab.transform.Find("Fill").GetComponent<SpriteRenderer>();

        float width = MapSize.transform.localScale.x;
        float height = MapSize.transform.localScale.y;
        float centerX = MapStartX + width / 2;
        float centerY = MapStartY + height / 2;

        // 가로 선은 두께만큼 더 길게 해서 모서리가 비지 않게
        CreateFrameLine(fill, new Vector2(centerX, MapStartY), new Vector2(width + FrameThickness, FrameThickness));          // 아래
        CreateFrameLine(fill, new Vector2(centerX, MapStartY + height), new Vector2(width + FrameThickness, FrameThickness)); // 위
        CreateFrameLine(fill, new Vector2(MapStartX, centerY), new Vector2(FrameThickness, height));                        // 왼쪽
        CreateFrameLine(fill, new Vector2(MapStartX + width, centerY), new Vector2(FrameThickness, height));                // 오른쪽
    }

    void CreateFrameLine(SpriteRenderer fill, Vector2 position, Vector2 size)
    {
        GameObject line = new GameObject("BoardFrame");
        line.transform.position = new Vector3(position.x, position.y, 1);
        line.transform.localScale = new Vector3(size.x, size.y, 1);

        SpriteRenderer renderer = line.AddComponent<SpriteRenderer>();
        renderer.sprite = fill.sprite;
        renderer.sharedMaterial = fill.sharedMaterial;
        renderer.color = FrameColor;
        renderer.sortingOrder = 2; // 타일 선(1)보다 위
    }
}