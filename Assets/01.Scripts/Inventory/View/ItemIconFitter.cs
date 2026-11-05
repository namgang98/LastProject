using UnityEngine;
using UnityEngine.UI;

public static class ItemIconFitter
{
    public static void Fit(Image icon, ItemInfo info, Vector2 area, int fullCells)
    {
        if(area.x <= 0f || area.y <= 0f)
            return; 

        float cellSize = Mathf.Min(area.x, area.y) / fullCells;
        Vector2 size = GetBounds(info.Shape) * cellSize;
        float scale = Mathf.Min(1f, area.x / size.x, area.y / size.y);

        icon.preserveAspect = true;
        icon.rectTransform.sizeDelta = size * scale;
    }

    public static Vector2 GetBounds(ItemShape shape)
    {
        int maxX = 0;
        int maxY = 0;

        foreach(Vector2Int cell in shape.Cells)
        {
            maxX = Mathf.Max(maxX, cell.x);
            maxY = Mathf.Max(maxY, cell.y);
        }

        return new Vector2(maxX + 1, maxY + 1);
    }
}