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
    public string PrefabName { get; }
    public GameObject Prefab { get; private set; }
    public string IconName { get; }
    public Sprite Icon { get; private set; }
    public int UnlockFloor { get; }
    public int UnlockCost { get; }
    public float CritChance { get; }
    public float ElementChance { get; }
    public int StarterWeaponId { get; }
    public WeaponInfo StarterWeapon { get; private set; }

    public bool IsStarter => UnlockFloor <= 0 && UnlockCost <= 0;
    public ClassInfo(int id, string name, int maxHp, WeaponType type, float dmg, float cool, string desc, string prefabName, int unlockfloor, int unlockcost, string iconName = "", float critChance = 0f, float elementChance = 0f, int starterWeaponId = 0)
    {
        Id = id;
        Name = name;
        MaxHp = maxHp;
        Type = type;
        DamageRate = dmg;
        CooldownRate = cool;
        Description = desc;
        PrefabName = prefabName;
        UnlockFloor = unlockfloor;
        UnlockCost = unlockcost;
        IconName = iconName;
        CritChance = critChance;
        ElementChance = elementChance;
        StarterWeaponId = starterWeaponId;
    }

    public void Apply(PlayerStats stats)
    {
        stats.AddMaxHp(MaxHp);
        stats.AddTypeDamageBonus(Type, DamageRate - 1f);
        stats.AddTypeCooldownReduction(Type, 1f - CooldownRate);
        stats.AddCritChance(CritChance);
        stats.AddElementChance(ElementChance);
    }

    public void SetIcon(Sprite icon)
    {
        if(Icon != null)
            return;
        Icon = icon;
    }

    public void SetStarterWeapon(WeaponInfo weapon)
    {
        if(StarterWeapon != null)
            return;
        StarterWeapon = weapon;
    }

    public void SetPrefab(GameObject prefab)
    {
        if(Prefab != null)
            return;
        Prefab = prefab;
    }
}
