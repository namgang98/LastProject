using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public enum MonsterType
{
    Normal,
    Boss
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

    private readonly List<MonsterDropInfo> drops = new();
    public IReadOnlyList<MonsterDropInfo> Drops => drops;

    public GameObject Prefab { get; private set; }
    public MonsterInfo(int id, string name, string prefabName, int maxHp, AttackData attack, float equipChance, MonsterType type)
    {
        Id = id;
        Name = name;
        PrefabName = prefabName;
        MaxHp = maxHp;
        Attack = attack;
        EquipChance = equipChance;
        Type = type;
    }

    public bool IsBoss => Type == MonsterType.Boss;

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
