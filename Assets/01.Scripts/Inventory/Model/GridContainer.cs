using System;
using System.Collections.Generic;
using UnityEngine;

public class GridContainer
{
    private static readonly Vector2Int[] Dirs = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

    private readonly Dictionary<Vector2Int, ItemInstance> occupied = new();
    private readonly List<ItemInstance> items = new();
    private readonly Func<ItemInstance, bool> acceptRule;

    public Vector2Int Size { get; }
    public IReadOnlyList<ItemInstance> Items => items;

    public event Action<GridContainer> Changed;

    public GridContainer(Vector2Int size, Func<ItemInstance, bool> acceptRule = null)
    {
        Size = size;
        this.acceptRule = acceptRule;
    }

    #region 조회
    public bool IsInGrid(Vector2Int cell)
    {
        return cell.x >= 0 && cell.x < Size.x && cell.y >= 0 && cell.y < Size.y;
    }

    public ItemInstance GetItemAt(Vector2Int cell)
    {
        return occupied.TryGetValue(cell, out var item) ? item : null;
    }

    public bool Contains(ItemInstance item)
    {
        return item != null && item.Owner == this;
    }

    public bool CanPlace(ItemInstance item, Vector2Int anchor, int rotation)
    {
        if(item == null)
            return false;

        if(acceptRule != null && !acceptRule(item))
            return false;

        foreach(var cell in item.GetCellsAt(anchor, rotation))
        {
            if(!IsInGrid(cell))
                return false;

            if(occupied.TryGetValue(cell, out var other) && other != item)
                return false;
        }
        
        return true;
    }
    #endregion

    #region 배치 / 제거
    public bool TryPlace(ItemInstance item, Vector2Int anchor, int rotation)
    {
        if(!CanPlace(item, anchor, rotation))
            return false;

        if(item.Owner == this)
            Unregister(item);

        else
            item.Owner?.Remove(item);

        Register(item, anchor, rotation);
        Changed?.Invoke(this);
        return true;
    }

    public bool CanAutoPlace(ItemInstance item)
    {
        return TryFindSpot(item, out _, out _);
    }

    public bool TryAutoPlace(ItemInstance item)
    {
        if(!TryFindSpot(item, out Vector2Int anchor, out int rotation))
            return false;

        return TryPlace(item, anchor, rotation);
    }

    private bool TryFindSpot(ItemInstance item, out Vector2Int anchor, out int rotation)
    {
        for(rotation = 0; rotation < 4; rotation++)
        {
            for(int y = 0; y < Size.y; y++)
            {
                for(int x = 0; x < Size.x; x++)
                {
                    anchor = new Vector2Int(x, y);

                    if(CanPlace(item, anchor, rotation))
                        return true;
                }
            }
        }

        anchor = default;
        rotation = 0;
        return false;
    }

    public bool Remove(ItemInstance item)
    {
        if(!Contains(item))
            return false;

        Unregister(item);
        item.ClearPlacement();
        Changed?.Invoke(this);
        return true;
    }
    public List<ItemInstance> TakeAll()
    {
        var result = new List<ItemInstance>(items);

        foreach(var item in result)
        {
            item.ClearPlacement();
        }

        items.Clear();
        occupied.Clear();
        Changed?.Invoke(this);
        return result;
    }

    private void Register(ItemInstance item, Vector2Int anchor, int rotation)
    {
        item.SetPlacement(this, anchor, rotation);

        foreach(var c in item.GetOccupiedCells())
        {
            occupied[c] = item;
        }

        items.Add(item);
    }

    private void Unregister(ItemInstance item)
    {
        foreach(var cell in item.GetOccupiedCells())
        {
            occupied.Remove(cell);
        }

        items.Remove(item);
    }
    #endregion

    #region 인접 (시너지 / 세트 효과 계산용)
    public HashSet<ItemInstance> GetNeighbors(ItemInstance item)
    {
        var result = new HashSet<ItemInstance>();

        if(!Contains(item))
            return result;

        foreach(var cell in item.GetOccupiedCells())
        {
            foreach(var dir in Dirs)
            {
                var other = GetItemAt(cell + dir);

                if(other != null && other != item)
                    result.Add(other);
            }
        }

        return result;
    }
    #endregion
}