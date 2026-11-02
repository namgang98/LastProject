using System.Collections.Generic;
using UnityEngine;

public class DataManager : Singleton<DataManager>
{
    #region IdRange
    private readonly struct IdRange
    {
        public readonly int Min;
        public readonly int Max;

        public IdRange(int min, int max)
        {
            Min = min;
            Max = max;
        }

        public bool Contains(int id)
        {
            return id >= Min && id <= Max;
        }
    }
    private static readonly IdRange BagIds = new(100, 103);
    private static readonly IdRange WeaponIds = new(1000, 1999);
    private static readonly IdRange MonsterIds = new(2000,2999);
    private static readonly IdRange ClassIds = new(3000, 3999);
    private static readonly IdRange ArmorIds = new(4000, 4999);
    private static readonly IdRange SetIds = new(6000, 6999);
    private static readonly IdRange AccessoryIds = new(5000, 5999);
    private static readonly IdRange PouchIds = new(9000, 9010);
    private static readonly IdRange UpgradeIds = new(7000, 7999);
    private static readonly IdRange MaterialIds = new(8000, 8499);
    private static readonly IdRange ConsumableIds = new(8500, 8999);
    private static readonly IdRange PotionCaseIds = new(9100, 9199);
    private static readonly IdRange StageIds = new(9500, 9599);
    private static readonly IdRange QuestIds = new(9600, 9699);
    private static readonly IdRange ForgeIds = new(9700, 9799);
    private static readonly IdRange TowerIds = new(9800, 9899);
    private static readonly IdRange BlessingIds = new(9900, 9999);

    #endregion

    #region 필드 + 라이브러리
    [Header("CSV문서")]
    [SerializeField] TextAsset bagCsv;
    [SerializeField] TextAsset weaponCsv;
    [SerializeField] TextAsset monsterCsv;
    [SerializeField] TextAsset classCsv;
    [SerializeField] TextAsset armorCsv;
    [SerializeField] TextAsset accessoryCsv;
    [SerializeField] TextAsset pouchCsv;
    [SerializeField] TextAsset setCsv;
    [SerializeField] TextAsset setBonusCsv;
    [SerializeField] TextAsset upgradeCsv;
    [SerializeField] TextAsset trainingStatCsv;
    [SerializeField] TextAsset materialCsv;
    [SerializeField] TextAsset monsterDropCsv;
    [SerializeField] TextAsset shopStockCsv;
    [SerializeField] TextAsset consumableCsv;
    [SerializeField] TextAsset potionCaseCsv;
    [SerializeField] TextAsset stageCsv;
    [SerializeField] TextAsset questCsv;
    [SerializeField] TextAsset forgeCsv;
    [SerializeField] TextAsset towerCsv;
    [SerializeField] TextAsset blessingCsv;
    [Header("이미지,프리팹")]
    [SerializeField] IconLibrary iconLibrary;
    [SerializeField] PrefabLibrary prefabLibrary;
    [SerializeField] IconLibrary backgroundLibrary;
    #endregion

    #region 딕셔너리모음
    private readonly Dictionary<int, ItemInfo> items = new();
    private readonly Dictionary<int, MonsterInfo> monsters = new();
    private readonly Dictionary<int, ClassInfo> classes = new();
    private readonly Dictionary<int, SetInfo> sets = new();
    private readonly Dictionary<int, List<SetInfo>> setsByItem = new();
    private static readonly List<SetInfo> NoSets = new();
    private readonly Dictionary<UpgradeTrack, UpgradeTrackInfo> upgrades = new();
    private readonly Dictionary<TrainingStat, TrainingStatInfo> trainingStats = new();
    private readonly List<ShopStockInfo> shopStocks = new();
    private readonly List<StageInfo> stages = new();
    private readonly Dictionary<int, QuestInfo> quests = new();
    private readonly ForgeTable forge = new();
    private readonly List<TowerInfo> towers = new();
    private readonly List<BlessingInfo> blessings = new();
    #endregion

    private delegate bool RowParser<T>(CsvRow row, List<string> errors, out T info);

    private bool isLoading;
    public bool IsLoaded { get; private set; }

