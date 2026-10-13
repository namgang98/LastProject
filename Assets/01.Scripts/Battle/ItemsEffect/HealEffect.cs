public class HealEffect : IBattleEffect
{
    private readonly UnitHealth target;
    private readonly int amount;
    public HealEffect(UnitHealth target, int amount)
    {
        this.target = target;
        this.amount = amount;
    }

    public void Execute()
    {
        target.Heal(amount);
    }
}

