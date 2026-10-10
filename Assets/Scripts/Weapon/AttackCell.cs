using System;
using System.Text.RegularExpressions;
using UnityEngine;

// 공격 범위 한 칸 + 그 칸의 데미지
// 시트에는 "(x,y):데미지" (예: "(-1,1):3 | (0,1):5")
[Serializable]
public struct AttackCell : ISheetCell
{
    [Tooltip("플레이어 기준 칸")]
    public Vector2Int cell;
    [Tooltip("이 칸의 데미지")]
    public int damage;

    private static readonly Regex Format = new Regex(@"^\(?\s*(-?\d+)\s*,\s*(-?\d+)\s*\)?\s*:\s*(-?\d+)$");

    public bool TryParse(string text, out string error)
    {
        Match m = Format.Match(text.Trim());

        if (!m.Success)
        {
            error = $"'{text.Trim()}'는 공격 칸이 아님. \"(-1,1):3\"처럼 적어주세요";
            return false;
        }

        cell = new Vector2Int(int.Parse(m.Groups[1].Value), int.Parse(m.Groups[2].Value));
        damage = int.Parse(m.Groups[3].Value);
        error = null;
        return true;
    }

    public string ToCellText()
    {
        return $"({cell.x},{cell.y}):{damage}";
    }
}
