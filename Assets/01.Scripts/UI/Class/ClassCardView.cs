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
    [SerializeField] GameObject fallenMark;
    [SerializeField] TextMeshProUGUI statusText;
    [SerializeField] Color lockedTint = new(0.25f, 0.25f, 0.25f, 1f);
    [SerializeField] Color fallenTint = new(0.55f, 0.55f, 0.6f, 1f);


    public ClassInfo Info { get; private set; }
    public ClassAvailability State { get; private set; }
    public bool IsLocked => State != ClassAvailability.Available;
    
    public event Action<ClassCardView> Clicked;

    private void Awake()
    {
        button.onClick.AddListener(() => Clicked?.Invoke(this));
    }

    public void Bind(ClassInfo info)
    {
        Info = info;
        nameText.text = info.Name;
        descText.text = BuildDescription(info);
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
        if(info.Icon != null)
            return info.Icon;

        if(info.Prefab == null)
            return null;

        SpriteRenderer renderer = info.Prefab.GetComponentInChildren<SpriteRenderer>();
        return renderer != null ? renderer.sprite : null;
    }

    public void SetState(ClassAvailability state, string status)
    {
        State = state;
        statusText.text = status;
        lockMark.SetActive(state == ClassAvailability.Locked);

        if(fallenMark != null)
            fallenMark.SetActive(state == ClassAvailability.Fallen);

        portrait.color = GetTint(state);
    }

    private Color GetTint(ClassAvailability state)
    {
        switch(state)
        {
            case ClassAvailability.Locked:
                return lockedTint;
            case ClassAvailability.Fallen:
                return fallenTint;
            default:
                return Color.white;
        }
    }

    private static string BuildDescription(ClassInfo info)
    {
        string type = WeaponTypeText.ToKorean(info.Type);
        string text = $"체력 {info.MaxHp}\n{type} 데미지 +{(info.DamageRate - 1f) * 100f:0}%";

        if(info.CooldownRate < 1f)
            text += $"\n{type} 쿨타임 -{(1f - info.CooldownRate) * 100f:0}%";

        if(info.CritChance > 0f)
            text += $"\n치명타 확률 +{info.CritChance * 100f:0}%";

        if(info.ElementChance > 0f)
            text += $"\n속성 확률 +{info.ElementChance * 100f:0}%";

        return text;
    }
}