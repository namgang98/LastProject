using System.Collections.Generic;
using UnityEngine;

public class DataManager : Singleton<DataManager>
{
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

    private delegate bool RowParser<T>(CsvRow row, List<string> errors, out T info);

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
    [Header("이미지,프리팹")]
    [SerializeField] IconLibrary iconLibrary;
    [SerializeField] MonsterPrefabs monsterPrefabs;
    


    private readonly Dictionary<int, ItemInfo> items = new();
    private readonly Dictionary<int, MonsterInfo> monsters = new();
    private readonly Dictionary<int, ClassInfo> classes = new();
    private readonly Dictionary<int, SetInfo> sets = new();
    private readonly Dictionary<int, List<SetInfo>> setsByItem = new();
    private static readonly List<SetInfo> NoSets = new();
    private bool isLoading;
    public bool IsLoaded { get; private set; }
    public IReadOnlyCollection<ItemInfo> Items => items.Values;
    public IReadOnlyCollection<MonsterInfo> Monsters => monsters.Values; 
    public IReadOnlyCollection<ClassInfo> Classes => classes.Values;

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

        //메인 스레드
        string weaponText = ReadText(weaponCsv, nameof(weaponCsv));
        string pouchText = ReadText(pouchCsv, nameof(pouchCsv));
        string bagText = ReadText(bagCsv, nameof(bagCsv));
        string monsterText = ReadText(monsterCsv, nameof(monsterCsv));
        string classText = ReadText(classCsv, nameof(classCsv));
        string armorText = ReadText(armorCsv, nameof(armorCsv));
        string accessoryText = ReadText(accessoryCsv, nameof(accessoryCsv));
        string setText = ReadText(setCsv, nameof(setCsv));
        string setBonusText = ReadText(setBonusCsv, nameof(setBonusCsv));


        //백그라운드 스레드
        await Awaitable.BackgroundThreadAsync();

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

        //메인 스레드
        await Awaitable.MainThreadAsync();

        if(iconLibrary != null)
            iconLibrary.Init();
        else
            errors.Add("IconLibrary가 연결되지 않았습니다. 아이콘 없이 진행합니다");
        if(monsterPrefabs != null)
            monsterPrefabs.Init();
        else
            errors.Add("monsterPrefabs가 연결되지 않았습니다");

        RegisterItems(DataTag.Weapons, weaponList, WeaponIds, errors);
        RegisterItems(DataTag.Pouches, pouchList, PouchIds, errors);
        RegisterItems(DataTag.Bags, bagList, BagIds, errors);
        RegisterMonsters(DataTag.Monsters, monsterList, MonsterIds, errors);
        RegisterClasses(DataTag.Classes, classList, ClassIds, errors);
        RegisterItems(DataTag.Armors, armorList, ArmorIds, errors);
        RegisterItems(DataTag.Accessories, accessoryList, AccessoryIds, errors);
        RegisterSets(DataTag.Sets, setList, SetIds, errors);
        RegisterSetBonuses(DataTag.SetBonuses, setBonusList, errors);

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
        if(string.IsNullOrEmpty(prefabName) || monsterPrefabs == null)
            return null;

        if(monsterPrefabs.TryGet(prefabName, out GameObject prefab))
            return prefab;

        errors.Add($"{sheetName} : ID {id} 의 아이콘 '{prefabName}' 을 MonsterPrefabs 에서 찾을 수 없습니다");
        return null;
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
        if (setsByItem.TryGetValue(itemId, out List<SetInfo> list))
            return list;

        return NoSets;
    }
    #endregion
}
