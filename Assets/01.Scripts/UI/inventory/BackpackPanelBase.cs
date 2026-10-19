using UnityEngine;

public abstract class BackpackPanelBase : MonoBehaviour
{
    [Header("뷰")]
    [SerializeField] protected GridView bagView;
    [SerializeField] protected GridView pouchView;
    [SerializeField, Min(0f)] protected float pouchSpacing = 24f;

    [Header("장착 칸")]
    [SerializeField] protected BagSlotView bagSlotView;
    [SerializeField] protected PouchSlotView pouchSlotView;
    [SerializeField, Min(0f)] protected float slotGap = 12f;
    protected RectTransform BagSlotRect => (RectTransform)bagSlotView.transform;
    protected RectTransform PouchSlotRect => (RectTransform)pouchSlotView.transform;
    public float SlotColumnWidth => BagSlotRect.rect.width + slotGap;

    public BagSlotView BagSlot => bagSlotView;
    private Backpack backpack;
    private InventoryUIContext context;

    public GridView BagView => bagView;
    public GridView PouchView => pouchView;
    public float PouchSpacing => pouchSpacing;

    protected virtual void Awake()
    {
        pouchView.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        EventManager.instance.AddListener<OnPouchChanged>(HandlePouchChanged);
    }

    private void OnDisable()
    {
        if(!EventManager.HasInstance)
            return;
        EventManager.instance.RemoveListener<OnPouchChanged>(HandlePouchChanged);
    }

    public void Bind(Backpack target, InventoryUIContext ctx)
    {
        backpack = target;
        context = ctx;
        bagView.Bind(backpack.Grid, context);
        bagSlotView.Bind(backpack, context);
        pouchSlotView.Bind(backpack, context);
        ShowPouch(backpack.EquippedPouch);
    }

    private void HandlePouchChanged(OnPouchChanged e)
    {
        if(e.Backpack != backpack)
            return;
        ShowPouch(e.Pouch);
    }

    private void ShowPouch(PouchInstance pouch)
    {
        bool hasPouch = pouch != null;
        if(hasPouch)
        {
            pouchView.gameObject.SetActive(true);
            pouchView.Bind(pouch.Storage, context);
        }
        else
        {
            pouchView.Unbind();
            pouchView.gameObject.SetActive(false);
        }

        pouchSlotView.Refresh();

        Layout(hasPouch);
    }
    protected float PlaceGrid(GridView view, float left, float top)
    {
        Vector2 size = view.PixelSize;
        view.SetCenter(new Vector2(left + size.x * 0.5f, top - size.y * 0.5f));
        return size.y;
    }

    protected void PlaceSlot(RectTransform slot, float left, float top)
    {
        slot.anchorMin = new Vector2(0.5f, 0.5f);
        slot.anchorMax = new Vector2(0.5f, 0.5f);
        slot.pivot = new Vector2(0f, 1f);
        slot.anchoredPosition = new Vector2(left, top);
    }
    protected abstract void Layout(bool hasPouch);
}
