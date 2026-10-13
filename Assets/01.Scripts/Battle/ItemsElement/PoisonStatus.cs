using UnityEngine;

public class PoisonStatus : DotStatus
{
    public override DamageElement Element => DamageElement.Poison;

    public PoisonStatus(UnitHealth target, int damage, float duration) : base(damage, 1f, target, duration) { }
}
