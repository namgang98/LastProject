using System.Collections.Generic;

public class Backpack
{
    public BagInstance Bag { get; }
    public GridContainer Grid => Bag.Storage;
    public PouchInstance EquippedPouch { get; private set; }
    public GridContainer PouchStorage => EquippedPouch?.Storage;

    public Backpack(BagInstance bag)
    {
        Bag = bag;
    }
    #region 파우치 장착
    public bool CanEquipPouch(ItemInstance item)
    {
        return EquippedPouch == null && item is PouchInstance;
    }

    public bool TryEquipPouch(ItemInstance item)
    {
        if(EquippedPouch != null || item is not PouchInstance pouch)
            return false;

        pouch.Owner?.Remove(pouch);
        EquippedPouch = pouch;
        NotifyPouchChanged();
        return true;
    }

    public PouchInstance UnequipPouch()
    {
        PouchInstance pouch = EquippedPouch;

        if(pouch == null)
            return null;

        EquippedPouch = null;
        NotifyPouchChanged();
        return pouch;
    }

    private void NotifyPouchChanged()
    {
        EventManager.instance.PostNotification(new OnPouchChanged(this, EquippedPouch));
    }
    #endregion

    public List<ItemInstance> ExtractAll()
    {
        return Grid.TakeAll();
    }

    public PouchInstance LoseAllOnDeath()
    {
        PouchInstance survived = EquippedPouch;
        Grid.TakeAll();
        return survived;
    }

    public IEnumerable<ItemInstance> EquippedItems()
    {
        foreach(var item in Grid.Items)
        {
            if(item is ContainerInstance)
                continue;
            yield return item;
        }

        if(PouchStorage == null)
            yield break;

        foreach(var item in PouchStorage.Items)
        {
            if(item is ContainerInstance)
                continue;
            yield return item;
        }
    }
}