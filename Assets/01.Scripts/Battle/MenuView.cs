using UnityEngine;
using UnityEngine.UI;

public class MenuView : MonoBehaviour
{
    [SerializeField] GameObject panel;
    [SerializeField] Button continueButton;
    [SerializeField] GameSpeedController speedCon;
    [SerializeField] Button titleButton;

    private void Awake()
    {
        if(continueButton != null)
            continueButton.onClick.AddListener(Close);

        if(titleButton != null)
            titleButton.onClick.AddListener(GoToTitle);
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
    private void GoToTitle()
    {
        titleButton.interactable = false;
        Close();
        GameSession.instance.SaveInVillage();
        SceneLoader.instance.Load(SceneNames.Title);
    }
}