    #region 읽기전용
    public IReadOnlyCollection<ItemInfo> Items => items.Values;
    public IReadOnlyCollection<MonsterInfo> Monsters => monsters.Values; 
    public IReadOnlyCollection<ClassInfo> Classes => classes.Values;
    public IReadOnlyList<ShopStockInfo> ShopStocks => shopStocks;
    public IReadOnlyList<StageInfo> Stages => stages;
    public IReadOnlyCollection<QuestInfo> Quests => quests.Values;
    public ForgeTable Forge => forge;
    public IReadOnlyList<TowerInfo> Towers => towers;
    public TowerInfo PrologueTower { get; private set; }
    public TowerInfo OriginTower { get; private set; }
    public IReadOnlyList<BlessingInfo> Blessings => blessings;
    #endregion

    #region 초기화
    public async Awaitable InitAsync()
    {
        if(IsLoaded)
            return;

        if(isLoading)
        {
            while(!IsLoaded)
            {
                await Awaitable.NextFrameAsync();
            }

            return;
        }

        isLoading = true;

        #region 메인스레드 ReadText
        string weaponText = ReadText(weaponCsv, nameof(weaponCsv));
        string pouchText = ReadText(pouchCsv, nameof(pouchCsv));
        string bagText = ReadText(bagCsv, nameof(bagCsv));
        string monsterText = ReadText(monsterCsv, nameof(monsterCsv));
        string classText = ReadText(classCsv, nameof(classCsv));
        string armorText = ReadText(armorCsv, nameof(armorCsv));
        string accessoryText = ReadText(accessoryCsv, nameof(accessoryCsv));
        string setText = ReadText(setCsv, nameof(setCsv));
        string setBonusText = ReadText(setBonusCsv, nameof(setBonusCsv));
        string upgradeText = ReadText(upgradeCsv, nameof(upgradeCsv));
        string trainingStatText = ReadText(trainingStatCsv, nameof(trainingStatCsv));
        string materialText = ReadText(materialCsv, nameof(materialCsv));
        string monsterDropText = ReadText(monsterDropCsv, nameof(monsterDropCsv));
        string shopStockText = ReadText(shopStockCsv, nameof(shopStockCsv));
        string consumableText = ReadText(consumableCsv, nameof(consumableCsv));
        string potioncaseText = ReadText(potionCaseCsv, nameof(potionCaseCsv));
        string stageText = ReadText(stageCsv, nameof(stageCsv));
        string questText = ReadText(questCsv, nameof(questCsv));
        string forgeText = ReadText(forgeCsv, nameof(forgeCsv));
        string towerText = ReadText(towerCsv, nameof(towerCsv));
        string blessingText = ReadText(blessingCsv, nameof(blessingCsv));
        #endregion

        await Awaitable.BackgroundThreadAsync();

        #region 백그라운드 스레드 ParseTable
        var errors = new List<string>();
        List<WeaponInfo> weaponList = ParseTable<WeaponInfo>(DataTag.Weapons, weaponText, errors, InfoParser.TryParseWeapon);
        List<PouchInfo> pouchList = ParseTable<PouchInfo>(DataTag.Pouches, pouchText, errors, InfoParser.TryParsePouch);
        List<BagInfo> bagList = ParseTable<BagInfo>(DataTag.Bags, bagText, errors, InfoParser.TryParseBag);
        List<MonsterInfo> monsterList = ParseTable<MonsterInfo>(DataTag.Monsters,monsterText,errors, InfoParser.TryParseMonster);
        List<ClassInfo> classList = ParseTable<ClassInfo>(DataTag.Classes,classText,errors, InfoParser.TryParseClass);
        List<ArmorInfo> armorList = ParseTable<ArmorInfo>(DataTag.Armors, armorText, errors, InfoParser.TryParseArmor);
        List<AccessoryInfo> accessoryList = ParseTable<AccessoryInfo>(DataTag.Accessories, accessoryText, errors, InfoParser.TryParseAccessory);
        List<SetInfo> setList = ParseTable<SetInfo>(DataTag.Sets, setText, errors, InfoParser.TryParseSet);
        List<SetBonusInfo> setBonusList = ParseTable<SetBonusInfo>(DataTag.SetBonuses, setBonusText, errors, InfoParser.TryParseSetBonus);
        List<UpgradeLevelInfo> upgradeList = ParseTable<UpgradeLevelInfo>(DataTag.Upgrades, upgradeText, errors, InfoParser.TryParseUpgrade);
        List<TrainingStatInfo> trainingStatList = ParseTable<TrainingStatInfo>(DataTag.TrainingStats, trainingStatText, errors, InfoParser.TryParseTrainingStat);
        List<MaterialInfo> materialList = ParseTable<MaterialInfo>(DataTag.Materials, materialText, errors, InfoParser.TryParseMaterial);
        List<MonsterDropInfo> monsterDropList = ParseTable<MonsterDropInfo>(DataTag.MonsterDrops, monsterDropText, errors, InfoParser.TryParseMonsterDrop);
        List<ShopStockInfo> shopStockList = ParseTable<ShopStockInfo>(DataTag.ShopStock, shopStockText, errors, InfoParser.TryParseShopStock);
        List<ConsumableInfo> consumableList = ParseTable<ConsumableInfo>(DataTag.Consumables, consumableText, errors, InfoParser.TryParseConsumable);
        List<PotionCaseInfo> potionCaseList = ParseTable<PotionCaseInfo>(DataTag.PotionCases, potioncaseText, errors, InfoParser.TryParsePotionCase);
        List<StageInfo> stageList = ParseTable<StageInfo>(DataTag.Stages, stageText, errors, InfoParser.TryParseStage);
        List<QuestInfo> questList = ParseTable<QuestInfo>(DataTag.Quests, questText, errors, InfoParser.TryParseQuest);
        List<ForgeLevelInfo> forgeList = ParseTable<ForgeLevelInfo>(DataTag.Forge, forgeText, errors, InfoParser.TryParseForge);
        List<TowerInfo> towerList = ParseTable<TowerInfo>(DataTag.Towers, towerText, errors, InfoParser.TryParseTower);
        List<BlessingInfo> blessingList = ParseTable<BlessingInfo>(DataTag.Blessings, blessingText, errors, InfoParser.TryParseBlessing);
        #endregion

        //메인 스레드
        await Awaitable.MainThreadAsync();

        if(iconLibrary != null)
            iconLibrary.Init();
        else
            errors.Add("IconLibrary가 연결되지 않았습니다. 아이콘 없이 진행합니다");
        if(prefabLibrary != null)
            prefabLibrary.Init();
        else
            errors.Add("monsterPrefabs가 연결되지 않았습니다");
        if(backgroundLibrary != null)
            backgroundLibrary.Init();

        #region Register
        RegisterItems(DataTag.Weapons, weaponList, WeaponIds, errors);
        RegisterItems(DataTag.Pouches, pouchList, PouchIds, errors);
        RegisterItems(DataTag.Bags, bagList, BagIds, errors);
        RegisterItems(DataTag.Armors, armorList, ArmorIds, errors);
        RegisterItems(DataTag.Accessories, accessoryList, AccessoryIds, errors);
        RegisterItems(DataTag.Consumables, consumableList, ConsumableIds, errors);
        RegisterItems(DataTag.Materials, materialList, MaterialIds, errors);
        RegisterItems(DataTag.PotionCases, potionCaseList, PotionCaseIds, errors);
        RegisterMonsters(DataTag.Monsters, monsterList, MonsterIds, errors);
        RegisterMonsterDrops(DataTag.MonsterDrops, monsterDropList, errors);
        RegisterClasses(DataTag.Classes, classList, ClassIds, errors);
        RegisterSets(DataTag.Sets, setList, SetIds, errors);
        RegisterSetBonuses(DataTag.SetBonuses, setBonusList, errors);
        RegisterUpgrades(DataTag.Upgrades, upgradeList, UpgradeIds, errors);
        RegisterTrainingStats(DataTag.TrainingStats, trainingStatList, errors);
        RegisterShopStocks(DataTag.ShopStock, shopStockList, errors);
        RegisterTowers(DataTag.Towers, towerList, TowerIds, errors);
        RegisterBlessings(DataTag.Blessings, blessingList, BlessingIds, errors);
        RegisterStages(DataTag.Stages, stageList, StageIds, errors);
        RegisterForge(DataTag.Forge, forgeList, ForgeIds, errors);
        RegisterQuests(DataTag.Quests, questList, QuestIds, errors);
        #endregion

        foreach(var error in errors)
        {
            Debug.LogWarning($"[DataManager] {error}", this);
        }

        Debug.Log($"아이템 {items.Count}, 몬스터 {monsters.Count}, 직업 {classes.Count}, 세트 {sets.Count} / 경고 {errors.Count}", this);
        isLoading = false;
        IsLoaded = true;
    }

