using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ItemShape
{
    [SerializeField] private List<Vector2Int> cells = new() { Vector2Int.zero };

    public IReadOnlyList<Vector2Int> Cells => cells;
    public ItemShape(){}
    public ItemShape(params Vector2Int[] shapeCells)
    {
        cells = new List<Vector2Int>(shapeCells);
    }

    // 1칸짜리 모양 (안전 주머니 등)
    public static ItemShape Single()
    {
        return new ItemShape(Vector2Int.zero);
    }

    // rotation : 0 = 0도, 1 = 90도, 2 = 180도, 3 = 270도 (시계 방향)
    public List<Vector2Int> GetRotatedCells(int rotation)
    {
        int turn = ((rotation % 4) + 4) % 4;
        var result = new List<Vector2Int>(cells.Count);

        foreach(var cell in cells)
        {
            Vector2Int rotated = cell;

            for(int i = 0; i < turn; i++)
            {
                rotated = new Vector2Int(rotated.y, -rotated.x);
            }

            result.Add(rotated);
        }

        Normalize(result);
        return result;
    }

    // 회전하면 좌표가 음수가 되므로 가장 작은 x, y를 0으로 당겨준다
    private static void Normalize(List<Vector2Int> list)
    {
        if(list.Count == 0)
            return;

        int minX = int.MaxValue;
        int minY = int.MaxValue;

        foreach(var c in list)
        {
            minX = Mathf.Min(minX, c.x);
            minY = Mathf.Min(minY, c.y);
        }

        for(int i = 0; i < list.Count; i++)
        {
            list[i] = new Vector2Int(list[i].x - minX, list[i].y - minY);
        }
    }
    public static bool TryParse(string pattern, out ItemShape shape)
    {
        shape = null;

        if(string.IsNullOrWhiteSpace(pattern))
            return false;

        string[] rows = pattern.Trim().Split('|');
        var cells = new List<Vector2Int>();

        for(int row = 0; row < rows.Length; row++)
        {
            int y = rows.Length - 1 - row;

            for(int x = 0; x < rows[row].Length; x++)
            {
                char c = rows[row][x];

                if(c == 'X' || c == 'x')
                    cells.Add(new Vector2Int(x, y));
                else if(c != '.')
                    return false;
            }
        }

        if(cells.Count == 0)
            return false;

        shape = new ItemShape(cells.ToArray());
        return true;
    }
}
