using System;
using UnityEngine.EventSystems;

public class SellAreaView : LootAreaView
{
    public Func<ItemInstance, bool> CanAccept { get; set; }

    public event Action<ItemInstance> Entering;
    public event Action<ItemInstance> Rejected;

    public override bool TryDrop(ItemInstance item, DragPose pose, PointerEventData eventData)
    {
        if(CanAccept != null && !CanAccept(item))
        {
            Rejected?.Invoke(item);
            return false;
        }

        if(Area != null && !Area.Contains(item))
            Entering?.Invoke(item);

        return base.TryDrop(item, pose, eventData);
    }
}