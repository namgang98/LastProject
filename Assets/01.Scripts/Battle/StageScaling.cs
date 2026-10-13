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
    private readonly float damageGrowth;

    public StageScaling(float hpGrowth, float damageGrowth)
    {
        this.hpGrowth = hpGrowth;
        this.damageGrowth = damageGrowth;
    }

    public MonsterStats Apply(MonsterInfo info, int stage)
    {
        int level = stage - 1;
        int hp = Mathf.RoundToInt(info.MaxHp * (1f + level * hpGrowth));
        int damage = Mathf.RoundToInt(info.Attack.Damage * (1f + level * damageGrowth));

        return new MonsterStats(hp, damage);
    }

}
