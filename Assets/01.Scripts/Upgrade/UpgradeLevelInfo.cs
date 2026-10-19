using UnityEngine;
public enum UpgradeTrack
{
    Shop,
    Chest,
    Training,
}
public class UpgradeLevelInfo
{
    public int Id { get; }
    public UpgradeTrack Track { get; }
    public int Level { get; }
    public int Cost { get; }
    public int Value { get; }
    public string Description { get; }

    public UpgradeLevelInfo(int id, UpgradeTrack track, int level, int cost, int value, string desc)
    {
        Id = id;
        Track = track;
        Level = level;
        Cost = cost;
        Value = value;
        Description = desc;
    }
}
