using System.Collections.Generic;
using UnityEngine;

public class TowerRuleSet : IStatModifier
{
    private static readonly List<TowerRuleInfo> NoRules = new();

    private IReadOnlyList<TowerRuleInfo> rules = NoRules;

    public TowerInfo Source { get; private set; }
    public float MonsterLifesteal => Sum(TowerRuleType.MonsterLifesteal);
    public float MonsterRegen => Sum(TowerRuleType.MonsterRegen);
    public float MonsterRevive => Sum(TowerRuleType.MonsterRevive);

    public void SetSource(TowerInfo tower)
    {
        Source = tower;
        rules = tower != null ? tower.Rules : NoRules;
    }

    public void Apply(PlayerStats stats)
    {
        stats.AddCooldownReduction(-Sum(TowerRuleType.PlayerCooldown));
    }

    public AttackData ModifyMonsterAttack(AttackData data)
    {
        int damage = Mathf.Max(1, Mathf.RoundToInt(data.Damage * (1f + Sum(TowerRuleType.MonsterDamage))));
        AttackData result = data.WithDamage(damage);

        foreach(TowerRuleInfo rule in rules)
        {
            if(rule.Type == TowerRuleType.IceAttack)
                result = AddElement(result, DamageElement.Ice, rule, Mathf.RoundToInt(rule.Value2));

            if(rule.Type == TowerRuleType.PoisonAttack)
                result = AddElement(result, DamageElement.Poison, rule, Mathf.Max(1, Mathf.RoundToInt(damage * rule.Value2)));
        }

        return result;
    }

    public int GetFloorDamage(int floor, int maxHp)
    {
        float rate = 0f;
        int step = (floor - 1) / 10;

        foreach(TowerRuleInfo rule in rules)
        {
            if(rule.Type == TowerRuleType.FloorPoison)
                rate += rule.Value + rule.Value2 * step;
        }

        return Mathf.RoundToInt(maxHp * rate);
    }

    private float Sum(TowerRuleType type)
    {
        float total = 0f;

        foreach(TowerRuleInfo rule in rules)
        {
            if(rule.Type == type)
                total += rule.Value;
        }

        return total;
    }

    private static AttackData AddElement(AttackData data, DamageElement element, TowerRuleInfo rule, int value)
    {
        bool same = data.Element == element;
        float chance = Mathf.Clamp01(same ? data.ElementChance + rule.Value : rule.Value);
        int elementValue = same ? Mathf.Max(data.ElementValue, value) : value;
        float duration = same ? Mathf.Max(data.ElementDuration, rule.Duration) : rule.Duration;

        return new AttackData(data.Damage, data.Cooldown, data.CritChance, data.CritMultiplier, element, chance, elementValue, duration);
    }
}