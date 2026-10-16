using System.Collections.Generic;
using UnityEngine;

public class LootTable
{
    private readonly Dictionary<ItemGrade, List<ItemInfo>> table = new();
    public bool IsEmpty => !table.ContainsKey(ItemGrade.Normal);

    public LootTable(IEnumerable<ItemInfo> items)
    {
        foreach(var item in items)
        {
            if(!item.IsLootable)
                continue;
            if(!table.TryGetValue(item.Grade, out List<ItemInfo> list))
            {
                list = new List<ItemInfo>();
                table.Add(item.Grade, list);
            }

            list.Add(item);
        }
    }

    public ItemInfo Pick(ItemGrade grade)
    {
        if(!table.TryGetValue(grade, out List<ItemInfo> list))
            list = table[ItemGrade.Normal];

        return list[Random.Range(0, list.Count)];
    }
}
