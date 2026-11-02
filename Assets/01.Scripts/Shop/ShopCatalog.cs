using System.Collections.Generic;
using UnityEngine;

public class ShopCatalog
{
    private readonly List<ShopStockInfo> stocks = new();
    private readonly Dictionary<int, ShopStockInfo> byItem = new();
    private readonly List<ShopStockInfo> candidates = new();

    public ShopCatalog(IEnumerable<ShopStockInfo> source)
    {
        foreach(ShopStockInfo stock in source)
        {
            stocks.Add(stock);
            byItem.TryAdd(stock.ItemId, stock);
        }
    }

    public ShopStockInfo Pick(int level)
    {
        candidates.Clear();
        int total = 0;

        foreach(ShopStockInfo stock in stocks)
        {
            if(stock.MinLevel > level)
                continue;

            candidates.Add(stock);
            total += stock.Weight;
        }

        if(total <= 0)
            return null;

        int roll = Random.Range(0, total);

        foreach(ShopStockInfo stock in candidates)
        {
            if(roll < stock.Weight)
                return stock;

            roll -= stock.Weight;
        }

        return null;
    }

    public bool TryFind(int itemId, out ShopStockInfo stock)
    {
        return byItem.TryGetValue(itemId, out stock);
    }
}