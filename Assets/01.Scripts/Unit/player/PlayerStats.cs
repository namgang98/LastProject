using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerStats
{
    private const float MinCooldownRate = 0.2f;

    public int MaxHp { get; private set; }

    private float damageBonus;
    private float cooldownReduction;
    private float critChanceBonus;
    private float critMultiplierBonus;

    private readonly Dictionary<WeaponType, float> typeDamageBonus = new();
    private readonly Dictionary<WeaponType, float> typeCooldownReduction = new();

    public void AddMaxHp(int value) => MaxHp += value;
    public void AddDamageBonus(float value) => damageBonus += value;
    public void AddCooldownReduction(float value) => cooldownReduction += value;
    public void AddCritChance(float value) => critChanceBonus += value;
    public void AddCritMultiplier(float value) => critMultiplierBonus += value;

    public void AddTypeDamageBonus(WeaponType type, float value)
    {
        typeDamageBonus.TryGetValue(type, out float current);
        typeDamageBonus[type] = current + value;
    }
    public void AddTypeCooldownReduction(WeaponType type, float value)
    {
        typeCooldownReduction.TryGetValue(type, out float current);
        typeCooldownReduction[type] = current + value;
    }

    public AttackData Apply(WeaponType type, AttackData data)
    {
        typeDamageBonus.TryGetValue(type, out float typeBonus);
        typeCooldownReduction.TryGetValue(type, out float typeReduction);

        float damageRate = 1f + damageBonus + typeBonus;
        float cooldownRate = Mathf.Max(MinCooldownRate, 1f - cooldownReduction - typeReduction);

        int damage = Mathf.Max(1, Mathf.RoundToInt(data.Damage * damageRate));

        float chance = Mathf.Clamp01(data.CritChance + critChanceBonus);
        float multiplier = data.CritMultiplier + critMultiplierBonus;
        return data.WithDamage(damage).WithCooldown(data.Cooldown * cooldownRate).WithCrit(chance, multiplier);
    }
}