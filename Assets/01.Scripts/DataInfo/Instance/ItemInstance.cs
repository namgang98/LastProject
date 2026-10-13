using System.Collections.Generic;
using UnityEngine;

// 게임 안에 실제로 존재하는 아이템 1개 (순수 C# 클래스)
// ItemInfo(설계도)는 공유하고, 위치/회전/소속 그리드는 인스턴스마다 따로 가진다
public class ItemInstance
{
    public ItemInfo Info { get; } //시트설정 실패시 데이터로돌리기
    public Vector2Int Anchor { get; private set; }
    public int Rotation { get; private set; }
    public GridContainer Owner { get; private set; }

    public bool IsPlaced => Owner != null;

    public ItemInstance(ItemInfo info)
    {
        Info = info;
    }

    // anchor 위치에 rotation 회전으로 놓았을 때 차지하는 실제 그리드 좌표
    public List<Vector2Int> GetCellsAt(Vector2Int anchor, int rotation)
    {
        List<Vector2Int> cells = Info.Shape.GetRotatedCells(rotation);

        for(int i = 0; i < cells.Count; i++)
        {
            cells[i] += anchor;
        }

        return cells;
    }

    // 현재 놓여 있는 칸들
    public List<Vector2Int> GetOccupiedCells()
    {
        return GetCellsAt(Anchor, Rotation);
    }

    // 배치 정보는 GridContainer만 바꾸도록 internal로 막아둔다
    internal void SetPlacement(GridContainer owner, Vector2Int anchor, int rotation)
    {
        Owner = owner;
        Anchor = anchor;
        Rotation = ((rotation % 4) + 4) % 4;
    }

    internal void ClearPlacement()
    {
        Owner = null;
    }
}