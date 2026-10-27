using UnityEngine;

public abstract class UnitAnimator : MonoBehaviour
{
    [SerializeField] protected Animator animator;
    
    public virtual void AttackAni()
    {
        animator.ResetTrigger(AnimatorTag.Hurt);
        animator.SetTrigger(AnimatorTag.Attack);
    }
    public virtual void SkillAni()
    {
        AttackAni();
    }
    public virtual void HurtAni()
    {
        animator.SetTrigger(AnimatorTag.Hurt);
    }

    public virtual void SetMoving(bool moving)
    {
        animator.SetBool(AnimatorTag.Walk, moving);
    }
    public virtual void DeadAni()
    {
        animator.ResetTrigger(AnimatorTag.Attack);
        animator.ResetTrigger(AnimatorTag.Hurt);
        animator.SetTrigger(AnimatorTag.Death);
    }
}
