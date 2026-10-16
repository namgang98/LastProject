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

    public ClassInfo Info { get; private set; }
    public event Action<ClassCardView> Clicked;

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
}