using UnityEngine;

public class RunController : MonoBehaviour
{
    [SerializeField] int classId = 3000;
    [SerializeField] StarterKit kit;
    [SerializeField] InventoryUI ui;
    [SerializeField] BattleController battle;
    [SerializeField] StageController stage;
    [SerializeField] UnitController player;
    [SerializeField] ToolTipView toolTip;
    private ClassInfo currentClass;
    private Backpack backpack;

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

    public void StartFirstRun()
    {
        if(!DataManager.instance.TryGetClass(classId, out currentClass))
        {
            Debug.LogError($"[RunController] 직업 ID {classId}를 찾을 수 없습니다.", this);
            return;
        }

        if(!kit.TryCreate(out backpack))
            return;

        StartRun();
    }

    public void StartRun()
    {
        var stats = PlayerStatsBuilder.Build(currentClass, backpack);
        ui.Bind(backpack);
        battle.Bind(backpack, currentClass);
        toolTip.Bind(backpack);
        player.ResetUnit(stats.MaxHp);
        stage.Begin();
    }

   private void HandleRunEnded(OnRunEnded e)
    {
        if(e.Reason == RunEndReason.Death)
            LoseBackpack();

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
}
