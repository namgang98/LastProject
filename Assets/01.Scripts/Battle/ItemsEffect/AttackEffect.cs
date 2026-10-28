using UnityEngine;

public class AttackEffect : IBattleEffect
{
    private readonly UnitHealth target;
    private readonly AttackData data;
    private readonly StatusEffects statuses;
    private readonly PlayerAttackRules rules;
    private int hitCount;

    public AttackEffect(UnitHealth target, AttackData data, StatusEffects statuses, PlayerAttackRules rules = null)
    {
        this.target = target;
        this.data = data;
        this.statuses = statuses;
        this.rules = rules;
    }

    public void Execute()
    {
        hitCount++;

        bool isCritical = IsForcedCrit() || Random.value < data.CritChance;
        float rate = rules != null ? rules.GetDamageRate(target) : 1f;
        float raw = data.Damage * rate * (isCritical ? data.CritMultiplier : 1f);
        int damage = Mathf.Max(1, Mathf.RoundToInt(raw));

        int dealt = target.TakeDamage(new DamageInfo(damage, isCritical, data.Element));
        rules?.OnDealt(dealt);

        if(data.Element == DamageElement.None || Random.value >= data.ElementChance)
            return;

        IStatusEffect status = StatusEffectFactory.Create(data, target);

        if(status != null)
            statuses.Apply(status);
    }

    private bool IsForcedCrit()
    {
        if(rules == null || rules.CritEvery <= 0)
            return false;

        return hitCount % rules.CritEvery == 0;
    }
}