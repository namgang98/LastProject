using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OrbSlotView : MonoBehaviour
{
    [SerializeField] Image icon;
    [SerializeField] TMP_Text nameText;
    [SerializeField] TMP_Text countText;
    [SerializeField] GameObject selectedMark;
    [SerializeField] Button button;

    private OrbInfo orb;
    private Action<OrbInfo> onSelect;

    private void Awake()
    {
        button.onClick.AddListener(HandleClick);
    }

    public void Bind(OrbInfo target, int count, Action<OrbInfo> select)
    {
        orb = target;
        onSelect = select;
        icon.sprite = orb.Icon;
        nameText.text = orb.Name;
        countText.text = $"x{count}";
    }

    public void SetSelected(bool selected)
    {
        selectedMark.SetActive(selected);
    }

    private void HandleClick()
    {
        if(orb == null)
            return;

        onSelect?.Invoke(orb);
    }
}