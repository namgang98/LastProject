using System.Collections.Generic;

public class ForgeLevelInfo
{
    public int Id { get; }
    public int Level { get; }
    public int Cost { get; }
    public float Chance { get; }
    public float Bonus { get; }
    public bool Downgrade { get; }
    public IReadOnlyList<ItemCost> Materials { get; }

    public ForgeLevelInfo(int id, int level, int cost, float chance, float bonus, bool downgrade, List<ItemCost> materials)
    {
        Id = id;
        Level = level;
        Cost = cost;
        Chance = chance;
        Bonus = bonus;
        Downgrade = downgrade;
        Materials = materials ?? new List<ItemCost>();
    }
}
