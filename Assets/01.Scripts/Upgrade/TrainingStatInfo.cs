using UnityEngine;
public enum TrainingStat
{
    MaxHp,
    Damage,
    Cooldown,
    Crit,
}
public class TrainingStatInfo
{
    public const int StatCount = 4;

    public TrainingStat Stat { get; }
    public float Increase { get; }
    public int MaxPoints { get; }
    public string Description { get; }
    public TrainingStatInfo(TrainingStat stat, float increase, int maxPoints, string desc)
    {
        Stat = stat;
        Increase = increase;
        MaxPoints = maxPoints;
        Description = desc;
    }

    public float ValueAt(int points)
    {
        return Increase * points;
    }

    public string FormatValue(int points)
    {
        float value = ValueAt(points);

        if(Stat == TrainingStat.MaxHp)
            return $"+{value:0}";

        return $"{(Stat == TrainingStat.Cooldown ? "-" : "+")}{value * 100f:0}%";
    }
}
