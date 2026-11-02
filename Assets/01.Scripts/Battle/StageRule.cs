using UnityEngine;

public class StageRule
{
    private readonly int bossInterval;
    private readonly bool returnOnlyAfterBoss;
    public StageRule(int bossInterval, bool returnOnlyAfterBoss)
    {
        this.bossInterval = Mathf.Max(1, bossInterval);
        this.returnOnlyAfterBoss = returnOnlyAfterBoss;
    }

    public bool IsBossStage(int stage)
    {
        return stage % bossInterval == 0;
    }
    public bool IsRestStage(int stage)
    {
        return (stage + 1) % bossInterval == 0;
    }
    public bool CanReturn(int clearedStage)
    {
        if(!returnOnlyAfterBoss)
            return true;

        return IsBossStage(clearedStage);
    }
}
