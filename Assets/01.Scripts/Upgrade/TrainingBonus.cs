using UnityEngine;

public delegate bool TrainingStatLookup(TrainingStat stat, out TrainingStatInfo info);

public class TrainingBonus : IStatModifier
{
    private readonly ClassProgress progress;
    private readonly WeaponType type;
    private readonly TrainingStatLookup findStat;

    public TrainingBonus(ClassProgress progress, WeaponType type, TrainingStatLookup findStat)
    {
        this.progress = progress;
        this.type = type;
        this.findStat = findStat;
    }

    public void Apply(PlayerStats stats)
    {
        foreach(TrainingStat stat in System.Enum.GetValues(typeof(TrainingStat)))
        {
            int points = progress.GetPoints(stat);

            if(points <= 0 || !findStat(stat, out TrainingStatInfo info))
                continue;

            float value = info.ValueAt(points);

            switch(stat)
            {
                case TrainingStat.MaxHp:
                    stats.AddMaxHp(Mathf.RoundToInt(value));
                    break;
                case TrainingStat.Damage:
                    stats.AddTypeDamageBonus(type, value);
                    break;
                case TrainingStat.Cooldown:
                    stats.AddTypeCooldownReduction(type, value);
                    break;
                case TrainingStat.Crit:
                    stats.AddCritChance(value);
                    break;
            }
        }
    }
}