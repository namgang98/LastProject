using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class VolumeSliderView : MonoBehaviour
{
    [SerializeField] private SoundChannel channel;
    [SerializeField] private Slider slider;
    [SerializeField] private TMP_Text valueText;

    private VolumeSettings settings;

    private void OnEnable()
    {
        settings = SoundManager.instance.Volume;

        if(settings == null)
            return;

        float value = settings.Get(channel);
        slider.SetValueWithoutNotify(value);
        ShowValue(value);
        slider.onValueChanged.AddListener(HandleChanged);
    }

    private void OnDisable()
    {
        slider.onValueChanged.RemoveListener(HandleChanged);
    }

    private void HandleChanged(float value)
    {
        settings.Set(channel, value);
        ShowValue(value);
    }

    private void ShowValue(float value)
    {
        valueText.text = $"{Mathf.RoundToInt(value * 100f)}";
    }
}