using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

[Serializable]
public class Entry
{
    public PouchInfo pouch;
    [Min(0)] public int weight = 1;
}

[CreateAssetMenu(menuName = "Backpack/Drop/Safe Pouch Drop Table", fileName = "SafePouchDropTable")]
public class PouchDropTable : ScriptableObject
{

    [SerializeField, Range(0f, 1f)] private float dropChance = 0.01f;
    [SerializeField] private List<Entry> entries = new();

    public float DropChance => dropChance;

    public bool TryRoll(out PouchInfo result)
    {
        result = null;

        if(entries.Count == 0)
            return false;

        if(Random.value >= dropChance)
            return false;

        result = PickByWeight();
        return result != null;
    }

    private PouchInfo PickByWeight()
    {
        int total = 0;

        foreach(var e in entries)
        {
            if(e.pouch != null)
                total += e.weight;
        }

        if(total <= 0)
            return null;

        int roll = Random.Range(0, total);

        foreach(var e in entries)
        {
            if(e.pouch == null)
                continue;

            if(roll < e.weight)
                return e.pouch;

            roll -= e.weight;
        }

        return null;
    }
}