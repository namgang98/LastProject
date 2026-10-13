using System.Text;
using UnityEngine;

public abstract class EquipmentInfo : ItemInfo, IStatModifier
{

    public EffectData Effect { get; }
    public int MaxHp { get; }

    protected EquipmentInfo(int id, string name, string iconName, ItemShape shape, ItemGrade grade, int sellPrice, string desc, EffectData effect, int maxHp) : base(id, name, iconName, shape, grade, sellPrice, desc)
    {
        Effect = effect;
        MaxHp = maxHp;
    }

    public override bool IsLootable => true;

    public abstract void Apply(PlayerStats stats);
    public override void AppendTooltip(StringBuilder sb)
    {
        if(MaxHp > 0)
            sb.AppendLine($"최대 체력 +{MaxHp}");

        if(Effect.HasEffect)
            sb.AppendLine(Effect.Describe());

        base.AppendTooltip(sb);
    }
}
