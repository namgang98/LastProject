using UnityEngine;

public class BagInstance : ContainerInstance
{
    public BagInfo BagInfo { get; }
    public BagInstance(BagInfo info) : base(info)
    {
        BagInfo = info;
    }

    protected override bool CanStore(ItemInstance item)
    {
        if(!item.Info.CanStoreInBag)
            return false;

        if(item is not PouchInstance)
            return true;

        foreach(var stored in Storage.Items)
        {
            if(stored is PouchInstance && stored != item)
                return false;
        }

        return true;
    }
}
