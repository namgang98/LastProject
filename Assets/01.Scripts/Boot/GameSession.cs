using System;
using System.Collections.Generic;
using UnityEngine;

public class GameSession : Singleton<GameSession>
{
    private const string SaveFileName = "save.json";

    [SerializeField] int defaultClassId = 3000;
    [SerializeField] StarterKit kit;
    [SerializeField] Vector2Int chestTabSize = new (10, 8);
    [SerializeField, Min(1)] int startTabCount = 1;

    [Header("상점")]
    [SerializeField, Range(1, Shop.MaxSlots)] int shopSlotCount = 3;
    [SerializeField] float buyMultiplier = 3f;
    [SerializeField] int rerollCost = 20;
    [SerializeField] float shopNormal = 70f;
    [SerializeField] float shopRare = 25f;
    [SerializeField] float shopEpic = 5f;
    [SerializeField] float shopRarePerLevel = 5f;
    [SerializeField] float shopEpicPerLevel = 2f;

    public Shop Shop { get; private set; }
    public UpgradeService Upgrades { get; private set; }
    private static readonly UpgradeTrack[] DonationTracks = { UpgradeTrack.Shop, UpgradeTrack.Chest };
    public ClassTraining Training { get; private set; }
    public Chest Chest { get; private set; }
    private SaveService saveService;
    private SaveData saveData;

    public ClassInfo CurrentClass { get; private set; }
    public Backpack Backpack { get; private set; }
    public Wallet Wallet { get; private set; }

    public bool TryLoad()
    {
        saveService = new SaveService(new FileSaveStorage(SaveFileName));

        if(!saveService.TryLoad(out saveData))
            saveData = new SaveData { classId = defaultClassId };

        if(!DataManager.instance.TryGetClass(saveData.classId, out ClassInfo playerClass))
        {
            Debug.LogError($"[GameSession] 직업 ID {saveData.classId}를 찾을 수 없습니다.", this);
            return false;
        }

        CurrentClass = playerClass;
        Wallet = new Wallet(saveData.gold);
        Wallet.Changed += PostGold;

        if(!TryLoadBackpack())
            return false;
        
        LoadUpgrades();
        LoadChest();
        LoadShop();
        LoadTraining();
        if (!saveData.inRun)
            return true;

        Debug.Log("[GameSession] 런 도중 종료 기록 → 사망 처리", this);
        LoseBackpack();
        Save(false);
        return true;
    }

    public void BeginRun()
    {
        Save(true);
    }
    public void FinishRun(RunEndReason reason)
    {
        if(reason == RunEndReason.Death)
        {
            LoseBackpack();
            Wallet.LoseRun();
        }
        else
        {
            Wallet.CommitRun();
        }

        Shop.Restock();
        Save(false);
    }

    public void RefreshGold()
    {
        PostGold(Wallet);
    }

    public void DeleteSave()
    {
        saveService.Delete();
        Debug.Log($"[GameSession] 세이브 삭제 : {Application.persistentDataPath}", this);
    }

    private bool TryLoadBackpack()
    {
        var errors = new List<string>();

        if(saveData.backpack.bagId != 0 && InventorySaveMapper.TryRestore(saveData.backpack, errors, out Backpack restored))
        {
            foreach(var error in errors)
            {
                Debug.LogWarning($"[GameSession] {error}", this);
            }

            Backpack = restored;
            return true;
        }

        if(!kit.TryCreate(out Backpack created))
            return false;

        Backpack = created;
        return true;
    }

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
    private void LoseBackpack()
    {
        PouchInstance pouch = Backpack.LoseAllOnDeath();

        if(!kit.TryCreate(out Backpack newPack))
            return;

        if(pouch != null)
            newPack.TryEquipPouch(pouch);

        Backpack = newPack;
    }

    private void LoadChest()
    {
        var errors = new List<string>();
        Chest = new Chest(chestTabSize);
        int count = Mathf.Max(startTabCount, saveData.chestTabs.Count);
        Chest.EnsureTabCount(count);

        for(int i = 0; i < saveData.chestTabs.Count; i++)
        {
            InventorySaveMapper.Restore(saveData.chestTabs[i], Chest.Tabs[i], errors);
        }

        foreach(var error in errors)
        {
            Debug.LogWarning($"[GameSession] 창고 : {error}", this);
        }

        if(!Upgrades.TryGet(UpgradeTrack.Chest, out UpgradeProgress progress))
            return;

        progress.LevelUp -= HandleChestLevelUp;
        progress.LevelUp += HandleChestLevelUp;
    }

