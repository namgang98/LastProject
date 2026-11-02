public static class ConsumableSearch
{
    public static int Count(GridContainer grid, ConsumeType kind)
    {
        int count = 0;

        foreach(ItemInstance item in grid.Items)
        {
            if(IsKind(item, kind))
                count++;
            else if(item is PotionCaseInstance potionCase)
                count += Count(potionCase.Storage, kind);
        }

        return count;
    }

    public static bool TryTake(GridContainer grid, ConsumeType kind, out ConsumableInfo info)
    {
        if(TryTakeLoose(grid, kind, out info))
            return true;

        foreach(ItemInstance item in grid.Items)
        {
            if(item is not PotionCaseInstance potionCase)
                continue;

            if(TryTakeLoose(potionCase.Storage, kind, out info))
                return true;
        }

        return false;
    }

    private static bool TryTakeLoose(GridContainer grid, ConsumeType kind, out ConsumableInfo info)
    {
        info = null;

        foreach(ItemInstance item in grid.Items)
        {
            if(!IsKind(item, kind))
                continue;

            info = (ConsumableInfo)item.Info;
            grid.Remove(item);
            return true;
        }

        return false;
    }

    private static bool IsKind(ItemInstance item, ConsumeType kind)
    {
        return item.Info is ConsumableInfo consumable && consumable.Kind == kind;
    }
}