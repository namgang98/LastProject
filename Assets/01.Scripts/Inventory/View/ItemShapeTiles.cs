using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemShapeTiles : MonoBehaviour
{
    [SerializeField] Image tilePrefab;
    [SerializeField, Range(0f, 0.3f)] float gapRatio = 0.06f;
    [SerializeField, Range(0f, 1f)] float tileAlpha = 0.2f;

    private readonly List<Image> tiles = new();

    public void Show(ItemInfo info, float cellSize, Vector2 size, float angle)
    {
        RectTransform rect = (RectTransform)transform;
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = size;
        rect.localEulerAngles = new Vector3(0f, 0f, angle);

        List<Vector2Int> cells = info.Shape.GetRotatedCells(0);
        EnsureCount(cells.Count);

        Color color = GradeStyle.GetColor(info.Grade, tileAlpha);
        float gap = cellSize * gapRatio;

        for(int i = 0; i < tiles.Count; i++)
        {
            bool active = i < cells.Count;
            tiles[i].gameObject.SetActive(active);

            if(!active)
                continue;

            RectTransform tile = tiles[i].rectTransform;
            tile.anchoredPosition = (Vector2)cells[i] * cellSize + Vector2.one * (gap * 0.5f);
            tile.sizeDelta = Vector2.one * (cellSize - gap);
            tiles[i].color = color;
        }
    }

    public void Hide()
    {
        foreach(Image tile in tiles)
        {
            tile.gameObject.SetActive(false);
        }
    }

    private void EnsureCount(int count)
    {
        while(tiles.Count < count)
        {
            Image tile = Instantiate(tilePrefab, transform);
            tile.raycastTarget = false;

            RectTransform rect = tile.rectTransform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.zero;
            rect.pivot = Vector2.zero;
            tiles.Add(tile);
        }
    }
}