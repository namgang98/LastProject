public class ShieldEffect : IBattleEffect
{
    private readonly UnitHealth target;
    private readonly int amount;
    public ShieldEffect(UnitHealth target, int amount)
    {
        this.target = target;
        this.amount = amount;
    }
    public void Execute()
    {
        target.AddShield(amount);
    }
}
