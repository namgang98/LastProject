using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;
using UnityEngine.UI;

public class StageController : MonoBehaviour
{
    [Header("휴식 층")]
    [SerializeField, Range(0f, 1f)] float restHealRate = 0.5f;
    [SerializeField] GameObject monsterHpBack;
    [SerializeField] GameObject restVisual;

    [Header("층 이동 연출")]
    [SerializeField] FloorTransitionView transition;
    [SerializeField] PlayerWalk walker;


    [SerializeField] int bossInterval = 10;
    [SerializeField] Transform spawnPoint;
    [SerializeField] BattleController battle;
    [SerializeField] HpBarView monsterHpbar;
    [SerializeField] Button nextButton;
    [SerializeField] TextMeshProUGUI stageText;
    [SerializeField] bool returnOnlyAfterBoss;
    [SerializeField] Button returnButton;
    [SerializeField] StatusIconView statusIconView;
    [SerializeField] StageBackgroundView background;
    private StagePlanner planner;


    [SerializeField] private float hpGrowth = 0.1f;
    [SerializeField] private float damageGrowth = 0.05f;

    private StageRule rule;
    private StageScaling scaling;
    private UnitController player;

    private int stage;
    private Phase phase;
    private UnitController currentMonster;
    private MonsterInfo currentInfo;
    private AttackData currentAttack;

    private void Awake()
    {
        rule = new StageRule(bossInterval, returnOnlyAfterBoss);
        scaling = new StageScaling(hpGrowth, damageGrowth);

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
        if(player != null)
            player.Health.OnDie -= HandlePlayerDead;

        player = runner;
        player.Health.OnDie += HandlePlayerDead;

        planner = new StagePlanner(DataManager.instance.Stages, DataManager.instance.Monsters, bossInterval);

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
            background.Show(area.Background);

        bool isRest = rule.IsRestStage(stage);

        EventManager.instance.PostNotification(new OnStageStarted(stage, isRest));
        stageText.text = isRest ? $"{stage} F 휴식" : $"{stage} F";

        if(restVisual != null)
            restVisual.SetActive(isRest);

        monsterHpBack.SetActive(!isRest);
        monsterHpbar.gameObject.SetActive(!isRest);

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
        currentAttack = info.Attack.WithDamage(stats.Damage);

        monsterHpbar.Bind(currentMonster.Health);
        statusIconView.Bind(currentMonster);
    }

    private void BeginStage()
    {
        if(phase == Phase.Ended)
            return;

        if(rule.IsRestStage(stage))
        {
            StartRest();
            return;
        }

        if(currentMonster == null)
            return;

        currentMonster.Health.OnDie += HandleMonsterDead;
        battle.StartBattle(player, currentMonster, currentAttack, currentInfo.IsBoss);
        phase = Phase.Fighting;
    }

    private void StartRest()
    {
        player.Health.Heal(Mathf.RoundToInt(player.Health.MaxHp * restHealRate));
        phase = Phase.Looting;
        ShowButtons();
    }

    private void HandleMonsterDead()
    {
        currentMonster.Health.OnDie -= HandleMonsterDead;
        EventManager.instance.PostNotification(new OnMonsterKilled(spawnPoint.position, currentInfo));
        phase = Phase.Looting;
        ShowButtons();
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
        return area != null ? area.Name : null;
    }
    private enum Phase
    {
        Moving,
        Fighting,
        Looting,
        Ended,
    }


}