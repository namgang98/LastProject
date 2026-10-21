using System;
using DG.Tweening;
using TMPro;
using UnityEngine;

public enum FloorKind
{
    Normal,
    Rest,
    Boss
}

public class FloorTransitionView : MonoBehaviour
{
    [Header("암막")]
    [SerializeField] CanvasGroup curtain;

    [Header("층 자막")]
    [SerializeField] CanvasGroup titleGroup;
    [SerializeField] TextMeshProUGUI titleText;
    [SerializeField] TextMeshProUGUI subText;

    [Header("층 종류별 부제")]
    [SerializeField] string restSubtitle = "모닥불이 타오른다";
    [SerializeField] string bossSubtitle = "강한 기운이 느껴진다";

    [Header("시간")]
    [SerializeField] float fadeOut = 0.4f;
    [SerializeField] float titleFade = 0.25f;
    [SerializeField] float hold = 0.7f;
    [SerializeField] float fadeIn = 0.4f;

    private Sequence sequence;

    private void Awake()
    {
        SetCurtain(0f);
        titleGroup.alpha = 0f;
    }

    private void OnDisable()
    {
        sequence?.Kill();
    }
    public void Play(int floor, FloorKind kind, string areaName, Action onCovered, Action onReveal, Action onFinished = null, bool fromBlack = false)
    {
        sequence?.Kill();

        titleText.text = $"{floor}F";
        subText.text = string.IsNullOrEmpty(areaName) ? GetSubtitle(kind) : areaName;
        subText.text = GetSubtitle(kind);
        titleGroup.alpha = 0f;

        if(fromBlack)
            SetCurtain(1f);
        else
            curtain.blocksRaycasts = true;

        sequence = DOTween.Sequence();

        if(!fromBlack)
            sequence.Append(curtain.DOFade(1f, fadeOut));

        sequence.AppendCallback(() => onCovered?.Invoke())
            .Append(titleGroup.DOFade(1f, titleFade))
            .AppendInterval(hold)
            .Append(titleGroup.DOFade(0f, titleFade))
            .AppendCallback(() => onReveal?.Invoke())
            .Append(curtain.DOFade(0f, fadeIn))
            .OnComplete(() =>
            {
                curtain.blocksRaycasts = false;
                onFinished?.Invoke();
            })
            .SetLink(gameObject);
    }

    private string GetSubtitle(FloorKind kind)
    {
        if(kind == FloorKind.Rest)
            return restSubtitle;

        if(kind == FloorKind.Boss)
            return bossSubtitle;

        return string.Empty;
    }

    private void SetCurtain(float alpha)
    {
        curtain.alpha = alpha;
        curtain.blocksRaycasts = alpha > 0f;
    }
}