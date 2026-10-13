using UnityEngine;
using System;
using TMPro;
using DG.Tweening;
public class DamageText : MonoBehaviour
{
    [SerializeField] TextMeshPro text;
    [SerializeField] float rise = 0.8f;
    [SerializeField] float duration = 0.6f;

    private Sequence sequence;

    public void Play(string value, Color color, Vector3 pos, float scale, Action<DamageText> onDone)
    {
        sequence?.Kill();

        text.text = value;
        text.color = color;
        transform.position = pos;
        transform.localScale = Vector3.one * scale;
        text.alpha = 1f;

        sequence = DOTween.Sequence();
        sequence.Append(transform.DOMoveY(pos.y + rise, duration).SetEase(Ease.OutCubic));
        sequence.Join(DOTween.To(() => text.alpha, x => text.alpha = x, 0f, duration).SetEase(Ease.InQuad));
        sequence.OnComplete(() => onDone(this));
        sequence.SetLink(gameObject);

    }
}
