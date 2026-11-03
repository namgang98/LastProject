using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SocketSlotView : MonoBehaviour
{
    [SerializeField] Image icon;
    [SerializeField] TMP_Text nameText;
    [SerializeField] TMP_Text levelText;
    [SerializeField] GameObject wardMark;
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
        nameText.text = item.Info.Name;
        levelText.text = item is ForgeableInstance forged && forged.Level > 0 ? $"+{forged.Level}" : string.Empty;
        wardMark.SetActive(item.Ward != null);
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