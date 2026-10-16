using System;
using UnityEngine;

public class UpgradeProgress
{
    public UpgradeTrackInfo Info { get; }
    public int Level { get; private set; }
    public int Deposited { get; private set; }

    public bool IsMaxed => Level >= Info.MaxLevel;
    public UpgradeLevelInfo Current => Info.GetLevel(Level);
    public UpgradeLevelInfo Next => IsMaxed ? null : Info.GetLevel(Level + 1);
    public int NextCost => IsMaxed ? 0 : Next.Cost;
    public int Remaining => IsMaxed ? 0 : NextCost - Deposited;

    public event Action<UpgradeProgress> Changed;
    public event Action<UpgradeProgress> LevelUp;

    public UpgradeProgress(UpgradeTrackInfo info, int level, int deposited)
    {
        Info = info;
        Level = Mathf.Clamp(level, 1, info.MaxLevel);
        Deposited = IsMaxed ? 0 : Mathf.Clamp(deposited, 0, Mathf.Max(0, NextCost - 1));
    }

    public int Add(int amount)
    {
        if(amount <= 0 || IsMaxed)
            return 0;

        int used = Mathf.Min(amount, Remaining);
        Deposited += used;

        if(Deposited >= NextCost)
        {
            Deposited = 0;
            Level++;
            LevelUp?.Invoke(this);
        }

        Changed?.Invoke(this);
        return used;
    }
}