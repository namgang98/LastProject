using UnityEngine;

public class MonsterDropInfo
{
    public int MonsterId { get; }
    public int ItemId { get; }
    public float Chance { get; }
    public int Min { get; }
    public int Max { get; }
    public ItemInfo Item { get; private set; }

    public MonsterDropInfo(int monsterId, int itemId, float chance, int min, int max)
    {
        MonsterId = monsterId;
        ItemId = itemId;
        Chance = chance;
        Min = min;
        Max = max;
    }

    public void SetItem(ItemInfo item)
    {
        if(Item != null)
            return;

        Item = item;
    }

    public int RollCount()
    {
        if(Random.value >= Chance)
            return 0;

        return Random.Range(Min, Max + 1);
    }
}