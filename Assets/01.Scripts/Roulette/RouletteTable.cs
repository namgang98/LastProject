using System.Collections.Generic;
using UnityEngine;

public class RouletteTable
{
    private readonly IReadOnlyList<int> slots;

    public int SlotCount => slots.Count;

    public RouletteTable(IReadOnlyList<int> slots)
    {
        this.slots = slots;
    }

    public int GetMultiplier(int index)
    {
        return slots[index];
    }

    public int Spin()
    {
        return Random.Range(0, slots.Count);
    }
}