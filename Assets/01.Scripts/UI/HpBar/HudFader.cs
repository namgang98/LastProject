using DG.Tweening;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class HudFader : MonoBehaviour
{
    [SerializeField] float duration = 0.2f;

    private CanvasGroup group;
    private Tween tween;

    private void Awake()
    {
        group = GetComponent<CanvasGroup>();
    }

    private void OnDisable()
    {
        tween?.Kill();
    }

    public void Show()
    {
        Fade(1f);
    }

    public void Hide()
    {
        Fade(0f);
    }

    public void SetInstant(bool visible)
    {
        tween?.Kill();
        group.alpha = visible ? 1f : 0f;
    }

    private void Fade(float target)
    {
        tween?.Kill();
        tween = group.DOFade(target, duration).SetLink(gameObject);
    }
}