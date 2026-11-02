using System.Collections.Generic;
using UnityEngine;

public class BlessingPicker
{
    private static readonly ItemGrade[] Grades = { ItemGrade.Normal, ItemGrade.Rare, ItemGrade.Epic, ItemGrade.Unique, ItemGrade.Legendary };

    private readonly IReadOnlyList<BlessingInfo> pool;
    private readonly List<BlessingInfo> candidates = new();

    public BlessingPicker(IReadOnlyList<BlessingInfo> pool)
    {
        this.pool = pool;
    }

    public void Pick(RunBlessings owned, int count, BlessingGradeOdds odds, List<BlessingInfo> results)
    {
        results.Clear();
        candidates.Clear();

        foreach(BlessingInfo info in pool)
        {
            if(owned.CanTake(info) && BlessingStatRules.IsSupported(info))
                candidates.Add(info);
        }

        while(results.Count < count && candidates.Count > 0)
        {
            ItemGrade grade = RollGrade(odds);
            int index = PickIndex(grade);
            results.Add(candidates[index]);
            candidates.RemoveAt(index);
        }
    }

    private ItemGrade RollGrade(BlessingGradeOdds odds)
    {
        float total = 0f;

        foreach(ItemGrade grade in Grades)
        {
            if(HasGrade(grade))
                total += odds.Get(grade);
        }

        if(total <= 0f)
            return candidates[0].Grade;

        float roll = Random.Range(0f, total);

        foreach(ItemGrade grade in Grades)
        {
            if(!HasGrade(grade))
                continue;

            roll -= odds.Get(grade);

            if(roll < 0f)
                return grade;
        }

        return candidates[0].Grade;
    }

    private bool HasGrade(ItemGrade grade)
    {
        foreach(BlessingInfo info in candidates)
        {
            if(info.Grade == grade)
                return true;
        }

        return false;
    }

    private int PickIndex(ItemGrade grade)
    {
        int total = 0;

        foreach(BlessingInfo info in candidates)
        {
            if(info.Grade == grade)
                total += info.Weight;
        }

        int roll = Random.Range(0, total);

        for(int i = 0; i < candidates.Count; i++)
        {
            if(candidates[i].Grade != grade)
                continue;

            roll -= candidates[i].Weight;

            if(roll < 0)
                return i;
        }

        return 0;
    }
}