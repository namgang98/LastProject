using System;
using System.Collections.Generic;
using UnityEngine;

public class LootArea
{
    public class Entry
    {
        public ItemInstance Item { get; }
        public Vector2 Position { get; internal set; }  // 영역 왼쪽 아래 기준, 아이템 중심 좌표
        public float Tilt { get; internal set; }         // 바닥에 떨어진 연출용 기울기 (도)

        public Entry(ItemInstance item, Vector2 position, float tilt)
        {
            Item = item;
            Position = position;
            Tilt = tilt;
        }
    }

    private readonly Dictionary<ItemInstance, Entry> entries = new();

    public IReadOnlyCollection<Entry> Entries => entries.Values;
    public int Count => entries.Count;

    public event Action<LootArea> Changed;

    public bool Contains(ItemInstance item)
    {
        return item != null && entries.ContainsKey(item);
    }

    public bool TryGetEntry(ItemInstance item, out Entry entry)
    {
        return entries.TryGetValue(item, out entry);
    }

    public void Add(ItemInstance item, Vector2 position, float tilt)
    {
        if(item == null)
            return;

        if(entries.TryGetValue(item, out Entry entry))
        {
            entry.Position = position;
            entry.Tilt = tilt;
        }
        else
        {
            entries[item] = new Entry(item, position, tilt);
        }

        Changed?.Invoke(this);
    }

    public bool Remove(ItemInstance item)
    {
        if(!Contains(item))
            return false;

        entries.Remove(item);
        Changed?.Invoke(this);
        return true;
    }

    public List<ItemInstance> Clear()
    {
        var removed = new List<ItemInstance>(entries.Keys);

        entries.Clear();
        Changed?.Invoke(this);
        return removed;
    }
}