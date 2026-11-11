using UnityEngine;
public interface ICriticalEffect
{
    bool RollCritical();
}
public class AttackEffect : IBattleEffect, ICriticalEffect
{
    private readonly UnitHealth target;
    private readonly AttackData data;
    private readonly StatusEffects statuses;
    private readonly IAttackRules rules;
    private int hitCount;

    private bool hasRolled;
    private bool rolledCritical;

    public AttackEffect(UnitHealth target, AttackData data, StatusEffects statuses, IAttackRules rules = null)
    {
        this.target = target;
        this.data = data;
        this.statuses = statuses;
        this.rules = rules;
    }

    public void Execute()
    {
        hitCount++;

        bool isCritical = hasRolled ? rolledCritical : IsForcedCrit(hitCount) || Random.value < data.CritChance;
        hasRolled = false;

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

    private bool IsForcedCrit(int count)
    {
        if (rules == null || rules.CritEvery <= 0)
            return false;

        return count % rules.CritEvery == 0;
    }
    public bool RollCritical()
    {
        rolledCritical = IsForcedCrit(hitCount + 1) || Random.value < data.CritChance;
        hasRolled = true;
        return rolledCritical;
    }

}