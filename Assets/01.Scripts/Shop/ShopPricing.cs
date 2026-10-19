using UnityEngine;

public class ShopPricing
{
    private readonly float buyMultplier;
    public int RerollCost;
    public ShopPricing(float buyMultplier, int rerollCost)
    {
        this.buyMultplier = buyMultplier;
        RerollCost = rerollCost;
    }
    public int BuyPrice(ShopStockInfo stock)
    {
        if(stock.Price > 0)
            return stock.Price;

        return Mathf.Max(1, Mathf.RoundToInt(stock.Item.SellPrice * buyMultplier));
    }
    public int SellPrice(ItemInfo info)
    {
        return info.SellPrice;
    }

}
