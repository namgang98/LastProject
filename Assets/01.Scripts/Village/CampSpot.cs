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

    private void Awake()
    {
        SetupHitTest();
        ApplyHover(false, true);
    }

    private void OnDisable()
    {
        transform.DOKill();

        if (outline != null)
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
        onClick.Invoke();
    }

    private void ApplyHover(bool on, bool instant)
    {
        float scale = on ? hoverScale : 1f;
        float alpha = on ? hoverAlpha : idleAlpha;

        if (nameLabel != null)
            nameLabel.SetActive(on);

        transform.DOKill();

        if (outline != null)
            outline.DOKill();

        if (instant)
        {
            transform.localScale = Vector3.one * scale;
            SetOutlineAlpha(alpha);
            return;
        }

        transform.DOScale(scale, duration).SetEase(on ? Ease.OutBack : Ease.OutQuad);

        if (outline != null)
            outline.DOFade(alpha, duration);
    }

    private void SetOutlineAlpha(float alpha)
    {
        if (outline == null)
            return;

        Color c = outline.color;
        c.a = alpha;
        outline.color = c;
    }

    private void SetupHitTest()
    {
        Image image = GetComponent<Image>();
        image.alphaHitTestMinimumThreshold = alphaHitThreshold;
    }
}