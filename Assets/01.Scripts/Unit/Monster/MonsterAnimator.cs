public class MonsterAnimator : UnitAnimator
{
    public override void SkillAni()
    {
        animator.ResetTrigger(AnimatorTag.Hurt);
        animator.SetTrigger(AnimatorTag.Attack2);
    }

    public override void DeadAni()
    {
        animator.ResetTrigger(AnimatorTag.Attack2);
        base.DeadAni();
    }
}