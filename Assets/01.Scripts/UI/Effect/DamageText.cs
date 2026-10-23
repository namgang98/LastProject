using System;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class DamageText : MonoBehaviour
{
    [SerializeField] TextMeshPro text;
    [SerializeField] float rise = 0.8f;
    [SerializeField] float duration = 0.6f;
    [SerializeField] float punch = 0.25f;

    private Sequence sequence;
    private Action<DamageText> onDone;
    private float baseScale;

    public void Play(string value, Color color, Vector3 pos, float scale, Action<DamageText> onDone)
    {
        this.onDone = onDone;
        baseScale = scale;
        text.color = color;
        transform.position = pos;
        Run(value, rise);
    }

    public void Refresh(string value)
    {
        Run(value, rise * 0.5f);
        transform.DOPunchScale(Vector3.one * punch * baseScale, 0.15f, 1, 0f).SetLink(gameObject);
    }

    private void Run(string value, float height)
    {
        sequence?.Kill();
        transform.DOKill();

        text.text = value;
        text.alpha = 1f;
        transform.localScale = Vector3.one * baseScale;

        sequence = DOTween.Sequence()
            .Append(transform.DOMoveY(transform.position.y + height, duration).SetEase(Ease.OutCubic))
            .Join(DOTween.To(() => text.alpha, x => text.alpha = x, 0f, duration).SetEase(Ease.InQuad))
            .OnComplete(() => onDone?.Invoke(this))
            .SetLink(gameObject);
    }
}