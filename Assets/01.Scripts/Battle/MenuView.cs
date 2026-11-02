using UnityEngine;
using UnityEngine.UI;

public class MenuView : MonoBehaviour
{
    [SerializeField] GameObject panel;
    [SerializeField] Button continueButton;
    [SerializeField] GameSpeedController speedCon;

    private void Awake()
    {
        if(continueButton != null)
            continueButton.onClick.AddListener(Close);
    }

    public void Open()
    {
        PopupManager.instance.Open(panel, Close);

        if(speedCon != null)
            speedCon.SetPaused(true);
    }

    public void Close()
    {
        PopupManager.instance.Close(panel);

        if(speedCon != null)
            speedCon.SetPaused(false);
    }
}