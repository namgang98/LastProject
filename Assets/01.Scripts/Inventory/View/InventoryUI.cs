using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    [SerializeField] DungeonBackpackPanel backpackPanel;

    [Header("영역")]
    [SerializeField] private RectTransform bagArea;
    [SerializeField, Min(1f)] private float maxCellSize = 300f;

    [SerializeField, Min(0)] private int pouchReserveColumns = 4;

    [Header("뷰")]
    [SerializeField] LootAreaView[] lootViews;
    [SerializeField] DragController dragController;
    [SerializeField] ItemViewPool itemViewPool;
    [SerializeField] LockView lockView;
    private InventoryUIContext context;

    public Backpack Backpack { get; private set; }

    private void Awake()
    {
        itemViewPool.Init(dragController);
    }

    private void OnEnable()
    {
        EventManager.instance.AddListener<OnItemDropped>(HandleItemDropped);
        EventManager.instance.AddListener<OnStageStarted>(HandleStageStarted);
        EventManager.instance.AddListener<OnMonsterKilled>(HandleMonsterKilled);
    }

    private void OnDisable()
    {
        if(!EventManager.HasInstance)
            return;
        EventManager.instance.RemoveListener<OnItemDropped>(HandleItemDropped);
        EventManager.instance.RemoveListener<OnStageStarted>(HandleStageStarted);
        EventManager.instance.RemoveListener<OnMonsterKilled>(HandleMonsterKilled);
    }
    #region 연결
    public void Bind(Backpack backpack)
    {
        Backpack = backpack;
        Canvas.ForceUpdateCanvases();

        context = new InventoryUIContext(CalculateCellSize(backpack.Grid.Size), itemViewPool, dragController);
        dragController.SetContext(context);
        backpackPanel.Bind(backpack, context);

        foreach(var lootView in lootViews)
        {
            lootView.Bind(new LootArea(), context);
        }
    }

    private float CalculateCellSize(Vector2Int bagSize)
    {
        Rect area = bagArea.rect;
        float fixedWidth = backpackPanel.PouchSpacing + backpackPanel.SlotColumnWidth * 2f;
        float byWidth = (area.width - fixedWidth) / (bagSize.x + pouchReserveColumns);
        float byHeight = area.height / bagSize.y;
        return Mathf.Min(maxCellSize, byWidth, byHeight);
    }
    #endregion

    #region 전리품
    private void HandleItemDropped(OnItemDropped e)
    {
        if(lootViews.Length == 0)
            return;

        lootViews[Random.Range(0, lootViews.Length)].Scatter(e.Item);
    }

    public int LootCount
    {
        get
        {
            int count = 0;

            foreach(var lootView in lootViews)
            {
                if(lootView.Area != null)
                    count += lootView.Area.Count;
            }

            return count;
        }
    }

    public void ClearLoot()
    {
        foreach(var lootView in lootViews)
        {
            lootView.Area?.Clear();
        }
    }
    #endregion

    #region 스테이지

    private void HandleStageStarted(OnStageStarted e)
    {
        ClearLoot();
        dragController.Interactable = e.IsRest;
        lockView.SetLocked(!dragController.Interactable);
    }
    private void HandleMonsterKilled(OnMonsterKilled e)
    {
        dragController.Interactable = true;
        lockView.SetLocked(!dragController.Interactable);
    }
    #endregion
}
