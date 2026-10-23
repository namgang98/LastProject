using UnityEngine;

public class UnitController : MonoBehaviour
{
    [Header("피격 연출")]
    [SerializeField] float hurtCooldown = 0.6f;
    [SerializeField] float attackLock = 0.5f;
    [SerializeField] UnitHealth health;

    private float nextHurtTime;
    private float attackLockUntil;
    public UnitHealth Health => health;
    [SerializeField] UnitAnimator animator;
    [SerializeField] Transform headPoint;
    [SerializeField] float attackWindup = 0.3f;
    [SerializeField] SfxType attackSfx = SfxType.None;
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
        health.OnDie += animator.DeadAni;
    }
    private void OnDisable()
    {
        health.OnHit -= HandleHit;
        health.OnHealed -= HandleHealed;
        health.OnBlocked -= HandleShield;
        health.OnDie -= animator.DeadAni;
    }
    public void PlayAttack()
    {
        if(animator == null || health.IsDead)
            return;

        attackLockUntil = Time.time + attackLock;
        animator.AttackAni();
        SoundManager.instance.PlaySfx(attackSfx);
    }
    private void HandleHit(DamageInfo info)
    {
        Vector3 pos = headPoint != null ? headPoint.position : transform.position;

        EventManager.instance.PostNotification(new OnUnitDamaged(pos, info, GetInstanceID()));

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
    public void Init(int maxHp)
    {
        health.Init(maxHp);
    }
}
