using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GridView : ItemContainerView
{
    [Header("레이어")]
    [SerializeField] private RectTransform cellLayer;
    [SerializeField] private RectTransform previewLayer;

    [Header("프리팹")]
    [SerializeField] private Image cellPrefab;
    [SerializeField] private Image previewPrefab;

    [Header("표시")]
    [SerializeField, Min(0f)] private float cellPadding = 4f;
    private Color validColor = new(0.3f, 1f, 0.3f, 0.45f);
    private Color invalidColor = new(1f, 0.3f, 0.3f, 0.45f);

    private readonly List<Image> cells = new();
    private readonly List<Image> previews = new();

    private RectTransform rect;
    private GridContainer container;

    private bool previewVisible;
    private Vector2Int lastPreviewAnchor;
    private int lastPreviewRotation;

    public GridContainer Container => container;
    public Vector2 PixelSize => rect.sizeDelta;

    private ContainerInstance insertTarget;
    protected override void Awake()
    {
        base.Awake();
        RectSet();
        StretchLayer(cellLayer);
        StretchLayer(previewLayer);
    }

    private void OnDestroy()
    {
        if(container == null)
            return;

        container.Changed -= Refresh;
        container = null;
    }

    #region 연결
    public void Bind(GridContainer target, InventoryUIContext ctx)
    {
        Unbind();

        container = target;
        context = ctx;
        rect.sizeDelta = (Vector2)container.Size * context.CellSize;

        BuildCells();
        container.Changed += Refresh;
        Refresh(container);
    }

    public void Unbind()
    {
        if(container == null)
            return;

        container.Changed -= Refresh;
        HidePreview();
        ReleaseAllViews();
        container = null;
    }

    public void SetCenter(Vector2 position)
    {
        rect.anchoredPosition = position;
    }
    private void RectSet()
    {
        rect = (RectTransform)transform;
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
    }
    #endregion

    #region 그리기
    private void BuildCells()
    {
        int count = container.Size.x * container.Size.y;
        EnsureCount(cells, cellPrefab, cellLayer, count);

        for(int i = 0; i < cells.Count; i++)
        {
            bool active = i < count;
            cells[i].gameObject.SetActive(active);

            if(!active)
                continue;

            SetCellRect(cells[i], new Vector2Int(i % container.Size.x, i / container.Size.x));
        }
    }

    private void Refresh(GridContainer changed)
    {
        SyncViews(container.Items, container.Contains);
    }

    protected override void PlaceView(ItemView view)
    {
        view.ApplyLayout(context.CellSize, view.Item.Rotation, 0f);
        view.Rect.anchoredPosition = (Vector2)view.Item.Anchor * context.CellSize;
    }

    private void SetCellRect(Image image, Vector2Int cell)
    {
        RectTransform cellRect = image.rectTransform;
        cellRect.anchorMin = Vector2.zero;
        cellRect.anchorMax = Vector2.zero;
        cellRect.pivot = Vector2.zero;
        cellRect.anchoredPosition = (Vector2)cell * context.CellSize + Vector2.one * (cellPadding * 0.5f);
        cellRect.sizeDelta = Vector2.one * (context.CellSize - cellPadding);
    }

    private static void EnsureCount(List<Image> list, Image prefab, RectTransform parent, int count)
    {
        while(list.Count < count)
        {
            Image image = Instantiate(prefab, parent);
            image.raycastTarget = false;
            list.Add(image);
        }
    }


    #endregion

    #region 드래그 대상 (IItemDropTarget)
    public override void ShowPreview(ItemInstance item, DragPose pose, PointerEventData eventData)
    {
        if(container == null)
            return;

        if(TryGetInsertTarget(item, eventData, out ContainerInstance target))
        {
            ShowInsertPreview(item, target);
            return;
        }

        insertTarget = null;

        Vector2Int anchor = GetAnchor(pose, eventData);

        if(previewVisible && anchor == lastPreviewAnchor && pose.Rotation == lastPreviewRotation)
            return;

        previewVisible = true;
        lastPreviewAnchor = anchor;
        lastPreviewRotation = pose.Rotation;

        List<Vector2Int> targetCells = item.GetCellsAt(anchor, pose.Rotation);
        Color color = container.CanPlace(item, anchor, pose.Rotation) ? validColor : invalidColor;
        EnsureCount(previews, previewPrefab, previewLayer, targetCells.Count);

        for(int i = 0; i < previews.Count; i++)
        {
            bool show = i < targetCells.Count && container.IsInGrid(targetCells[i]);
            previews[i].gameObject.SetActive(show);

            if(!show)
                continue;

            previews[i].color = color;
            SetCellRect(previews[i], targetCells[i]);
        }
    }

    public override void HidePreview()
    {
        if(!previewVisible)
            return;

        foreach(var preview in previews)
        {
            preview.gameObject.SetActive(false);
        }

        previewVisible = false;
    }

    public override bool TryDrop(ItemInstance item, DragPose pose, PointerEventData eventData)
    {
        if(container == null)
            return false;

        if(TryGetInsertTarget(item, eventData, out ContainerInstance target))
            return target.Storage.TryAutoPlace(item);

        return container.TryPlace(item, GetAnchor(pose, eventData), pose.Rotation);
    }

    public override void Release(ItemInstance item)
    {
        if(container == null)
            return;

        container.Remove(item);
    }

    private Vector2Int GetAnchor(DragPose pose, PointerEventData eventData)
    {
        if(!TryGetLocalPoint(eventData, out Vector2 local))
            return new Vector2Int(-999, -999);

        return pose.GetAnchor(local / context.CellSize);
    }

    private bool TryGetInsertTarget(ItemInstance item, PointerEventData eventData, out ContainerInstance target)
    {
        target = null;

        if(!TryGetLocalPoint(eventData, out Vector2 local))
            return false;

        Vector2Int cell = new Vector2Int(Mathf.FloorToInt(local.x / context.CellSize), Mathf.FloorToInt(local.y / context.CellSize));

        if(container.GetItemAt(cell) is not ContainerInstance found || found == item)
            return false;

        target = found;
        return true;
    }

    private void ShowInsertPreview(ItemInstance item, ContainerInstance target)
    {
        if(previewVisible && insertTarget == target)
            return;

        insertTarget = target;
        previewVisible = true;

        List<Vector2Int> targetCells = target.GetOccupiedCells();
        Color color = target.Storage.CanAutoPlace(item) ? validColor : invalidColor;
        EnsureCount(previews, previewPrefab, previewLayer, targetCells.Count);

        for(int i = 0; i < previews.Count; i++)
        {
            bool show = i < targetCells.Count;
            previews[i].gameObject.SetActive(show);

            if(!show)
                continue;

            previews[i].color = color;
            SetCellRect(previews[i], targetCells[i]);
        }
    }
    #endregion
}
