using DG.Tweening;
using UnityEngine;

public class WoundedLook : MonoBehaviour
{
    [SerializeField] UnitHealth health;
    [SerializeField] Animator animator;
    [SerializeField] AnimatorOverrideController woundedController;
    [SerializeField, Range(0f, 1f)] float threshold = 0.5f;

    [Header("연출")]
    [SerializeField] float shakeDuration = 0.5f;
    [SerializeField] float shakeStrength = 0.2f;
    [SerializeField] SfxType roarSfx = SfxType.None;

    private bool applied;

    private void OnEnable()
    {
        health.OnHpChanged += HandleHpChanged;
    }

    private void OnDisable()
    {
        health.OnHpChanged -= HandleHpChanged;
    }

    private void HandleHpChanged(int current, int max)
    {
        if(applied || max <= 0 || health.IsDead)
            return;

        if((float)current / max > threshold)
            return;

        Apply();
    }

    private void Apply()
    {
        applied = true;

        AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
        animator.runtimeAnimatorController = woundedController;
        animator.Play(state.fullPathHash, 0, state.normalizedTime);

        transform.DOShakePosition(shakeDuration, shakeStrength).SetLink(gameObject);
        SoundManager.instance.PlaySfx(roarSfx);
    }
}