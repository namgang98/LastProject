using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public abstract class EquipSlotView : ItemContainerView, IItemDragSource
{
    [Header("표시")]
    [SerializeField] Image highlight;
    [SerializeField] GameObject dim;
    [SerializeField] Color validColor = new(0.3f, 1f, 0.3f, 0.45f);
    [SerializeField] Color invalidColor = new(1f, 0.3f, 0.3f, 0.45f);

    [Header("잠금")]
    [SerializeField] bool locked;

    private readonly ItemInstance[] single = new ItemInstance[1];
    protected Backpack Backpack { get; private set; }

    // 자식이 정하는 4가지
    protected abstract ItemInstance Current { get; }
    protected abstract bool CanEquip(ItemInstance item);
    protected abstract bool Equip(ItemInstance item);
    protected abstract void Unequip(ItemInstance item);

    protected override void Awake()
    {
        base.Awake();
        highlight.raycastTarget = false;
        highlight.enabled = false;
    }

    #region 연결
    public void Bind(Backpack target, InventoryUIContext ctx)
    {
        Unbind();
        Backpack = target;
        context = ctx;
        SetLocked(locked);
        Refresh();
    }

    public void Unbind()
    {
        HidePreview();
        ReleaseAllViews();
        Backpack = null;
    }

    public void SetLocked(bool value)
    {
        locked = value;

        if(dim != null)
            dim.SetActive(locked);
    }

    public void Refresh()
    {
        if(Backpack == null)
            return;

        single[0] = Current;
        ItemInstance[] current = single[0] == null ? Array.Empty<ItemInstance>() : single;
        SyncViews(current, item => item == Current);
    }
    #endregion

    protected override void PlaceView(ItemView view)
    {
        Vector2Int bounds = ItemView.GetBounds(view.Item, 0);
        Vector2 area = itemLayer.rect.size;
        float cellSize = Mathf.Min(area.x / bounds.x, area.y / bounds.y);

        view.ApplyLayout(cellSize, 0, 0f);
        view.Rect.anchoredPosition = (area - view.Rect.sizeDelta) * 0.5f;
    }

    public virtual bool CanDragOut(ItemInstance item)
    {
        return !locked;
    }

    #region 드롭 대상
    public override void ShowPreview(ItemInstance item, int rotation, PointerEventData eventData)
    {
        if(Backpack == null || item == Current)
            return;

        bool canEquip = !locked && CanEquip(item);
        highlight.enabled = true;
        highlight.color = canEquip || CanInsert(item) ? validColor : invalidColor;
    }

    public override void HidePreview()
    {
        highlight.enabled = false;
    }

    public override bool TryDrop(ItemInstance item, int rotation, PointerEventData eventData)
    {
        if(Backpack == null)
            return false;

        if(!locked && CanEquip(item))
            return Equip(item);

        if(!CanInsert(item))
            return false;

        return ((ContainerInstance)Current).Storage.TryAutoPlace(item);
    }

    public override void Release(ItemInstance item)
    {
        if(Backpack == null || item != Current)
            return;

        Unequip(item);
    }
    private bool CanInsert(ItemInstance item)
    {
        return Current is ContainerInstance target && item != target && item.Owner != target.Storage && target.Storage.CanAutoPlace(item);
    }
    #endregion
}
