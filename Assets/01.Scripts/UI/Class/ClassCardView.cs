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

    private static string BuildDescription(ClassInfo info)
    {
        string text = $"체력 {info.MaxHp}\n{ToKorean(info.Type)} 데미지 +{(info.DamageRate - 1f) * 100f:0}%";

        if(info.CooldownRate < 1f)
            text += $"\n{ToKorean(info.Type)} 쿨타임 -{(1f - info.CooldownRate) * 100f:0}%";

        return text;
    }

    private static string ToKorean(WeaponType type)
    {
        switch(type)
        {
            case WeaponType.Axe:
                return "도끼";
            case WeaponType.Bow:
                return "활";
            default:
                return "검";
        }
    }
}