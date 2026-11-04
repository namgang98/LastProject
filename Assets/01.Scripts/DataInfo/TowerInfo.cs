using UnityEngine;
using System.Collections.Generic;

public enum TowerKind
{
    Prologue,
    Themed,
    Origin,
}

public class TowerInfo
{
    private readonly List<int> stageSources = new();
    private readonly List<TowerRuleInfo> rules;

    public int Id { get; }
    public string Name { get; }
    public string Description { get; }
    public int TopFloor { get; }
    public int AbyssFloor { get; }
    private const int OffsetRampFloors = 10;

    public int LevelOffset { get; }
    public TowerKind Kind { get; }

    public bool IsPrologue => Kind == TowerKind.Prologue;
    public bool IsThemed => Kind == TowerKind.Themed;
    public bool IsEndless => Kind == TowerKind.Origin;
    public bool HasAbyss => !IsEndless && AbyssFloor > TopFloor;
    public int LastFloor => IsEndless ? int.MaxValue : HasAbyss ? AbyssFloor : TopFloor;
    public IReadOnlyList<int> StageSources => stageSources;
    public IReadOnlyList<TowerRuleInfo> Rules => rules;

    public TowerInfo(int id, string name, string description, int topFloor, int abyssFloor, TowerKind kind, List<TowerRuleInfo> rules = null, int levelOffset = 0)
    {
        Id = id;
        Name = name;
        Description = description;
        TopFloor = topFloor;
        AbyssFloor = abyssFloor;
        Kind = kind;
        LevelOffset = levelOffset;
        this.rules = rules ?? new List<TowerRuleInfo>();
    }

    public bool UsesStagesOf(int towerId)
    {
        return stageSources.Contains(towerId);
    }

    public int ToScalingFloor(int floor)
    {
        float ramp = Mathf.Clamp01((floor - 1) / (float)OffsetRampFloors);
        return floor + Mathf.RoundToInt(LevelOffset * ramp);
    }

    public int ToStageFloor(int floor)
    {
        if(!IsEndless)
            return floor;

        return (floor - 1) % TopFloor + 1;
    }

    internal void AddStageSource(int towerId)
    {
        if(stageSources.Contains(towerId))
            return;

        stageSources.Add(towerId);
    }
}
