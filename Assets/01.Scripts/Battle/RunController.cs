using System.Collections.Generic;
using UnityEngine;

public class RunController : MonoBehaviour
{
    [SerializeField] InventoryUI ui;
    [SerializeField] BattleController battle;
    [SerializeField] StageController stage;
    [SerializeField] Transform playerSpawnPoint;
    [SerializeField] StatusIconView playerStatusIcons;
    private UnitController player;
    [SerializeField] ToolTipView toolTip;
    [SerializeField] HpBarView playerHpBar;
    public Backpack CurrentBackpack => GameSession.instance.Backpack;
    private void OnEnable()
    {
        EventManager.instance.AddListener<OnRunEnded>(HandleRunEnded);
        EventManager.instance.AddListener<OnStageStarted>(HandleStageStarted);
    }

    private void OnDisable()
    {
        if(!EventManager.HasInstance)
            return;

        EventManager.instance.RemoveListener<OnRunEnded>(HandleRunEnded);
        EventManager.instance.RemoveListener<OnStageStarted>(HandleStageStarted);
    }
    private void Start()
    {
        GameSession session = GameSession.instance;

        if(!TrySpawnPlayer(session.CurrentClass))
            return;


        ui.Bind(session.Backpack);
        IStatModifier training = session.Training.GetBonus(session.CurrentClass);
        var stats = PlayerStatsBuilder.Build(session.CurrentClass, session.Backpack, training);
        battle.Bind(session.Backpack, session.CurrentClass, training);
        toolTip.Bind(session.Backpack);
        playerHpBar.Bind(player.Health);
        playerStatusIcons.Bind(player);
        player.ResetUnit(stats.MaxHp);
        session.RefreshGold();
        stage.Begin(player);
    }
    private bool TrySpawnPlayer(ClassInfo info)
    {
        if(info == null || info.Prefab == null)
        {
            Debug.LogError($"[RunController] 직업 {info?.Name} 의 프리팹이 없습니다.", this);
            return false;
        }

        GameObject obj = Instantiate(info.Prefab, playerSpawnPoint);

        if(obj.TryGetComponent(out player))
            return true;

        Debug.LogError($"[RunController] {info.Prefab.name} 에 UnitController가 없습니다.", this);
        Destroy(obj);
        return false;
    }
    private void HandleRunEnded(OnRunEnded e)
    {
        GameSession.instance.FinishRun(e.Reason);
    }
    private void HandleStageStarted(OnStageStarted e)
    {
        GameSession.instance.Training.ReportFloor(e.Stage);
    }
}
