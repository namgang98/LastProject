using UnityEngine;

public class FireStatus : DotStatus
{
    public override DamageElement Element => DamageElement.Fire;

    public FireStatus(UnitHealth target, int damage, float duration) : base(damage, 0.5f, target, duration) { }
}