    private string ReadText(TextAsset asset, string fieldName)
    {
        if(asset != null)
            return asset.text;

        Debug.LogError($"[DataManager] {fieldName} 이 비어 있습니다. 인스펙터에서 CSV 파일을 연결해주세요.", this);
        return string.Empty;
    }

    private static List<T> ParseTable<T>(string sheetName, string text, List<string> errors, RowParser<T> parser)
    {
        var result = new List<T>();
        CsvTable table = CsvTable.Parse(sheetName, text);

        for(int i = 0; i < table.RowCount; i++)
        {
            if(parser(table.GetRow(i), errors, out T info))
                result.Add(info);
        }

        return result;
    }
    #endregion

    #region 등록 + 검증
    private void RegisterItems<T>(string sheetName, List<T> list, IdRange range, List<string> errors) where T : ItemInfo
    {
        foreach(var info in list)
        {
            if(!range.Contains(info.Id))
            {
                errors.Add($"{sheetName} : ID {info.Id} ({info.Name}) 가 구간 {range.Min}~{range.Max} 밖입니다");
                continue;
            }

            if(items.TryGetValue(info.Id, out ItemInfo existing))
            {
                errors.Add($"{sheetName} : ID {info.Id} 중복 ({existing.Name} / {info.Name}) → 뒤의 것은 무시");
                continue;
            }

            info.SetIcon(FindIcon(sheetName, info.Id, info.IconName, errors));
            items.Add(info.Id, info);
        }
    }
    private void RegisterMonsters(string sheetName, List<MonsterInfo> list, IdRange range, List<string> errors)
    {
        foreach(var info in list)
        {
            if(!range.Contains(info.Id))
            {
                errors.Add($"{sheetName} : ID {info.Id} ({info.Name}) 가 구간 {range.Min}~{range.Max} 밖입니다");
                continue;
            }

            if(monsters.TryGetValue(info.Id, out MonsterInfo existing))
            {
                errors.Add($"{sheetName} : ID {info.Id} 중복 ({existing.Name} / {info.Name}) → 뒤의 것은 무시");
                continue;
            }

            info.SetPrefab(FindPrefab(sheetName, info.Id,info.PrefabName, errors));
            monsters.Add(info.Id, info);
        }
    }
    private void RegisterClasses(string sheetName, List<ClassInfo> list, IdRange range, List<string> errors)
    {
        foreach(var info in list)
        {
            if(!range.Contains(info.Id))
            {
                errors.Add($"{sheetName} : ID {info.Id} ({info.Name}) 가 구간 {range.Min}~{range.Max} 밖입니다");
                continue;
            }

            if(classes.TryGetValue(info.Id, out ClassInfo existing))
            {
                errors.Add($"{sheetName} : ID {info.Id} 중복 ({existing.Name} / {info.Name}) → 뒤의 것은 무시");
                continue;
            }

            info.SetPrefab(FindPrefab(sheetName, info.Id, info.PrefabName, errors));
            info.SetIcon(FindIcon(sheetName, info.Id, info.IconName, errors));

            if(info.StarterWeaponId != 0)
            {
                if(TryGetItem(info.StarterWeaponId, out WeaponInfo starter))
                    info.SetStarterWeapon(starter);
                else
                    errors.Add($"{sheetName} : 직업 {info.Name} 의 시작 무기 {info.StarterWeaponId} 가 없거나 무기가 아닙니다");
            }

            classes.Add(info.Id, info);
        }
    }
    private void RegisterSets(string sheetName, List<SetInfo> list, IdRange range, List<string> errors)
    {
        foreach(var info in list)
        {
            if(!range.Contains(info.Id))
            {
                errors.Add($"{sheetName} : ID {info.Id} ({info.Name}) 가 구간 {range.Min}~{range.Max} 밖입니다");
                continue;
            }

            if(sets.TryGetValue(info.Id, out SetInfo existing))
            {
                errors.Add($"{sheetName} : ID {info.Id} 중복 ({existing.Name} / {info.Name}) → 뒤의 것은 무시");
                continue;
            }

            if(!HasAllMembers(sheetName, info, errors))
                continue;

            sets.Add(info.Id, info);

            foreach(int memberId in info.Members)
            {
                if(!setsByItem.TryGetValue(memberId, out List<SetInfo> owners))
                {
                    owners = new List<SetInfo>();
                    setsByItem.Add(memberId, owners);
                }

                owners.Add(info);
            }
        }
    }
    private void RegisterSetBonuses(string sheetName, List<SetBonusInfo> list, List<string> errors)
    {
        foreach(var bonus in list)
        {
            if(!sets.TryGetValue(bonus.SetId, out SetInfo set))
            {
                errors.Add($"{sheetName} : 세트 {bonus.SetId} 가 없습니다 → 보너스 무시");
                continue;
            }

            if(HasBonusCount(set, bonus.Count))
            {
                errors.Add($"{sheetName} : 세트 {bonus.SetId} 의 {bonus.Count}세트 보너스 중복 → 뒤의 것은 무시");
                continue;
            }

            set.AddBonus(bonus);
        }
    }

