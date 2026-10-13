using System.Text;
using UnityEngine;

public abstract class ContainerInfo : ItemInfo
{
    public Vector2Int StorageSize { get; }

    protected ContainerInfo(int id, string name, string iconName, ItemShape shape, ItemGrade grade, int sellPrice, string desc,Vector2Int storageSize) : base(id, name, iconName, shape, grade, sellPrice, desc)
    {
        StorageSize = storageSize;
    }

    public override void AppendTooltip(StringBuilder sb)
    {
        sb.AppendLine($"공간 : 가로{StorageSize.x} / 세로{StorageSize.y}");
    }

}
