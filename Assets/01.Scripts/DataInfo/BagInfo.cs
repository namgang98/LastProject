using UnityEngine;

public class BagInfo : ContainerInfo
{

    public BagInfo(int id, string name, string iconName, ItemShape shape, ItemGrade grade, int sellPrice, Vector2Int storageSize, string desc) : base(id, name, iconName, shape, grade,sellPrice, desc,storageSize)
    {
    }

    public override bool CanStoreInPouch => false;
    public override bool CanStoreInBag => false;

    public override ItemInstance CreateInstance()
    {
        return new BagInstance(this);
    }
}
