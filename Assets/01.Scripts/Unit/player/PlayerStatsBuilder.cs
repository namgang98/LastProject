
public static class PlayerStatsBuilder
{
    public static PlayerStats Build(ClassInfo playerClass, Backpack backpack, IStatModifier training)
    {
        var stats = new PlayerStats();
        playerClass.Apply(stats);
        training.Apply(stats);

        foreach(var item in backpack.EquippedItems())
        {
            if(item is ForgeableInstance forged)
                forged.ApplyBonus(stats);

            if(item.Info is not IStatModifier modifier)
                continue;

            modifier.Apply(stats);
        }

        foreach(var pair in SetCounter.CountActiveSets(backpack))
        {
            foreach(var bonus in pair.Key.Bonuses)
            {
                if(pair.Value < bonus.Count)
                    break;

                bonus.Apply(stats);
            }
        }
        return stats;
    }

}