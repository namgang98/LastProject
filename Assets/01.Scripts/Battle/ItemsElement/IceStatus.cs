using UnityEngine;

public class IceStatus : IStatusEffect
{
    private readonly float slowRate;
    private float remaining;

    public UnitHealth Target { get; }
    public DamageElement Element => DamageElement.Ice;
    public float Duration { get; }
    public float SpeedMultiplier => 1f - slowRate;

    private float total;
    public float Progress => total <= 0f ? 0f : remaining / total;
    public IceStatus(UnitHealth target, float slowRate, float duration)
    {
        Target = target;
        this.slowRate = Mathf.Clamp(slowRate, 0f, 0.9f);
        total = duration;
        Duration = duration;
        remaining = duration;
    }

    public bool Tick(float deltaTime)
    {
        remaining -= deltaTime;
        return remaining > 0f && !Target.IsDead;
    }

    public void Refresh(float duration)
    {
        if(duration <= remaining)
            return;

        remaining = duration;
        total = duration;
    }
}