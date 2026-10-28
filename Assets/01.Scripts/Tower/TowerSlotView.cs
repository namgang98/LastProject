using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TowerSlotView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] int towerId;
    [SerializeField] Button button;
    [SerializeField] Image image;
    [SerializeField] Sprite towerSprite;
    [SerializeField] float spriteScale = 0.2f;
    [SerializeField] Sprite ruinSprite;
    [SerializeField] GameObject nameplate;
    [SerializeField] TMP_Text nameText;
    [SerializeField] float hoverScale = 1.05f;
    [SerializeField] float hoverTime = 0.15f;
    [SerializeField, Range(0f, 1f)] float hitAlpha = 0.1f;

    public int TowerId => towerId;
    public TowerInfo Info { get; private set; }
    public event Action<TowerInfo> Clicked;

    private void Awake()
    {
        image.alphaHitTestMinimumThreshold = hitAlpha;
        button.onClick.AddListener(HandleClick);
        nameplate.SetActive(false);
    }

    public void Bind(TowerInfo info, bool collapsed, bool enterable)
    {
        Info = info;
        image.sprite = collapsed ? ruinSprite : towerSprite;
        image.SetNativeSize();
        image.rectTransform.sizeDelta = image.sprite.rect.size * spriteScale;
        button.interactable = enterable;
        nameText.text = collapsed ? $"{info.Name}\n<size=70%>무너진 탑</size>" : info.Name;
        nameplate.SetActive(false);
        transform.DOKill();
        transform.localScale = Vector3.one;
    }

    public Tween PlayEnter()
    {
        transform.DOKill();
        return transform.DOPunchScale(Vector3.one * 0.12f, 0.4f, 6, 0.6f).SetUpdate(true).SetLink(gameObject);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        nameplate.SetActive(true);

        if(!button.interactable)
            return;

        transform.DOScale(hoverScale, hoverTime).SetUpdate(true).SetLink(gameObject);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        nameplate.SetActive(false);
        transform.DOScale(1f, hoverTime).SetUpdate(true).SetLink(gameObject);
    }

    private void HandleClick()
    {
        Clicked?.Invoke(Info);
    }
}