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
}
