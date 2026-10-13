using UnityEngine;

public class AttackEffect : IBattleEffect
{
    private readonly UnitHealth target;
    private readonly AttackData data; 
    private readonly StatusEffects statuses;

    public AttackEffect(UnitHealth target, AttackData data, StatusEffects statuses)
    {
        this.target = target;
        this.data = data;
        this.statuses = statuses;
    }
    public void Execute()
    {
        bool isCritical = Random.value < data.CritChance;
        int damage = isCritical ? Mathf.RoundToInt(data.Damage * data.CritMultiplier) : data.Damage;

        target.TakeDamage(new DamageInfo(damage, isCritical, data.Element));

        if(data.Element == DamageElement.None || Random.value >= data.ElementChance)
            return;

        IStatusEffect status = StatusEffectFactory.Create(data, target);

        if (status != null)
            statuses.Apply(status);
    }

}
