public enum DamageElement
{
    None,
    Poison,
    Fire,
    Ice,

}

public readonly struct DamageInfo
{
    public readonly int Amount;
    public readonly bool IsCritical;
    public readonly DamageElement Element;
    public readonly bool IsTick;
    public DamageInfo(int amount, bool isCritical, DamageElement element, bool isTick = false)
    {
        Amount = amount;
        IsCritical = isCritical;
        Element = element;
        IsTick = isTick;
    }
}
