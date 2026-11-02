using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DisplayOptionView : MonoBehaviour
{
    [SerializeField] TMP_Dropdown modeDropdown;
    [SerializeField] TMP_Dropdown resolutionDropdown;

    private static readonly string[] ModeNames = { "전체 화면", "테두리 없는 창", "창 모드" };

    private DisplaySettings settings;

    private void Awake()
    {
        settings = new DisplaySettings();

        modeDropdown.ClearOptions();
        modeDropdown.AddOptions(new List<string>(ModeNames));

        var labels = new List<string>();

        foreach(Vector2Int size in settings.Resolutions)
        {
            labels.Add($"{size.x} x {size.y}");
        }

        resolutionDropdown.ClearOptions();
        resolutionDropdown.AddOptions(labels);
    }

    private void OnEnable()
    {
        modeDropdown.SetValueWithoutNotify((int)settings.CurrentMode);
        resolutionDropdown.SetValueWithoutNotify(settings.CurrentResolutionIndex());
        RefreshInteractable();

        modeDropdown.onValueChanged.AddListener(HandleChanged);
        resolutionDropdown.onValueChanged.AddListener(HandleChanged);
    }

    private void OnDisable()
    {
        modeDropdown.onValueChanged.RemoveListener(HandleChanged);
        resolutionDropdown.onValueChanged.RemoveListener(HandleChanged);
    }

    private void HandleChanged(int _)
    {
        settings.Apply((DisplayMode)modeDropdown.value, resolutionDropdown.value);
        RefreshInteractable();
    }

    private void RefreshInteractable()
    {
        resolutionDropdown.interactable = (DisplayMode)modeDropdown.value != DisplayMode.Borderless;
    }
}