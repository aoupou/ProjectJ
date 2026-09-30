using UnityEngine;

public class Map : MonoBehaviour
{
    public int Width = 5;
    public int Height = 5;
    public GameObject MapSize;

    public float TileWidthSize;
    public float TileHeightSize;

    public float MapStartX = 0;
    public float MapStartY = 0;

    public GameObject TilePrefab;

    private void Awake()
    {
        TileWidthSize = MapSize.transform.localScale.x / Width;
        TileHeightSize = MapSize.transform.localScale.y/ Height;
        MapStartX = MapSize.transform.position.x- MapSize.transform.localScale.x / 2;
        MapStartY = MapSize.transform.position.y- MapSize.transform.localScale.y / 2;
    }

    void Start()
    {
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
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
            }
        }
    }
}