using UnityEngine;

[System.Serializable]
public class TileData
{
    public int x;
    public int y;

    public bool OnPlayer;
    public bool OnEnemy;
    public bool OnApple;

    // 현재 실행 중인 맵에서 사용하며 MapData 에셋에는 저장하지 않음
    [System.NonSerialized]
    public Vector3 worldPosition;
}