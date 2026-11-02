using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TitleMenuButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] Button button;
    [SerializeField] TMP_Text label;
    [SerializeField] Color normalColor = new(0.75f, 0.7f, 0.6f, 1f);
    [SerializeField] Color hoverColor = new(1f, 0.82f, 0.4f, 1f);
    [SerializeField] Color disabledColor = new(0.35f, 0.33f, 0.3f, 1f);
    [SerializeField] float hoverOffset = 14f;
    [SerializeField] float tweenTime = 0.15f;

    private RectTransform labelRect;
    private Vector2 basePosition;

    private void Awake()
    {
        labelRect = label.rectTransform;
        basePosition = labelRect.anchoredPosition;
    }

    private void OnEnable()
    {
        ResetVisual();
    }

    public void ResetVisual()
    {
        label.DOKill();
        labelRect.DOKill();
        labelRect.anchoredPosition = basePosition;
        label.color = button.interactable ? normalColor : disabledColor;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if(!button.interactable)
            return;

        Animate(hoverColor, basePosition + Vector2.right * hoverOffset);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if(!button.interactable)
            return;

        Animate(normalColor, basePosition);
    }

    private void Animate(Color color, Vector2 position)
    {
        label.DOKill();
        labelRect.DOKill();
        label.DOColor(color, tweenTime).SetUpdate(true).SetLink(label.gameObject);
        labelRect.DOAnchorPos(position, tweenTime).SetUpdate(true).SetLink(label.gameObject);
    }
}