using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// 아이템 1개의 화면 표시 + 드래그 입력 받기
// 로직은 없고, 드래그 입력은 전부 DragController에 넘긴다
// 구조 : ItemView(틀, 피벗 왼쪽 아래) > Icon(Image, 피벗 가운데, 회전 담당)
[RequireComponent(typeof(RectTransform), typeof(CanvasGroup))]
public class ItemView : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [SerializeField] Image icon;
    [SerializeField] ItemCooldownView cooldown;
    public ItemCooldownView Cooldown => cooldown;
    private RectTransform rect;
    private CanvasGroup canvasGroup;
    private DragController drag;

    private bool isHovered;
    public ItemInstance Item { get; private set; }
    public IItemDropTarget Home { get; private set; }
    public RectTransform Rect => rect;

    // 풀에서 처음 만들어질 때 1번만 호출
    public void Init(DragController dragController)
    {
        drag = dragController;
        rect = (RectTransform)transform;
        canvasGroup = GetComponent<CanvasGroup>();
        icon.preserveAspect = true;

        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.zero;
        rect.pivot = Vector2.zero;

        RectTransform iconRect = icon.rectTransform;
        iconRect.anchorMin = new Vector2(0.5f, 0.5f);
        iconRect.anchorMax = new Vector2(0.5f, 0.5f);
        iconRect.pivot = new Vector2(0.5f, 0.5f);
        iconRect.anchoredPosition = Vector2.zero;
        iconRect.localScale = Vector3.one;
    }

    public void Bind(ItemInstance item, IItemDropTarget home)
    {
        Item = item;
        Home = home;
        icon.sprite = item.Info.Icon;
    }

    // 회전된 모양에 맞춰 틀 크기를 정하고, 아이콘은 원래 크기 그대로 돌린다
    public void ApplyLayout(float cellSize, int rotation, float tilt)
    {
        Vector2Int rotatedSize = GetBounds(Item, rotation);
        Vector2Int originalSize = GetBounds(Item, 0);

        rect.sizeDelta = (Vector2)rotatedSize * cellSize;
        icon.rectTransform.sizeDelta = (Vector2)originalSize * cellSize;
        icon.rectTransform.localEulerAngles = new Vector3(0f, 0f, -90f * rotation + tilt);
        cooldown.SetOrientation(originalSize.x > originalSize.y);
    }

    public void SetRaycastBlocking(bool block)
    {
        canvasGroup.blocksRaycasts = block;
    }

    public void ResetView()
    {
        Item = null;
        Home = null;
        icon.sprite = null;
        canvasGroup.blocksRaycasts = true;
        icon.rectTransform.localEulerAngles = Vector3.zero;
        Cooldown.Unbind();
    }

    public static Vector2Int GetBounds(ItemInstance item, int rotation)
    {
        int maxX = 0;
        int maxY = 0;

        foreach(var cell in item.Info.Shape.GetRotatedCells(rotation))
        {
            maxX = Mathf.Max(maxX, cell.x);
            maxY = Mathf.Max(maxY, cell.y);
        }

        return new Vector2Int(maxX + 1, maxY + 1);
    }

    #region 드래그 입력 → DragController로 전달
    public void OnBeginDrag(PointerEventData eventData)
    {
        drag.BeginDrag(this, eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        drag.Drag(this, eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        drag.EndDrag(this, eventData);
    }

    #endregion
    public void OnPointerEnter(PointerEventData eventData)
    {
        if(drag.IsAnyDragging)
            return;
        isHovered = true;
        EventManager.instance.PostNotification(new OnItemHoverEnter(Item, rect));
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if(!isHovered)
            return;

        isHovered = false;
        EventManager.instance.PostNotification(new OnItemHoverExit());
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        if(eventData.dragging || Item == null)
            return;

        if(eventData.button == PointerEventData.InputButton.Right)
        {
            EventManager.instance.PostNotification(new OnItemHoverExit());
            EventManager.instance.PostNotification(new OnItemQuickMove(Item));
            return;
        }

        EventManager.instance.PostNotification(new OnItemClicked(Item));
    }
    private void OnDisable()
    {
        if(!isHovered)
            return;

        isHovered = false;

        if(!EventManager.HasInstance)
            return;

        EventManager.instance.PostNotification(new OnItemHoverExit());
    }
}