    private static bool HasBonusCount(SetInfo set, int count)
    {
        foreach(var existing in set.Bonuses)
        {
            if(existing.Count == count)
                return true;
        }

        return false;
    }
    private bool HasAllMembers(string sheetName, SetInfo info, List<string> errors)
    {
        bool valid = true;

        foreach(int memberId in info.Members)
        {
            if(items.ContainsKey(memberId))
                continue;

            errors.Add($"{sheetName} : 세트 {info.Id} ({info.Name}) 의 구성원 {memberId} 가 아이템 목록에 없습니다 → 세트 무시");
            valid = false;
        }

        return valid;
    }
    private Sprite FindIcon(string sheetName, int id, string iconName, List<string> errors)
    {
        if(string.IsNullOrEmpty(iconName) || iconLibrary == null)
            return null;

        if(iconLibrary.TryGet(iconName, out Sprite icon))
            return icon;

        errors.Add($"{sheetName} : ID {id} 의 아이콘 '{iconName}' 을 IconLibrary 에서 찾을 수 없습니다");
        return null;
    }
    private GameObject FindPrefab(string sheetName, int id, string prefabName, List<string> errors)
    {
        if(string.IsNullOrEmpty(prefabName) || prefabLibrary == null)
            return null;

        if(prefabLibrary.TryGet(prefabName, out GameObject prefab))
            return prefab;

        errors.Add($"{sheetName} : ID {id} 의 아이콘 '{prefabName}' 을 MonsterPrefabs 에서 찾을 수 없습니다");
        return null;
    }

