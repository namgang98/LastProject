using UnityEngine;

public class DungeonBackpackPanel : BackpackPanelBase
{
    protected override void Layout(bool hasPouch)
    {
        float slotColumn = SlotColumnWidth;
        float bagWidth = slotColumn + bagView.PixelSize.x;
        float pouchWidth = slotColumn + (hasPouch ? pouchView.PixelSize.x : 0f);
        float total = bagWidth + pouchSpacing + pouchWidth;

        float left = -total * 0.5f;
        float top = bagView.PixelSize.y * 0.5f;

        PlaceSlot(BagSlotRect, left, top);
        PlaceGrid(bagView, left + slotColumn, top);

        float pouchLeft = left + bagWidth + pouchSpacing;
        PlaceSlot(PouchSlotRect, pouchLeft, top);

        if(!hasPouch)
            return;
        PlaceGrid(pouchView, pouchLeft + slotColumn, top);
    }
}