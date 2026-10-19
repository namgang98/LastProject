public class ShopStockInfo
{
    public int ItemId { get; }
    public int Weight { get; }
    public int MinLevel { get; }
    public int Price { get; }
    public ItemInfo Item { get; private set; }

    public ShopStockInfo(int itemId, int weight, int minLevel, int price)
    {
        ItemId = itemId;
        Weight = weight;
        MinLevel = minLevel;
        Price = price;
    }

    public void SetItem(ItemInfo item)
    {
        if (Item != null)
            return;

        Item = item;
    }
}