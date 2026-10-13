using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    [Header("영역")]
    [SerializeField] private RectTransform bagArea;
    [SerializeField, Min(1f)] private float maxCellSize = 300f;

    [SerializeField, Min(0)] private int pouchReserveColumns = 4;
    [SerializeField, Min(0f)] private float pouchSpacing = 24f;

    [Header("뷰")]
    [SerializeField] GridView bagView;
    [SerializeField] GridView pouchView;
    [SerializeField] LootAreaView[] lootViews;
    [SerializeField] DragController dragController;
    [SerializeField] ItemViewPool itemViewPool;
    [SerializeField] LockView lockView;
    private InventoryUIContext context;

    public Backpack Backpack { get; private set; }

    private void Awake()
    {
        itemViewPool.Init(dragController);
        pouchView.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        EventManager.instance.AddListener<OnItemDropped>(HandleItemDropped);
        EventManager.instance.AddListener<OnPouchChanged>(HandleSafePouchChanged);
        EventManager.instance.AddListener<OnStageStarted>(HandleStageStarted);
        EventManager.instance.AddListener<OnMonsterKilled>(HandleMonsterKilled);
    }

    private void OnDisable()
    {
        if(!EventManager.HasInstance)
            return;
        EventManager.instance.RemoveListener<OnItemDropped>(HandleItemDropped);
        EventManager.instance.RemoveListener<OnPouchChanged>(HandleSafePouchChanged);
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
        bagView.Bind(backpack.Grid, context);

        foreach(var lootView in lootViews)
        {
            lootView.Bind(new LootArea(), context);
        }

        ShowPouch(backpack.EquippedPouch);
    }

    private float CalculateCellSize(Vector2Int bagSize)
    {
        Rect area = bagArea.rect;
        float byWidth = (area.width - pouchSpacing) / (bagSize.x + pouchReserveColumns);
        float byHeight = area.height / bagSize.y;
        return Mathf.Min(maxCellSize, byWidth, byHeight);
    }
    #endregion

    #region 주머니
    private void ShowPouch(PouchInstance pouch)
    {
        if(pouch == null)
        {
            pouchView.Unbind();
            pouchView.gameObject.SetActive(false);
        }
        else
        {
            pouchView.gameObject.SetActive(true);
            pouchView.Bind(pouch.Storage, context);
        }

        LayoutGrids();
    }

    private void LayoutGrids()
    {
        bool hasPouch = pouchView.gameObject.activeSelf;
        float bagWidth = bagView.PixelSize.x;
        float pouchWidth = hasPouch ? pouchView.PixelSize.x : 0f;
        float total = bagWidth + (hasPouch ? pouchSpacing + pouchWidth : 0f);
        float left = -total * 0.5f;

        bagView.SetCenter(new Vector2(left + bagWidth * 0.5f, 0f));

        if(!hasPouch)
            return;

        pouchView.SetCenter(new Vector2(left + bagWidth + pouchSpacing + pouchWidth * 0.5f, 0f));
    }

    private void HandleSafePouchChanged(OnPouchChanged e)
    {
        if(e.Backpack != Backpack)
            return;

        ShowPouch(e.Pouch);
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
        dragController.Interactable = false;
        lockView.SetLocked(!dragController.Interactable);
    }
    private void HandleMonsterKilled(OnMonsterKilled e)
    {
        dragController.Interactable = true;
        lockView.SetLocked(!dragController.Interactable);
    }
    #endregion
}
