using TMPro;
using UnityEditor;
using UnityEngine;

// Tile 프리팹에 범위 데미지 표시용 글자(DamageText)를 자식으로 추가한다 (한 번만 누르면 됨)
// 글자는 비워두고, 실행 중에 change_tile_color가 범위 칸에만 데미지 숫자를 넣는다
public static class TileDamageTextSetup
{
    private const string TilePrefabPath = "Assets/Prefabs/Tile.prefab";
    private const string ChildName = "DamageText";

    [MenuItem("ProjectJ/타일에 데미지 글자 추가")]
    private static void AddDamageText()
    {
        GameObject root = PrefabUtility.LoadPrefabContents(TilePrefabPath);

        try
        {
            if (root.transform.Find(ChildName) != null)
            {
                Debug.Log("[타일] DamageText가 이미 있음");
                return;
            }

            GameObject textObject = new GameObject(ChildName);
            textObject.transform.SetParent(root.transform, false);

            TextMeshPro text = textObject.AddComponent<TextMeshPro>();

            // 칸 크기 = 부모 Tile 스케일이라 로컬 1×1이 칸 한 개
            text.rectTransform.sizeDelta = new Vector2(1f, 1f);
            text.text = "";
            text.fontSize = 4;
            text.alignment = TextAlignmentOptions.Center;
            text.color = new Color(0.8f, 0.1f, 0.1f, 1f);

            TMP_FontAsset font = Resources.Load<TMP_FontAsset>(GameFont.AssetPath);
            if (font != null)
                text.font = font;

            // 적(ui 레이어, 10)보다 위에 보이게
            MeshRenderer meshRenderer = textObject.GetComponent<MeshRenderer>();
            meshRenderer.sortingLayerName = "ui";
            meshRenderer.sortingOrder = 20;

            PrefabUtility.SaveAsPrefabAsset(root, TilePrefabPath);
            Debug.Log("[타일] Tile 프리팹에 DamageText 추가함");
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }
}
