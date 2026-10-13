using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class LootAreaView : ItemContainerView
{
    [SerializeField, Range(0f, 45f)] private float maxTilt = 25f;
    [SerializeField, Min(1)] private int scatterTries = 10;

    private readonly List<ItemInstance> itemBuffer = new();
    private LootArea area;

    public LootArea Area => area;

    private void OnDestroy()
    {
        Unbind();
    }

    #region 연결
    public void Bind(LootArea target, InventoryUIContext ctx)
    {
        Unbind();

        area = target;
        context = ctx;
        area.Changed += Refresh;
        Refresh(area);
    }

    public void Unbind()
    {
        if(area == null)
            return;

        area.Changed -= Refresh;
        ReleaseAllViews();
        area = null;
    }
    #endregion

    #region 흩뿌리기
    public void Scatter(ItemInstance item)
    {
        if(area == null)
            return;

        Vector2 half = GetHalfSize(item);
        Rect bounds = itemLayer.rect;
        Vector2 min = half;
        Vector2 max = new(bounds.width - half.x, bounds.height - half.y);
        Vector2 position = bounds.size * 0.5f;

        if(max.x >= min.x && max.y >= min.y)
        {
            for(int i = 0; i < scatterTries; i++)
            {
                position = new Vector2(Random.Range(min.x, max.x), Random.Range(min.y, max.y));

                if(!Overlaps(position, half))
                    break;
            }
        }

        area.Add(item, position, Random.Range(-maxTilt, maxTilt));
    }

    private bool Overlaps(Vector2 center, Vector2 half)
    {
        foreach(var entry in area.Entries)
        {
            Vector2 otherHalf = GetHalfSize(entry.Item);
            bool overlapX = Mathf.Abs(center.x - entry.Position.x) < half.x + otherHalf.x;
            bool overlapY = Mathf.Abs(center.y - entry.Position.y) < half.y + otherHalf.y;

            if(overlapX && overlapY)
                return true;
        }

        return false;
    }

    private Vector2 GetHalfSize(ItemInstance item)
    {
        return 0.5f * context.CellSize * (Vector2)ItemView.GetBounds(item, 0);
    }
    #endregion

    #region 그리기
    private void Refresh(LootArea changed)
    {
        itemBuffer.Clear();

        foreach(var entry in area.Entries)
        {
            itemBuffer.Add(entry.Item);
        }

        SyncViews(itemBuffer, area.Contains);
    }

    protected override void PlaceView(ItemView view)
    {
        if(!area.TryGetEntry(view.Item, out LootArea.Entry entry))
            return;

        view.ApplyLayout(context.CellSize, 0, entry.Tilt);
        view.Rect.anchoredPosition = entry.Position - view.Rect.sizeDelta * 0.5f;
        view.transform.SetAsLastSibling();
    }
    #endregion

    #region 드래그 대상 (IItemDropTarget)
    public override void ShowPreview(ItemInstance item, int rotation, PointerEventData eventData){}

    public override void HidePreview(){}

    public override bool TryDrop(ItemInstance item, int rotation, PointerEventData eventData)
    {
        if(area == null)
            return false;

        if(!TryGetLocalPoint(eventData, out Vector2 local))
            return false;

        Rect bounds = itemLayer.rect;
        local.x = Mathf.Clamp(local.x, 0f, bounds.width);
        local.y = Mathf.Clamp(local.y, 0f, bounds.height);

        if(item.Owner != null)
            item.Owner.Remove(item);

        area.Add(item, local, 0f);
        return true;
    }

    public override void Release(ItemInstance item)
    {
        if(area == null)
            return;

        area.Remove(item);
    }
    #endregion
}
