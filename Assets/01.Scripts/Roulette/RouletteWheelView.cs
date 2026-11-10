using System;
using DG.Tweening;
using UnityEngine;

public class RouletteWheelView : MonoBehaviour
{
    [SerializeField] RectTransform needle;
    [SerializeField, Min(1)] int extraTurns = 5;
    [SerializeField, Min(0.1f)] float duration = 4f;
    [SerializeField, Range(0f, 0.45f)] float jitter = 0.35f;

    public void Spin(int index, int slotCount, Action onDone)
    {
        float step = 360f / slotCount;
        float current = Mathf.Repeat(-needle.localEulerAngles.z, 360f);
        float target = index * step + UnityEngine.Random.Range(-jitter, jitter) * step;
        float delta = Mathf.Repeat(target - current, 360f) + 360f * extraTurns;

        needle.DOKill();
        needle.localEulerAngles = new Vector3(0f, 0f, -current);
        needle.DOLocalRotate(new Vector3(0f, 0f, -(current + delta)), duration, RotateMode.FastBeyond360)
            .SetEase(Ease.OutQuart)
            .SetLink(needle.gameObject)
            .OnComplete(() => onDone?.Invoke());
    }
}