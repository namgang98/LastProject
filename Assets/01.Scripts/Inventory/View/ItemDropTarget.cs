using UnityEngine.EventSystems;

public interface IItemDropTarget
{
    void ShowPreview(ItemInstance item, DragPose pose, PointerEventData eventData);
    void HidePreview();

    bool TryDrop(ItemInstance item, DragPose pose, PointerEventData eventData);

    void Release(ItemInstance item);

    void Restore(ItemInstance item);
}