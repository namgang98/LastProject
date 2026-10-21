using System.Collections.Generic;
using UnityEngine;

public class StagePlanner
{
    private readonly IReadOnlyList<StageInfo> stages;
    private readonly List<MonsterInfo> allBosses = new();
    private readonly int blockSize;
    private readonly Dictionary<int, StageInfo> picked = new();
    private readonly List<StageInfo> candidates = new();

    public StagePlanner(IReadOnlyList<StageInfo> stages, IEnumerable<MonsterInfo> monsters, int blockSize)
    {
        this.stages = stages;
        this.blockSize = Mathf.Max(1, blockSize);

        foreach(MonsterInfo monster in monsters)
        {
            if(monster.IsBoss)
                allBosses.Add(monster);
        }
    }

    public StageInfo Get(int floor)
    {
        int block = (floor - 1) / blockSize;

        if(picked.TryGetValue(block, out StageInfo stage))
            return stage;

        picked.TryGetValue(block - 1, out StageInfo previous);
        stage = Pick(block * blockSize + 1, previous);
        picked[block] = stage;
        return stage;
    }

    public bool IsFirstFloor(int floor)
    {
        return (floor - 1) % blockSize == 0;
    }

    public MonsterInfo PickMonster(int floor, bool isBoss)
    {
        StageInfo stage = Get(floor);

        if(stage == null)
            return null;

        if(!isBoss)
            return stage.Monsters[Random.Range(0, stage.Monsters.Count)];

        if(stage.Boss != null)
            return stage.Boss;

        if(allBosses.Count == 0)
            return null;

        return allBosses[Random.Range(0, allBosses.Count)];
    }

    private StageInfo Pick(int startFloor, StageInfo previous)
    {
        candidates.Clear();

        foreach(StageInfo stage in stages)
        {
            if(stage.IsAvailable(startFloor) && stage != previous)
                candidates.Add(stage);
        }

        if(candidates.Count == 0 && previous != null && previous.IsAvailable(startFloor))
            candidates.Add(previous);

        if(candidates.Count == 0)
            return null;

        return candidates[Random.Range(0, candidates.Count)];
    }
}
