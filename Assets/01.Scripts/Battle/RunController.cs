using System.Collections.Generic;
using UnityEngine;

public class RunController : MonoBehaviour
{
    [SerializeField] InventoryUI ui;
    [SerializeField] BattleController battle;
    [SerializeField] StageController stage;
    [SerializeField] UnitController player;
    [SerializeField] ToolTipView toolTip;
    [SerializeField] HpBarView playerHpBar;
    public Backpack CurrentBackpack => GameSession.instance.Backpack;
    private void OnEnable()
    {
        EventManager.instance.AddListener<OnRunEnded>(HandleRunEnded);
        EventManager.instance.AddListener<OnMonsterKilled>(HandleMonsterKilled);
    }

    private void OnDisable()
    {
        if (!EventManager.HasInstance)
            return;

        EventManager.instance.RemoveListener<OnRunEnded>(HandleRunEnded);
        EventManager.instance.RemoveListener<OnMonsterKilled>(HandleMonsterKilled);
    }
    private void Start()
    {
        GameSession session = GameSession.instance;
        var stats = PlayerStatsBuilder.Build(session.CurrentClass, session.Backpack);

        ui.Bind(session.Backpack);
        battle.Bind(session.Backpack, session.CurrentClass);
        toolTip.Bind(session.Backpack);
        playerHpBar.Bind(player.Health);
        player.ResetUnit(stats.MaxHp);
        session.RefreshGold();
        stage.Begin();
    }
    
    private void HandleRunEnded(OnRunEnded e)
    {
        GameSession.instance.FinishRun(e.Reason);
    }
    private void HandleMonsterKilled(OnMonsterKilled e)
    {
        GameSession.instance.Wallet.AddRunGold(e.Monster.DropGold);
    }
}
