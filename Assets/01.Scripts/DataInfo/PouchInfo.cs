using UnityEngine;

public class PouchInfo : ContainerInfo
{
    public PouchInfo(int id, string name, string iconName, ItemShape shape, ItemGrade grade, int sellPrice, string desc, Vector2Int size) : base(id, name, iconName, shape, grade, sellPrice, desc ,size)
    {}

    public override bool CanStoreInPouch => false;

    public override ItemInstance CreateInstance()
    {
        return new PouchInstance(this);
    }

}
