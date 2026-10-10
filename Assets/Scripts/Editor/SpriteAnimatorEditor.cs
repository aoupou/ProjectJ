using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// SpriteAnimator 인스펙터에서 플레이 안 누르고 씬에서 바로 애니메이션 미리보기
//  컬렉션의 상태/콤보 애니메이션 중 하나 골라서 ▶ 재생 → 씬의 스프라이트가 움직임
//  '다른 컬렉션' / '애니메이션 직접'에 넣으면 플레이어 복제 안 하고 다른 무기 애니도 볼 수 있음 (씬엔 저장 안 됨)
//  정지하거나 다른 오브젝트 선택하면 원래 스프라이트로 돌려놓음 (씬 저장할 때도 돌려놓고 멈춤)
[CustomEditor(typeof(SpriteAnimator))]
public class SpriteAnimatorEditor : Editor
{
    // static이라 다른 오브젝트 갔다 와도 유지됨
    private static SpriteAnimationCollection otherCollection;
    private static SpriteAnimationData directAnim;

    private int selected;
    private bool previewing;
    private SpriteRenderer spriteRenderer;
    private Sprite originalSprite;
    private SpriteAnimationData previewAnim;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        // 플레이 중엔 SpriteAnimator가 직접 돌림
        if (Application.isPlaying)
            return;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("씬 미리보기", EditorStyles.boldLabel);

        otherCollection = (SpriteAnimationCollection)EditorGUILayout.ObjectField(
            "다른 컬렉션", otherCollection, typeof(SpriteAnimationCollection), false);
        directAnim = (SpriteAnimationData)EditorGUILayout.ObjectField(
            "애니메이션 직접", directAnim, typeof(SpriteAnimationData), false);

        var collection = otherCollection != null
            ? otherCollection
            : serializedObject.FindProperty("collection").objectReferenceValue as SpriteAnimationCollection;

        List<string> names = new List<string>();
        List<SpriteAnimationData> anims = new List<SpriteAnimationData>();

        // 직접 넣은 애니가 있으면 그것만
        if (directAnim != null)
        {
            names.Add(directAnim.name);
            anims.Add(directAnim);
        }
        else if (collection != null)
        {
            CollectAnimations(collection, names, anims);
        }

        if (anims.Count == 0)
            return;

        selected = Mathf.Clamp(selected, 0, anims.Count - 1);
        selected = EditorGUILayout.Popup("애니메이션", selected, names.ToArray());

        // 재생 중에 골라도 바로 바뀜
        if (previewing)
            previewAnim = anims[selected];

        if (GUILayout.Button(previewing ? "■ 정지" : "▶ 재생"))
        {
            if (previewing)
                StopPreview();
            else
                StartPreview(anims[selected]);
        }
    }

    private static void CollectAnimations(SpriteAnimationCollection collection, List<string> names, List<SpriteAnimationData> anims)
    {
        foreach (var entry in collection.entries)
        {
            if (entry.animation == null)
                continue;

            names.Add(entry.state);
            anims.Add(entry.animation);
        }

        foreach (var combo in collection.combos)
        {
            if (combo.steps == null)
                continue;

            for (int i = 0; i < combo.steps.Count; i++)
            {
                if (combo.steps[i].attack != null)
                {
                    names.Add($"콤보/{combo.name}/{i + 1} 공격");
                    anims.Add(combo.steps[i].attack);
                }

                if (combo.steps[i].end != null)
                {
                    names.Add($"콤보/{combo.name}/{i + 1} 마무리");
                    anims.Add(combo.steps[i].end);
                }
            }
        }
    }

    private void StartPreview(SpriteAnimationData anim)
    {
        spriteRenderer = ((SpriteAnimator)target).GetComponent<SpriteRenderer>();

        if (spriteRenderer == null)
            return;

        originalSprite = spriteRenderer.sprite;
        previewAnim = anim;
        previewing = true;

        EditorApplication.update += Tick;
        EditorSceneManager.sceneSaving += OnSceneSaving;
    }

    private void StopPreview()
    {
        if (!previewing)
            return;

        previewing = false;
        EditorApplication.update -= Tick;
        EditorSceneManager.sceneSaving -= OnSceneSaving;

        // 미리보기 프레임이 씬에 저장되지 않게 원래 스프라이트로 복구
        if (spriteRenderer != null)
            spriteRenderer.sprite = originalSprite;

        SceneView.RepaintAll();
    }

    private void Tick()
    {
        if (spriteRenderer == null || Application.isPlaying)
        {
            StopPreview();
            return;
        }

        Sprite frame = SpriteAnimationPreview.CurrentFrame(previewAnim);

        if (frame != null && spriteRenderer.sprite != frame)
        {
            spriteRenderer.sprite = frame;
            SceneView.RepaintAll();
        }
    }

    private void OnSceneSaving(UnityEngine.SceneManagement.Scene scene, string path)
    {
        StopPreview();
        Repaint();
    }

    private void OnDisable()
    {
        StopPreview();
    }
}
