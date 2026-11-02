using System.Collections.Generic;
using UnityEngine;

public class ForgeTable
{
    private readonly List<ForgeLevelInfo> levels = new();

    public int MaxLevel => levels.Count;

    internal void Add(ForgeLevelInfo info)
    {
        levels.Add(info);
        levels.Sort((a, b) => a.Level.CompareTo(b.Level));
    }

    internal void Clear()
    {
        levels.Clear();
    }

    public bool HasLevel(int level)
    {
        foreach(ForgeLevelInfo info in levels)
        {
            if(info.Level == level)
                return true;
        }

        return false;
    }

    public bool TryGetLevel(int level, out ForgeLevelInfo info)
    {
        info = null;

        if(level < 1 || level > levels.Count)
            return false;

        info = levels[level - 1];
        return true;
    }

    public float GetBonus(int level)
    {
        if(level <= 0 || levels.Count == 0)
            return 0f;

        return levels[Mathf.Min(level, levels.Count) - 1].Bonus;
    }

    public bool IsValid(out string reason)
    {
        reason = string.Empty;

        for(int i = 0; i < levels.Count; i++)
        {
            if(levels[i].Level != i + 1)
            {
                reason = $"{i + 1}단계가 빠졌습니다";
                return false;
            }

            if(i > 0 && levels[i].Bonus <= levels[i - 1].Bonus)
            {
                reason = $"{i + 1}단계 보너스가 이전 단계보다 작거나 같습니다";
                return false;
            }
        }

        return true;
    }
}
