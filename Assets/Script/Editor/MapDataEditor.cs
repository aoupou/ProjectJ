using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MapData))]
public class MapDataEditor : Editor
{
    private bool[] tileFoldouts;

    public override void OnInspectorGUI()
    {
        MapData mapData = (MapData)target;

        // Width
        mapData.width = EditorGUILayout.IntField("Width", mapData.width);

        // Height
        mapData.height = EditorGUILayout.IntField("Height", mapData.height);

        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Tiles");

        // Foldout 배열 크기 맞추기
        if (tileFoldouts == null || tileFoldouts.Length != mapData.tiles.Count)
        {
            tileFoldouts = new bool[mapData.tiles.Count];
        }

        for (int i = 0; i < mapData.tiles.Count; i++)
        {
            TileData tile = mapData.tiles[i];

            // 좌표를 제목으로 사용
            tileFoldouts[i] = EditorGUILayout.Foldout(
                tileFoldouts[i],
                $"({tile.x}, {tile.y})",
                true
            );

            if (tileFoldouts[i])
            {
                EditorGUI.indentLevel++;

                tile.x = EditorGUILayout.IntField("X", tile.x);
                tile.y = EditorGUILayout.IntField("Y", tile.y);

                EditorGUI.indentLevel--;
            }
        }

        EditorGUILayout.Space();

        if (GUILayout.Button("타일 자동 생성"))
        {
            mapData.GenerateTiles();

            // 리스트 크기가 바뀌었으므로 Foldout도 다시 생성
            tileFoldouts = new bool[mapData.tiles.Count];

            EditorUtility.SetDirty(mapData);
        }
    }
}