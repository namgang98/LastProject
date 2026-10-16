using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public static class InfoParser
{
    private struct InfoBase
    {
        public int id;
        public string name;
    }

    private struct ItemBase
    {
        public InfoBase common;
        public string iconName;
        public ItemShape shape;
        public ItemGrade grade;
        public int sellPrice;
        public string desc;
    }
    #region 탭별 변환
    public static bool TryParseWeapon(CsvRow row, List<string> errors, out WeaponInfo info)
    {
        info = null;

        if(!TryParseItemBase(row, errors, out ItemBase i))
            return false;

        if(!TryParseAttack(row, errors, out AttackData attack))
            return false;

        if(!row.TryGetEnum(CSVStamp.Type, out WeaponType type))
            return Fail(row, errors, CSVStamp.Type);

        info = new WeaponInfo(i.common.id, i.common.name, i.iconName, i.shape, i.grade, i.sellPrice, i.desc, attack, type);
        return true;
    }

    public static bool TryParsePouch(CsvRow row, List<string> errors, out PouchInfo info)
    {
        info = null;

        if(!TryParseItemBase(row, errors, out ItemBase i))
            return false;

        if(!TryParseSize(row, errors, CSVStamp.StorageX, CSVStamp.StorageY, out Vector2Int storage))
            return false;

        info = new PouchInfo(i.common.id, i.common.name, i.iconName, i.shape, i.grade, i.sellPrice, i.desc, storage);
        return true;
    }

    public static bool TryParseBag(CsvRow row, List<string> errors, out BagInfo info)
    {
        info = null;

        if(!TryParseItemBase(row, errors, out ItemBase i))
            return false;

        if(!TryParseSize(row, errors, CSVStamp.StorageX, CSVStamp.StorageY, out Vector2Int storage))
            return false;

        if(!row.TryGetInt(CSVStamp.Price, out int price))
            return Fail(row, errors, CSVStamp.Price);

        info = new BagInfo(i.common.id, i.common.name, i.iconName, i.shape, i.grade, i.sellPrice, storage, i.desc, price);
        return true;

    }

    public static bool TryParseMonster(CsvRow row, List<string> errors, out MonsterInfo info)
    {
        info = null;

        if(!TryParseInfoBase(row, errors, out InfoBase b))
            return false;

        string prefab = row.GetString(CSVStamp.Prefab);
        if(string.IsNullOrEmpty(prefab))
            return Fail(row, errors, CSVStamp.Prefab);

        if(!row.TryGetInt(CSVStamp.MaxHp, out int maxHp))
            return Fail(row, errors, CSVStamp.MaxHp);

        if(!TryParseAttack(row, errors, out AttackData attack))
            return false;

        if(!row.TryGetInt(CSVStamp.DropGold, out int dropgold))
            return Fail(row, errors, CSVStamp.DropGold);

        if(!row.TryGetEnum(CSVStamp.Type, out MonsterType type))
            return Fail(row, errors, CSVStamp.Type);

        info = new MonsterInfo(b.id, b.name, prefab, maxHp, attack, dropgold, type);
        return true;
    }

    public static bool TryParseClass(CsvRow row, List<string> errors, out ClassInfo info)
    {
        info = null;

        if(!TryParseInfoBase(row, errors, out InfoBase b))
            return false;

        if(!row.TryGetInt(CSVStamp.MaxHp, out int maxHp))
            return Fail(row, errors, CSVStamp.MaxHp);

        if(!row.TryGetEnum(CSVStamp.Type, out WeaponType type))
            return Fail(row, errors, CSVStamp.Type);

        if(!row.TryGetFloat(CSVStamp.DamageRate, out float damageRate))
            return Fail(row, errors, CSVStamp.DamageRate);

        if(!row.TryGetFloat(CSVStamp.CooldownRate, out float cooldownRate))
            return Fail(row, errors, CSVStamp.CooldownRate);

        string desc = row.GetString(CSVStamp.Description);

        string prefab = row.GetString(CSVStamp.Prefab);
        if(string.IsNullOrEmpty(prefab))
            return Fail(row, errors, CSVStamp.Prefab);
        
        info = new ClassInfo(b.id, b.name, maxHp, type, damageRate, cooldownRate, desc, prefab);
        return true;
    }

    public static bool TryParseArmor(CsvRow row, List<string> errors, out ArmorInfo info)
    {
        info = null;

        if(!TryParseItemBase(row, errors, out ItemBase b))
            return false;

        if(!TryParseEffect(row, errors, out EffectData effect))
            return false;

        if(!row.TryGetInt(CSVStamp.MaxHp, out int maxHp))
            return Fail(row, errors, CSVStamp.MaxHp);

        info = new ArmorInfo(b.common.id,b.common.name,b.iconName,b.shape,b.grade,b.sellPrice,b.desc, effect, maxHp);
        return true;

    }
    public static bool TryParseAccessory(CsvRow row, List<string> errors, out AccessoryInfo info)
    {
        info = null;

        if(!TryParseItemBase(row, errors, out ItemBase b))
            return false;

        if(!TryParseEffect(row, errors, out EffectData effect))
            return false;

        if(!row.TryGetInt(CSVStamp.MaxHp, out int maxHp))
            return Fail(row, errors, CSVStamp.MaxHp);

        if(!row.TryGetFloat(CSVStamp.DamageBonus, out float damageBonus))
            return Fail(row, errors, CSVStamp.DamageBonus);

        if(!row.TryGetFloat(CSVStamp.CooldownReduction, out float cooldownReduction))
            return Fail(row, errors, CSVStamp.CooldownReduction);

        info = new AccessoryInfo(b.common.id, b.common.name, b.iconName, b.shape, b.grade, b.sellPrice, b.desc, effect, maxHp, damageBonus, cooldownReduction);
        return true;
    }

    public static bool TryParseSet(CsvRow row, List<string> errors, out SetInfo info)
    {
        info = null;
        if(!row.TryGetInt(CSVStamp.SetId, out int setid))
            return Fail(row, errors, CSVStamp.SetId);
        string name = row.GetString(CSVStamp.Name);
        if(!row.TryGetIntList(CSVStamp.Members, '|', out List<int> members) || members.Count < 2)
            return Fail(row, errors, CSVStamp.Members);
        string desc = row.GetString(CSVStamp.Description);
        info = new SetInfo(setid, name, members, desc);
        return true;
    }

    public static bool TryParseSetBonus(CsvRow row, List<string> errors, out SetBonusInfo info)
    {
        info = null;

        if(!row.TryGetInt(CSVStamp.SetId, out int setid))
            return Fail(row, errors,CSVStamp.SetId);
        if(!row.TryGetInt(CSVStamp.Count, out int count) || count < 1)
            return Fail(row, errors, CSVStamp.Count);
        if(!row.TryGetInt(CSVStamp.MaxHp, out int maxHp))
            return Fail(row, errors, CSVStamp.MaxHp);
        if(!row.TryGetFloat(CSVStamp.DamageBonus, out float damageBonus))
            return Fail(row, errors, CSVStamp.DamageBonus);
        if(!row.TryGetFloat(CSVStamp.CooldownReduction, out float cooldownReduction))
            return Fail(row, errors, CSVStamp.CooldownReduction);
        if(!row.TryGetFloat(CSVStamp.CritChance, out float critChance))
            return Fail(row, errors, CSVStamp.CritChance);
        string desc = row.GetString(CSVStamp.Description);
        info = new SetBonusInfo(setid,count,maxHp,damageBonus,cooldownReduction,critChance,desc);
        return true;
    }

    public static bool TryParseUpgrade(CsvRow row, List<string> errors, out UpgradeLevelInfo info)
    {
        info = null;

        if(!row.TryGetInt(CSVStamp.Id, out int id))
            return Fail(row, errors, CSVStamp.Id);

        if(!row.TryGetEnum(CSVStamp.Track, out UpgradeTrack track))
            return Fail(row, errors, CSVStamp.Track);

        if(!row.TryGetInt(CSVStamp.Level, out int level) || level < 1)
            return Fail(row, errors, CSVStamp.Level);

        if(!row.TryGetInt(CSVStamp.Cost, out int cost) || cost < 0)
            return Fail(row, errors, CSVStamp.Cost);

        if(!row.TryGetInt(CSVStamp.Value, out int value))
            return Fail(row, errors, CSVStamp.Value);

        string desc = row.GetString(CSVStamp.Description);
        info = new UpgradeLevelInfo(id, track, level, cost, value, desc);
        return true;
    }
    #endregion

    #region 공통
    private static bool TryParseInfoBase(CsvRow row, List<string> errors, out InfoBase result)
    {
        result = default;

        if(!row.TryGetInt(CSVStamp.Id, out result.id))
            return Fail(row, errors, CSVStamp.Id);

        result.name = row.GetString(CSVStamp.Name);
        return true;
    }


    private static bool TryParseItemBase(CsvRow row, List<string> errors, out ItemBase result)
    {
        result = default;

        if(!TryParseInfoBase(row, errors, out result.common))
            return false;

        if(!row.TryGetEnum(CSVStamp.Grade, out result.grade))
            return Fail(row, errors, CSVStamp.Grade);

        if(!ItemShape.TryParse(row.GetString(CSVStamp.Shape), out result.shape))
            return Fail(row, errors, CSVStamp.Shape);

        if(!row.TryGetInt(CSVStamp.SellPrice, out result.sellPrice))
            return Fail(row, errors, CSVStamp.SellPrice);

        result.desc = row.GetString(CSVStamp.Description);

        result.iconName = row.GetString(CSVStamp.Icon);
        return true;
    }

    private static bool TryParseSize(CsvRow row, List<string> errors, string xColumn, string yColumn, out Vector2Int size)
    {
        size = Vector2Int.zero;

        if(!row.TryGetInt(xColumn, out int x) || x < 1)
            return Fail(row, errors, xColumn);

        if(!row.TryGetInt(yColumn, out int y) || y < 1)
            return Fail(row, errors, yColumn);

        size = new Vector2Int(x, y);
        return true;
    }

    private static bool TryParseAttack(CsvRow row, List<string> errors, out AttackData attack)
    {
        attack = default;

        if(!row.TryGetInt(CSVStamp.Damage, out int damage))
            return Fail(row, errors, CSVStamp.Damage);

        if(!row.TryGetFloat(CSVStamp.Cooldown, out float cooldown) || cooldown <= 0f)
            return Fail(row, errors, CSVStamp.Cooldown);

        if(!row.TryGetFloat(CSVStamp.CritChance, out float critChance) || critChance < 0f || critChance > 1f)
            return Fail(row, errors, CSVStamp.CritChance);

        if(!row.TryGetFloat(CSVStamp.CritMultiplier, out float critMultiplier) || critMultiplier < 1f)
            return Fail(row, errors, CSVStamp.CritMultiplier);

        if(!row.TryGetEnum(CSVStamp.Element, out DamageElement element))
            return Fail(row, errors,CSVStamp.Element);
        
        if(element == DamageElement.None)
        {
            attack = new AttackData(damage, cooldown, critChance, critMultiplier,element,0,0,0);
            return true;
        }  

        if(!row.TryGetFloat(CSVStamp.Elementchance, out float chance) || chance <= 0f || chance > 1f)
            return Fail(row, errors,CSVStamp.Elementchance);

        if(!row.TryGetInt(CSVStamp.Elementvalue, out int value) || value <= 0)
            return Fail(row, errors, CSVStamp.Elementvalue);

        if(!row.TryGetFloat(CSVStamp.Elementduration, out float duration) || duration <= 0f)
            return Fail(row, errors,CSVStamp.Elementduration);

        attack = new AttackData(damage, cooldown, critChance, critMultiplier,element,chance,value,duration);
        return true;
    }

    private static bool TryParseEffect(CsvRow row, List<string> errors, out EffectData effect)
    {
        effect = default;

        if(!row.TryGetEnum(CSVStamp.EffectType, out EffectType type))
            return Fail(row, errors, CSVStamp.EffectType);

        if(type == EffectType.None)
            return true;

        if(!row.TryGetInt(CSVStamp.EffectValue, out int value) || value <= 0)
            return Fail(row, errors, CSVStamp.EffectValue);

        if(!row.TryGetFloat(CSVStamp.EffectCooldown, out float cooldown) || cooldown <= 0f)
            return Fail(row, errors, CSVStamp.EffectCooldown);

        effect = new EffectData(type, value, cooldown);
        return true;
    }

    private static bool Fail(CsvRow row, List<string> errors, string column)
    {
        errors.Add($"{row.Where} : {column} 값 '{row.GetString(column)}' 오류 → 이 행은 건너뜁니다");
        return false;
    }
    #endregion
}
