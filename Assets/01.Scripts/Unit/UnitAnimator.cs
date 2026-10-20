using UnityEngine;

public abstract class UnitAnimator : MonoBehaviour
{
    [SerializeField] protected Animator animator;
    
    public virtual void AttackAni()
    {
        animator.SetTrigger(AnimatorTag.Attack);
    }
    public virtual void HurtAni()
    {
        animator.SetTrigger(AnimatorTag.Hurt);
    }
    public virtual void DeadAni()
    {
        animator.ResetTrigger(AnimatorTag.Attack);
        animator.ResetTrigger(AnimatorTag.Hurt);
        animator.SetTrigger(AnimatorTag.Death);
    }
}
