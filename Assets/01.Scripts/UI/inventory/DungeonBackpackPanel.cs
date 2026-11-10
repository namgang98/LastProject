using UnityEngine;

public class DungeonBackpackPanel : BackpackPanelBase
{
    [SerializeField, Min(0f)] protected float labelSpace = 40f;
    public float LabelSpace => labelSpace;
    protected override void Layout(bool hasPouch)
    {
        float slotColumn = SlotColumnWidth;
        float bagWidth = slotColumn + bagView.PixelSize.x;
        float pouchWidth = slotColumn + (hasPouch ? pouchView.PixelSize.x : 0f);
        float total = bagWidth + pouchSpacing + pouchWidth;

        float left = -total * 0.5f;
        float top = GetRowTop();

        PlaceSlot(BagSlotRect, left, top);
        PlaceGrid(bagView, left + slotColumn, top);

        float pouchLeft = left + bagWidth + pouchSpacing;
        PlaceSlot(PouchSlotRect, pouchLeft, top);

        if(!hasPouch)
            return;

        PlaceGrid(pouchView, pouchLeft + slotColumn, top);
    }

    protected virtual float GetRowTop()
    {
        return (GetRowHeight() - labelSpace) * 0.5f;
    }
    private float GetRowHeight()
    {
        float height = Mathf.Max(bagView.PixelSize.y, BagSlotRect.rect.height);

        if(pouchView.gameObject.activeSelf)
            height = Mathf.Max(height, pouchView.PixelSize.y);

        return height;
    }
}