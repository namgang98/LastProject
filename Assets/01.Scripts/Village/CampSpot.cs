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

    [Header("확대 대상 (비우면 자기 자신)")]
    [SerializeField] RectTransform scaleTarget;

    private Vector2 opaqueCenter;
    private Tween hoverTween;
    private float currentScale = 1f;
    private Vector2 baseAnchored;
    private Vector2 focusOffset;
    private bool hasBase;
    private void Awake()
    {
        if(scaleTarget == null)
            scaleTarget = (RectTransform)transform;

        SetupHitTest();

        if(nameLabel != null)
            nameLabel.SetActive(false);

        SetOutlineAlpha(idleAlpha);
    }

    private void OnDisable()
    {
        hoverTween?.Kill();

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
        float target = on ? hoverScale : 1f;
        float alpha = on ? hoverAlpha : idleAlpha;

        if(nameLabel != null)
            nameLabel.SetActive(on);

        hoverTween?.Kill();

        if(outline != null)
            outline.DOKill();

        if(on && Mathf.Approximately(currentScale, 1f))
            CaptureBase();

        if(instant)
        {
            SetScale(target);
            SetOutlineAlpha(alpha);
            return;
        }

        hoverTween = DOTween.To(() => currentScale, SetScale, target, duration)
            .SetEase(on ? Ease.OutBack : Ease.OutQuad)
            .SetLink(gameObject);

        if(outline != null)
            outline.DOFade(alpha, duration);
    }
    private void CaptureBase()
    {
        hasBase = true;
        baseAnchored = scaleTarget.anchoredPosition;

        if(focusPoint == null)
        {
            focusOffset = Vector2.zero;
            return;
        }

        RectTransform parent = (RectTransform)scaleTarget.parent;
        Vector3 worldOffset = scaleTarget.position - focusPoint.position;
        focusOffset = parent.InverseTransformVector(worldOffset);
    }

    private void SetScale(float scale)
    {
        currentScale = scale;
        scaleTarget.localScale = Vector3.one * scale;

        if(Mathf.Approximately(scale, 1f))
        {
            if(hasBase)
                scaleTarget.anchoredPosition = baseAnchored;

            return;
        }
        scaleTarget.anchoredPosition = baseAnchored + focusOffset * (scale - 1f);
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

    private void MovePivotToFocus()
    {
        if(focusPoint == null)
            return;

        Vector2 size = scaleTarget.rect.size;

        if(size.x <= 0f || size.y <= 0f)
            return;

        Vector2 local = scaleTarget.InverseTransformPoint(focusPoint.position);
        Vector2 pivotShift = new(local.x / size.x, local.y / size.y);

        scaleTarget.pivot += pivotShift;
        scaleTarget.localPosition += (Vector3)Vector2.Scale(local, (Vector2)scaleTarget.localScale);
    }
}