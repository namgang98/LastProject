public static class StatusEffectFactory
{
    public static IStatusEffect Create(AttackData data, UnitHealth target)
    {
        switch(data.Element)
        {
            case DamageElement.Poison:
                return new PoisonStatus(target, data.ElementValue, data.ElementDuration);
            case DamageElement.Fire:
                return new FireStatus(target, data.ElementValue, data.ElementDuration);
            case DamageElement.Ice:
                return new SpeedStatus(target, DamageElement.Ice, 1f - UnityEngine.Mathf.Clamp(data.ElementValue / 100f, 0f, 0.9f), data.ElementDuration);
            default:
                return null;
        }
    }
}