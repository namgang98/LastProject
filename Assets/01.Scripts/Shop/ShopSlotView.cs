using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopSlotView : MonoBehaviour
{
    [Header("아이콘 크기")]
    [SerializeField] Vector2 areaRatio = new(0.75f, 0.75f);
    [SerializeField, Min(1)] int fullCells = 2;

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
        FitIcon(info);
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
    private void FitIcon(ItemInfo info)
    {
        Vector2 area = Vector2.Scale(((RectTransform)transform).rect.size, areaRatio);
        float cellSize = Mathf.Min(area.x, area.y) / fullCells;
        Vector2 size = GetBounds(info.Shape) * cellSize;
        float scale = Mathf.Min(1f, area.x / size.x, area.y / size.y);

        icon.preserveAspect = true;
        icon.rectTransform.sizeDelta = size * scale;
    }

    private static Vector2 GetBounds(ItemShape shape)
    {
        int maxX = 0;
        int maxY = 0;

        foreach(Vector2Int cell in shape.Cells)
        {
            maxX = Mathf.Max(maxX, cell.x);
            maxY = Mathf.Max(maxY, cell.y);
        }

        return new Vector2(maxX + 1, maxY + 1);
    }
}
