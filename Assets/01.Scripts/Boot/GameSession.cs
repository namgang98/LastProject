using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GameSession : Singleton<GameSession>, IMaterialStorage, IQuestContext
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

    public Shop Shop { get; private set; }
    public UpgradeService Upgrades { get; private set; }
    private static readonly UpgradeTrack[] DonationTracks = { UpgradeTrack.Shop, UpgradeTrack.Chest };
    private static readonly UpgradeTrack[] FavorTracks = { UpgradeTrack.FavorMerchant, UpgradeTrack.FavorTrainer, UpgradeTrack.FavorBlacksmith };
    public NpcFavor Favor { get; private set; }
    public ClassTraining Training { get; private set; }
    public Chest Chest { get; private set; }
    private SaveService saveService;
    private SaveData saveData;

    public ClassInfo CurrentClass { get; private set; }
    public Backpack Backpack { get; private set; }
    public Wallet Wallet { get; private set; }
    public QuestBook Quests { get; private set; }
    public ForgeService Forge { get; private set; }

    private readonly List<ForgeableInstance> forgeBuffer = new();
    public int BestFloor => Training.BestFloor;
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
    public TowerProgress Towers { get; private set; }
    public TowerInfo CurrentTower { get; private set; }
    public CutsceneKind PendingCutscene { get; private set; }

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
        LoadTowers();
        EnsurePlayableClass();
        LoadQuests();
        Forge = new ForgeService(DataManager.instance.Forge, () => Favor.GetRate(NpcType.Blacksmith));

        if(!saveData.inRun)
            return true;

        LoseBackpack();
        Save(false);
        return true;
    }

    public void BeginRun()
    {
        EnsureStarterWeapon();
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

        if(reason == RunEndReason.Victory)
            HandleTopReached();

        if(reason == RunEndReason.Conquest)
            HandleConquest();

        Shop.Restock();
        Save(false);
    }
    private void HandleTopReached()
    {
        saveData.clearCount++;

        if(Towers.MarkCleared(CurrentTower))
            PendingCutscene = CutsceneKind.AbyssRise;
    }

    private void HandleConquest()
    {
        saveData.abyssClearCount++;
        PendingCutscene = ToCutscene(Towers.Conquer(CurrentTower, CurrentClass));
        EnsurePlayableClass();
    }

    private static CutsceneKind ToCutscene(ConquestResult result)
    {
        switch(result)
        {
            case ConquestResult.Collapsed:
                return CutsceneKind.Collapse;
            case ConquestResult.TowersRisen:
                return CutsceneKind.TowersRise;
            case ConquestResult.Ending:
                return CutsceneKind.Ending;
            default:
                return CutsceneKind.None;
        }
    }

    public CutsceneKind ConsumeCutscene()
    {
        CutsceneKind kind = PendingCutscene;
        PendingCutscene = CutsceneKind.None;
        return kind;
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
            saveData.shopOffers.Add(offer != null ? offer.ItemId : 0);
        }
        saveData.classId = CurrentClass.Id;
        saveData.gold = Wallet.Gold;
        Training.WriteTo(saveData);
        Towers.WriteTo(saveData);
        saveData.towerId = CurrentTower != null ? CurrentTower.Id : 0;
        Quests.WriteTo(saveData.quests);
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
        var pricing = new ShopPricing(buyMultiplier, rerollCost, () => Favor.GetRate(NpcType.Merchant));
        var catalog = new ShopCatalog(DataManager.instance.ShopStocks);

        int level = 1;
        int slotCount = shopSlotCount;

        if(Upgrades.TryGet(UpgradeTrack.Shop, out UpgradeProgress progress))
        {
            level = progress.Level;
            slotCount = progress.Current.Value;
            progress.LevelUp -= HandleShopLevelUp;
            progress.LevelUp += HandleShopLevelUp;
        }

        Shop = new Shop(catalog, pricing, level, slotCount);

        if(saveData.shopOffers.Count == 0)
        {
            Shop.Restock();
            return;
        }

        var saved = new List<ShopStockInfo>();

        foreach(int id in saveData.shopOffers)
        {
            catalog.TryFind(id, out ShopStockInfo stock);
            saved.Add(stock);
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
        RegisterTracks(DonationTracks);
        RegisterTracks(FavorTracks);
        Favor = new NpcFavor(Upgrades);
    }

    private void RegisterTracks(UpgradeTrack[] tracks)
    {
        foreach(UpgradeTrack track in tracks)
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
        Training = new ClassTraining(saveData, costs, DataManager.instance.TryGetTrainingStat, () => 1f - Favor.GetRate(NpcType.Trainer));

        if(Training.IsUnlocked(CurrentClass))
            return;

        if(DataManager.instance.TryGetClass(defaultClassId, out ClassInfo starter))
            CurrentClass = starter;
    }

    private void LoadQuests()
    {
        Quests = new QuestBook(DataManager.instance.Quests, saveData.quests, this);
    }
    public int CountItem(int itemId)
    {
        int count = ItemSearch.Count(Backpack.Grid, itemId);

        if(Backpack.PouchStorage != null)
            count += ItemSearch.Count(Backpack.PouchStorage, itemId);

        foreach(GridContainer tab in Chest.Tabs)
        {
            count += ItemSearch.Count(tab, itemId);
        }

        return count;
    }

    public QuestClaimResult TryClaimQuest(QuestProgress quest, out int favorLevelUps)
    {
        favorLevelUps = 0;

        if(!Quests.IsVisible(quest) || !Quests.IsReady(quest))
            return QuestClaimResult.NotReady;

        QuestInfo info = quest.Info;
        var given = new List<ItemInstance>();

        if(info.HasRewardItem)
        {
            for(int i = 0; i < info.RewardCount; i++)
            {
                ItemInstance item = info.RewardItem.CreateInstance();

                if(TryReceive(item))
                {
                    given.Add(item);
                    continue;
                }

                foreach(ItemInstance placed in given)
                {
                    placed.Owner?.Remove(placed);
                }

                return QuestClaimResult.NoSpace;
            }
        }

        if(info.Type == QuestType.Deliver)
            TakeItems(info.Target, info.Count);

        Wallet.Earn(info.RewardGold);
        favorLevelUps = Favor.AddFavor(info.Npc, info.Favor);
        Quests.MarkClaimed(quest);
        SaveInVillage();
        return QuestClaimResult.Ok;
    }

    public void TakeItems(int itemId, int amount)
    {
        amount -= ItemSearch.Take(Backpack.Grid, itemId, amount);

        if(amount > 0 && Backpack.PouchStorage != null)
            amount -= ItemSearch.Take(Backpack.PouchStorage, itemId, amount);

        foreach(GridContainer tab in Chest.Tabs)
        {
            if(amount <= 0)
                return;

            amount -= ItemSearch.Take(tab, itemId, amount);
        }
    }
    public void CollectForgeables(List<ForgeableInstance> results)
    {
        results.Clear();
        AddForgeables(Backpack.Grid, results);

        if(Backpack.PouchStorage != null)
            AddForgeables(Backpack.PouchStorage, results);

        foreach(GridContainer tab in Chest.Tabs)
        {
            AddForgeables(tab, results);
        }
    }

    private static void AddForgeables(GridContainer grid, List<ForgeableInstance> results)
    {
        foreach(ItemInstance item in grid.Items)
        {
            if(item is ForgeableInstance forged)
                results.Add(forged);
        }
    }
    private void LoadTowers()
    {
        DataManager data = DataManager.instance;
        Towers = new TowerProgress(data.Towers, data.PrologueTower, saveData);

        if(Towers.TryGetTower(saveData.towerId, out TowerInfo saved) && Towers.CanEnter(saved))
        {
            CurrentTower = saved;
            return;
        }

        CurrentTower = data.PrologueTower;
    }

    public void SelectTower(TowerInfo tower)
    {
        if(tower == null || !Towers.CanEnter(tower))
            return;

        CurrentTower = tower;
    }

    public ClassAvailability GetAvailability(ClassInfo info)
    {
        if(Towers.IsFallen(info))
            return ClassAvailability.Fallen;

        if(!Training.IsUnlocked(info))
            return ClassAvailability.Locked;

        return ClassAvailability.Available;
    }

    private void EnsurePlayableClass()
    {
        if(GetAvailability(CurrentClass) == ClassAvailability.Available)
            return;

        var classes = new List<ClassInfo>(DataManager.instance.Classes);
        classes.Sort((a, b) => a.Id.CompareTo(b.Id));

        foreach(ClassInfo info in classes)
        {
            if(GetAvailability(info) != ClassAvailability.Available)
                continue;

            CurrentClass = info;
            return;
        }
    }
    private void EnsureStarterWeapon()
    {
        WeaponInfo starter = CurrentClass.StarterWeapon;

        if (starter == null || HasClassWeapon(CurrentClass.Type))
            return;

        ItemInstance weapon = starter.CreateInstance();

        if (Backpack.Grid.TryAutoPlace(weapon))
            return;

        if (Backpack.PouchStorage != null && Backpack.PouchStorage.TryAutoPlace(weapon))
            return;

        Debug.LogWarning($"[GameSession] {starter.Name}을(를) 넣을 자리가 없습니다.", this);
    }

    private bool HasClassWeapon(WeaponType type)
    {
        foreach (ItemInstance item in Backpack.EquippedItems())
        {
            if (item.Info is WeaponInfo weapon && weapon.Type == type)
                return true;
        }

        return false;
    }
}
