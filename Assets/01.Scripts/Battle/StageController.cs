using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StageController : MonoBehaviour
{
    [Header("휴식 층")]
    [SerializeField, Range(0f, 1f)] float restHealRate = 0.5f;
    [SerializeField] GameObject restVisual;

    [Header("층 이동 연출")]
    [SerializeField] FloorTransitionView transition;
    [SerializeField] PlayerWalk walker;

    [Header("귀환 규칙")]
    [SerializeField] int veteranFloor = 10;
    [SerializeField] bool returnOnlyAfterBoss;

    [Header("HUD")]
    [SerializeField] HudFader playerHud;
    [SerializeField] HudFader monsterHud;

    [Header("층 보상")]
    [SerializeField] BlessingRewardController blessingReward;

    [SerializeField] int bossInterval = 10;
    [SerializeField] Transform spawnPoint;
    [SerializeField] BattleController battle;
    [SerializeField] HpBarView monsterHpbar;
    [SerializeField] Button nextButton;
    [SerializeField] TextMeshProUGUI stageText;
    [SerializeField] Button returnButton;
    [SerializeField] StatusIconView statusIconView;
    [SerializeField] StageBackgroundView background;
    [SerializeField] StageAmbienceView ambience;
    private IRunBonus runBonus = EmptyRunBonus.Instance;
    private StagePlanner planner;

    [SerializeField] private float hpGrowth = 0.05f;
    [SerializeField] float hpCurve = 0.004f;
    [SerializeField] private float damageGrowth = 0.04f;
    [SerializeField] float damageCurve = 0.0025f;

    private StageRule rule;
    private StageScaling scaling;
    private UnitController player;
    private TowerInfo tower;

    private int stage;
    private int finalFloor;
    private Phase phase;
    private UnitController currentMonster;
    private MonsterInfo currentInfo;
    private MonsterAttackSet currentAttacks;
    private TowerRuleSet towerRules = new();
    private void Awake()
    {
        scaling = new StageScaling(hpGrowth, hpCurve, damageGrowth, damageCurve);

        nextButton.onClick.AddListener(NextStage);
        returnButton.onClick.AddListener(ReturnToTown);
    }

    private void OnDisable()
    {
        if(player != null)
            player.Health.OnDie -= HandlePlayerDead;
    }

    public void Begin(UnitController runner)
    {
        bool isVeteran = GameSession.instance.BestFloor >= veteranFloor;
        rule = new StageRule(bossInterval, isVeteran);

        tower = GameSession.instance.CurrentTower;
        finalFloor = GameSession.instance.Towers.GetFinalFloor(tower);

        playerHud.SetInstant(false);
        monsterHud.SetInstant(false);

        if(player != null)
            player.Health.OnDie -= HandlePlayerDead;

        player = runner;
        player.Health.OnDie += HandlePlayerDead;

        planner = new StagePlanner(DataManager.instance.Stages, DataManager.instance.Monsters, bossInterval, tower);
        stage = 1;
        phase = Phase.Moving;
        HideButtons();
        walker.Bind(player);
        transition.Play(stage, GetKind(stage), GetAreaTitle(stage), PrepareStage, WalkIn, null, true);
    }

    public void NextStage()
    {
        if(phase != Phase.Looting)
            return;

        phase = Phase.Moving;
        HideButtons();
        playerHud.Hide();
        walker.WalkOut();
        transition.Play(stage + 1, GetKind(stage + 1), GetAreaTitle(stage + 1), EnterNextFloor, WalkIn);
    }

    public void ReturnToTown()
    {
        if(phase != Phase.Looting)
            return;

        EndRun(RunEndReason.Return);
    }

    public void EndRun(RunEndReason reason)
    {
        if(phase == Phase.Ended)
            return;

        phase = Phase.Ended;
        battle.EndBattle();
        ClearMonster();
        HideButtons();
        EventManager.instance.PostNotification(new OnRunEnded(stage, reason));
    }

    private void EnterNextFloor()
    {
        ClearMonster();
        stage++;
        PrepareStage();
    }

    private void PrepareStage()
    {
        StageInfo area = planner.Get(stage);

        if(area != null)
        {
            background.Show(area.Background);
            ambience.Show(area.AmbiencePrefab);
            ApplyAreaRules(area);
        }

        bool isRest = rule.IsRestStage(stage);

        EventManager.instance.PostNotification(new OnStageStarted(stage, isRest));
        stageText.text = isRest ? $"{stage} F 휴식" : $"{stage} F";

        if(restVisual != null)
            restVisual.SetActive(isRest);

        if(isRest)
            return;

        SpawnMonster();
    }

    private void SpawnMonster()
    {
        MonsterInfo info = planner.PickMonster(stage, rule.IsBossStage(stage));

        if(info == null)
        {
            Debug.LogError($"[StageController] {stage}층에 나올 몬스터가 없습니다. Stages 데이터를 확인하세요.", this);
            return;
        }

        GameObject obj = Instantiate(info.Prefab, spawnPoint);

        if(!obj.TryGetComponent(out UnitController monster))
        {
            Debug.LogError($"[StageController] {info.Name} 프리팹에 UnitController 가 없습니다.", this);
            Destroy(obj);
            return;
        }

        currentInfo = info;
        currentMonster = monster;

        MonsterStats stats = scaling.Apply(info, stage);
        currentMonster.Health.Init(stats.MaxHp);

        if(currentMonster.Health is IRevivable revivable)
            revivable.SetRevive(Mathf.Max(info.ReviveRate, towerRules.MonsterRevive));

        currentAttacks = MonsterAttackSet.Create(info, stats.Damage);

        monsterHpbar.Bind(currentMonster.Health);
        monsterHpbar.gameObject.SetActive(info.HpDisplay != HpDisplay.Hidden);
        statusIconView.Bind(currentMonster);
    }

    private void BeginStage()
    {
        if(phase == Phase.Ended)
            return;

        playerHud.Show();

        if(rule.IsRestStage(stage))
        {
            StartRest();
            return;
        }

        if(currentMonster == null)
            return;

        monsterHud.Show();
        currentMonster.Health.OnDie += HandleMonsterDead;
        battle.StartBattle(player, currentMonster, currentAttacks, currentInfo.IsBoss);
        phase = Phase.Fighting;
    }

    private void StartRest()
    {
        Sfx.Play(SfxType.RestEnter);
        float rate = restHealRate * (1f + runBonus.Sum(BlessingStat.RestHeal));
        player.Health.Heal(Mathf.RoundToInt(player.Health.MaxHp * rate));
        phase = Phase.Looting;
        ShowButtons();
    }

    private void HandleMonsterDead()
    {
        monsterHud.Hide();
        currentMonster.Health.OnDie -= HandleMonsterDead;
        EventManager.instance.PostNotification(new OnMonsterKilled(spawnPoint.position, currentInfo));

        if(stage >= finalFloor && currentInfo.IsBoss)
        {
            EndRun(RunEndReason.Conquest);
            return;
        }

        phase = Phase.Looting;
        
        if(blessingReward != null && blessingReward.TryOffer(stage, ShowButtons))
            return;
        
        ShowButtons();
    }
    public void SetTowerRules(TowerRuleSet rules)
    {
        towerRules = rules ?? new TowerRuleSet();
    }
    private void HandlePlayerDead()
    {
        EndRun(RunEndReason.Death);
    }

    private void ClearMonster()
    {
        if(currentMonster == null)
            return;

        currentMonster.Health.OnDie -= HandleMonsterDead;
        Destroy(currentMonster.gameObject);
        currentMonster = null;
    }

    private FloorKind GetKind(int floor)
    {
        if(rule.IsRestStage(floor))
            return FloorKind.Rest;

        if(rule.IsBossStage(floor))
            return FloorKind.Boss;

        return FloorKind.Normal;
    }

    private void ShowButtons()
    {
        nextButton.gameObject.SetActive(true);
        returnButton.gameObject.SetActive(rule.CanReturn(stage));
    }

    private void HideButtons()
    {
        nextButton.gameObject.SetActive(false);
        returnButton.gameObject.SetActive(false);
    }

    private void WalkIn()
    {
        walker.WalkIn(BeginStage);
    }

    private string GetAreaTitle(int floor)
    {
        if(!planner.IsFirstFloor(floor))
            return null;

        StageInfo area = planner.Get(floor);

        if(area == null)
            return null;

        if(!tower.IsEndless || !DataManager.instance.TryGetTower(area.TowerId, out TowerInfo source))
            return area.Name;

        return $"{area.Name} · {source.Name}";
    }
    public void SetRunBonus(IRunBonus bonus)
    {
        runBonus = bonus ?? EmptyRunBonus.Instance;
    }
    private void ApplyAreaRules(StageInfo area)
    {
        DataManager.instance.TryGetTower(area.TowerId, out TowerInfo source);
        towerRules.SetSource(source);
    }
    private enum Phase
    {
        Moving,
        Fighting,
        Looting,
        Ended,
    }
}