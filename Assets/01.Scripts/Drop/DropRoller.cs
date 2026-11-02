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

    public void Roll(MonsterInfo monster, int stage, List<ItemInfo> results, float materialRate = 1f, float equipRate = 1f)
    {
        results.Clear();
        RollDrops(monster, results, materialRate);
        RollEquipment(monster, stage, results, equipRate);
    }

    private static void RollDrops(MonsterInfo monster, List<ItemInfo> results, float materialRate)
    {
        foreach(MonsterDropInfo drop in monster.Drops)
        {
            int count = drop.RollCount(materialRate);

            for(int i = 0; i < count; i++)
            {
                results.Add(drop.Item);
            }
        }
    }

    private void RollEquipment(MonsterInfo monster, int stage, List<ItemInfo> results, float equipRate)
    {
        if(equipTable.IsEmpty)
            return;

        int rolls = monster.IsBoss ? bossEquipRolls : 1;

        for(int i = 0; i < rolls; i++)
        {
            if(Random.value >= monster.EquipChance * equipRate)
                continue;

            ItemInfo picked = equipTable.Pick(rarity.Roll(stage));

            if(picked != null)
                results.Add(picked);
        }
    }
}