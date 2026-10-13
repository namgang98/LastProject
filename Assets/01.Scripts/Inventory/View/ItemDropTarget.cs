using UnityEngine.EventSystems;

public interface IItemDropTarget
{
    // 드래그 중 미리보기
    void ShowPreview(ItemInstance item, int rotation, PointerEventData eventData);
    void HidePreview();

    //  시도
    bool TryDrop(ItemInstance item, int rotation, PointerEventData eventData);

    // 성공시 옮기기
    void Release(ItemInstance item);

    // 실패시 되돌리기
    void Restore(ItemInstance item);
}