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
        SceneLoader.instance.Load(GetNextScene(GameSession.instance.ConsumeCutscene()));
    }
    private static string GetNextScene(CutsceneKind kind)
    {
        switch(kind)
        {
            case CutsceneKind.Collapse:
            case CutsceneKind.TowersRise:
            case CutsceneKind.OriginRise:
                return SceneNames.TowerMap;
            default:
                return SceneNames.Village;
        }
    }

    private void Show(RunEndReason reason, int stage)
    {
        titleText.text = GetTitle(reason);
        stageText.text = GetStageText(stage);

        PopupManager.instance.Open(panel, closeOnEscape: false);
    }

    private static string GetStageText(int stage)
    {
        GameSession session = GameSession.instance;

        if(!session.CurrentTower.IsEndless)
            return $"{stage}F";

        return $"{stage}F  (최고 {session.Towers.OriginBestFloor}F)";
    }

    private static string GetTitle(RunEndReason reason)
    {
        switch(reason)
        {
            case RunEndReason.Death:
                return "쓰러졌다...";
            case RunEndReason.Conquest:
                return $"{GameSession.instance.CurrentTower.Name}이 무너진다...";
            default:
                return "안전 귀환 성공!";
        }
    }
}