using System.Collections.Generic;
using UnityEngine;

public class UpgradeService
{
    private readonly Dictionary<UpgradeTrack, UpgradeProgress> progresses = new();

    public void Register(UpgradeProgress progress)
    {
        progresses[progress.Info.Track] = progress;
    }

    public bool TryGet(UpgradeTrack track, out UpgradeProgress progress)
    {
        return progresses.TryGetValue(track, out progress);
    }

    public int Donate(UpgradeTrack track, int amount, Wallet wallet)
    {
        if(!progresses.TryGetValue(track, out UpgradeProgress progress))
            return 0;

        int use = Mathf.Min(amount, wallet.Gold, progress.Remaining);

        if(use <= 0 || !wallet.TrySpend(use))
            return 0;

        return progress.Add(use);
    }

    public List<UpgradeSaveData> ToData()
    {
        var list = new List<UpgradeSaveData>();

        foreach(var progress in progresses.Values)
        {
            list.Add(new UpgradeSaveData{track = progress.Info.Track.ToString(), level = progress.Level, deposited = progress.Deposited});
        }

        return list;
    }
}