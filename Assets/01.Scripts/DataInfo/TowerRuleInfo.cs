public enum TowerRuleType
{
    MonsterDamage,
    MonsterLifesteal,
    MonsterRegen,
    IceAttack,
    PoisonAttack,
    PlayerCooldown,
    FloorPoison,
    MonsterRevive,
}

public readonly struct TowerRuleInfo
{
    public TowerRuleType Type { get; }
    public float Value { get; }
    public float Value2 { get; }
    public float Duration { get; }

    public TowerRuleInfo(TowerRuleType type, float value, float value2, float duration)
    {
        Type = type;
        Value = value;
        Value2 = value2;
        Duration = duration;
    }
}
