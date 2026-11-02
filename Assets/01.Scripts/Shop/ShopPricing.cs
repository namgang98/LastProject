using System;
using UnityEngine;

public class ShopPricing
{
    private readonly float buyMultplier;
    private readonly Func<float> favorRate;
    public int RerollCost;

    public float FavorRate => favorRate == null ? 0f : Mathf.Clamp01(favorRate());

    public ShopPricing(float buyMultplier, int rerollCost, Func<float> favorRate = null)
    {
        this.buyMultplier = buyMultplier;
        this.favorRate = favorRate;
        RerollCost = rerollCost;
    }

    public int BuyPrice(ShopStockInfo stock)
    {
        int basePrice = stock.Price > 0 ? stock.Price : Mathf.RoundToInt(stock.Item.SellPrice * buyMultplier);
        return Mathf.Max(1, Mathf.RoundToInt(basePrice * (1f - FavorRate)));
    }

    public int SellPrice(ItemInfo info)
    {
        return Mathf.RoundToInt(info.SellPrice * (1f + FavorRate));
    }

}
