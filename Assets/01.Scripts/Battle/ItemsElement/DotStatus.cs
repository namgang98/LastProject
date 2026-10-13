using UnityEngine;

public abstract class DotStatus : IStatusEffect
{
    private readonly int damagePerTick;
    private readonly float tickInterval;
    private float remaining;
    private float tickTimer;
    private float total;
    
    public UnitHealth Target { get; }

    public float Duration { get; }

    public float SpeedMultiplier => 1f;
    public abstract DamageElement Element { get; }

    public float Progress => total <= 0f ? 0f : remaining / total;

    protected DotStatus(int damagePerTick, float tickInterval,UnitHealth target, float duration)
    {
        this.damagePerTick = damagePerTick;
        this.tickInterval = tickInterval;
        total = duration;
        Target = target;
        Duration = duration;
        remaining = duration;
        tickTimer = tickInterval;
    }
    public void Refresh(float duration)
    {
        if(duration <= remaining)
            return;
        
        remaining = duration;
        total = duration;
    }

    public bool Tick(float deltaTime)
    {
        remaining -= deltaTime;
        tickTimer -= deltaTime;
        if (tickTimer <= 0)
        {
            Target.TakeDamage(new DamageInfo(damagePerTick, false, Element, true));
            tickTimer += tickInterval;
        }

        return remaining > 0 && !Target.IsDead;
    }
}
