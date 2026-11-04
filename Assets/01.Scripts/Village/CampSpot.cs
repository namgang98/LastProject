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
}