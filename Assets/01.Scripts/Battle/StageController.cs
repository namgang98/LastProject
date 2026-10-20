using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StageController : MonoBehaviour
{
    [Header("휴식 층")]
    [SerializeField, Range(0f, 1f)] float restHealRate = 0.5f;
    [SerializeField] GameObject monsterHpBack;
    [SerializeField] GameObject restVisual;

    [SerializeField] int bossInterval = 10;
    [SerializeField] Transform spawnPoint;
    [SerializeField] BattleController battle;
    private UnitController player;
    [SerializeField] HpBarView monsterHpbar;
    [SerializeField] Button nextButton;
    [SerializeField] TextMeshProUGUI stageText;
    [SerializeField] bool returnOnlyAfterBoss;
    [SerializeField] Button returnButton;
    [SerializeField] StatusIconView statusIconView;
    private StageRule rule;

    private int stage;
    private Phase phase;
    private UnitController currentMonster;
    private MonsterInfo currentInfo;

    [SerializeField] private float hpGrowth = 0.1f;
    [SerializeField] private float damageGrowth = 0.05f;
    private StageScaling scaling;

    private List<MonsterInfo> normalMonsters = new();
    private List<MonsterInfo> bossMonsters = new();

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

        normalMonsters.Clear();
        bossMonsters.Clear();

        foreach(MonsterInfo monster in DataManager.instance.Monsters)
        {
            if(monster.IsBoss)
            {
                bossMonsters.Add(monster);
            }
            else 
            {
                normalMonsters.Add(monster);    
            }
        }

        stage = 1;
        StartStage();
    }
    private void StartStage()
    {
        bool isRest = rule.IsRestStage(stage);
        EventManager.instance.PostNotification(new OnStageStarted(stage, isRest));
        stageText.text = isRest ? $"{stage} F 휴식" : $"{stage} F";

        if(restVisual != null)
            restVisual.SetActive(isRest);

        if(isRest)
        {
            StartRest();
            return;
        }
        monsterHpBack.SetActive(true);
        monsterHpbar.gameObject.SetActive(true);
        List<MonsterInfo> pool = rule.IsBossStage(stage) ? bossMonsters : normalMonsters;

        if (pool.Count == 0)
        {
            Debug.LogError($"[StageController] {stage}층에 나올 몬스터가 없습니다.", this);
            return;
        }

        MonsterInfo info = pool[Random.Range(0, pool.Count)];
        currentInfo = info;

        GameObject obj = Instantiate(info.Prefab, spawnPoint);
        if(!obj.TryGetComponent(out UnitController monster))
        {
            Debug.LogError($"[StageController] {info.Name} 프리팹에 UnitController 가 없습니다.", this);
            Destroy(obj);
            return;
        }
        currentMonster = monster;
        MonsterStats stats = scaling.Apply(info, stage);

        currentMonster.Health.Init(stats.MaxHp);
        monsterHpbar.Bind(currentMonster.Health);
        currentMonster.Health.OnDie += HandleMonsterDead;
        var monsterAttack = info.Attack.WithDamage(stats.Damage);
        battle.StartBattle(player, monster, monsterAttack, info.IsBoss);
        statusIconView.Bind(monster);
        phase = Phase.Fighting;
        nextButton.gameObject.SetActive(false);
        returnButton.gameObject.SetActive(false);
    }

    private void HandleMonsterDead()
    {
        currentMonster.Health.OnDie -= HandleMonsterDead;
        EventManager.instance.PostNotification(new OnMonsterKilled(spawnPoint.position, currentInfo));
        phase = Phase.Looting;
        nextButton.gameObject.SetActive(true);
        returnButton.gameObject.SetActive(rule.CanReturn(stage));
    }
    private void HandlePlayerDead()
    {
        EndRun(RunEndReason.Death);
    }

    public void NextStage()
    {
        if(phase != Phase.Looting)
            return;
        if(currentMonster != null)
            Destroy(currentMonster.gameObject);

        currentMonster = null;
        stage++;
        StartStage();
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

        if(currentMonster != null)
        {
            currentMonster.Health.OnDie -= HandleMonsterDead;
            Destroy(currentMonster.gameObject);
            currentMonster = null;
        }

        nextButton.gameObject.SetActive(false);
        returnButton.gameObject.SetActive(false);
        EventManager.instance.PostNotification(new OnRunEnded(stage, reason));
    }
    private void StartRest()
    {
        monsterHpBack.SetActive(false);
        monsterHpbar.gameObject.SetActive(false);
        player.Health.Heal(Mathf.RoundToInt(player.Health.MaxHp * restHealRate));

        phase = Phase.Looting;
        nextButton.gameObject.SetActive(true);
        returnButton.gameObject.SetActive(rule.CanReturn(stage));
    }

    private enum Phase
    {
        Fighting,
        Looting,
        Ended,
    }
}
