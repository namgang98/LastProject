public class SetBonusInfo : IStatModifier
{
    public int SetId { get; }
    public int Count { get; }
    public int MaxHp { get; }
    public float DamageBonus { get; }
    public float CooldownReduction { get; }
    public float CritChance { get; }
    public string Description { get; }

    public SetBonusInfo(int setid, int count, int maxHp, float damageBonus, float cooldownReduction, float critChance, string desc)
    {
        SetId = setid;
        Count = count;
        MaxHp = maxHp;
        DamageBonus = damageBonus;
        CooldownReduction = cooldownReduction;
        CritChance = critChance;
        Description = desc;
    }

    public void Apply(PlayerStats stats)
    {
        stats.AddMaxHp(MaxHp);
        stats.AddDamageBonus(DamageBonus);
        stats.AddCooldownReduction(CooldownReduction);
        stats.AddCritChance(CritChance);
    }
}