public class SpeedStatus : IStatusEffect
{
    private float remaining;
    private float total;

    public UnitHealth Target { get; }
    public DamageElement Element { get; }
    public float Duration { get; }
    public float SpeedMultiplier { get; }
    public float Progress => total <= 0f ? 0f : remaining / total;

    public SpeedStatus(UnitHealth target, DamageElement element, float multiplier, float duration)
    {
        Target = target;
        Element = element;
        SpeedMultiplier = multiplier;
        Duration = duration;
        total = duration;
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