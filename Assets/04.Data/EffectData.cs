public enum EffectType
{
    None,
    Heal,
    Shield,
}

public readonly struct EffectData
{
    public readonly EffectType Type;
    public readonly int Value;
    public readonly float Cooldown;

    public EffectData(EffectType type, int value, float cooldown)
    {
        Type = type;
        Value = value;
        Cooldown = cooldown;
    }

    public bool HasEffect => Type != EffectType.None;

    public string Describe()
    {
        switch(Type)
        {
            case EffectType.Heal:
                return $"{Cooldown:0.#}초마다 체력 {Value} 회복";
            case EffectType.Shield:
                return $"{Cooldown:0.#}초마다 실드 {Value}회";
            default:
                return string.Empty;
        }
    }
}
