using System.Collections.Generic;
using UnityEngine;

public class UpgradeTrackInfo
{
    private readonly List<UpgradeLevelInfo> levels = new();

    public UpgradeTrack Track { get; }
    public IReadOnlyList<UpgradeLevelInfo> Levels => levels;
    public int MaxLevel => levels.Count;

    public UpgradeTrackInfo(UpgradeTrack track)
    {
        Track = track;
    }

    public void Add(UpgradeLevelInfo info)
    {
        levels.Add(info);
        levels.Sort((a, b) => a.Level.CompareTo(b.Level));
    }

    public bool HasLevel(int level)
    {
        foreach(var info in levels)
        {
            if(info.Level == level)
                return true;
        }

        return false;
    }

    public UpgradeLevelInfo GetLevel(int level)
    {
        int index = Mathf.Clamp(level, 1, MaxLevel) - 1;
        return levels[index];
    }

    public bool IsValid(out string reason)
    {
        reason = string.Empty;

        if(levels.Count == 0)
        {
            reason = "레벨이 하나도 없습니다";
            return false;
        }

        for(int i = 0; i < levels.Count; i++)
        {
            if(levels[i].Level != i + 1)
            {
                reason = $"{i + 1}레벨이 빠졌습니다";
                return false;
            }

            if(i > 0 && levels[i].Cost <= 0)
            {
                reason = $"{i + 1}레벨 비용이 0 이하입니다";
                return false;
            }
        }

        return true;
    }
}
