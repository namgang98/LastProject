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

    private readonly Vector3[] targetCorners = new Vector3[4];
    private readonly Vector3[] tipCorners = new Vector3[4];
    private Backpack backpack;
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
        e.Item.AppendStateTooltip(sb);
        if(backpack != null)
            SetTooltipWriter.Append(sb, info, SetCounter.CollectEquippedIds(backpack));
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
        LayoutRebuilder.ForceRebuildLayoutImmediate(rect);

        target.GetWorldCorners(targetCorners);
        rect.pivot = new Vector2(0f, 1f);
        rect.position = targetCorners[2];
        rect.GetWorldCorners(tipCorners);

        if(tipCorners[2].x > Screen.width)                  // 오른쪽 넘침 → 아이템 왼쪽으로
        {
            rect.pivot = new Vector2(1f, 1f);
            rect.position = targetCorners[1];
            rect.GetWorldCorners(tipCorners);
        }

        float bottom = tipCorners[0].y;

        if(bottom >= 0f)                                    // 아래 안 넘침 → 끝
            return;

        rect.position += new Vector3(0f, -bottom, 0f);      // 넘친 픽셀만큼 위로
    }
    public void Bind(Backpack backpack)
    {
        this.backpack = backpack;
    }
}
