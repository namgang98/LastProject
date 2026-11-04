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
        EventManager.instance.AddListener<OnRunSettled>(HandleRunSettled);
    }

    private void OnDisable()
    {
        if(!EventManager.HasInstance)
            return;

        EventManager.instance.RemoveListener<OnRunSettled>(HandleRunSettled);
    }

    private void HandleRunSettled(OnRunSettled e)
    {
        Show(e.Reason, e.Stage, e.SavedCount);
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

    private void Show(RunEndReason reason, int stage, int savedCount)
    {
        titleText.text = GetTitle(reason);
        stageText.text = GetStageText(stage) + GetNote(reason, savedCount);

        if(SoundManager.HasInstance)
        {
            SoundManager.instance.StopBgm(0.3f);
            SoundManager.instance.PlayJingle(GetJingle(reason));
        }
        PopupManager.instance.Open(panel, closeOnEscape: false);
    }

    private static string GetNote(RunEndReason reason, int savedCount)
    {
        string note = string.Empty;

        if(reason == RunEndReason.Conquest)
            note += "\n<size=70%>무너지는 탑에 가방 속 물건이 묻혔다. 주머니와 에고 장비만 남았다</size>";

        if(savedCount > 0)
            note += $"\n<size=70%>망자의 가호로 장비 {savedCount}개를 지켜 냈다</size>";

        return note;
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

    private static SfxType GetJingle(RunEndReason reason)
    {
        switch(reason)
        {
            case RunEndReason.Death:
                return SfxType.JingleDeath;
            case RunEndReason.Conquest:
                return SfxType.JingleConquest;
            default:
                return SfxType.JingleReturn;
        }
    }
}