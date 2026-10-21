using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class CampSpot : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("테두리")]
    [SerializeField] Image outline;
    [SerializeField, Range(0f, 1f)] float idleAlpha = 0.08f;
    [SerializeField, Range(0f, 1f)] float hoverAlpha = 1f;

    [Header("이름표")]
    [SerializeField] GameObject nameLabel;

    [Header("확대 연출")]
    [SerializeField] float hoverScale = 1.02f;
    [SerializeField] float duration = 0.2f;

    [Header("클릭 판정")]
    [SerializeField, Range(0f, 1f)] float alphaHitThreshold = 0.1f;

    [Header("클릭했을 때")]
    [SerializeField] UnityEvent onClick;

    [Header("줌")]
    [SerializeField] VillageZoom zoom;
    [SerializeField] Transform focusPoint;

    private Vector2 opaqueCenter;

    private void Awake()
    {
        SetupHitTest();
        CacheOpaqueCenter();
        ApplyHover(false, true);
    }

    private void OnDisable()
    {
        transform.DOKill();

        if(outline != null)
            outline.DOKill();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        ApplyHover(true, false);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ApplyHover(false, false);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        ApplyHover(false, true);

        if(zoom == null || focusPoint == null)
        {
            onClick.Invoke();
            return;
        }

        zoom.Focus(focusPoint.position, onClick.Invoke);
    }

    private void ApplyHover(bool on, bool instant)
    {
        float scale = on ? hoverScale : 1f;
        float alpha = on ? hoverAlpha : idleAlpha;

        if(nameLabel != null)
            nameLabel.SetActive(on);

        transform.DOKill();

        if(outline != null)
            outline.DOKill();

        if(instant)
        {
            transform.localScale = Vector3.one * scale;
            SetOutlineAlpha(alpha);
            return;
        }

        transform.DOScale(scale, duration).SetEase(on ? Ease.OutBack : Ease.OutQuad);

        if(outline != null)
            outline.DOFade(alpha, duration);
    }

    private void SetOutlineAlpha(float alpha)
    {
        if(outline == null)
            return;

        Color c = outline.color;
        c.a = alpha;
        outline.color = c;
    }

    private void SetupHitTest()
    {
        Image image = GetComponent<Image>();

        if(image.sprite == null || !image.sprite.texture.isReadable)
            return;

        image.alphaHitTestMinimumThreshold = alphaHitThreshold;
    }

    private Vector3 GetFocusWorldPoint()
    {
        if(focusPoint != null)
            return focusPoint.position;

        return transform.TransformPoint(opaqueCenter);
    }

    private void CacheOpaqueCenter()
    {
        RectTransform rect = (RectTransform)transform;
        opaqueCenter = rect.rect.center;

        Sprite sprite = GetComponent<Image>().sprite;

        if(sprite == null || !sprite.texture.isReadable)
            return;

        Rect area = sprite.textureRect;
        int width = (int)area.width;
        int height = (int)area.height;
        Color[] pixels = sprite.texture.GetPixels((int)area.x, (int)area.y, width, height);

        int minX = width;
        int minY = height;
        int maxX = -1;
        int maxY = -1;

        for(int y = 0; y < height; y++)
        {
            for(int x = 0; x < width; x++)
            {
                if(pixels[y * width + x].a < alphaHitThreshold)
                    continue;

                minX = Mathf.Min(minX, x);
                maxX = Mathf.Max(maxX, x);
                minY = Mathf.Min(minY, y);
                maxY = Mathf.Max(maxY, y);
            }
        }

        if(maxX < 0)
            return;

        Vector2 center01 = new((minX + maxX + 1) * 0.5f / width, (minY + maxY + 1) * 0.5f / height);
        opaqueCenter = rect.rect.min + Vector2.Scale(center01, rect.rect.size);
    }
}