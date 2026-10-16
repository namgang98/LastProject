public static class BattleEffectFactory
{
    public static IBattleEffect Create(EffectData data, UnitHealth owner)
    {
        switch(data.Type)
        {
            case EffectType.Heal:
                return new HealEffect(owner, data.Value);
            case EffectType.Shield:
                return new ShieldEffect(owner, data.Value);
            default:
                return null;
        }
    }
}