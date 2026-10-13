public readonly struct AttackData
{
    public readonly int Damage;
    public readonly float Cooldown;
    public readonly float CritChance;
    public readonly float CritMultiplier;
    public readonly DamageElement Element;
    public readonly float ElementChance;
    public readonly int ElementValue;
    public readonly float ElementDuration;

    public AttackData(int damage, float cooldown, float critchance, float critMultiplier, DamageElement element, float elementChanceE, int elementValue, float elementDuration)
    {
        Damage = damage;
        Cooldown = cooldown;
        CritChance = critchance;
        CritMultiplier = critMultiplier;
        Element = element;
        ElementChance = elementChanceE;
        ElementValue = elementValue;
        ElementDuration = elementDuration;        
    }
    public AttackData WithDamage(int damage) => new AttackData(damage, Cooldown, CritChance, CritMultiplier, Element, ElementChance, ElementValue, ElementDuration);
    public AttackData WithCooldown(float cooldown) => new AttackData(Damage, cooldown, CritChance, CritMultiplier, Element, ElementChance, ElementValue, ElementDuration);
    public AttackData WithCrit(float chance, float multiplier) => new AttackData(Damage, Cooldown, chance, multiplier, Element, ElementChance, ElementValue, ElementDuration);
}
