using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MapData", menuName = "Game/Map Data")]
public class MapData : ScriptableObject
{
    public int width;
    public int height;

    public List<TileData> tiles;

    public void GenerateTiles() // 리스트 하나하나 추가하기 귀찮은 사람을 위한 기능
    {
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

