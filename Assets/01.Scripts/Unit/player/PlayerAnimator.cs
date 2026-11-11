using UnityEngine;

public class PlayerAnimator : UnitAnimator
{
    [SerializeField] bool useCriticalAttack = true;

    public override void CriticalAni()
    {
        if(!useCriticalAttack)
        {
            AttackAni();
            return;
        }

        animator.ResetTrigger(AnimatorTag.Hurt);
        animator.SetTrigger(AnimatorTag.Attack2);
    }

    public override void DeadAni()
    {
        animator.ResetTrigger(AnimatorTag.Attack2);
        base.DeadAni();
    }
}