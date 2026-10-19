using System;

public class ClassProgress
{
    private readonly int[] points = new int[TrainingStatInfo.StatCount];

    public int Level { get; private set; } = 1;

    public int SpentPoints
    {
        get
        {
            int sum = 0;

            foreach(int p in points)
            {
                sum += p;
            }

            return sum;
        }
    }

    public int FreePoints => Math.Max(0, Level - 1 - SpentPoints);

    public int GetPoints(TrainingStat stat)
    {
        return points[(int)stat];
    }

    internal void Restore(int level, int[] saved)
    {
        Level = Math.Max(1, level);

        for(int i = 0; i < points.Length && i < saved.Length; i++)
        {
            points[i] = Math.Max(0, saved[i]);
        }

        while(SpentPoints > Level - 1)
        {
            RemoveOne();
        }
    }

    internal void LevelUp()
    {
        Level++;
    }

    internal void AddPoint(TrainingStat stat)
    {
        points[(int)stat]++;
    }

    internal bool ResetPoints()
    {
        if(SpentPoints == 0)
            return false;

        Array.Clear(points, 0, points.Length);
        return true;
    }

    internal int[] ToArray()
    {
        return (int[])points.Clone();
    }

    private void RemoveOne()
    {
        for(int i = points.Length - 1; i >= 0; i--)
        {
            if(points[i] <= 0)
                continue;

            points[i]--;
            return;
        }
    }
}