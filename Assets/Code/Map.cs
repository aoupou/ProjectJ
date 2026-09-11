using UnityEngine;

public class Map : MonoBehaviour
{
    public int Width = 5;
    public int Height = 5;
    public float MapSize = 20;

    public float TileWidthSize;
    public float TileHeightSize;

    public GameObject TilePrefab;

    void Start()
    {
        TileWidthSize = MapSize / Width;
        TileHeightSize = MapSize / Height;

        Debug.Log(TileWidthSize);

        for (int x = 0; x < Width;x++)
        {
            for (int y = 0; y < Height;y++)
            {
                GameObject Tile = Instantiate(TilePrefab);
                Tile.transform.localScale = new Vector3(TileWidthSize, TileHeightSize, 1);
                Tile.transform.position= new Vector3((MapSize / 2 * -1 ) + (TileWidthSize /2) + (TileWidthSize * x), (-MapSize / 2 + (TileHeightSize / 2)+ TileHeightSize * y), 1);
            }
        }
    }
}