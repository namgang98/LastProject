using UnityEngine;

public class PlayerAttackRules : IAttackRules
{
    private const float ExecuteThreshold = 0.3f;
    private const float LowHpThreshold = 0.5f;

    private readonly IRunBonus bonus;
    private readonly UnitHealth self;
    private readonly bool isBoss;

    public int CritEvery { get; }

    public PlayerAttackRules(IRunBonus bonus, UnitHealth self, bool isBoss)
    {
        this.bonus = bonus;
        this.self = self;
        this.isBoss = isBoss;
        CritEvery = Mathf.RoundToInt(bonus.Lowest(BlessingStat.CritEvery));
    }

    public float GetDamageRate(UnitHealth target)
    {
        float rate = 1f;

        if(isBoss)
            rate += bonus.Sum(BlessingStat.BossDamage);

        if(IsBelow(target, ExecuteThreshold))
            rate += bonus.Sum(BlessingStat.ExecuteDamage);

        if(IsBelow(self, LowHpThreshold))
            rate += bonus.Sum(BlessingStat.LowHpDamage);

        return Mathf.Max(0.1f, rate);
    }

    public void OnDealt(int amount)
    {
        float rate = bonus.Sum(BlessingStat.Lifesteal);

        if(rate <= 0f || amount <= 0)
            return;

        self.Heal(Mathf.Max(1, Mathf.RoundToInt(amount * rate)));
    }

    public AttackData ApplyElement(AttackData data)
    {
        if(data.Element == DamageElement.None)
            return data;

        float chance = Mathf.Clamp01(data.ElementChance + bonus.Sum(BlessingStat.ElementChance));
        int value = data.ElementValue;
        float duration = data.ElementDuration;

        if(data.Element == DamageElement.Fire)
            value = Mathf.RoundToInt(value * (1f + bonus.Sum(BlessingStat.BurnDamage)));

        if(data.Element == DamageElement.Poison)
            value = Mathf.RoundToInt(value * (1f + bonus.Sum(BlessingStat.PoisonDamage)));

        if(data.Element == DamageElement.Ice)
            duration *= 1f + bonus.Sum(BlessingStat.FreezeDuration);

        return new AttackData(data.Damage, data.Cooldown, data.CritChance, data.CritMultiplier, data.Element, chance, value, duration);
    }

    private static bool IsBelow(UnitHealth unit, float ratio)
    {
        return unit.MaxHp > 0 && unit.CurrentHP <= unit.MaxHp * ratio;
    }
}