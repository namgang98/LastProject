using System;
using DG.Tweening;
using UnityEngine;

public class VillageZoom : MonoBehaviour
{
    [Header("확대할 마을 그림 (건물들의 부모)")]
    [SerializeField] RectTransform content;
    [SerializeField] CanvasGroup contentGroup;
    [SerializeField] float zoomScale = 1.6f;

    [Header("걸어가는 연출")]
    [SerializeField] SfxType stepSfx = SfxType.FootstepsWood;
    [SerializeField] int inSteps = 3;
    [SerializeField] int outSteps = 2;
    [SerializeField] float stepTime = 0.28f;
    [SerializeField, Range(0.1f, 1f)] float moveRate = 0.55f;

    private RectTransform viewport;
    private Vector2 homePosition;
    private Sequence sequence;
    private bool isZoomed;

    private void Awake()
    {
        viewport = content.parent as RectTransform;
        homePosition = content.anchoredPosition;
    }

    private void OnEnable()
    {
        EventManager.instance.AddListener<OnPopupCountChanged>(HandlePopupCountChanged);
    }

    private void OnDisable()
    {
        sequence?.Kill();

        if(!EventManager.HasInstance)
            return;

        EventManager.instance.RemoveListener<OnPopupCountChanged>(HandlePopupCountChanged);
    }

    public void Focus(Vector3 worldPoint, Action onZoomed)
    {
        if(isZoomed || sequence.IsActive())
            return;

        Vector2 point = content.InverseTransformPoint(worldPoint);
        contentGroup.blocksRaycasts = false;

        sequence = DOTween.Sequence();

        for(int i = 1; i <= inSteps; i++)
        {
            float scale = Mathf.Lerp(1f, zoomScale, (float)i / inSteps);
            AppendStep(scale, ClampToView(-point * scale, scale));
        }

        sequence.OnComplete(() =>
        {
            isZoomed = true;
            onZoomed?.Invoke();
        })
            .SetUpdate(true)
            .SetLink(gameObject);
    }

    private void ZoomOut()
    {
        isZoomed = false;
        sequence?.Kill();

        float startScale = content.localScale.x;
        Vector2 startPosition = content.anchoredPosition;

        sequence = DOTween.Sequence();

        for(int i = 1; i <= outSteps; i++)
        {
            float rate = (float)i / outSteps;
            AppendStep(Mathf.Lerp(startScale, 1f, rate), Vector2.Lerp(startPosition, homePosition, rate));
        }

        sequence.OnComplete(() => contentGroup.blocksRaycasts = true)
            .SetUpdate(true)
            .SetLink(gameObject);
    }

    private void AppendStep(float scale, Vector2 position)
    {
        float move = stepTime * moveRate;

        sequence.AppendCallback(() => SoundManager.instance.PlaySfx(stepSfx))
            .Append(content.DOScale(scale, move).SetEase(Ease.OutCubic))
            .Join(content.DOAnchorPos(position, move).SetEase(Ease.OutCubic))
            .AppendInterval(stepTime - move);
    }

    private void HandlePopupCountChanged(OnPopupCountChanged e)
    {
        if(e.Count > 0 || !isZoomed)
            return;

        ZoomOut();
    }

    private Vector2 ClampToView(Vector2 position, float scale)
    {
        Vector2 limit = (content.rect.size * scale - viewport.rect.size) * 0.5f;
        limit = Vector2.Max(limit, Vector2.zero);

        float x = Mathf.Clamp(position.x, homePosition.x - limit.x, homePosition.x + limit.x);
        float y = Mathf.Clamp(position.y, homePosition.y - limit.y, homePosition.y + limit.y);
        return new Vector2(x, y);
    }
}