using System;
using System.Collections.Generic;
using UnityEngine;

public class Shop
{
    public const int MaxSlots = 9;

    private readonly List<ShopStockInfo> offers = new();
    private readonly ShopCatalog catalog;

    public IReadOnlyList<ShopStockInfo> Offers => offers;
    private readonly LootTable table;
    private readonly DropRarity rarity;

    public ShopPricing Pricing { get; }
    public int Level { get; private set; } = 1;
    public int SlotCount { get; private set; }

    public event Action Changed;

    public Shop(ShopCatalog catalog, ShopPricing pricing, int level, int slotCount)
    {
        this.catalog = catalog;
        Pricing = pricing;
        Level = level;
        SlotCount = Mathf.Clamp(slotCount, 1, MaxSlots);
    }

    public void Restock()
    {
        offers.Clear();

        for(int i = 0; i < SlotCount; i++)
        {
            offers.Add(catalog.Pick(Level));
        }

        Changed?.Invoke();
    }

    public void Restore(IReadOnlyList<ShopStockInfo> saved)
    {
        offers.Clear();

        for(int i = 0; i < SlotCount; i++)
        {
            offers.Add(i < saved.Count ? saved[i] : null);
        }

        Changed?.Invoke();
    }

    public bool TryBuy(int index, Wallet wallet, Func<ItemInstance, bool> receive)
    {
        if(index < 0 || index >= offers.Count)
            return false;

        ShopStockInfo stock = offers[index];

        if(stock == null)
            return false;

        int price = Pricing.BuyPrice(stock);

        if(wallet.Gold < price)
            return false;

        if(!receive(stock.Item.CreateInstance()))
            return false;

        wallet.TrySpend(price);
        offers[index] = null;
        Changed?.Invoke();
        return true;
    }
    public bool TryReroll(Wallet wallet)
    {
        if(!wallet.TrySpend(Pricing.RerollCost))
            return false;

        Restock();
        return true;
    }
    public bool CanSell(ItemInstance item)
    {
        if(item is ContainerInstance container && container.Storage.Items.Count > 0)
            return false;

        return true;
    }

    public bool TrySell(ItemInstance item, Wallet wallet)
    {
        if(!CanSell(item))
            return false;

        wallet.Earn(Pricing.SellPrice(item.Info));
        return true;
    }

    public int GetSellTotal(IEnumerable<LootArea.Entry> entries)
    {
        int total = 0;

        foreach(var entry in entries)
        {
            if(!CanSell(entry.Item))
                continue;

            total += Pricing.SellPrice(entry.Item.Info);
        }

        return total;
    }

    public int SellAll(LootArea basket, Wallet wallet)
    {
        var sold = new List<ItemInstance>();
        int total = 0;

        foreach(var entry in basket.Entries)
        {
            if(!CanSell(entry.Item))
                continue;

            total += Pricing.SellPrice(entry.Item.Info);
            sold.Add(entry.Item);
        }

        foreach(var item in sold)
        {
            basket.Remove(item);
        }

        wallet.Earn(total);
        return total;
    }
    public void ApplyLevel(int level, int slotCount)
    {
        Level = level;
        SlotCount = Mathf.Clamp(slotCount, 1, MaxSlots);

        while(offers.Count < SlotCount)
        {
            offers.Add(catalog.Pick(Level));
        }

        Changed?.Invoke();
    }
}
