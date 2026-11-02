public interface IAttackRules
{
    int CritEvery { get; }
    float GetDamageRate(UnitHealth target);
    void OnDealt(int amount);
}

public class MonsterAttackRules : IAttackRules
{
    private readonly UnitHealth self;
    private readonly float lifesteal;

    public int CritEvery => 0;

    public MonsterAttackRules(UnitHealth self, float lifesteal)
    {
        this.self = self;
        this.lifesteal = lifesteal;
    }

    public float GetDamageRate(UnitHealth target)
    {
        return 1f;
    }

    public void OnDealt(int amount)
    {
        if(lifesteal <= 0f || amount <= 0 || self.IsDead)
            return;

        self.Heal(UnityEngine.Mathf.Max(1, UnityEngine.Mathf.RoundToInt(amount * lifesteal)));
    }
}