using System.Collections.Generic;
using UnityEngine;

public class ForgeTable
{
    private readonly List<ForgeLevelInfo> levels = new();
    private readonly Dictionary<WeaponType, Dictionary<int, ForgeLevelInfo>> weaponLevels = new();

    public int MaxLevel => levels.Count;

    internal void Add(ForgeLevelInfo info)
    {
        if(info.Weapon.HasValue)
        {
            AddWeaponLevel(info.Weapon.Value, info);
            return;
        }

        levels.Add(info);
        levels.Sort((a, b) => a.Level.CompareTo(b.Level));
    }

    private void AddWeaponLevel(WeaponType type, ForgeLevelInfo info)
    {
        if(!weaponLevels.TryGetValue(type, out Dictionary<int, ForgeLevelInfo> map))
        {
            map = new Dictionary<int, ForgeLevelInfo>();
            weaponLevels.Add(type, map);
        }

        map[info.Level] = info;
    }

    internal void Clear()
    {
        levels.Clear();
        weaponLevels.Clear();
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

    public bool HasLevel(int level, WeaponType? weapon)
    {
        if(!weapon.HasValue)
            return HasLevel(level);

        return weaponLevels.TryGetValue(weapon.Value, out Dictionary<int, ForgeLevelInfo> map) && map.ContainsKey(level);
    }

    public bool TryGetLevel(int level, out ForgeLevelInfo info)
    {
        info = null;

        if(level < 1 || level > levels.Count)
            return false;

        info = levels[level - 1];
        return true;
    }

    public bool TryGetLevel(int level, ItemInfo item, out ForgeLevelInfo info)
    {
        if(item is WeaponInfo weapon && weaponLevels.TryGetValue(weapon.Type, out Dictionary<int, ForgeLevelInfo> map) && map.TryGetValue(level, out info))
            return true;

        return TryGetLevel(level, out info);
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

        foreach(KeyValuePair<WeaponType, Dictionary<int, ForgeLevelInfo>> pair in weaponLevels)
        {
            foreach(int level in pair.Value.Keys)
            {
                if(HasLevel(level))
                    continue;

                reason = $"{pair.Key} {level}단계는 공통 {level}단계가 없습니다";
                return false;
            }
        }

        return true;
    }
}
