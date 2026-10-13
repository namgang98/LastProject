using UnityEngine;

public abstract class ContainerInstance : ItemInstance
{
    public ContainerInfo ContainerInfo { get; }

    public GridContainer Storage { get; }

    public ContainerInstance(ContainerInfo info) : base(info)
    {
        ContainerInfo = info;
        Storage = new GridContainer(info.StorageSize, CanStore);
    }

    protected abstract bool CanStore(ItemInstance item);

}
