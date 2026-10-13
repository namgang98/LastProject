using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ResultView : MonoBehaviour
{
    [SerializeField] GameObject panel;
    [SerializeField] TextMeshProUGUI titleText;
    [SerializeField] TextMeshProUGUI stageText;
    [SerializeField] Button restartButton;

    private void Awake()
    {
        restartButton.onClick.AddListener(ReturnToVillage);
    }

    private void OnEnable()
    {
        EventManager.instance.AddListener<OnRunEnded>(HandleRunEnded);
    }
    private void OnDisable()
    {
        if(!EventManager.HasInstance)
            return;

        EventManager.instance.RemoveListener<OnRunEnded>(HandleRunEnded);
    }
    private void HandleRunEnded(OnRunEnded e)
    {
        Show(e.Reason, e.Stage);
    }
    private void ReturnToVillage()
    {
        restartButton.interactable = false;
        SceneManager.LoadScene(SceneNames.Village);
    }
    private void Show(RunEndReason reason, int stage)
    {
        titleText.text = reason == RunEndReason.Death ? "쓰러졌다..." : "안전 귀환 성공!";
        stageText.text = $"{stage}F";

        PopupManager.instance.Open(panel);
    }

}
