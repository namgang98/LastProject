public class NpcFavor
{
    private readonly UpgradeService upgrades;

    public NpcFavor(UpgradeService upgrades)
    {
        this.upgrades = upgrades;
    }

    public bool TryGet(NpcType npc, out UpgradeProgress progress)
    {
        progress = null;

        if(!npc.TryGetFavorTrack(out UpgradeTrack track))
            return false;

        return upgrades.TryGet(track, out progress);
    }

    public int GetLevel(NpcType npc)
    {
        if(!TryGet(npc, out UpgradeProgress progress))
            return 0;

        return progress.Level;
    }

    public float GetRate(NpcType npc)
    {
        if(!TryGet(npc, out UpgradeProgress progress))
            return 0f;

        return progress.Current.Value / 100f;
    }

    public int AddFavor(NpcType npc, int amount)
    {
        if(amount <= 0 || !TryGet(npc, out UpgradeProgress progress))
            return 0;

        int startLevel = progress.Level;

        while(amount > 0 && !progress.IsMaxed)
        {
            amount -= progress.Add(amount);
        }

        return progress.Level - startLevel;
    }
}
