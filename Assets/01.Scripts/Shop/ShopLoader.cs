using System.Collections.Generic;

public static class ShopLoader
{
    public static Shop Create(ShopPricing pricing, UpgradeProgress progress, int defaultSlots, IReadOnlyList<int> savedOffers)
    {
        var catalog = new ShopCatalog(DataManager.instance.ShopStocks);
        int level = progress != null ? progress.Level : 1;
        int slots = progress != null ? progress.Current.Value : defaultSlots;
        var shop = new Shop(catalog, pricing, level, slots);

        if(savedOffers.Count == 0)
        {
            shop.Restock();
            return shop;
        }

        var saved = new List<ShopStockInfo>();

        foreach(int id in savedOffers)
        {
            catalog.TryFind(id, out ShopStockInfo stock);
            saved.Add(stock);
        }

        shop.Restore(saved);
        return shop;
    }

    public static void WriteTo(Shop shop, List<int> offers)
    {
        offers.Clear();

        foreach(ShopStockInfo offer in shop.Offers)
        {
            offers.Add(offer != null ? offer.ItemId : 0);
        }
    }
}