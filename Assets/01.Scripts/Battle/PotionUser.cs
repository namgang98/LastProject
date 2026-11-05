using UnityEngine;

public class PotionUser
{
    private readonly Backpack backpack;
    private readonly UnitHealth health;
    private readonly StatusEffects statuses;
    private readonly float threshold;
    private readonly float cooldown;
    private float healTimer;
    private float cureTimer;
    private int biggestHit;

    private readonly float potionPower;

    public PotionUser(Backpack backpack, UnitHealth health, StatusEffects statuses, float threshold, float cooldown, float potionPower = 0f)
    {
        this.backpack = backpack;
        this.health = health;
        this.statuses = statuses;
        this.threshold = threshold;
        this.cooldown = cooldown;
        this.potionPower = potionPower;
        health.OnHit += RecordHit;
    }

    public void OnBattleStart(bool isBoss)
    {
        if(!isBoss)
            return;

        if(!backpack.TryTakeConsumable(ConsumeType.Haste, out ConsumableInfo potion))
            return;

        float multiplier = 1f + potion.Value / 100f;
        statuses.Apply(new SpeedStatus(health, DamageElement.Haste, multiplier, potion.Duration));
        Sfx.Play(SfxType.PotionDrink);
    }

    private bool TryHeal()
    {
        if(!IsInDanger())
            return false;

        if(!backpack.TryTakeConsumable(ConsumeType.Heal, out ConsumableInfo potion))
            return false;

        health.Heal(Mathf.RoundToInt(health.MaxHp * potion.Value / 100f * (1f + potionPower)));
        Sfx.Play(SfxType.PotionDrink);
        return true;
    }

    private bool TryCure()
    {
        if(!statuses.Has(health, DamageElement.Poison))
            return false;

        if(!backpack.TryTakeConsumable(ConsumeType.Cure, out _))
            return false;

        statuses.Remove(health, DamageElement.Poison);
        Sfx.Play(SfxType.PotionDrink);
        return true;
    }
    public void Tick(float dt)
    {
        healTimer -= dt;
        cureTimer -= dt;

        if(health.IsDead)
            return;

        if(healTimer <= 0f && TryHeal())
            healTimer = cooldown;

        if(cureTimer <= 0f && TryCure())
            cureTimer = cooldown;
    }

    public void Dispose()
    {
        health.OnHit -= RecordHit;
    }

    private void RecordHit(DamageInfo info)
    {
        if(info.IsTick)
            return;

        biggestHit = Mathf.Max(biggestHit, info.Amount);
    }

    private bool IsInDanger()
    {
        if(health.MaxHp <= 0)
            return false;

        float safeLine = Mathf.Max(health.MaxHp * threshold, biggestHit * 1.1f);
        return health.CurrentHP <= safeLine;
    }
}
