using System;
using System.Collections.Generic;

public class ClassTraining
{
    private readonly HashSet<int> unlocked = new();
    private readonly Dictionary<int, ClassProgress> progresses = new();
    private readonly UpgradeTrackInfo costs;
    private readonly TrainingStatLookup findStat;

    public int BestFloor { get; private set; }

    public event Action<ClassInfo> Changed;

    public ClassTraining(SaveData data, UpgradeTrackInfo costs, TrainingStatLookup findStat)
    {
        this.costs = costs;
        this.findStat = findStat;
        BestFloor = Math.Max(0, data.bestFloor);

        foreach(int id in data.unlockedClasses)
        {
            unlocked.Add(id);
        }

        foreach(var saved in data.classProgress)
        {
            var progress = new ClassProgress();
            progress.Restore(saved.level, saved.points.ToArray());
            progresses[saved.classId] = progress;
        }
    }

    #region 해금
    public bool IsUnlocked(ClassInfo info)
    {
        return info.IsStarter || unlocked.Contains(info.Id);
    }

    public bool MeetsFloor(ClassInfo info)
    {
        return BestFloor >= info.UnlockFloor;
    }

    public bool CanUnlock(ClassInfo info, Wallet wallet)
    {
        return !IsUnlocked(info) && MeetsFloor(info) && wallet.Gold >= info.UnlockCost;
    }

    public bool TryUnlock(ClassInfo info, Wallet wallet)
    {
        if(!CanUnlock(info, wallet))
            return false;

        if(info.UnlockCost > 0 && !wallet.TrySpend(info.UnlockCost))
            return false;

        unlocked.Add(info.Id);
        Changed?.Invoke(info);
        return true;
    }

    public void ReportFloor(int floor)
    {
        if(floor <= BestFloor)
            return;

        BestFloor = floor;
    }
    #endregion

    #region 훈련 (레벨업)
    public ClassProgress GetProgress(ClassInfo info)
    {
        if(!progresses.TryGetValue(info.Id, out ClassProgress progress))
        {
            progress = new ClassProgress();
            progresses.Add(info.Id, progress);
        }

        return progress;
    }

    public bool TryGetTrainCost(ClassInfo info, out int cost)
    {
        cost = 0;
        int next = GetProgress(info).Level + 1;

        if(costs == null || next > costs.MaxLevel)
            return false;

        cost = costs.GetLevel(next).Cost;
        return true;
    }

    public bool TryTrain(ClassInfo info, Wallet wallet)
    {
        if(!IsUnlocked(info) || !TryGetTrainCost(info, out int cost))
            return false;

        if(!wallet.TrySpend(cost))
            return false;

        GetProgress(info).LevelUp();
        Changed?.Invoke(info);
        return true;
    }
    #endregion

    #region 포인트
    public bool CanAddPoint(ClassInfo info, TrainingStat stat)
    {
        if(!IsUnlocked(info) || !findStat(stat, out TrainingStatInfo statInfo))
            return false;

        ClassProgress progress = GetProgress(info);
        return progress.FreePoints > 0 && progress.GetPoints(stat) < statInfo.MaxPoints;
    }

    public bool TryAddPoint(ClassInfo info, TrainingStat stat)
    {
        if(!CanAddPoint(info, stat))
            return false;

        GetProgress(info).AddPoint(stat);
        Changed?.Invoke(info);
        return true;
    }

    public bool ResetPoints(ClassInfo info)
    {
        if(!GetProgress(info).ResetPoints())
            return false;

        Changed?.Invoke(info);
        return true;
    }
    #endregion

    public IStatModifier GetBonus(ClassInfo info)
    {
        return new TrainingBonus(GetProgress(info), info.Type, findStat);
    }

    public void WriteTo(SaveData data)
    {
        data.bestFloor = BestFloor;
        data.unlockedClasses = new List<int>(unlocked);
        data.classProgress.Clear();

        foreach(var pair in progresses)
        {
            data.classProgress.Add(new ClassProgressSaveData
            {
                classId = pair.Key,
                level = pair.Value.Level,
                points = new List<int>(pair.Value.ToArray())
            });
        }
    }
}