using System.Collections.Generic;
using UnityEngine;

public class StageInfo
{
    public int Id { get; }
    public string Name { get; }
    public string BackgroundName { get; }
    public IReadOnlyList<int> MonsterIds { get; }
    public int BossId { get; }
    public int MinFloor { get; }
    public int MaxFloor { get; }
    public string AmbienceName { get; }
    public int TowerId { get; private set; }

    private readonly List<MonsterInfo> monsters = new();
    public IReadOnlyList<MonsterInfo> Monsters => monsters;
    public MonsterInfo Boss { get; private set; }
    public Sprite Background { get; private set; }
    public GameObject AmbiencePrefab { get; private set; }

    public StageInfo(int id, string name, string backgroundName, List<int> monsterIds, int bossId, int minFloor, int maxFloor = int.MaxValue, string ambienceName = "", int towerId = 0)
    {
        Id = id;
        Name = name;
        BackgroundName = backgroundName;
        MonsterIds = monsterIds;
        BossId = bossId;
        MinFloor = minFloor;
        MaxFloor = maxFloor;
        AmbienceName = ambienceName;
        TowerId = towerId;
    }

    public bool IsAvailable(int towerId, int floor)
    {
        return towerId == TowerId && floor >= MinFloor && floor <= MaxFloor && monsters.Count > 0;
    }

    internal void AddMonster(MonsterInfo monster)
    {
        monsters.Add(monster);
    }

    internal void SetTower(int towerId)
    {
        TowerId = towerId;
    }

    internal void SetBoss(MonsterInfo boss)
    {
        Boss = boss;
    }

    internal void SetBackground(Sprite sprite)
    {
        Background = sprite;
    }

    internal void SetAmbience(GameObject prefab)
    {
        AmbiencePrefab = prefab;
    }
}