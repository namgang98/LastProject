using UnityEngine;

public interface IStatModifier
{
    void Apply(PlayerStats stats);
}
public class ClassInfo : IStatModifier
{
    public int Id { get; }
    public string Name { get; }
    public int MaxHp { get; }
    public WeaponType Type { get; }
    public float DamageRate { get; }
    public float CooldownRate { get; }
    public string Description { get; }

    public ClassInfo(int id, string name, int maxHp, WeaponType type, float dmg, float cool, string desc)
    {
        Id = id;
        Name = name;
        MaxHp = maxHp;
        Type = type;
        DamageRate = dmg;
        CooldownRate = cool;
        Description = desc;
    }

    public void Apply(PlayerStats stats)
    {
        stats.AddMaxHp(MaxHp);
        stats.AddTypeDamageBonus(Type, DamageRate - 1f);
        stats.AddTypeCooldownReduction(Type, 1f - CooldownRate);
    }

    
}
