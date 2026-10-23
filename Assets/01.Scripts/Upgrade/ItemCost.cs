using System.Collections.Generic;
using System.Globalization;

public class ItemCost
{
    public int ItemId { get; }
    public int Count { get; }
    public ItemInfo Item { get; private set; }

    public ItemCost(int itemId, int count)
    {
        ItemId = itemId;
        Count = count;
    }

    internal void SetItem(ItemInfo item)
    {
        Item = item;
    }

    public static bool TryParseList(string raw, out List<ItemCost> costs)
    {
        costs = new List<ItemCost>();

        if(string.IsNullOrWhiteSpace(raw))
            return true;

        foreach(string part in raw.Split('|'))
        {
            string[] pair = part.Trim().ToLowerInvariant().Split('x');

            if(pair.Length != 2)
                return false;

            if(!int.TryParse(pair[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out int id) || id <= 0)
                return false;

            if(!int.TryParse(pair[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out int count) || count <= 0)
                return false;

            costs.Add(new ItemCost(id, count));
        }

        return true;
    }
}