    private void RegisterUpgrades(string sheetName, List<UpgradeLevelInfo> list, IdRange range, List<string> errors)
    {
        foreach(var info in list)
        {
            if(!range.Contains(info.Id))
            {
                errors.Add($"{sheetName} : ID {info.Id}가 구간 {range.Min}~{range.Max} 밖입니다");
                continue;
            }

            if(!upgrades.TryGetValue(info.Track, out UpgradeTrackInfo track))
            {
                track = new UpgradeTrackInfo(info.Track);
                upgrades.Add(info.Track, track);
            }

            if(track.HasLevel(info.Level))
            {
                errors.Add($"{sheetName} : {info.Track} {info.Level}레벨 중복 → 뒤의 것은 무시");
                continue;
            }

            track.Add(info);
        }

        var broken = new List<UpgradeTrack>();

        foreach(var pair in upgrades)
        {
            if(pair.Value.IsValid(out string reason))
                continue;

            errors.Add($"{sheetName} : {pair.Key} {reason} → 이 강화는 사용하지 않습니다");
            broken.Add(pair.Key);
        }

        foreach(var key in broken)
        {
            upgrades.Remove(key);
        }
    }

    private void RegisterTrainingStats(string sheetName, List<TrainingStatInfo> list, List<string> errors)
    {
        foreach(var info in list)
        {
            if(trainingStats.ContainsKey(info.Stat))
            {
                errors.Add($"{sheetName} : {info.Stat} 중복 → 뒤의 것은 무시");
                continue;
            }

            trainingStats.Add(info.Stat, info);
        }

        foreach(TrainingStat stat in System.Enum.GetValues(typeof(TrainingStat)))
        {
            if(!trainingStats.ContainsKey(stat))
                errors.Add($"{sheetName} : {stat} 줄이 없습니다 → 이 스탯은 훈련할 수 없습니다");
        }
    }

