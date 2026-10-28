using System.Collections.Generic;
using UnityEngine;

public class RunController : MonoBehaviour, IPlayerStatsSource
{
    [SerializeField] InventoryUI ui;
    [SerializeField] BattleController battle;
    [SerializeField] StageController stage;
    [SerializeField] Transform playerSpawnPoint;
    [SerializeField] StatusIconView playerStatusIcons;
    private UnitController player;
    [SerializeField] ToolTipView toolTip;
    [SerializeField] HpBarView playerHpBar;
    [SerializeField] BlessingRewardController blessingReward;
    [SerializeField] ItemDropper dropper;
    private RunBlessings blessings;
    private IStatModifier training;
    public Backpack CurrentBackpack => GameSession.instance.Backpack;
    public Backpack Backpack => GameSession.instance.Backpack;
    public RunBlessings Blessings => blessings;

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
    private void OnDestroy()
    {
        if(blessings == null)
            return;

        blessings.Changed -= HandleBlessingChanged;
    }
    private void Start()
    {
        GameSession session = GameSession.instance;

        if(!TrySpawnPlayer(session.CurrentClass))
            return;


        ui.Bind(session.Backpack);
        training = session.Training.GetBonus(session.CurrentClass);
        blessings = new RunBlessings();
        blessings.Changed += HandleBlessingChanged;
        blessingReward.Bind(blessings);
        battle.SetRunBonus(blessings);
        stage.SetRunBonus(blessings);
        dropper.SetRunBonus(blessings);

        var stats = PlayerStatsBuilder.Build(session.CurrentClass, session.Backpack, training, blessings);
        battle.Bind(session.Backpack, session.CurrentClass, training, blessings);
        toolTip.Bind(session.Backpack);
        playerHpBar.Bind(player.Health);
        playerStatusIcons.Bind(player);
        player.Init(stats.MaxHp);
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

        if(e.Stage <= 1 || blessings == null)
            return;

        int heal = Mathf.RoundToInt(blessings.Sum(BlessingStat.FloorHeal));

        if(heal > 0)
            player.Health.Heal(heal);
    }
    private void HandleBlessingChanged(BlessingInfo info)
    {
        GameSession session = GameSession.instance;
        var stats = BuildStats();
        int before = player.Health.MaxHp;

        player.Health.SetMaxHp(stats.MaxHp);

        int gained = stats.MaxHp - before;

        if(gained > 0)
            player.Health.Heal(gained);
    }
    public PlayerStats BuildStats()
    {
        GameSession session = GameSession.instance;
        return PlayerStatsBuilder.Build(session.CurrentClass, session.Backpack, training, blessings);
    }
}
