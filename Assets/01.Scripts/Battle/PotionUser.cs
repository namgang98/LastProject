using UnityEngine;

public class PotionUser
{
    private readonly Backpack backpack;
    private readonly UnitHealth health;
    private readonly StatusEffects statuses;
    private readonly float threshold;
    private readonly float cooldown;
    private float timer;

    private readonly float potionPower;

    public PotionUser(Backpack backpack, UnitHealth health, StatusEffects statuses, float threshold, float cooldown, float potionPower = 0f)
    {
        this.backpack = backpack;
        this.health = health;
        this.statuses = statuses;
        this.threshold = threshold;
        this.cooldown = cooldown;
        this.potionPower = potionPower;
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

    public void Tick(float dt)
    {
        if(timer > 0f)
        {
            timer -= dt;
            return;
        }

        if(health.IsDead)
            return;

        if(TryHeal() || TryCure())
            timer = cooldown;
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

    private bool IsInDanger()
    {
        if(health.MaxHp <= 0)
            return false;

        return health.CurrentHP <= health.MaxHp * threshold;
    }
}
