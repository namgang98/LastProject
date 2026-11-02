using UnityEngine;
public readonly struct MonsterStats
{
    public readonly int MaxHp;
    public readonly int Damage;

    public MonsterStats(int max, int damage)
    {
        MaxHp = max;
        Damage = damage;
    }

}


public class StageScaling
{
    private readonly float hpGrowth;
    private readonly float hpCurve;
    private readonly float damageGrowth;
    private readonly float damageCurve;

    public StageScaling(float hpGrowth, float hpCurve, float damageGrowth, float damageCurve)
    {
        this.hpGrowth = hpGrowth;
        this.hpCurve = hpCurve;
        this.damageGrowth = damageGrowth;
        this.damageCurve = damageCurve;
    }

    public MonsterStats Apply(MonsterInfo info, int stage)
    {
        int level = Mathf.Max(0, stage - 1);
        float hpRate = Rate(level, hpGrowth, hpCurve);
        float damageRate = Rate(level, damageGrowth, damageCurve);

        int hp = Mathf.RoundToInt(info.MaxHp * hpRate);
        int damage = Mathf.RoundToInt(info.Attack.Damage * damageRate);

        return new MonsterStats(hp, damage);
    }

    private static float Rate(int level, float growth, float curve)
    {
        return 1f + level * growth + level * level * curve;
    }
}
