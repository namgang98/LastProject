using System.Collections.Generic;
using UnityEngine;

public class RunController : MonoBehaviour
{
    private const string SaveFileName = "save.json";

    [SerializeField] int defaultclassId = 3000;
    [SerializeField] StarterKit kit;
    [SerializeField] InventoryUI ui;
    [SerializeField] BattleController battle;
    [SerializeField] StageController stage;
    [SerializeField] UnitController player;
    [SerializeField] ToolTipView toolTip;
    private ClassInfo currentClass;
    private Backpack backpack;
    private SaveService saveService;
    private SaveData saveData;
    private Wallet wallet;
    public Backpack CurrentBackpack => backpack;
    private void Awake()
    {
        saveService = new SaveService(new FileSaveStorage(SaveFileName));
    }
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

    public void StartFirstRun()
    {
        if (!saveService.TryLoad(out saveData))
            saveData = new SaveData { classId = defaultclassId };
        
        wallet = new Wallet(saveData.gold);
        wallet.Changed += PostGold;
        PostGold(wallet);

        if (!DataManager.instance.TryGetClass(saveData.classId, out currentClass))
        {
            Debug.LogError($"[RunController] 직업 ID {saveData.classId}를 찾을 수 없습니다.", this);
            return;
        }
        if (!TryLoadBackpack())
            return;

        if (saveData.inRun)
        {
            Debug.Log("[RunController] 런 도중 종료 기록 → 사망 처리", this);
            LoseBackpack();
        }

        StartRun();
    }

    public void StartRun()
    {
        var stats = PlayerStatsBuilder.Build(currentClass, backpack);
        ui.Bind(backpack);
        battle.Bind(backpack, currentClass);
        toolTip.Bind(backpack);
        player.ResetUnit(stats.MaxHp);
        Save(true);
        stage.Begin();
    }

    private void HandleRunEnded(OnRunEnded e)
    {
        if(e.Reason == RunEndReason.Death)
        {
            LoseBackpack();
            wallet.LoseRun();
        }

        wallet.CommitRun();
        Save(false);
    }
    private void HandleMonsterKilled(OnMonsterKilled e)
    {
        wallet.AddRunGold(e.Monster.DropGold);
    }

    private bool TryLoadBackpack()
    {
        var errors = new List<string>();

        if (saveData.backpack.bagId != 0 && InventorySaveMapper.TryRestore(saveData.backpack, errors, out backpack))
        {
            foreach (var error in errors)
            {
                Debug.LogWarning($"[RunController] {error}", this);
            }

            return true;
        }

        return kit.TryCreate(out backpack);
    }
    private void LoseBackpack()
    {

       PouchInstance pouch = backpack.LoseAllOnDeath();

       if(!kit.TryCreate(out Backpack newPack))
           return;

       if(pouch != null && !newPack.Grid.TryAutoPlace(pouch))
       {
               Debug.LogWarning("[RunController] 새 가방에 안전 주머니를 넣지 못했습니다.", this);   
       }

       backpack = newPack;
   
    }

    private void Save(bool inRun)
    {
        saveData.classId = currentClass.Id;
        saveData.inRun = inRun;
        saveData.backpack = InventorySaveMapper.ToData(backpack);
        saveData.gold = wallet.Gold;
        saveService.Save(saveData);
    }

    public void DeleteSave()
    {
        saveService.Delete();
        Debug.Log($"[RunController] 세이브 삭제 : {Application.persistentDataPath}", this);
    }
    private void PostGold(Wallet w)
    {
        EventManager.instance.PostNotification(new OnChangeGold(w.Gold, w.RunGold));
    }
}
