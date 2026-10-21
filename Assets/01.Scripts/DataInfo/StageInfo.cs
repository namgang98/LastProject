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

    private readonly List<MonsterInfo> monsters = new();
    public IReadOnlyList<MonsterInfo> Monsters => monsters;
    public MonsterInfo Boss { get; private set; }
    public Sprite Background { get; private set; }

    public StageInfo(int id, string name, string backgroundName, List<int> monsterIds, int bossId, int minFloor)
    {
        Id = id;
        Name = name;
        BackgroundName = backgroundName;
        MonsterIds = monsterIds;
        BossId = bossId;
        MinFloor = minFloor;
    }

    public bool IsAvailable(int floor)
    {
        return floor >= MinFloor && monsters.Count > 0;
    }

    internal void AddMonster(MonsterInfo monster)
    {
        monsters.Add(monster);
    }

    internal void SetBoss(MonsterInfo boss)
    {
        Boss = boss;
    }

    internal void SetBackground(Sprite sprite)
    {
        Background = sprite;
    }
}