using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class change_tile_color : MonoBehaviour
{
    [SerializeField] private Map map;
    private SpriteRenderer[,] tileFills;
    private TMP_Text[,] tileText;
    public Color attackRangeColor = new Color(1f, 1f, 1f, 1f);

    private void Awake()
    {
       tileFills = new SpriteRenderer[map.Width,map.Height];
        tileText = new TMP_Text[map.Width, map.Height];
    }

    public void AddTile(int x, int y, SpriteRenderer fill, TMP_Text damageText)
    {
        tileFills[x,y] = fill;
        tileText[x,y] = damageText;
    }

    public void Paint(Vector2Int cell, int damage)
    {
        if (cell.x < 0 || cell.x >= map.Width || cell.y < 0 || cell.y >= map.Height)
            return;
        tileFills[cell.x,cell.y].color = attackRangeColor;
        tileText[cell.x, cell.y].text = damage.ToString();
    }

    public void ResetColor()
    {
        for (int x = 0;  x < map.Width; x++)
        {
            for (int y =0;  y < map.Height; y++)
            {
                tileFills[x, y].color = (x + y) % 2 == 0 ? map.CheckerColor : Color.clear;
                tileText[x, y].text = " ";
            }
        }

    }
    public Vector2Int WorldTocell(Vector3 poisiton)
    {
        int x = Mathf.FloorToInt((poisiton.x - map.MapStartX) / map.TileWidthSize);
        int y = Mathf.FloorToInt((poisiton.y - map.MapStartY) / map.TileHeightSize);

        return new Vector2Int(x, y);
    }
    // cells: 플레이어 기준 칸 → 그 칸 데미지
    public void ShowRange(Vector3 playerPosition, Dictionary<Vector2Int, int> cells)
    {
        ResetColor();

        Vector2Int playerCell = WorldTocell(playerPosition);
        foreach (KeyValuePair<Vector2Int, int> cell in cells)
        {
            Paint(playerCell + cell.Key, cell.Value);
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
