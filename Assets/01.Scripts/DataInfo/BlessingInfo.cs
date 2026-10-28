using System.Collections.Generic;

public enum BlessingStat
{
    None,
    MaxHp,
    Damage,
    Cooldown,
    CritChance,
    CritMultiplier,
    SwordDamage,
    SwordCooldown,
    AxeDamage,
    AxeCooldown,
    BowDamage,
    BowCooldown,
    FloorHeal,
    RestHeal,
    PotionPower,
    Lifesteal,
    StartShield,
    DamageReduction,
    Thorns,
    Dodge,
    Revive,
    BossDamage,
    ExecuteDamage,
    LowHpDamage,
    FirstStrike,
    ElementChance,
    BurnDamage,
    PoisonDamage,
    FreezeDuration,
    MaterialDrop,
    DropChance,
    CritEvery,
    SetResonance,
    StaffDamage,
    StaffCooldown,
    DaggerDamage,
    DaggerCooldown,
}

public readonly struct BlessingModifier
{
    public readonly BlessingStat Stat;
    public readonly float Value;

    public BlessingModifier(BlessingStat stat, float value)
    {
        Stat = stat;
        Value = value;
    }
}

public class BlessingInfo
{
    public int Id { get; }
    public string Name { get; }
    public ItemGrade Grade { get; }
    public IReadOnlyList<BlessingModifier> Modifiers { get; }
    public int MaxStack { get; }
    public int Weight { get; }
    public string Description { get; }

    public BlessingInfo(int id, string name, ItemGrade grade, IReadOnlyList<BlessingModifier> modifiers, int maxStack, int weight, string description)
    {
        Id = id;
        Name = name;
        Grade = grade;
        Modifiers = modifiers;
        MaxStack = maxStack;
        Weight = weight;
        Description = description;
    }
}
