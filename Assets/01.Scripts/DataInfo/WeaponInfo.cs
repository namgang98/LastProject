using System;
using System.Text;
public enum WeaponType
{
    Sword,
    Axe,
    Bow,
    Staff,
    Dagger,
}

[Serializable]
public class WeaponInfo : ItemInfo
{
    public AttackData Attack { get; }
    public WeaponType Type { get; }

public WeaponInfo(int id, string name, string iconName, ItemShape shape, ItemGrade grade, int sellPrice, string desc, AttackData attack, WeaponType type) : base(id, name, iconName, shape, grade, sellPrice, desc)
    {
        Attack = attack;
        Type = type;
    }
    public override bool IsLootable => true;
    public override ItemInstance CreateInstance()
    {
        return new ForgeableInstance(this);
    }
    public override void AppendTooltip(StringBuilder sb)
    {
        sb.AppendLine($"데미지 : {Attack.Damage}");
        sb.AppendLine($"치명타 {Attack.CritChance * 100f:0}% (x{Attack.CritMultiplier:0.#})");
        sb.Append($"쿨타임 : {Attack.Cooldown}");
        base.AppendTooltip(sb);
    }
}