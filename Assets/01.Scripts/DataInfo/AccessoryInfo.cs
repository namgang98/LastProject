using System.Text;
using UnityEngine;

public class AccessoryInfo : EquipmentInfo
{
    public float DamageBonus { get; }
    public float CooldownReduction { get; }

    public AccessoryInfo(int id, string name, string iconName, ItemShape shape, ItemGrade grade, int sellPrice, string desc, EffectData effect, int maxHp, float damageBonus, float cooldownReduction) : base(id, name, iconName, shape, grade, sellPrice, desc, effect, maxHp)
    {
        DamageBonus = damageBonus;
        CooldownReduction = cooldownReduction;
    }
    public override void Apply(PlayerStats stats)
    {
        stats.AddDamageBonus(DamageBonus);
        stats.AddCooldownReduction(CooldownReduction);
        stats.AddMaxHp(MaxHp);
    }
    public override void AppendTooltip(StringBuilder sb)
    {
        if(DamageBonus > 0f)
            sb.AppendLine($"공격력 +{DamageBonus * 100f:0}%");

        if(CooldownReduction > 0f)
            sb.AppendLine($"쿨타임 -{CooldownReduction * 100f:0}%");

        base.AppendTooltip(sb);
    }
}
