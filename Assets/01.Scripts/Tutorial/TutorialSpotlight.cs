using UnityEngine;
using UnityEngine.UI;

public class TutorialSpotlight : MonoBehaviour
{
    [SerializeField] RectTransform area;
    [SerializeField] RectTransform top;
    [SerializeField] RectTransform bottom;
    [SerializeField] RectTransform left;
    [SerializeField] RectTransform right;
    [SerializeField] Image frame;
    [SerializeField] float padding = 12f;

    private readonly Vector3[] corners = new Vector3[4];
    private RectTransform target;
    private Camera targetCamera;
    private Camera areaCamera;

    private void Awake()
    {
        areaCamera = GetCamera(area);
        Hide();
    }

    public void Focus(RectTransform focus, bool allowClick)
    {
        target = focus;
        targetCamera = GetCamera(focus);
        frame.raycastTarget = !allowClick;
        area.gameObject.SetActive(true);
        Refresh();
    }

    public void DimAll()
    {
        target = null;
        area.gameObject.SetActive(true);
        frame.gameObject.SetActive(false);
        Layout(Rect.zero);
    }

    public void Hide()
    {
        target = null;
        area.gameObject.SetActive(false);
    }

    private void LateUpdate()
    {
        if(target == null)
            return;

        Refresh();
    }

    private void Refresh()
    {
        bool visible = target.gameObject.activeInHierarchy;
        frame.gameObject.SetActive(visible);

        if(!visible)
        {
            Layout(Rect.zero);
            return;
        }

        Layout(GetHole());
    }

    private Rect GetHole()
    {
        target.GetWorldCorners(corners);
        Vector2 min = ToAreaPoint(corners[0]);
        Vector2 max = ToAreaPoint(corners[2]);
        return Rect.MinMaxRect(min.x - padding, min.y - padding, max.x + padding, max.y + padding);
    }

    private Vector2 ToAreaPoint(Vector3 world)
    {
        Vector2 screen = RectTransformUtility.WorldToScreenPoint(targetCamera, world);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(area, screen, areaCamera, out Vector2 local);
        return local - area.rect.min;
    }

    private void Layout(Rect hole)
    {
        Vector2 size = area.rect.size;
        Place(bottom, 0f, 0f, size.x, hole.yMin);
        Place(top, 0f, hole.yMax, size.x, size.y);
        Place(left, 0f, hole.yMin, hole.xMin, hole.yMax);
        Place(right, hole.xMax, hole.yMin, size.x, hole.yMax);
        Place(frame.rectTransform, hole.xMin, hole.yMin, hole.xMax, hole.yMax);
    }

    private static void Place(RectTransform rect, float xMin, float yMin, float xMax, float yMax)
    {
        rect.anchoredPosition = new Vector2(xMin, yMin);
        rect.sizeDelta = new Vector2(Mathf.Max(0f, xMax - xMin), Mathf.Max(0f, yMax - yMin));
    }

    private static Camera GetCamera(Component target)
    {
        Canvas canvas = target.GetComponentInParent<Canvas>(true);

        if(canvas == null)
            return null;

        canvas = canvas.rootCanvas;
        return canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
    }
}