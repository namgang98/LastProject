using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SpeedButton : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI label;
    [SerializeField] Button button;
    [SerializeField] GameSpeedController speedCon;
    private void Awake()
    {
        button.onClick.AddListener(HandleClick);
    }
    private void Start()
    {
        Refresh();
    }
    private void HandleClick()
    {
        speedCon.ToggleSpeed();
        Refresh();
    }
    private void Refresh()
    {
        label.text = $"x{speedCon.Speed}";
    }
}
