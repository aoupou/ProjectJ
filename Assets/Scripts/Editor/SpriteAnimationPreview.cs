using UnityEditor;
using UnityEngine;

public static class SpriteAnimationPreview
{
    public static Sprite CurrentFrame(SpriteAnimationData data)
    {
        if (data == null || data.sprites == null || data.sprites.Length == 0) return null;
        int count = data.sprites.Length;
        int frame = (int)(EditorApplication.timeSinceStartup * Mathf.Max(0.01f, data.framesPerSecond));
        frame = data.loop ? frame % count : Mathf.Min(frame % (count + 1), count - 1);
        return data.sprites[frame];
    }

    public static void Draw(Rect rect, SpriteAnimationData data)
    {
        EditorGUI.DrawRect(rect, new Color(0.16f, 0.16f, 0.16f));
        Sprite s = CurrentFrame(data);
        if (s == null || s.texture == null) return;

        Rect tr = s.textureRect;
        Texture2D tex = s.texture;
        Rect uv = new Rect(tr.x / tex.width, tr.y / tex.height, tr.width / tex.width, tr.height / tex.height);

        // 모든 프레임의 pivot 기준 최대 범위 -> 프레임이 바뀌어도 배율/기준점이 고정된다
        float left = 0, right = 0, up = 0, down = 0;
        foreach (Sprite f in data.sprites)
        {
            if (f == null) continue;
            Vector2 pv = f.pivot;
            left = Mathf.Max(left, pv.x);
            right = Mathf.Max(right, f.rect.width - pv.x);
            down = Mathf.Max(down, pv.y);
            up = Mathf.Max(up, f.rect.height - pv.y);
        }
        float scale = Mathf.Min(rect.width / Mathf.Max(1f, left + right), rect.height / Mathf.Max(1f, up + down));
        // 공통 범위(bounding box)를 미리보기 중앙에 배치하고, 그 안에서 pivot 기준으로 프레임을 놓는다
        Vector2 anchor = new Vector2(
            rect.center.x + (left - right) * 0.5f * scale,
            rect.center.y + (up - down) * 0.5f * scale);   // GUI는 y가 아래로 증가
        Vector2 pivot = s.pivot;
        Rect dst = new Rect(
            anchor.x - pivot.x * scale,
            anchor.y - (tr.height - pivot.y) * scale,
            tr.width * scale, tr.height * scale);
        GUI.DrawTextureWithTexCoords(dst, tex, uv, true);
    }
}

[CustomEditor(typeof(SpriteAnimationData))]
public class SpriteAnimationDataEditor : Editor
{
    public override bool RequiresConstantRepaint() => true;
    public override bool HasPreviewGUI() => true;

    public override void OnPreviewGUI(Rect r, GUIStyle background)
    {
        SpriteAnimationPreview.Draw(r, (SpriteAnimationData)target);
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        var data = (SpriteAnimationData)target;
        EditorGUILayout.Space();
        Rect r = GUILayoutUtility.GetRect(0, 160, GUILayout.ExpandWidth(true));
        SpriteAnimationPreview.Draw(r, data);
        int n = data.sprites?.Length ?? 0;
        EditorGUILayout.LabelField($"{n} frames / {(n / Mathf.Max(0.01f, data.framesPerSecond)):0.00}s");
    }
}
