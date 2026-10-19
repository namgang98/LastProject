using System;

public class BagSlotView : EquipSlotView
{
    public Func<BagInstance, bool> SwapHandler { get; set; }

    protected override ItemInstance Current => Backpack.Bag;

    protected override bool CanEquip(ItemInstance item)
    {
        return SwapHandler != null && item is BagInstance && item != Backpack.Bag;
    }

    protected override bool Equip(ItemInstance item)
    {
        if(SwapHandler == null || item is not BagInstance bag)
            return false;

        return SwapHandler(bag);
    }

    protected override void Unequip(ItemInstance item) { }

    public override bool CanDragOut(ItemInstance item) => false;
}