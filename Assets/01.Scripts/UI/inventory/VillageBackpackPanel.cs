using UnityEngine;

public class VillageBackpackPanel : BackpackPanelBase
{
    [SerializeField] Vector2 padding = new(16f, 16f);

    protected override void Layout(bool hasPouch)
    {
        Rect rect = ((RectTransform)transform).rect;
        float left = -rect.width * 0.5f + padding.x;
        float top = rect.height * 0.5f - padding.y;
        float gridLeft = left + BagSlotRect.rect.width + slotGap;

        PlaceSlot(BagSlotRect, left, top);
        float bagHeight = PlaceGrid(bagView, gridLeft, top);
        float rowHeight = Mathf.Max(BagSlotRect.rect.height, bagHeight);

        float pouchTop = top - rowHeight - pouchSpacing;
        PlaceSlot(PouchSlotRect, left, pouchTop);
        if(!hasPouch)
            return;
        PlaceGrid(pouchView, gridLeft, pouchTop);
    }
}
