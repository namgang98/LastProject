using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class DragController : MonoBehaviour
{
    [Tooltip("드래그 중인 아이템을 맨 위에 그릴 레이어 (DragCanvas 안)")]
    [SerializeField] private RectTransform dragLayer;

    private InventoryUIContext context;
    private ItemView dragging;
    private IItemDropTarget origin;
    private IItemDropTarget hover;
    private PointerEventData lastEvent;
    private DragPose pose;

    public bool IsAnyDragging => dragging != null;
    public bool Interactable { get; set; } = true;

    public event Action OnDragBlocked;

    private void Awake()
    {
        DragLayerSet();
    }
    private void DragLayerSet()
    {
        dragLayer.anchorMin = Vector2.zero;
        dragLayer.anchorMax = Vector2.one;
        dragLayer.pivot = Vector2.zero;
        dragLayer.offsetMin = Vector2.zero;
        dragLayer.offsetMax = Vector2.zero;
    }

    public void SetContext(InventoryUIContext ctx)
    {
        context = ctx;
    }

    public bool IsDragging(ItemView view)
    {
        return view != null && dragging == view;
    }

    private void Update()
    {
        if(dragging == null)
            return;

        if(Keyboard.current == null || !Keyboard.current.rKey.wasPressedThisFrame)
            return;

        pose = pose.RotateClockwise(dragging.Item);
        dragging.ApplyLayout(context.CellSize, pose.Rotation, 0f);
        FollowPointer();
        UpdatePreview();
    }

    #region 마우스가 올라간 대상
    public void SetHover(IItemDropTarget target)
    {
        if(hover == target)
            return;

        hover?.HidePreview();

        hover = target;
        UpdatePreview();
    }

    public void ClearHover(IItemDropTarget target)
    {
        if(hover != target)
            return;

        hover.HidePreview();
        hover = null;
    }
    #endregion

    #region 드래그
    public void BeginDrag(ItemView view, PointerEventData eventData)
    {
        if(!Interactable)
        {
            OnDragBlocked?.Invoke();
            return;
        }

        if(view.Home is IItemDragSource source && !source.CanDragOut(view.Item))
            return;

        if(dragging != null || context == null)
            return;

        dragging = view;
        origin = view.Home;
        lastEvent = eventData;

        int rotation = view.Item.IsPlaced ? view.Item.Rotation : 0;
        pose = DragPose.Grabbed(view.Item, rotation, GetPressedCell(view, rotation, eventData));

        view.transform.SetParent(dragLayer, false);
        view.SetRaycastBlocking(false);
        view.ApplyLayout(context.CellSize, pose.Rotation, 0f);

        EventManager.instance.PostNotification(new OnItemHoverExit());
        FollowPointer();
        SetHover(origin);
    }

    public void Drag(ItemView view, PointerEventData eventData)
    {
        if(!IsDragging(view))
            return;

        lastEvent = eventData;
        FollowPointer();
        UpdatePreview();
    }

    public void EndDrag(ItemView view, PointerEventData eventData)
    {
        if(!IsDragging(view))
            return;

        IItemDropTarget target = hover;
        IItemDropTarget from = origin;
        ItemInstance item = view.Item;

        target?.HidePreview();

        dragging = null;
        origin = null;
        lastEvent = null;
        view.SetRaycastBlocking(true);

        bool success = target != null && target.TryDrop(item, pose, eventData);

        if(!success)
        {
            from?.Restore(item);
            return;
        }

        if(target != from)
            from?.Release(item);
    }

    private void FollowPointer()
    {
        if(!RectTransformUtility.ScreenPointToLocalPointInRectangle(dragLayer, lastEvent.position, lastEvent.pressEventCamera, out Vector2 local))
            return;

        dragging.Rect.anchoredPosition = local - pose.Grab * context.CellSize;
    }

    private void UpdatePreview()
    {
        if(dragging == null || hover == null || lastEvent == null)
            return;

        hover.ShowPreview(dragging.Item, pose, lastEvent);
    }
    private static Vector2 GetPressedCell(ItemView view, int rotation, PointerEventData eventData)
    {
        Vector2 bounds = ItemView.GetBounds(view.Item, rotation);
        Vector2 size = view.Rect.rect.size;

        if(size.x <= 0f || size.y <= 0f)
            return bounds * 0.5f;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(view.Rect, eventData.pressPosition, eventData.pressEventCamera, out Vector2 local);
        return Vector2.Scale(local, new Vector2(bounds.x / size.x, bounds.y / size.y));
    }
    #endregion
}

public interface IItemDragSource
{
    bool CanDragOut(ItemInstance item);
}
