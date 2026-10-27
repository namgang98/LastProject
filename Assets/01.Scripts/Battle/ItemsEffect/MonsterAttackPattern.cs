using UnityEngine;

public class MonsterAttackPattern : IBattleEffect
{
    private readonly IBattleEffect normal;
    private readonly IBattleEffect skill;
    private readonly float skillChance;
    private bool skillQueued;

    public MonsterAttackPattern(IBattleEffect normal, IBattleEffect skill, float skillChance)
    {
        this.normal = normal;
        this.skill = skill;
        this.skillChance = skillChance;
    }

    public bool Prepare()
    {
        skillQueued = skill != null && Random.value < skillChance;
        return skillQueued;
    }

    public void Execute()
    {
        if(skillQueued)
            skill.Execute();
        else
            normal.Execute();

        skillQueued = false;
    }
}