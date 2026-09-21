using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewAnimationCollection", menuName = "Animation/Animation Collection")]
public class SpriteAnimationCollection : ScriptableObject
{
    [Serializable]
    public struct Entry
    {
        public string state;
        public SpriteAnimationData animation;
    }

    [Serializable]
    public struct ComboStep
    {
        public SpriteAnimationData attack;
        [Tooltip("이 단계 다음 콤보 입력을 기다리는 동안 재생되는 마무리 애니메이션(선택)")]
        public SpriteAnimationData end;
    }

    [Serializable]
    public struct Combo
    {
        public string name;
        public List<ComboStep> steps;
    }

    public string defaultState = "Idle";
    public List<Entry> entries = new List<Entry>();
    public List<Combo> combos = new List<Combo>();

    private Dictionary<string, SpriteAnimationData> lookup;

    private void OnEnable() => lookup = null;
    private void OnValidate() => lookup = null;

    public List<ComboStep> GetCombo(string name)
    {
        foreach (Combo c in combos)
            if (string.Equals(c.name, name, StringComparison.OrdinalIgnoreCase)) return c.steps;
        return null;
    }

    public SpriteAnimationData Get(string state)
    {
        if (lookup == null)
        {
            lookup = new Dictionary<string, SpriteAnimationData>(StringComparer.OrdinalIgnoreCase);
            foreach (Entry e in entries)
                if (!string.IsNullOrEmpty(e.state) && e.animation != null)
                    lookup[e.state] = e.animation;
        }
        return state != null && lookup.TryGetValue(state, out var anim) ? anim : null;
    }
}
