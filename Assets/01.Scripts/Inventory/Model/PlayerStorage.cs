using System.Collections.Generic;
using UnityEngine;

public class PlayerStorage : IMaterialStorage
{
    private readonly StarterKit kit;
    private readonly List<ForgeableInstance> forgeBuffer = new();

    public Backpack Backpack { get; private set; }
    public Chest Chest { get; }

    public int MaxForgeLevel
    {
        get
        {
            CollectForgeables(forgeBuffer);
            int max = 0;

            foreach(ForgeableInstance item in forgeBuffer)
            {
                max = Mathf.Max(max, item.Level);
            }

            return max;
        }
    }

    public PlayerStorage(StarterKit kit, Vector2Int tabSize)
    {
        this.kit = kit;
        Chest = new Chest(tabSize);
    }

    #region 저장
    public bool TryLoad(SaveData data, int startTabCount)
    {
        LoadChest(data.chestTabs, startTabCount);
        return TryLoadBackpack(data.backpack);
    }

    public void WriteTo(SaveData data)
    {
        data.chestTabs.Clear();

        foreach(GridContainer tab in Chest.Tabs)
        {
            data.chestTabs.Add(InventorySaveMapper.ToData(tab));
        }

        data.backpack = InventorySaveMapper.ToData(Backpack);
    }

    private bool TryLoadBackpack(BackpackSaveData saved)
    {
        var errors = new List<string>();

        if(saved.bagId != 0 && InventorySaveMapper.TryRestore(saved, errors, out Backpack restored))
        {
            LogErrors("가방", errors);
            Backpack = restored;
            return true;
        }

        if(!kit.TryCreate(out Backpack created))
            return false;

        Backpack = created;
        return true;
    }

    private void LoadChest(List<GridSaveData> savedTabs, int startTabCount)
    {
        var errors = new List<string>();
        Chest.EnsureTabCount(Mathf.Max(startTabCount, savedTabs.Count));

        for(int i = 0; i < savedTabs.Count; i++)
        {
            InventorySaveMapper.Restore(savedTabs[i], Chest.Tabs[i], errors);
        }

        LogErrors("창고", errors);
    }

    private static void LogErrors(string label, List<string> errors)
    {
        foreach(string error in errors)
        {
            Debug.LogWarning($"[PlayerStorage] {label} : {error}");
        }
    }
    #endregion

    #region 가방
    public bool TrySwapBag(BagInstance newBag)
    {
        GridContainer from = newBag.Owner;

        if(from == null || newBag == Backpack.Bag)
            return false;

        Vector2Int anchor = newBag.Anchor;
        int rotation = newBag.Rotation;
        BagInstance oldBag = Backpack.Bag;

        from.Remove(newBag);

        if(!from.TryPlace(oldBag, anchor, rotation) && !from.TryAutoPlace(oldBag))
        {
            from.TryPlace(newBag, anchor, rotation);
            return false;
        }

        Backpack old = Backpack;
        Backpack = new Backpack(newBag);

        PouchInstance pouch = old.UnequipPouch();

        if(pouch != null)
            Backpack.TryEquipPouch(pouch);

        EventManager.instance.PostNotification(new OnBackpackChanged(Backpack));
        return true;
    }

    public void LoseOnDeath()
    {
        PouchInstance pouch = Backpack.LoseAllOnDeath();

        if(!kit.TryCreate(out Backpack newPack))
            return;

        if(pouch != null)
            newPack.TryEquipPouch(pouch);

        Backpack = newPack;
    }

    public void GiveStarterWeapon(ClassInfo info)
    {
        WeaponInfo starter = info.StarterWeapon;

        if(starter == null || HasWeapon(info.Type))
            return;

        if(TryPlaceInBackpack(starter.CreateInstance()))
            return;

        Debug.LogWarning($"[PlayerStorage] {starter.Name}을(를) 넣을 자리가 없습니다.");
    }

    public bool TryReceive(ItemInstance item)
    {
        if(Backpack.EquippedPouch == null && Backpack.TryEquipPouch(item))
            return true;

        if(TryPlaceInBackpack(item))
            return true;

        return Chest.TryStore(item);
    }

    private bool TryPlaceInBackpack(ItemInstance item)
    {
        if(Backpack.Grid.TryAutoPlace(item))
            return true;

        return Backpack.PouchStorage != null && Backpack.PouchStorage.TryAutoPlace(item);
    }

    private bool HasWeapon(WeaponType type)
    {
        foreach(ItemInstance item in Backpack.EquippedItems())
        {
            if(item.Info is WeaponInfo weapon && weapon.Type == type)
                return true;
        }

        return false;
    }
    #endregion

    #region 검색
    public int CountConsumable(ConsumeType kind)
    {
        return Backpack.CountConsumable(kind) + Chest.CountConsumable(kind);
    }

    public bool TryTakeConsumable(ConsumeType kind, out ConsumableInfo info)
    {
        if(Backpack.TryTakeConsumable(kind, out info))
            return true;

        return Chest.TryTakeConsumable(kind, out info);
    }

    public int CountItem(int itemId)
    {
        int count = 0;

        foreach(GridContainer grid in AllGrids())
        {
            count += ItemSearch.Count(grid, itemId);
        }

        return count;
    }

    public void TakeItems(int itemId, int amount)
    {
        foreach(GridContainer grid in AllGrids())
        {
            if(amount <= 0)
                return;

            amount -= ItemSearch.Take(grid, itemId, amount);
        }
    }

    public void CollectForgeables(List<ForgeableInstance> results)
    {
        results.Clear();

        foreach(GridContainer grid in AllGrids())
        {
            foreach(ItemInstance item in grid.Items)
            {
                if(item is ForgeableInstance forged)
                    results.Add(forged);
            }
        }
    }

    private IEnumerable<GridContainer> AllGrids()
    {
        yield return Backpack.Grid;

        if(Backpack.PouchStorage != null)
            yield return Backpack.PouchStorage;

        foreach(GridContainer tab in Chest.Tabs)
        {
            yield return tab;
        }
    }
    #endregion
}