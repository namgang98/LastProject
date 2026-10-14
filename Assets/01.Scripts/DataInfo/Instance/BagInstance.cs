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
        return item != this && item.Info.CanStoreInBag;
    }
}
