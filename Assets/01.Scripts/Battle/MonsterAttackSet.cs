public readonly struct MonsterAttackSet
{
    public AttackData Normal { get; }
    public AttackData Skill { get; }
    public float SkillChance { get; }

    public bool HasSkill => SkillChance > 0f;

    public MonsterAttackSet(AttackData normal, AttackData skill, float skillChance)
    {
        Normal = normal;
        Skill = skill;
        SkillChance = skillChance;
    }

    public static MonsterAttackSet Create(MonsterInfo info, int damage)
    {
        AttackData normal = info.Attack.WithDamage(damage);
        AttackData skill = info.Attack.WithDamage(UnityEngine.Mathf.RoundToInt(damage * info.SkillMultiplier));

        return new MonsterAttackSet(normal, skill, info.SkillChance);
    }
}