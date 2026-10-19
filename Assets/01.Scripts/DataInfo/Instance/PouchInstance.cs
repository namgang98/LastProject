public class PouchInstance : ContainerInstance
{
    public PouchInfo PouchInfo { get; }

    public PouchInstance(PouchInfo info) : base(info)
    {
        PouchInfo = info;
    }
    protected override bool CanStore(ItemInstance item)
    {
        return item != this && item.Info.CanStoreInPouch;
    }
}