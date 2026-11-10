using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CreditsView : MonoBehaviour
{
    [SerializeField] GameObject panel;
    [SerializeField] Button openButton;
    [SerializeField] Button closeButton;
    [SerializeField] TMP_Text contentText;
    [SerializeField] ScrollRect scroll;
    [SerializeField] TextAsset credits;

    private void Awake()
    {
        openButton.onClick.AddListener(Open);
        closeButton.onClick.AddListener(Close);
        panel.SetActive(false);
    }

    public void Open()
    {
        if(credits != null)
            contentText.text = credits.text;

        panel.SetActive(true);
        scroll.verticalNormalizedPosition = 1f;
    }

    public void Close()
    {
        panel.SetActive(false);
    }
}