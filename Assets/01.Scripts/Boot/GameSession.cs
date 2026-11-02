using System;
using System.Collections.Generic;
using UnityEngine;

public class GameSession : Singleton<GameSession>, IMaterialStorage, IQuestContext
{
    private const string SaveFileName = "save.json";
    private static readonly UpgradeTrack[] SessionTracks =
    {
        UpgradeTrack.Shop,
        UpgradeTrack.Chest,
        UpgradeTrack.FavorMerchant,
        UpgradeTrack.FavorTrainer,
        UpgradeTrack.FavorBlacksmith
    };

    [SerializeField] int defaultClassId = 3000;
    [SerializeField] StarterKit kit;
    [SerializeField] Vector2Int chestTabSize = new(10, 8);
    [SerializeField, Min(1)] int startTabCount = 1;

    [Header("상점")]
    [SerializeField, Range(1, Shop.MaxSlots)] int shopSlotCount = 3;
    [SerializeField] float buyMultiplier = 3f;
    [SerializeField] int rerollCost = 20;

    private SaveService saveService;
    private SaveData saveData;

    public PlayerStorage Storage { get; private set; }
    public ClassSelection Classes { get; private set; }
    public Wallet Wallet { get; private set; }
    public UpgradeService Upgrades { get; private set; }
    public NpcFavor Favor { get; private set; }
    public Shop Shop { get; private set; }
    public ClassTraining Training { get; private set; }
    public TowerProgress Towers { get; private set; }
    public QuestBook Quests { get; private set; }
    public ForgeService Forge { get; private set; }
    public TowerInfo CurrentTower { get; private set; }
    public CutsceneKind PlayingCutscene { get; private set; }
    public CutsceneKind PendingCutscene { get; private set; }

    public Backpack Backpack => Storage.Backpack;
    public Chest Chest => Storage.Chest;
    public ClassInfo CurrentClass => Classes.Current;
    public int BestFloor => Training.BestFloor;
    public int MaxForgeLevel => Storage.MaxForgeLevel;

    #region 불러오기
    public bool TryLoad()
    {
        saveService = new SaveService(new FileSaveStorage(SaveFileName));

        if(!saveService.TryLoad(out saveData))
            saveData = new SaveData { classId = defaultClassId };

        if(!DataManager.instance.TryGetClass(saveData.classId, out ClassInfo savedClass))
        {
            Debug.LogError($"[GameSession] 직업 ID {saveData.classId}를 찾을 수 없습니다.", this);
            return false;
        }

        Wallet = new Wallet(saveData.gold);
        Wallet.Changed += PostGold;
        Storage = new PlayerStorage(kit, chestTabSize);

        if(!Storage.TryLoad(saveData, startTabCount))
            return false;

        LoadUpgrades();
        LoadShop();
        LoadTraining();
        LoadTowers();
        LoadClasses(savedClass);
        Quests = new QuestBook(DataManager.instance.Quests, saveData.quests, this);
        Forge = new ForgeService(DataManager.instance.Forge, () => Favor.GetRate(NpcType.Blacksmith));

        if(!saveData.inRun)
            return true;

        Storage.LoseOnDeath();
        Save(false);
        return true;
    }

    private void LoadUpgrades()
    {
        Upgrades = UpgradeLoader.Load(saveData.upgrades, SessionTracks);
        Favor = new NpcFavor(Upgrades);
        SubscribeLevelUp(UpgradeTrack.Shop, HandleShopLevelUp);
        SubscribeLevelUp(UpgradeTrack.Chest, HandleChestLevelUp);
    }

    private void SubscribeLevelUp(UpgradeTrack track, Action<UpgradeProgress> handler)
    {
        if(!Upgrades.TryGet(track, out UpgradeProgress progress))
            return;

        progress.LevelUp -= handler;
        progress.LevelUp += handler;
    }

    private void LoadShop()
    {
        var pricing = new ShopPricing(buyMultiplier, rerollCost, () => Favor.GetRate(NpcType.Merchant));
        Upgrades.TryGet(UpgradeTrack.Shop, out UpgradeProgress progress);
        Shop = ShopLoader.Create(pricing, progress, shopSlotCount, saveData.shopOffers);
    }

    private void LoadTraining()
    {
        DataManager.instance.TryGetUpgrade(UpgradeTrack.Training, out UpgradeTrackInfo costs);
        Training = new ClassTraining(saveData, costs, DataManager.instance.TryGetTrainingStat, () => 1f - Favor.GetRate(NpcType.Trainer));
    }

    private void LoadTowers()
    {
        DataManager data = DataManager.instance;
        Towers = new TowerProgress(data.Towers, data.PrologueTower, data.OriginTower, saveData);

        if(Towers.TryGetTower(saveData.towerId, out TowerInfo saved) && Towers.CanEnter(saved))
        {
            CurrentTower = saved;
            return;
        }

        CurrentTower = data.PrologueTower;
    }

    private void LoadClasses(ClassInfo savedClass)
    {
        DataManager.instance.TryGetClass(defaultClassId, out ClassInfo fallback);
        Classes = new ClassSelection(Training, Towers, savedClass, fallback);
    }
    #endregion

