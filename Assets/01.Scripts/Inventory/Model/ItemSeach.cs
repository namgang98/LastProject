public static class ItemSearch
{
    public static int Count(GridContainer grid, int itemId)
    {
        int count = 0;

        foreach(ItemInstance item in grid.Items)
        {
            if(item.Info.Id == itemId)
                count++;
        }

        return count;
    }

    public static int Take(GridContainer grid, int itemId, int amount)
    {
        int taken = 0;
        var items = new System.Collections.Generic.List<ItemInstance>(grid.Items);

        foreach(ItemInstance item in items)
        {
            if(taken >= amount)
                break;

            if(item.Info.Id != itemId)
                continue;

            grid.Remove(item);
            taken++;
        }

        return taken;
    }
}