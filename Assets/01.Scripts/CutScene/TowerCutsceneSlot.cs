using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public enum TowerSlotState
{
    Hidden,
    Standing,
    Ruin,
}

public class TowerCutsceneSlot : MonoBehaviour
{
    [SerializeField] int towerId;
    [SerializeField] RectTransform mask;
    [SerializeField] Image towerImage;
    [SerializeField] Image ruinImage;
    [SerializeField] Sprite towerSprite;
    [SerializeField] Sprite ruinSprite;
    [SerializeField] float spriteScale = 0.2f;
    [SerializeField] ParticleSystem dust;

    private Vector2 towerSize;

    public int TowerId => towerId;
    public TowerInfo Info { get; private set; }

    private void Awake()
    {
        ApplyLayout();
    }

    [ContextMenu("레이아웃 미리보기")]
    private void ApplyLayout()
    {
        towerSize = Fit(towerImage, towerSprite, spriteScale);
        Fit(ruinImage, ruinSprite, spriteScale);
        SetBottom(mask);
        mask.sizeDelta = towerSize;
    }

    public void Bind(TowerInfo info)
    {
        Info = info;
    }

    public void SetState(TowerSlotState state)
    {
        gameObject.SetActive(state != TowerSlotState.Hidden);
        towerImage.rectTransform.anchoredPosition = Vector2.zero;
        SetAlpha(towerImage, state == TowerSlotState.Standing ? 1f : 0f);
        SetAlpha(ruinImage, state == TowerSlotState.Ruin ? 1f : 0f);
    }

    public Tween PlayCollapse(float duration, float shake)
    {
        SetState(TowerSlotState.Standing);
        PlayDust();

        Sequence sequence = DOTween.Sequence().SetUpdate(true).SetLink(gameObject);
        sequence.Append(towerImage.rectTransform.DOAnchorPosY(-towerSize.y, duration).SetEase(Ease.InQuad));
        sequence.Join(mask.DOShakeAnchorPos(duration, shake, 30, 90f, false, false));
        sequence.Insert(duration * 0.5f, ruinImage.DOFade(1f, duration * 0.5f));
        sequence.OnComplete(StopDust);
        return sequence;
    }

    public Tween PlayRise(float duration, float shake)
    {
        gameObject.SetActive(true);
        SetAlpha(ruinImage, 0f);
        SetAlpha(towerImage, 1f);
        towerImage.rectTransform.anchoredPosition = new Vector2(0f, -towerSize.y);
        PlayDust();

        Sequence sequence = DOTween.Sequence().SetUpdate(true).SetLink(gameObject);
        sequence.Append(towerImage.rectTransform.DOAnchorPosY(0f, duration).SetEase(Ease.OutCubic));
        sequence.Join(mask.DOShakeAnchorPos(duration, shake, 30, 90f, false, false));
        sequence.OnComplete(StopDust);
        return sequence;
    }

    private void PlayDust()
    {
        if(dust == null)
            return;

        dust.Play();
    }

    private void StopDust()
    {
        if(dust == null)
            return;

        dust.Stop(true, ParticleSystemStopBehavior.StopEmitting);
    }

    private static Vector2 Fit(Image image, Sprite sprite, float scale)
    {
        RectTransform rect = image.rectTransform;
        SetBottom(rect);
        image.sprite = sprite;

        if(sprite == null)
            return Vector2.zero;

        rect.sizeDelta = sprite.rect.size * scale;
        return rect.sizeDelta;
    }

    private static void SetBottom(RectTransform rect)
    {
        rect.anchorMin = new Vector2(0.5f, 0f);
        rect.anchorMax = new Vector2(0.5f, 0f);
        rect.pivot = new Vector2(0.5f, 0f);
        rect.anchoredPosition = Vector2.zero;
    }

    private static void SetAlpha(Image image, float alpha)
    {
        Color color = image.color;
        color.a = alpha;
        image.color = color;
    }
}