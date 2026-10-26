using TMPro;
using UnityEngine;
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

        string next = GameSession.instance.ConsumeTowerRise() ? SceneNames.TowerRise : SceneNames.Village;
        SceneLoader.instance.Load(next);
    }

    private void Show(RunEndReason reason, int stage)
    {
        titleText.text = GetTitle(reason);
        stageText.text = $"{stage}F";

        PopupManager.instance.Open(panel, closeOnEscape: false);
    }

    private static string GetTitle(RunEndReason reason)
    {
        switch(reason)
        {
            case RunEndReason.Death:
                return "쓰러졌다...";
            case RunEndReason.Victory:
                return "고대 용을 쓰러뜨렸다!";
            case RunEndReason.AbyssVictory:
                return "심연의 끝에 닿았다!";
            default:
                return "안전 귀환 성공!";
        }
    }
}