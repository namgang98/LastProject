using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public abstract class ItemContainerView : MonoBehaviour, IItemDropTarget, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] protected RectTransform itemLayer;

    protected readonly Dictionary<ItemInstance, ItemView> views = new();
    private readonly List<ItemInstance> removeBuffer = new();

    protected InventoryUIContext context;

    protected virtual void Awake()
    {
        StretchLayer(itemLayer);
    }

    protected static void StretchLayer(RectTransform layer)
    {
        if(layer == null)
            return;

        layer.anchorMin = Vector2.zero;
        layer.anchorMax = Vector2.one;
        layer.pivot = Vector2.zero;
        layer.offsetMin = Vector2.zero;
        layer.offsetMax = Vector2.zero;
    }

    protected void SyncViews(IEnumerable<ItemInstance> current, Func<ItemInstance, bool> isAlive)
    {
        removeBuffer.Clear();

        foreach(var item in views.Keys)
        {
            if(!isAlive(item))
                removeBuffer.Add(item);
        }

        foreach(var item in removeBuffer)
        {
            context.Pool.Release(views[item]);
            views.Remove(item);
        }

        foreach(var item in current)
        {
            if(!views.TryGetValue(item, out ItemView view))
            {
                view = context.Pool.Get();
                view.Bind(item, this);
                views[item] = view;
            }

            if(context.Drag.IsDragging(view))
                continue;

            view.transform.SetParent(itemLayer, false);
            PlaceView(view);
        }
    }

    protected void ReleaseAllViews()
    {
        if(context == null)
            return;

        foreach(var view in views.Values)
        {
            context.Pool.Release(view);
        }

        views.Clear();
    }

    protected bool TryGetLocalPoint(PointerEventData eventData, out Vector2 local)
    {
        return RectTransformUtility.ScreenPointToLocalPointInRectangle(itemLayer, eventData.position, eventData.pressEventCamera, out local);
    }

    public bool TryGetView(ItemInstance item, out ItemView view)
    {
        return views.TryGetValue(item, out view);
    }
    protected abstract void PlaceView(ItemView view);

    #region IItemDropTarget
    public abstract void ShowPreview(ItemInstance item, DragPose pose, PointerEventData eventData);
    public abstract void HidePreview();
    public abstract bool TryDrop(ItemInstance item, DragPose pose, PointerEventData eventData);
    public abstract void Release(ItemInstance item);

    public void Restore(ItemInstance item)
    {
        if(!views.TryGetValue(item, out ItemView view))
            return;

        view.transform.SetParent(itemLayer, false);
        PlaceView(view);
    }
    #endregion

    #region 마우스 위치 알림
    public void OnPointerEnter(PointerEventData eventData)
    {
        if(context == null)
            return;

        context.Drag.SetHover(this);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if(context == null)
            return;

        context.Drag.ClearHover(this);
    }
    #endregion
}
