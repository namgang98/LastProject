using System;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class CloudParticle : MonoBehaviour
{
    [SerializeField, Range(0f, 0.5f)] float fadeRate = 0.2f;

    private Image image;
    private RectTransform rect;
    private float speed;
    private float maxAlpha;
    private float startX;
    private float endX;
    private Action<CloudParticle> onFinished;

    public float Width => rect.rect.width * Mathf.Abs(rect.localScale.x);

    private void Awake()
    {
        image = GetComponent<Image>();
        rect = (RectTransform)transform;
        image.raycastTarget = false;
    }

    public void Setup(Sprite sprite, float scale, bool flip, float alpha, float moveSpeed, Action<CloudParticle> finished)
    {
        image.sprite = sprite;
        image.SetNativeSize();
        rect.localScale = new Vector3(flip ? -scale : scale, scale, 1f);

        maxAlpha = alpha;
        speed = moveSpeed;
        onFinished = finished;
    }

    public void Place(float fromX, float toX, float x, float y)
    {
        startX = fromX;
        endX = toX;
        rect.localPosition = new Vector3(x, y, 0f);
        ApplyAlpha();
    }

    private void Update()
    {
        Vector3 position = rect.localPosition;
        position.x += speed * Time.deltaTime;
        rect.localPosition = position;

        if(position.x >= endX)
        {
            onFinished?.Invoke(this);
            return;
        }

        ApplyAlpha();
    }

    private void ApplyAlpha()
    {
        float progress = Mathf.InverseLerp(startX, endX, rect.localPosition.x);
        float fadeIn = Mathf.InverseLerp(0f, fadeRate, progress);
        float fadeOut = Mathf.InverseLerp(1f, 1f - fadeRate, progress);

        Color color = image.color;
        color.a = maxAlpha * Mathf.Min(fadeIn, fadeOut);
        image.color = color;
    }
}
