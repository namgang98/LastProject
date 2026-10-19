using System.Collections.Generic;
using UnityEngine;

public class DropRoller
{
    private readonly LootTable equipTable;
    private readonly DropRarity rarity;
    private readonly int bossEquipRolls;

    public DropRoller(LootTable equipTable, DropRarity rarity, int bossEquipRolls)
    {
        this.equipTable = equipTable;
        this.rarity = rarity;
        this.bossEquipRolls = bossEquipRolls;
    }

    public void Roll(MonsterInfo monster, int stage, List<ItemInfo> results)
    {
        results.Clear();
        RollDrops(monster, results);
        RollEquipment(monster, stage, results);
    }

    private static void RollDrops(MonsterInfo monster, List<ItemInfo> results)
    {
        foreach(MonsterDropInfo drop in monster.Drops)
        {
            int count = drop.RollCount();

            for(int i = 0; i < count; i++)
            {
                results.Add(drop.Item);
            }
        }
    }

    private void RollEquipment(MonsterInfo monster, int stage, List<ItemInfo> results)
    {
        if(equipTable.IsEmpty)
            return;

        int rolls = monster.IsBoss ? bossEquipRolls : 1;

        for(int i = 0; i < rolls; i++)
        {
            if(Random.value >= monster.EquipChance)
                continue;

            results.Add(equipTable.Pick(rarity.Roll(stage)));
        }
    }
}