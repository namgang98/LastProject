using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TutorialDialogView : MonoBehaviour
{
    [SerializeField] GameObject root;
    [SerializeField] TMP_Text lineText;
    [SerializeField] GameObject nextHint;
    [SerializeField] Button nextButton;
    [SerializeField] Button skipButton;
    [SerializeField, Min(1f)] float charsPerSecond = 40f;

    public event Action NextRequested;
    public event Action SkipRequested;

    private Tween typing;
    private bool clickToNext;

    private void Awake()
    {
        nextButton.onClick.AddListener(HandleNext);
        skipButton.onClick.AddListener(HandleSkip);
        root.SetActive(false);
    }

    public void Show(string line, bool waitClick)
    {
        root.SetActive(true);
        clickToNext = waitClick;
        nextHint.SetActive(false);
        lineText.text = line;
        lineText.ForceMeshUpdate();

        int count = lineText.textInfo.characterCount;
        lineText.maxVisibleCharacters = 0;
        typing?.Kill();
        typing = DOTween.To(() => lineText.maxVisibleCharacters, value => lineText.maxVisibleCharacters = value, count, count / charsPerSecond)
            .SetEase(Ease.Linear)
            .SetUpdate(true)
            .SetLink(gameObject)
            .OnComplete(ShowHint);
    }

    public void Hide()
    {
        typing?.Kill();
        root.SetActive(false);
    }

    private void ShowHint()
    {
        nextHint.SetActive(clickToNext);
    }

    private void HandleNext()
    {
        if(typing != null && typing.IsActive() && typing.IsPlaying())
        {
            typing.Complete();
            return;
        }

        if(!clickToNext)
            return;

        NextRequested?.Invoke();
    }

    private void HandleSkip()
    {
        SkipRequested?.Invoke();
    }
}