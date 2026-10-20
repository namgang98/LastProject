using DG.Tweening;
using TMPro;
using UnityEngine;

public class FeedbackUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI messageText;
    [SerializeField] CanvasGroup canvasGroup;
    [SerializeField] float showDuration = 1.2f;
    [SerializeField] float fadeDuration = 0.3f;

    [Header("색")]
    [SerializeField] Color infoColor = new(0.84f, 0.78f, 0.66f, 1f);
    [SerializeField] Color warningColor = new(0.78f, 0.32f, 0.26f, 1f);

    private Sequence currentSequence;

    private void Awake()
    {
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
    }

    private void OnEnable()
    {
        EventManager.instance.AddListener<OnFeedback>(HandleFeedback);
        canvasGroup.alpha = 0f;
    }

    private void OnDisable()
    {
        currentSequence?.Kill();
        canvasGroup.alpha = 0f;

        if (!EventManager.HasInstance)
            return;

        EventManager.instance.RemoveListener<OnFeedback>(HandleFeedback);
    }

    private void HandleFeedback(OnFeedback e)
    {
        ShowMessage(e.Message, e.Type);
    }

    public void ShowMessage(string message, FeedbackType type)
    {
        messageText.text = message;
        messageText.color = type == FeedbackType.Warning ? warningColor : infoColor;

        currentSequence?.Kill();
        currentSequence = DOTween.Sequence()
            .Append(canvasGroup.DOFade(1f, fadeDuration))
            .AppendInterval(showDuration)
            .Append(canvasGroup.DOFade(0f, fadeDuration))
            .SetUpdate(true)
            .SetLink(gameObject);
    }
}