    private void RegisterMonsterDrops(string sheetName, List<MonsterDropInfo> list, List<string> errors)
    {
        foreach(var drop in list)
        {
            if(!monsters.TryGetValue(drop.MonsterId, out MonsterInfo monster))
            {
                errors.Add($"{sheetName} : 몬스터 {drop.MonsterId} 가 없습니다 → 이 줄은 무시");
                continue;
            }

            if(!items.TryGetValue(drop.ItemId, out ItemInfo item))
            {
                errors.Add($"{sheetName} : 아이템 {drop.ItemId} 가 없습니다 → {monster.Name} 드랍에서 제외");
                continue;
            }

            drop.SetItem(item);
            monster.AddDrop(drop);
        }
    }
    private void RegisterShopStocks(string sheetName, List<ShopStockInfo> list, List<string> errors)
    {
        var seen = new HashSet<int>();

        foreach(var stock in list)
        {
            if(!items.TryGetValue(stock.ItemId, out ItemInfo item))
            {
                errors.Add($"{sheetName} : 아이템 {stock.ItemId} 가 없습니다 → 진열 제외");
                continue;
            }

            if(!seen.Add(stock.ItemId))
            {
                errors.Add($"{sheetName} : 아이템 {stock.ItemId} ({item.Name}) 중복 → 뒤의 것은 무시");
                continue;
            }

            if(item.Grade == ItemGrade.Legendary)
            {
                errors.Add($"{sheetName} : 레전더리 {item.Name} 은 상점에 진열할 수 없습니다 → 진열 제외");
                continue;
            }

            stock.SetItem(item);
            shopStocks.Add(stock);
        }
    }
    private void RegisterStages(string sheetName, List<StageInfo> list, IdRange range, List<string> errors)
    {
        var ids = new HashSet<int>();

        foreach(var info in list)
        {
            if(!range.Contains(info.Id))
            {
                errors.Add($"{sheetName} : ID {info.Id} ({info.Name}) 가 구간 {range.Min}~{range.Max} 밖입니다");
                continue;
            }

            if(!ids.Add(info.Id))
            {
                errors.Add($"{sheetName} : ID {info.Id} 중복 → 뒤의 것은 무시");
                continue;
            }

            if(info.TowerId == 0 && PrologueTower != null)
                info.SetTower(PrologueTower.Id);

            if(!TryGetTower(info.TowerId, out _))
            {
                errors.Add($"{sheetName} : 지역 {info.Name} 의 탑 {info.TowerId} 가 없습니다 → 지역 무시");
                continue;
            }

            foreach(int monsterId in info.MonsterIds)
            {
                if(!monsters.TryGetValue(monsterId, out MonsterInfo monster) || monster.IsBoss)
                {
                    errors.Add($"{sheetName} : 지역 {info.Name} 의 몬스터 {monsterId} 가 없거나 보스입니다 → 제외");
                    continue;
                }

                info.AddMonster(monster);
            }

            if(info.Monsters.Count == 0)
            {
                errors.Add($"{sheetName} : 지역 {info.Name} 에 일반 몬스터가 없습니다 → 지역 무시");
                continue;
            }

            if(info.BossId != 0)
            {
                if(monsters.TryGetValue(info.BossId, out MonsterInfo boss) && boss.IsBoss)
                    info.SetBoss(boss);
                else
                    errors.Add($"{sheetName} : 지역 {info.Name} 의 보스 {info.BossId} 가 없거나 보스가 아닙니다 → 랜덤 보스");
            }

            info.SetBackground(FindBackground(sheetName, info));

            if(!string.IsNullOrEmpty(info.AmbienceName))
                info.SetAmbience(FindPrefab(sheetName, info.Id, info.AmbienceName, errors));

            stages.Add(info);
        }
    }

