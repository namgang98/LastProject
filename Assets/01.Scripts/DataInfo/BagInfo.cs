using UnityEngine;

public class BagInfo : ContainerInfo
{
    public int Price { get; }

    public BagInfo(int id, string name, string iconName, ItemShape shape, ItemGrade grade, int sellPrice, Vector2Int storageSize, string desc, int price) : base(id, name, iconName, shape, grade,sellPrice, desc,storageSize)
    {
        Price = price;
    }

    public override bool CanStoreInPouch => false;
    public override bool CanStoreInBag => false;

    public override ItemInstance CreateInstance()
    {
        return new BagInstance(this);
    }
}
