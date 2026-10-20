public class PotionCaseInstance : ContainerInstance
{
    public PotionCaseInfo CaseInfo { get; }

    public PotionCaseInstance(PotionCaseInfo info) : base(info)
    {
        CaseInfo = info;
    }

    protected override bool CanStore(ItemInstance item)
    {
        return item.Info is ConsumableInfo consumable && consumable.IsPotion;
    }
}