    private void RegisterBlessings(string sheetName, List<BlessingInfo> list, IdRange range, List<string> errors)
    {
        var ids = new HashSet<int>();

        foreach(BlessingInfo info in list)
        {
            if(!range.Contains(info.Id))
            {
                errors.Add($"{sheetName} : ID {info.Id} ({info.Name}) 가 구간 {range.Min}~{range.Max} 밖입니다");
                continue;
            }

            if(!ids.Add(info.Id))
            {
                errors.Add($"{sheetName} : ID {info.Id} 중복 → 뒤의 것은 무시");
                continue;
            }

            blessings.Add(info);
        }

        blessings.Sort((a, b) => a.Id.CompareTo(b.Id));
    }

    private void RegisterTowers(string sheetName, List<TowerInfo> list, IdRange range, List<string> errors)
    {
        foreach(TowerInfo info in list)
        {
            if(!range.Contains(info.Id))
            {
                errors.Add($"{sheetName} : ID {info.Id} ({info.Name}) 가 구간 {range.Min}~{range.Max} 밖입니다");
                continue;
            }

            if(TryGetTower(info.Id, out _))
            {
                errors.Add($"{sheetName} : ID {info.Id} 중복 → 뒤의 것은 무시");
                continue;
            }

            if(info.IsPrologue)
            {
                if(PrologueTower != null)
                {
                    errors.Add($"{sheetName} : 첫 탑이 여러 개입니다 ({PrologueTower.Name} / {info.Name}) → {info.Name} 무시");
                    continue;
                }

                PrologueTower = info;
            }

            if(info.IsEndless)
            {
                if(OriginTower != null)
                {
                    errors.Add($"{sheetName} : 근원의 탑이 여러 개입니다 ({OriginTower.Name} / {info.Name}) → {info.Name} 무시");
                    continue;
                }

                OriginTower = info;
            }

            towers.Add(info);
        }

        towers.Sort((a, b) => a.Id.CompareTo(b.Id));
        LinkStageSources();

        if(PrologueTower == null)
            errors.Add($"{sheetName} : kind 가 Prologue 인 첫 탑이 없습니다");

        if(OriginTower == null)
            errors.Add($"{sheetName} : kind 가 Origin 인 근원의 탑이 없습니다");
    }

    private void LinkStageSources()
    {
        foreach(TowerInfo tower in towers)
        {
            if(!tower.IsEndless)
            {
                tower.AddStageSource(tower.Id);
                continue;
            }

            foreach(TowerInfo source in towers)
            {
                if(source.IsThemed)
                    tower.AddStageSource(source.Id);
            }
        }
    }

    private void RegisterForge(string sheetName, List<ForgeLevelInfo> list, IdRange range, List<string> errors)
    {
        foreach(ForgeLevelInfo info in list)
        {
            if(!range.Contains(info.Id))
            {
                errors.Add($"{sheetName} : ID {info.Id}가 구간 {range.Min}~{range.Max} 밖입니다");
                continue;
            }

            if(forge.HasLevel(info.Level))
            {
                errors.Add($"{sheetName} : {info.Level}단계 중복 → 뒤의 것은 무시");
                continue;
            }

            if(!TryLinkMaterials(sheetName, info, errors))
                continue;

            forge.Add(info);
        }

        if(forge.IsValid(out string reason))
            return;

        errors.Add($"{sheetName} : {reason} → 강화를 사용하지 않습니다");
        forge.Clear();
    }

    private bool TryLinkMaterials(string sheetName, ForgeLevelInfo info, List<string> errors)
    {
        bool ok = true;

        foreach(ItemCost cost in info.Materials)
        {
            if(items.TryGetValue(cost.ItemId, out ItemInfo item))
            {
                cost.SetItem(item);
                continue;
            }

            errors.Add($"{sheetName} : {info.Level}단계 재료 {cost.ItemId} 가 없습니다");
            ok = false;
        }

        return ok;
    }

