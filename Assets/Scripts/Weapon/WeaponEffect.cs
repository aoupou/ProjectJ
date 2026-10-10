using System;
using System.Reflection;
using UnityEngine;

// 무기 효과 하나 + 수치
// 시트에는 "효과:수치" (예: "Poison:2 | MagicStack:5"). 효과는 코드 이름 또는 한글 이름
[Serializable]
public struct WeaponEffect : ISheetCell
{
    [Tooltip("무기 효과")]
    public WeaponEffectType effectType;
    [Tooltip("효과 수치")]
    public int value;

    public bool TryParse(string text, out string error)
    {
        string[] parts = text.Split(':');

        if (parts.Length != 2 || !int.TryParse(parts[1].Trim(), out int number))
        {
            error = $"'{text.Trim()}'는 무기 효과가 아님. \"Poison:2\"처럼 적어주세요";
            return false;
        }

        string name = parts[0].Trim();

        foreach (WeaponEffectType option in Enum.GetValues(typeof(WeaponEffectType)))
        {
            if (name.Equals(option.ToString(), StringComparison.OrdinalIgnoreCase) || name == Label(option))
            {
                effectType = option;
                value = number;
                error = null;
                return true;
            }
        }

        error = $"'{name}'는 없는 무기 효과";
        return false;
    }

    public string ToCellText()
    {
        return $"{effectType}:{value}";
    }

    // [InspectorName] 한글 이름 (없으면 영문)
    private static string Label(WeaponEffectType option)
    {
        InspectorNameAttribute attribute = typeof(WeaponEffectType).GetField(option.ToString()).GetCustomAttribute<InspectorNameAttribute>();
        return attribute != null ? attribute.displayName : option.ToString();
    }
}
