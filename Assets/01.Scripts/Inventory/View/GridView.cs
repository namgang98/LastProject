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

    protected override void Awake()
    {
        base.Awake();
        RectSet();
        StretchLayer(cellLayer);
        StretchLayer(previewLayer);
    }

    private void OnDestroy()
    {
        Unbind();
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
    public override void ShowPreview(ItemInstance item, int rotation, PointerEventData eventData)
    {
        if(container == null)
            return;

        Vector2Int anchor = GetAnchor(item, rotation, eventData);

        if(previewVisible && anchor == lastPreviewAnchor && rotation == lastPreviewRotation)
            return;

        previewVisible = true;
        lastPreviewAnchor = anchor;
        lastPreviewRotation = rotation;

        List<Vector2Int> targetCells = item.GetCellsAt(anchor, rotation);
        Color color = container.CanPlace(item, anchor, rotation) ? validColor : invalidColor;
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

    public override bool TryDrop(ItemInstance item, int rotation, PointerEventData eventData)
    {
        if(container == null)
            return false;

        return container.TryPlace(item, GetAnchor(item, rotation, eventData), rotation);
    }

    public override void Release(ItemInstance item)
    {
        if(container == null)
            return;

        container.Remove(item);
    }

    // 마우스 위치 = 아이템 중심이 되도록 왼쪽 아래 칸(anchor)을 계산
    private Vector2Int GetAnchor(ItemInstance item, int rotation, PointerEventData eventData)
    {
        if(!TryGetLocalPoint(eventData, out Vector2 local))
            return new Vector2Int(-999, -999);

        Vector2 size = ItemView.GetBounds(item, rotation);
        Vector2 origin = local / context.CellSize - size * 0.5f;
        return new Vector2Int(Mathf.RoundToInt(origin.x), Mathf.RoundToInt(origin.y));
    }
    #endregion
}