    #region 저장
    private void Save(bool inRun)
    {
        saveData.classId = CurrentClass.Id;
        saveData.gold = Wallet.Gold;
        saveData.upgrades = Upgrades.ToData();
        saveData.towerId = CurrentTower != null ? CurrentTower.Id : 0;
        saveData.inRun = inRun;
        Storage.WriteTo(saveData);
        ShopLoader.WriteTo(Shop, saveData.shopOffers);
        Training.WriteTo(saveData);
        Towers.WriteTo(saveData);
        Quests.WriteTo(saveData.quests);
        saveService.Save(saveData);
    }

    public void SaveInVillage()
    {
        Save(false);
    }

    public void DeleteSave()
    {
        saveService.Delete();
        Debug.Log($"[GameSession] 세이브 삭제 : {Application.persistentDataPath}", this);
    }
    #endregion

    #region 인게임
    public void BeginRun()
    {
        Storage.GiveStarterWeapon(CurrentClass);
        Save(true);
    }

    public void FinishRun(RunEndReason reason)
    {
        if(reason == RunEndReason.Death)
            Storage.LoseOnDeath();

        if(reason == RunEndReason.Conquest)
            HandleConquest();

        Shop.Restock();
        Save(false);
    }

    private void HandleConquest()
    {
        saveData.abyssClearCount++;
        PendingCutscene = ToCutscene(Towers.Conquer(CurrentTower, CurrentClass));
        Classes.EnsurePlayable();
    }

    private static CutsceneKind ToCutscene(ConquestResult result)
    {
        switch(result)
        {
            case ConquestResult.Collapsed:
                return CutsceneKind.Collapse;
            case ConquestResult.TowersRisen:
                return CutsceneKind.TowersRise;
            case ConquestResult.OriginRisen:
                return CutsceneKind.OriginRise;
            default:
                return CutsceneKind.None;
        }
    }

    public CutsceneKind ConsumeCutscene()
    {
        PlayingCutscene = PendingCutscene;
        PendingCutscene = CutsceneKind.None;
        return PlayingCutscene;
    }

    public void ReportFloor(int floor)
    {
        Training.ReportFloor(floor);
        Towers.ReportFloor(CurrentTower, floor);
    }
    public void ResetTowers()
    {
        Towers.ResetProgress();
        CurrentTower = Towers.Prologue;
        SaveInVillage();
    }
    #endregion

    #region 선택창
    public void SelectClass(ClassInfo info)
    {
        Classes.Select(info);
    }

    public ClassAvailability GetAvailability(ClassInfo info)
    {
        return Classes.GetAvailability(info);
    }

    public void SelectTower(TowerInfo tower)
    {
        if(tower == null || !Towers.CanEnter(tower))
            return;

        CurrentTower = tower;
    }
    #endregion

    #region 창고
    public bool TrySwapBag(BagInstance newBag)
    {
        return Storage.TrySwapBag(newBag);
    }

    public bool TryReceive(ItemInstance item)
    {
        return Storage.TryReceive(item);
    }

    public int CountConsumable(ConsumeType kind)
    {
        return Storage.CountConsumable(kind);
    }

    public bool TryTakeConsumable(ConsumeType kind, out ConsumableInfo info)
    {
        return Storage.TryTakeConsumable(kind, out info);
    }

    public int CountItem(int itemId)
    {
        return Storage.CountItem(itemId);
    }

    public void TakeItems(int itemId, int amount)
    {
        Storage.TakeItems(itemId, amount);
    }

    public void CollectForgeables(List<ForgeableInstance> results)
    {
        Storage.CollectForgeables(results);
    }
    #endregion

    #region 퀘스트
    public QuestClaimResult TryClaimQuest(QuestProgress quest, out int favorLevelUps)
    {
        favorLevelUps = 0;

        if(!Quests.IsVisible(quest) || !Quests.IsReady(quest))
            return QuestClaimResult.NotReady;

        QuestInfo info = quest.Info;

        if(info.HasRewardItem && !TryGiveRewards(info))
            return QuestClaimResult.NoSpace;

        if(info.Type == QuestType.Deliver)
            TakeItems(info.Target, info.Count);

        Wallet.Earn(info.RewardGold);
        favorLevelUps = Favor.AddFavor(info.Npc, info.Favor);
        Quests.MarkClaimed(quest);
        SaveInVillage();
        return QuestClaimResult.Ok;
    }

    private bool TryGiveRewards(QuestInfo info)
    {
        var given = new List<ItemInstance>();

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

            return false;
        }

        return true;
    }
    #endregion

    #region 이벤트
    public void RefreshGold()
    {
        PostGold(Wallet);
    }

    private void PostGold(Wallet wallet)
    {
        EventManager.instance.PostNotification(new OnChangeGold(wallet.Gold));
    }

    private void HandleShopLevelUp(UpgradeProgress progress)
    {
        Shop.ApplyLevel(progress.Level, progress.Current.Value);
        SaveInVillage();
    }

    private void HandleChestLevelUp(UpgradeProgress progress)
    {
        Chest.EnsureTabCount(progress.Current.Value);
        EventManager.instance.PostNotification(new OnChestExpanded(Chest.Tabs.Count));
        SaveInVillage();
    }
    #endregion
}