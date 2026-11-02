public interface IRunBonus
{
    float Sum(BlessingStat stat);
    float Lowest(BlessingStat stat);
}

public sealed class EmptyRunBonus : IRunBonus
{
    public static readonly EmptyRunBonus Instance = new();

    public float Sum(BlessingStat stat)
    {
        return 0f;
    }

    public float Lowest(BlessingStat stat)
    {
        return 0f;
    }
}

public interface ISetBonusModifier
{
    int ExtraSetCount { get; }
}