using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ToolTipView : MonoBehaviour
{
    [SerializeField] GameObject panel;
    [SerializeField] TextMeshProUGUI nameText;
    [SerializeField] TextMeshProUGUI descText;
    [SerializeField] TextMeshProUGUI infoText;
    [SerializeField] Color[] gradeColors;

    [SerializeField] RectTransform rect;
    [SerializeField] RectTransform canvasRect;

    private readonly Vector3[] corners = new Vector3[4];
    
    StringBuilder sb = new();
    private void Awake()
    {
        panel.SetActive(false);
    }

    private void OnEnable()
    {
        EventManager.instance.AddListener<OnItemHoverEnter>(HandlePlace);
        EventManager.instance.AddListener<OnItemHoverExit>(HandleHoverExit);
    }
    private void OnDisable()
    {
        if(!EventManager.HasInstance)
            return;

        EventManager.instance.RemoveListener<OnItemHoverEnter>(HandlePlace);
        EventManager.instance.RemoveListener<OnItemHoverExit>(HandleHoverExit);
    }
    private void HandlePlace(OnItemHoverEnter e)
    {
        ItemInfo info = e.Item.Info;

        nameText.text = $"{info.Name}\n등급 : {info.Grade}";
        nameText.color = gradeColors[(int)info.Grade];

        descText.text = info.Description;

        sb.Clear();
        info.AppendTooltip(sb);
        infoText.text = sb.ToString();

        panel.SetActive(true);
        Place(e.Target);
    }
    private void HandleHoverExit(OnItemHoverExit e)
    {
        panel.SetActive(false);
    }

    private void Place(RectTransform target)
    {
        LayoutRebuilder.ForceRebuildLayoutImmediate(rect);   // 텍스트 넣은 직후 크기 확정

        target.GetWorldCorners(corners);
        rect.pivot = new Vector2(0f, 1f);
        rect.position = corners[2];                          // 아이템 오른쪽 위에 툴팁 왼쪽 위를 붙임

        Rect area = canvasRect.rect;                         // 캔버스 기준 화면 범위
        Vector2 local = canvasRect.InverseTransformPoint(rect.position);
        Vector2 size = rect.rect.size;

        if(local.x + size.x > area.xMax)                     // 오른쪽 넘침 → 아이템 왼쪽으로
        {
            rect.pivot = new Vector2(1f, 1f);
            rect.position = corners[1];
            local = canvasRect.InverseTransformPoint(rect.position);
        }

        if(local.y - size.y < area.yMin)                     // 아래 넘침 → 넘친 만큼 위로
        {
            local.y = area.yMin + size.y;
            rect.position = canvasRect.TransformPoint(local);
        }
    }
}