    private void RegisterQuests(string sheetName, List<QuestInfo> list, IdRange range, List<string> errors)
    {
        foreach(var info in list)
        {
            if(!range.Contains(info.Id))
            {
                errors.Add($"{sheetName} : ID {info.Id} ({info.Name}) 가 구간 {range.Min}~{range.Max} 밖입니다");
                continue;
            }

            if(!quests.TryAdd(info.Id, info))
            {
                errors.Add($"{sheetName} : ID {info.Id} 중복 → 뒤의 것은 무시");
                continue;
            }

            if(info.RewardItemId != 0)
            {
                if(items.TryGetValue(info.RewardItemId, out ItemInfo reward))
                    info.SetRewardItem(reward);
                else
                    errors.Add($"{sheetName} : {info.Name} 의 보상 아이템 {info.RewardItemId} 가 없습니다");
            }

            if(!IsValidTarget(info))
                errors.Add($"{sheetName} : {info.Name} 의 대상 {info.Target} 가 없습니다");
        }

        foreach(var info in quests.Values)
        {
            if(info.PrereqId != 0 && !quests.ContainsKey(info.PrereqId))
                errors.Add($"{sheetName} : {info.Name} 의 선행 퀘스트 {info.PrereqId} 가 없습니다");
        }
    }
    private bool IsValidTarget(QuestInfo info)
    {
        if(info.Type == QuestType.Deliver)
            return items.ContainsKey(info.Target);

        if(info.Type == QuestType.ForgeLevel)
            return info.Target <= forge.MaxLevel;

        if(info.Type == QuestType.ReachFloor || info.Target == 0)
            return true;

        if(!monsters.TryGetValue(info.Target, out MonsterInfo monster))
            return false;

        return info.Type == QuestType.Kill || monster.IsBoss;
    }
    #endregion

    #region 조회
    public bool TryGetItem(int id, out ItemInfo info)
    {
        return items.TryGetValue(id, out info);
    }
    public bool TryGetMonster(int id, out MonsterInfo info)
    {
        return monsters.TryGetValue(id, out info);
    }
    public bool TryGetClass(int id, out ClassInfo info)
    {
        return classes.TryGetValue(id, out info);
    }

    // 타입까지 맞춰서 꺼내기 : TryGetItem(1001, out WeaponInfo weapon)
    public bool TryGetItem<T>(int id, out T info) where T : ItemInfo
    {
        info = null;

        if(!items.TryGetValue(id, out ItemInfo found))
            return false;

        info = found as T;
        return info != null;
    }
    public bool TryGetSet(int setId, out SetInfo info)
    {
        return sets.TryGetValue(setId, out info);
    }

    public IReadOnlyList<SetInfo> GetSetsOf(int itemId)
    {
        if(setsByItem.TryGetValue(itemId, out List<SetInfo> list))
            return list;

        return NoSets;
    }
    public bool TryGetUpgrade(UpgradeTrack track, out UpgradeTrackInfo info)
    {
        return upgrades.TryGetValue(track, out info);
    }
    public bool TryGetTrainingStat(TrainingStat stat, out TrainingStatInfo info)
    {
        return trainingStats.TryGetValue(stat, out info);
    }
    private Sprite FindBackground(string sheetName, StageInfo info)
    {
        if(string.IsNullOrEmpty(info.BackgroundName) || backgroundLibrary == null)
            return null;

        if(backgroundLibrary.TryGet(info.BackgroundName, out Sprite sprite))
            return sprite;

        return null;
    }

    public bool TryGetBlessing(int id, out BlessingInfo info)
    {
        foreach(BlessingInfo blessing in blessings)
        {
            if(blessing.Id != id)
                continue;

            info = blessing;
            return true;
        }

        info = null;
        return false;
    }

    public bool TryGetTower(int id, out TowerInfo info)
    {
        foreach(TowerInfo tower in towers)
        {
            if(tower.Id != id)
                continue;

            info = tower;
            return true;
        }

        info = null;
        return false;
    }

    public bool TryGetQuest(int id, out QuestInfo info)
    {
        return quests.TryGetValue(id, out info);
    }
    #endregion
}
