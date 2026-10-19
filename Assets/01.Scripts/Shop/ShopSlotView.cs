using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopSlotView : MonoBehaviour
{
    [SerializeField] Button button;
    [SerializeField] Image icon;
    [SerializeField] TMP_Text priceText;
    [SerializeField] GameObject lockedMark;
    [SerializeField] Color affordableColor = new(0.95f, 0.85f, 0.5f);
    [SerializeField] Color poorColor = new(0.8f, 0.3f, 0.3f);

    private int index;

    public event Action<int> Clicked;

    private void Awake()
    {
        button.onClick.AddListener(HandleClick);
    }

    public void SetIndex(int value)
    {
        index = value;
    }

    public void ShowItem(ItemInfo info, int price, bool affordable)
    {
        icon.enabled = true;
        icon.sprite = info.Icon;
        priceText.text = $"{price}G";
        priceText.color = affordable ? affordableColor : poorColor;
        button.interactable = true;
        SetLocked(false);
    }

    public void ShowEmpty()
    {
        icon.enabled = false;
        priceText.text = string.Empty;
        button.interactable = false;
        SetLocked(false);
    }

    public void ShowLocked()
    {
        icon.enabled = false;
        priceText.text = string.Empty;
        button.interactable = false;
        SetLocked(true);
    }

    private void SetLocked(bool locked)
    {
        if(lockedMark != null)
            lockedMark.SetActive(locked);
    }

    private void HandleClick()
    {
        Clicked?.Invoke(index);
    }
}
