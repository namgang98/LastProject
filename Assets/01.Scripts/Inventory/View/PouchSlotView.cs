public class PouchSlotView : EquipSlotView
{
    protected override ItemInstance Current => Backpack.EquippedPouch;
    protected override bool CanEquip(ItemInstance item) => Backpack.CanEquipPouch(item);
    protected override bool Equip(ItemInstance item) => Backpack.TryEquipPouch(item);
    protected override void Unequip(ItemInstance item) => Backpack.UnequipPouch();
}