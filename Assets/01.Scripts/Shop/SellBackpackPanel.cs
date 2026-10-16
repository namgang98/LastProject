using UnityEngine;

public class SellBackpackPanel : DungeonBackpackPanel
{
    [SerializeField, Min(0f)] float topPadding = 8f;

    protected override float GetRowTop()
    {
        RectTransform area = (RectTransform)transform;
        return area.rect.height * 0.5f - topPadding;
    }
}
