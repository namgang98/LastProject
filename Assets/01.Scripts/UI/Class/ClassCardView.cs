using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ClassCardView : MonoBehaviour
{
    [SerializeField] Button button;
    [SerializeField] Image portrait;
    [SerializeField] TextMeshProUGUI nameText;
    [SerializeField] TextMeshProUGUI descText;
    [SerializeField] GameObject selectedMark;

    [Header("잠금")]
    [SerializeField] GameObject lockMark;
    [SerializeField] TextMeshProUGUI statusText;
    [SerializeField] Color lockedTint = new(0.25f, 0.25f, 0.25f, 1f);

    public ClassInfo Info { get; private set; }
    public event Action<ClassCardView> Clicked;

    public bool IsLocked { get; private set; }

    private void Awake()
    {
        button.onClick.AddListener(() => Clicked?.Invoke(this));
    }

    public void Bind(ClassInfo info)
    {
        Info = info;
        nameText.text = info.Name;
        descText.text = $"체력 {info.MaxHp}\n{info.Description}";
        portrait.sprite = FindPortrait(info);
        portrait.preserveAspect = true;
        SetSelected(false);
    }

    public void SetSelected(bool selected)
    {
        selectedMark.SetActive(selected);
    }

    private static Sprite FindPortrait(ClassInfo info)
    {
        if(info.Prefab == null)
            return null;

        SpriteRenderer renderer = info.Prefab.GetComponentInChildren<SpriteRenderer>();
        return renderer != null ? renderer.sprite : null;
    }

    public void SetStatus(string status, bool locked)
    {
        IsLocked = locked;
        statusText.text = status;
        lockMark.SetActive(locked);
        portrait.color = locked ? lockedTint : Color.white;
    }
}