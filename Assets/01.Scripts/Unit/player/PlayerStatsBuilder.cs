
public static class PlayerStatsBuilder
{
    public static PlayerStats Build(ClassInfo playerClass, Backpack backpack, params IStatModifier[] bonuses)
    {
        var stats = new PlayerStats();
        playerClass.Apply(stats);
        int extraSet = 0;

        foreach(IStatModifier bonus in bonuses)
        {
            if(bonus == null)
                continue;

            bonus.Apply(stats);

            if(bonus is ISetBonusModifier setModifier)
                extraSet += setModifier.ExtraSetCount;
        }

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
                if(pair.Value + extraSet < bonus.Count)
                    break;

                bonus.Apply(stats);
            }
        }
        return stats;
    }

}