using UnityEngine;

public class UnitController : MonoBehaviour
{
    [Header("피격 연출")]
    [SerializeField] float hurtCooldown = 0.6f;
    [SerializeField] float attackLock = 0.5f;
    [SerializeField] UnitHealth health;

    private float nextHurtTime;
    private float attackLockUntil;
    private IRevivable revivable;
    public UnitHealth Health => health;
    [SerializeField] UnitAnimator animator;
    [SerializeField] Transform headPoint;
    [SerializeField] float attackWindup = 0.3f;
    [SerializeField] SfxType attackSfx = SfxType.None;
    [SerializeField] SfxType skillSfx = SfxType.None;
    [SerializeField] SfxType hurtSfx = SfxType.None;
    [SerializeField] SfxType deathSfx = SfxType.None;
    [SerializeField] float hurtSfxCooldown = 0.25f;
    private float nextHurtSfxTime;
    public float AttackWindup => attackWindup;
    private void Reset()
    {
        health = GetComponent<UnitHealth>();
    }
    private void Awake()
    {
        if(health == null)
            Debug.LogError($"[UnitController] {name}에 UnitHealth가 연결되지 않았습니다.", this);
    }
    private void OnEnable()
    {
        health.OnHit += HandleHit;
        health.OnHealed += HandleHealed;
        health.OnBlocked += HandleShield;
        health.OnDie += HandleDie;
        health.OnDodged += HandleDodged;

        revivable = health as IRevivable;

        if(revivable != null)
            revivable.OnRevived += HandleRevived;
    }
    private void OnDisable()
    {
        health.OnHit -= HandleHit;
        health.OnHealed -= HandleHealed;
        health.OnBlocked -= HandleShield;
        health.OnDie -= HandleDie;
        health.OnDodged -= HandleDodged;

        if(revivable != null)
            revivable.OnRevived -= HandleRevived;
    }
    public void PlayAttack()
    {
        PlayAttackMotion(false);
    }

    public void PlaySkill()
    {
        PlayAttackMotion(true);
    }

    private void PlayAttackMotion(bool isSkill)
    {
        if(animator == null || health.IsDead)
            return;

        attackLockUntil = Time.time + attackLock;

        if(isSkill)
            animator.SkillAni();
        else
            animator.AttackAni();

        SfxType sfx = isSkill && skillSfx != SfxType.None ? skillSfx : attackSfx;
        Sfx.Play(sfx);
    }
    private void HandleHit(DamageInfo info)
    {
        Vector3 pos = headPoint != null ? headPoint.position : transform.position;

        EventManager.instance.PostNotification(new OnUnitDamaged(pos, info, GetInstanceID()));
        PlayHurtSfx(info);
        if(animator == null || info.IsTick || health.CurrentHP <= 0)
            return;

        if(Time.time < nextHurtTime || Time.time < attackLockUntil)
            return;

        nextHurtTime = Time.time + hurtCooldown;
        animator.HurtAni();
    }
    public void SetMoving(bool moving)
    {
        if(animator == null || health.IsDead)
            return;

        animator.SetMoving(moving);
    }
    private void PlayHurtSfx(DamageInfo info)
    {
        if(info.IsTick || health.CurrentHP <= 0 || Time.time < nextHurtSfxTime)
            return;

        nextHurtSfxTime = Time.time + hurtSfxCooldown;
        Sfx.Play(hurtSfx);
    }

    private void HandleDie()
    {
        Sfx.Play(deathSfx);

        if(animator != null)
            animator.DeadAni();
    }
    private void HandleHealed(int value)
    {
        Vector3 pos = headPoint != null? headPoint.position : transform.position;

        EventManager.instance.PostNotification(new OnUnitHealed(pos, value));
    }
    private void HandleShield()
    {
        Vector3 pos = headPoint != null ? headPoint.position : transform.position;
        EventManager.instance.PostNotification(new OnUnitBlocked(pos));
    }
    private void HandleRevived()
    {
        Vector3 pos = headPoint != null ? headPoint.position : transform.position;
        EventManager.instance.PostNotification(new OnUnitRevived(pos));
    }
    private void HandleDodged()
    {
        Vector3 pos = headPoint != null ? headPoint.position : transform.position;
        EventManager.instance.PostNotification(new OnUnitDodged(pos));
    }
    public void Init(int maxHp)
    {
        health.Init(maxHp);
    }
}
