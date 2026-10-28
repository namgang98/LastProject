using System.Collections.Generic;

public enum ConquestResult
{
    Replay,
    Collapsed,
    TowersRisen,
    Ending,
}

public class TowerProgress
{
    private readonly IReadOnlyList<TowerInfo> towers;
    private readonly HashSet<int> cleared = new();
    private readonly HashSet<int> collapsed = new();
    private readonly HashSet<int> fallen = new();

    public TowerInfo Prologue { get; }
    public IReadOnlyList<TowerInfo> Towers => towers;
    public bool IsFreeMode => towers.Count > 0 && collapsed.Count >= towers.Count;
    public bool IsPrologueCollapsed => Prologue != null && collapsed.Contains(Prologue.Id);

    public TowerProgress(IReadOnlyList<TowerInfo> towers, TowerInfo prologue, SaveData data)
    {
        this.towers = towers;
        Prologue = prologue;

        cleared.UnionWith(data.clearedTowers);
        collapsed.UnionWith(data.collapsedTowers);
        fallen.UnionWith(data.fallenClasses);

        if(prologue != null && data.clearCount > 0)
            cleared.Add(prologue.Id);
    }

    public bool TryGetTower(int id, out TowerInfo info)
    {
        foreach(TowerInfo tower in towers)
        {
            if(tower.Id != id)
                continue;

            info = tower;
            return true;
        }

        info = null;
        return false;
    }

    public bool IsCollapsed(TowerInfo tower)
    {
        return collapsed.Contains(tower.Id);
    }

    public bool IsRisen(TowerInfo tower)
    {
        return tower.IsPrologue || IsPrologueCollapsed;
    }

    public bool CanEnter(TowerInfo tower)
    {
        return IsRisen(tower) && (IsFreeMode || !IsCollapsed(tower));
    }

    public bool IsAbyssOpen(TowerInfo tower)
    {
        return tower.HasAbyss && cleared.Contains(tower.Id);
    }

    public int GetFinalFloor(TowerInfo tower)
    {
        return IsAbyssOpen(tower) ? tower.AbyssFloor : tower.TopFloor;
    }

    public bool IsFallen(ClassInfo info)
    {
        return !IsFreeMode && fallen.Contains(info.Id);
    }

    public bool MarkCleared(TowerInfo tower)
    {
        return cleared.Add(tower.Id);
    }

    public ConquestResult Conquer(TowerInfo tower, ClassInfo hero)
    {
        if(IsFreeMode || IsCollapsed(tower))
            return ConquestResult.Replay;

        collapsed.Add(tower.Id);
        fallen.Add(hero.Id);

        if(IsFreeMode)
            return ConquestResult.Ending;

        if(tower.IsPrologue)
            return ConquestResult.TowersRisen;

        return ConquestResult.Collapsed;
    }

    public void WriteTo(SaveData data)
    {
        data.clearedTowers = new List<int>(cleared);
        data.collapsedTowers = new List<int>(collapsed);
        data.fallenClasses = new List<int>(fallen);
    }
}