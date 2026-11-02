using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class CrossfadeSpritePlayer : MonoBehaviour
{
    [SerializeField] SpriteRenderer back;
    [SerializeField] SpriteRenderer front;

    public void Show(Sprite sprite)
    {
        back.sprite = sprite;
        front.sprite = null;
        SetAlpha(front, 0f);
    }

    public IEnumerator Play(IReadOnlyList<Sprite> frames, float frameTime)
    {
        foreach(Sprite frame in frames)
        {
            front.sprite = frame;
            SetAlpha(front, 0f);

            yield return front.DOFade(1f, frameTime)
                .SetEase(Ease.Linear)
                .SetUpdate(true)
                .SetLink(front.gameObject)
                .WaitForCompletion();

            back.sprite = frame;
            SetAlpha(front, 0f);
        }
    }

    private static void SetAlpha(SpriteRenderer renderer, float alpha)
    {
        Color color = renderer.color;
        color.a = alpha;
        renderer.color = color;
    }
}