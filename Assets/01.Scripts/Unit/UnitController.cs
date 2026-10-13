using UnityEngine;

public class UnitController : MonoBehaviour
{
    [SerializeField] UnitHealth health;
    public UnitHealth Health => health;
    [SerializeField] UnitAnimator animator;
    [SerializeField] Transform headPoint;

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
        if(animator == null)
            return;

        animator.AttackAni();
    }
    private void HandleHit(DamageInfo info)
    {
        Vector3 pos = headPoint != null ? headPoint.position : transform.position;

        EventManager.instance.PostNotification(new OnUnitDamaged(pos, info));

        if(animator == null)
            return;
        animator.HurtAni();
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
    public void ResetUnit(int maxHp)
    {
        bool wasDead = health.IsDead;
        health.Init(maxHp);

        if(!wasDead || animator == null)
            return;

        animator.ResetAni();
    }
}
