using System.Collections.Generic;
using UnityEngine;

public enum MonsterType
{
    Normal,
    Boss
}

public enum HpDisplay
{
    Full,
    Hidden
}

public class MonsterInfo
{
    public int Id { get; }
    public string Name { get; }
    public string PrefabName { get; }
    public int MaxHp { get; }
    public AttackData Attack { get; }
    public float EquipChance { get; }
    public MonsterType Type { get; }
    public HpDisplay HpDisplay { get; }
    public float SkillChance { get; }
    public float SkillMultiplier { get; }
    public float ReviveRate { get; }

    private readonly List<MonsterDropInfo> drops = new();
    public IReadOnlyList<MonsterDropInfo> Drops => drops;

    public GameObject Prefab { get; private set; }
    public MonsterInfo(int id, string name, string prefabName, int maxHp, AttackData attack, float equipChance, MonsterType type, HpDisplay hpDisplay = HpDisplay.Full, float skillChance = 0f, float skillMultiplier = 1f, float reviveRate = 0f)
    {
        Id = id;
        Name = name;
        PrefabName = prefabName;
        MaxHp = maxHp;
        Attack = attack;
        EquipChance = equipChance;
        Type = type;
        HpDisplay = hpDisplay;
        SkillChance = skillChance;
        SkillMultiplier = skillMultiplier;
        ReviveRate = reviveRate;
    }

    public bool IsBoss => Type == MonsterType.Boss;
    public bool HasSkill => SkillChance > 0f;
    public bool CanRevive => ReviveRate > 0f;

    internal void AddDrop(MonsterDropInfo drop)
    {
        drops.Add(drop);
    }

    public void SetPrefab(GameObject prefab)
    {
        if(Prefab != null)
            return;
        Prefab = prefab;
    }
}
