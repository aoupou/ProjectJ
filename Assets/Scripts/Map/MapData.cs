using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MapData", menuName = "Game/Map Data")]
public class MapData : ScriptableObject
{
    public int width;
    public int height;
    public List<TileData> tiles;

    // 설정한 가로·세로 크기에 맞춰 타일 데이터를 생성
    public void GenerateTiles()
    {
        if (tiles == null)
            tiles = new List<TileData>();
        else
            tiles.Clear();

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                tiles.Add(new TileData
                {
                    x = x,
                    y = y
                });
            }
        }
    }
}