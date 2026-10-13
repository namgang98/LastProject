using UnityEngine;

public class ArmorInfo : EquipmentInfo
{

    public ArmorInfo(int id, string name, string iconName, ItemShape shape, ItemGrade grade, int sellPrice, string desc, EffectData effect, int maxHp) :base(id, name, iconName, shape, grade, sellPrice, desc,effect,maxHp)
    {
    }

    public override void Apply(PlayerStats stats)
    {
        stats.AddMaxHp(MaxHp);
    }
}