    private void Save(bool inRun)
    {
        saveData.chestTabs.Clear();
        saveData.shopOffers.Clear();
        saveData.upgrades = Upgrades.ToData();

        foreach(var tab in Chest.Tabs)
        {
            saveData.chestTabs.Add(InventorySaveMapper.ToData(tab));
        }

        foreach(var offer in Shop.Offers)
        {
            saveData.shopOffers.Add(offer != null ? offer.Id : 0);
        }
        saveData.classId = CurrentClass.Id;
        saveData.gold = Wallet.Gold;
        Training.WriteTo(saveData);
        saveData.inRun = inRun;
        saveData.backpack = InventorySaveMapper.ToData(Backpack);
        saveService.Save(saveData);
    }
    public void SaveInVillage()
    {
        Save(false);
    }
    private void PostGold(Wallet wallet)
    {
        EventManager.instance.PostNotification(new OnChangeGold(wallet.Gold, wallet.RunGold));
    }
    public void SelectClass(ClassInfo info)
    {
        if(info == null)
            return;
        CurrentClass = info;
    }
    private void LoadShop()
    {
        var rarity = new DropRarity(shopNormal, shopRare, shopEpic, shopRarePerLevel, shopEpicPerLevel);
        var pricing = new ShopPricing(buyMultiplier, rerollCost);

        int level = 1;
        int slotCount = shopSlotCount;

        if(Upgrades.TryGet(UpgradeTrack.Shop, out UpgradeProgress progress))
        {
            level = progress.Level;
            slotCount = progress.Current.Value;
            progress.LevelUp -= HandleShopLevelUp;
            progress.LevelUp += HandleShopLevelUp;
        }

        Shop = new Shop(new LootTable(DataManager.instance.Items), rarity, pricing, level, slotCount);

        if(saveData.shopOffers.Count == 0)
        {
            Shop.Restock();
            return;
        }

        var saved = new List<ItemInfo>();

        foreach(int id in saveData.shopOffers)
        {
            DataManager.instance.TryGetItem(id, out ItemInfo info);
            saved.Add(info);
        }

        Shop.Restore(saved);
    }

    public bool TryReceive(ItemInstance item)
    {
        if(Backpack.EquippedPouch == null && Backpack.TryEquipPouch(item))
            return true;

        if(Backpack.Grid.TryAutoPlace(item))
            return true;

        if(Backpack.PouchStorage != null && Backpack.PouchStorage.TryAutoPlace(item))
            return true;

        return Chest.TryStore(item);
    }

    private void LoadUpgrades()
    {
        Upgrades = new UpgradeService();

        foreach(UpgradeTrack track in DonationTracks)
        {
            if(!DataManager.instance.TryGetUpgrade(track, out UpgradeTrackInfo info))
            {
                Debug.LogWarning($"[GameSession] 강화 데이터 {track} 가 없습니다.", this);
                continue;
            }

            UpgradeSaveData saved = FindUpgradeSave(track);
            int level = saved != null ? saved.level : 1;
            int deposited = saved != null ? saved.deposited : 0;
            Upgrades.Register(new UpgradeProgress(info, level, deposited));
        }
    }

    private UpgradeSaveData FindUpgradeSave(UpgradeTrack track)
    {
        string key = track.ToString();

        foreach(var data in saveData.upgrades)
        {
            if(data.track == key)
                return data;
        }

        return null;
    }

    private void HandleShopLevelUp(UpgradeProgress progress)
    {
        Shop.ApplyLevel(progress.Level, progress.Current.Value);
        SaveInVillage();
    }
    private int GetUnlockedTabCount()
    {
        if(Upgrades.TryGet(UpgradeTrack.Chest, out UpgradeProgress progress))
            return Mathf.Max(1, progress.Current.Value);

        return startTabCount;
    }

    private void HandleChestLevelUp(UpgradeProgress progress)
    {
        Chest.EnsureTabCount(progress.Current.Value);
        EventManager.instance.PostNotification(new OnChestExpanded(Chest.Tabs.Count));
        SaveInVillage();
    }
    private void LoadTraining()
    {
        DataManager.instance.TryGetUpgrade(UpgradeTrack.Training, out UpgradeTrackInfo costs);
        Training = new ClassTraining(saveData, costs, DataManager.instance.TryGetTrainingStat);

        if (Training.IsUnlocked(CurrentClass))
            return;

        if (DataManager.instance.TryGetClass(defaultClassId, out ClassInfo starter))
            CurrentClass = starter;
    }
}
