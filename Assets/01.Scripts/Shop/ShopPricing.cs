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
    public int BuyPrice(ItemInfo info)
    {
        return Mathf.Max(1, Mathf.RoundToInt(info.SellPrice * buyMultplier));
    }
    public int SellPrice(ItemInfo info)
    {
        return info.SellPrice;
    }

}
