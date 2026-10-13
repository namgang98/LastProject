
public static class PlayerStatsBuilder
{
    public static PlayerStats Build(ClassInfo playerClass, Backpack backpack)
    {
        var stats = new PlayerStats();

        playerClass.Apply(stats);

        foreach(var item in backpack.EquippedItems())
        {
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