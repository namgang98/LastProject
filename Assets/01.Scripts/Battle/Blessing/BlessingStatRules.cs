using UnityEngine;

public static class BlessingStatRules
{
    public static bool IsSupported(BlessingInfo info)
    {
        foreach(BlessingModifier modifier in info.Modifiers)
        {
            if(!IsSupported(modifier.Stat))
                return false;
        }

        return true;
    }
    public static bool IsSupported(BlessingStat stat)
    {
        return stat != BlessingStat.None;
    }
    public static void Apply(PlayerStats stats, BlessingStat stat, float value)
    {
        switch(stat)
        {
            case BlessingStat.MaxHp:
                stats.AddMaxHp(Mathf.RoundToInt(value));
                return;
            case BlessingStat.Damage:
                stats.AddDamageBonus(value);
                return;
            case BlessingStat.Cooldown:
                stats.AddCooldownReduction(value);
                return;
            case BlessingStat.CritChance:
                stats.AddCritChance(value);
                return;
            case BlessingStat.CritMultiplier:
                stats.AddCritMultiplier(value);
                return;
            case BlessingStat.SwordDamage:
                stats.AddTypeDamageBonus(WeaponType.Sword, value);
                return;
            case BlessingStat.SwordCooldown:
                stats.AddTypeCooldownReduction(WeaponType.Sword, value);
                return;
            case BlessingStat.AxeDamage:
                stats.AddTypeDamageBonus(WeaponType.Axe, value);
                return;
            case BlessingStat.AxeCooldown:
                stats.AddTypeCooldownReduction(WeaponType.Axe, value);
                return;
            case BlessingStat.BowDamage:
                stats.AddTypeDamageBonus(WeaponType.Bow, value);
                return;
            case BlessingStat.BowCooldown:
                stats.AddTypeCooldownReduction(WeaponType.Bow, value);
                return;
            case BlessingStat.StaffDamage:
                stats.AddTypeDamageBonus(WeaponType.Staff, value);
                return;
            case BlessingStat.StaffCooldown:
                stats.AddTypeCooldownReduction(WeaponType.Staff, value);
                return;
            case BlessingStat.DaggerDamage:
                stats.AddTypeDamageBonus(WeaponType.Dagger, value);
                return;
            case BlessingStat.DaggerCooldown:
                stats.AddTypeCooldownReduction(WeaponType.Dagger, value);
                return;
        }
    }
}