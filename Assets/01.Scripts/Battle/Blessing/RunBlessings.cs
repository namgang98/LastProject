using System;
using UnityEngine;
using System.Collections.Generic;

public class RunBlessings : IStatModifier, IRunBonus, ISetBonusModifier
{
    private readonly Dictionary<BlessingInfo, int> stacks = new();
    public int ExtraSetCount => Mathf.RoundToInt(Sum(BlessingStat.SetResonance));
    public IReadOnlyDictionary<BlessingInfo, int> Stacks => stacks;
    public event Action<BlessingInfo> Changed;

    public int GetStack(BlessingInfo info)
    {
        return stacks.TryGetValue(info, out int count) ? count : 0;
    }

    public bool CanTake(BlessingInfo info)
    {
        return GetStack(info) < info.MaxStack;
    }

    public bool TryAdd(BlessingInfo info)
    {
        if(info == null || !CanTake(info))
            return false;

        stacks[info] = GetStack(info) + 1;
        Changed?.Invoke(info);
        return true;
    }

    public float Sum(BlessingStat stat)
    {
        float total = 0f;

        foreach(var pair in stacks)
        {
            foreach(BlessingModifier modifier in pair.Key.Modifiers)
            {
                if(modifier.Stat == stat)
                    total += modifier.Value * pair.Value;
            }
        }

        return total;
    }

    public void Apply(PlayerStats stats)
    {
        foreach(var pair in stacks)
        {
            foreach(BlessingModifier modifier in pair.Key.Modifiers)
            {
                BlessingStatRules.Apply(stats, modifier.Stat, modifier.Value * pair.Value);
            }
        }
    }

    public float Lowest(BlessingStat stat)
    {
        float lowest = 0f;

        foreach(var pair in stacks)
        {
            foreach(BlessingModifier modifier in pair.Key.Modifiers)
            {
                if(modifier.Stat != stat || modifier.Value <= 0f)
                    continue;

                if(lowest <= 0f || modifier.Value < lowest)
                    lowest = modifier.Value;
            }
        }

        return lowest;
    }
}