using System.Collections.Generic;

public enum ConquestResult
{
    Replay,
    Collapsed,
    TowersRisen,
    OriginRisen,
}

public class TowerProgress
{
    private readonly IReadOnlyList<TowerInfo> towers;
    private readonly HashSet<int> cleared = new();
    private readonly HashSet<int> collapsed = new();
    private readonly HashSet<int> fallen = new();

    public TowerInfo Prologue { get; }
    public TowerInfo Origin { get; }
    public IReadOnlyList<TowerInfo> Towers => towers;
    public int OriginBestFloor { get; private set; }
    public bool IsPrologueCollapsed => Prologue != null && collapsed.Contains(Prologue.Id);
    public bool IsFreeMode => IsOriginRisen;

    public bool IsOriginRisen
    {
        get
        {
            int themed = 0;

            foreach(TowerInfo tower in towers)
            {
                if(!tower.IsThemed)
                    continue;

                themed++;

                if(!collapsed.Contains(tower.Id))
                    return false;
            }

            return themed > 0;
        }
    }

    public TowerProgress(IReadOnlyList<TowerInfo> towers, TowerInfo prologue, TowerInfo origin, SaveData data)
    {
        this.towers = towers;
        Prologue = prologue;
        Origin = origin;
        OriginBestFloor = data.originBestFloor;

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
        switch(tower.Kind)
        {
            case TowerKind.Prologue:
                return true;
            case TowerKind.Themed:
                return IsPrologueCollapsed;
            default:
                return IsOriginRisen;
        }
    }

    public bool CanEnter(TowerInfo tower)
    {
        return IsRisen(tower) && !IsCollapsed(tower);
    }

    public bool IsAbyssOpen(TowerInfo tower)
    {
        return tower.HasAbyss && cleared.Contains(tower.Id);
    }

    public int GetFinalFloor(TowerInfo tower)
    {
        if(IsAbyssOpen(tower))
            return tower.AbyssFloor;

        return tower.IsEndless ? tower.LastFloor : tower.TopFloor;
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
        if(tower.IsEndless || IsCollapsed(tower))
            return ConquestResult.Replay;

        collapsed.Add(tower.Id);
        fallen.Add(hero.Id);

        if(IsOriginRisen)
            return ConquestResult.OriginRisen;

        if(tower.IsPrologue)
            return ConquestResult.TowersRisen;

        return ConquestResult.Collapsed;
    }

    public bool ReportFloor(TowerInfo tower, int floor)
    {
        if(!tower.IsEndless || floor <= OriginBestFloor)
            return false;

        OriginBestFloor = floor;
        return true;
    }

    public void WriteTo(SaveData data)
    {
        data.clearedTowers = new List<int>(cleared);
        data.collapsedTowers = new List<int>(collapsed);
        data.fallenClasses = new List<int>(fallen);
        data.originBestFloor = OriginBestFloor;
    }
    public void ResetProgress()
    {
        cleared.Clear();
        collapsed.Clear();
        fallen.Clear();
        OriginBestFloor = 0;
    }
}