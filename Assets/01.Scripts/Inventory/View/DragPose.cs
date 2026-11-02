using UnityEngine;

public readonly struct DragPose
{
    public readonly int Rotation;
    public readonly Vector2 Grab;

    public DragPose(int rotation, Vector2 grab)
    {
        Rotation = rotation;
        Grab = grab;
    }

    public static DragPose Grabbed(ItemInstance item, int rotation, Vector2 pointInCells)
    {
        Vector2Int best = Vector2Int.zero;
        float bestDistance = float.MaxValue;

        foreach(Vector2Int cell in item.Info.Shape.GetRotatedCells(rotation))
        {
            Vector2 center = (Vector2)cell + Vector2.one * 0.5f;
            float distance = (center - pointInCells).sqrMagnitude;

            if(distance >= bestDistance)
                continue;

            bestDistance = distance;
            best = cell;
        }

        return new DragPose(rotation, (Vector2)best + Vector2.one * 0.5f);
    }

    public DragPose RotateClockwise(ItemInstance item)
    {
        Vector2Int bounds = ItemView.GetBounds(item, Rotation);
        return new DragPose((Rotation + 1) % 4, new Vector2(Grab.y, bounds.x - Grab.x));
    }

    public Vector2Int GetAnchor(Vector2 pointInCells)
    {
        Vector2 origin = pointInCells - Grab;
        return new Vector2Int(Mathf.RoundToInt(origin.x), Mathf.RoundToInt(origin.y));
    }

    public Vector2 GetCenterOffset(ItemInstance item)
    {
        Vector2 half = (Vector2)ItemView.GetBounds(item, Rotation) * 0.5f;
        return half - Grab;
    }
}