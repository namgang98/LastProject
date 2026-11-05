using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SocketSlotView : MonoBehaviour
{
    [SerializeField] Image icon;
    [SerializeField] RectTransform iconArea;
    [SerializeField, Min(1)] int fullCells = 2;
    [SerializeField] TMP_Text nameText;
    [SerializeField] TMP_Text levelText;
    [SerializeField] ItemBadgeView badges;
    [SerializeField] GameObject selectedMark;
    [SerializeField] Button button;

    private ItemInstance item;
    private Action<ItemInstance> onSelect;

    private void Awake()
    {
        button.onClick.AddListener(HandleClick);
    }

    public void Bind(ItemInstance target, Action<ItemInstance> select)
    {
        item = target;
        onSelect = select;
        icon.sprite = item.Info.Icon;
        ItemIconFitter.Fit(icon, item.Info, iconArea.rect.size, fullCells);
        nameText.text = item.Info.Name;
        levelText.text = item is ForgeableInstance forged && forged.Level > 0 ? $"+{forged.Level}" : string.Empty;
        badges.Show(item);
    }

    public void SetSelected(bool selected)
    {
        selectedMark.SetActive(selected);
    }

    private void HandleClick()
    {
        if(item == null)
            return;

        onSelect?.Invoke(item);
    }
}