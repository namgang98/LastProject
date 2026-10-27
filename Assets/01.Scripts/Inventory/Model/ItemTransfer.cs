public static class ItemTransfer
{
    public static bool ToBackpack(Backpack backpack, ItemInstance item)
    {
        if(backpack.Grid.TryAutoPlace(item))
            return true;

        return backpack.PouchStorage != null && backpack.PouchStorage.TryAutoPlace(item);
    }

    public static bool ToChest(Chest chest, ItemInstance item, int preferredTab)
    {
        if(preferredTab >= 0 && preferredTab < chest.Tabs.Count && chest.Tabs[preferredTab].TryAutoPlace(item))
            return true;

        return chest.TryStore(item);
    }
}