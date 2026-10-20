using System.Collections.Generic;
using System.Text;
using UnityEngine;

public abstract class ContainerInstance : ItemInstance
{
    private static readonly Dictionary<string, int> counts = new();

    public ContainerInfo ContainerInfo { get; }

    public GridContainer Storage { get; }

    public ContainerInstance(ContainerInfo info) : base(info)
    {
        ContainerInfo = info;
        Storage = new GridContainer(info.StorageSize, CanStore);
    }

    protected abstract bool CanStore(ItemInstance item);

    public override void AppendStateTooltip(StringBuilder sb)
    {
        sb.AppendLine();

        if(Storage.Items.Count == 0)
        {
            sb.Append("비어 있음");
            return;
        }

        counts.Clear();

        foreach(ItemInstance item in Storage.Items)
        {
            counts.TryGetValue(item.Info.Name, out int count);
            counts[item.Info.Name] = count + 1;
        }

        sb.Append($"들어 있는 것 ({Storage.Items.Count})");

        foreach(var pair in counts)
        {
            sb.AppendLine();
            sb.Append($"· {pair.Key} x{pair.Value}");
        }
    }

}
