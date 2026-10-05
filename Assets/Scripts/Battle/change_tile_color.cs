using UnityEngine;
using System.Collections.Generic; 

public class change_tile_color : MonoBehaviour
{
    [SerializeField] private Map map;
    private SpriteRenderer[,] tileFills;
    public Color attackRangeColor = new Color(1f, 1f, 1f, 1f);

    private void Awake()
    {
       tileFills = new SpriteRenderer[map.Width,map.Height];
    }

    public void AddTile(int x, int y, SpriteRenderer fill)
    {
        tileFills[x,y] = fill;
    }

    public void Paint(Vector2Int cell)
    {
        if (cell.x < 0 || cell.x >= map.Width || cell.y < 0 || cell.y >= map.Height)
            return;
        tileFills[cell.x,cell.y].color = attackRangeColor;
    }

    public void ResetColor()
    {
        for (int x = 0;  x < map.Width; x++)
        {
            for (int y =0;  y < map.Height; y++)
            {
                tileFills[x, y].color = (x + y) % 2 == 0 ? map.CheckerColor : Color.clear;
            }
        }
    }
    public Vector2Int WorldTocell(Vector3 poisiton)
    {
        int x = Mathf.FloorToInt((poisiton.x - map.MapStartX) / map.TileWidthSize);
        int y = Mathf.FloorToInt((poisiton.y - map.MapStartY) / map.TileHeightSize);

        return new Vector2Int(x, y);
    }
    public void ShowRange(Vector3 playerPosition , List<Vector2Int > cells)
    {
        ResetColor();

        Vector2Int playerCell = WorldTocell(playerPosition);
        foreach (Vector2Int cell in cells)
        {
            Paint(playerCell + cell);
        }
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
