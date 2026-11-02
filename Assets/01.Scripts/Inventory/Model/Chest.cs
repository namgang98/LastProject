using System.Collections.Generic;
using UnityEngine;

public class Chest
{
    private readonly List<GridContainer> tabs = new();

    public Vector2Int TabSize { get; }
    public IReadOnlyList<GridContainer> Tabs => tabs;

    public Chest(Vector2Int tabSize)
    {
        TabSize = tabSize;
    }

    public GridContainer AddTab()
    {
        var tab = new GridContainer(TabSize);
        tabs.Add(tab);
        return tab;
    }
    public List<ItemInstance> StoreAll(GridContainer source)
    {
        var items = new List<ItemInstance>(source.Items);
        items.Sort((a, b) => CellCount(b).CompareTo(CellCount(a)));

        var leftover = new List<ItemInstance>();

        foreach(var item in items)
        {
            if(!TryStore(item))
                leftover.Add(item);
        }

        return leftover;
    }
    public void EnsureTabCount(int count)
    {
        while(tabs.Count < count)
        {
            AddTab();
        }
    }

    public bool TryStore(ItemInstance item)
    {
        foreach(var tab in tabs)
        {
            if(tab.TryAutoPlace(item))
                return true;
        }

        return false;
    }
    public bool TryTakeConsumable(ConsumeType kind, out ConsumableInfo info)
    {
        info = null;

        foreach(GridContainer tab in tabs)
        {
            if(ConsumableSearch.TryTake(tab, kind, out info))
                return true;
        }

        return false;
    }

    public int CountConsumable(ConsumeType kind)
    {
        int count = 0;

        foreach(GridContainer tab in tabs)
        {
            count += ConsumableSearch.Count(tab, kind);
        }

        return count;
    }
    private static int CellCount(ItemInstance item)
    {
        return item.Info.Shape.GetRotatedCells(0).Count;
    }
    public bool Owns(GridContainer grid)
    {
        return grid != null && tabs.Contains(grid);
    }
}
