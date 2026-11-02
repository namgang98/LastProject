using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ForgeSlotView : MonoBehaviour
{
    [SerializeField] Image icon;
    [SerializeField] TMP_Text nameText;
    [SerializeField] TMP_Text levelText;
    [SerializeField] GameObject selectedMark;
    [SerializeField] Button button;

    private ForgeableInstance item;
    private Action<ForgeableInstance> onSelect;

    private void Awake()
    {
        button.onClick.AddListener(HandleClick);
    }

    public void Bind(ForgeableInstance target, Action<ForgeableInstance> select)
    {
        item = target;
        onSelect = select;
        icon.sprite = item.Info.Icon;
        nameText.text = item.Info.Name;
        levelText.text = item.Level > 0 ? $"+{item.Level}" : string.Empty;
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