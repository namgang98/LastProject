using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MenuView : MonoBehaviour
{
    [SerializeField] GameObject panel;
    [SerializeField] Button menuButton;
    [SerializeField] Button continueButton;
    [SerializeField] GameSpeedController speedCon;


    private void Awake()
    {
        menuButton.onClick.AddListener(Open);
        continueButton.onClick.AddListener(Close);
    }

    private void Open()
    {
        PopupManager.instance.Open(panel, Close);
        speedCon.SetPaused(true);
    }
    private void Close()
    {
        PopupManager.instance.Close(panel);
        if(speedCon != null)
        {
            speedCon.SetPaused(false);
        }
    }
}
