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
    public int DropGold { get; }
    public MonsterType Type { get; }

    public GameObject Prefab { get; private set; }
    public MonsterInfo(int id, string name, string prefabName, int maxHp, AttackData attack, int dropGold, MonsterType type)
    {
        Id = id;
        Name = name;
        PrefabName = prefabName;
        MaxHp = maxHp;
        Attack = attack;
        DropGold = dropGold;
        Type = type;
    }

    public bool IsBoss => Type == MonsterType.Boss;

    public void SetPrefab(GameObject prefab)
    {
        if(Prefab != null)
            return;
        Prefab = prefab;
    }
}
