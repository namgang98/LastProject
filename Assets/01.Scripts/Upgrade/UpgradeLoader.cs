using System.Collections.Generic;
using UnityEngine;

public static class UpgradeLoader
{
    public static UpgradeService Load(IReadOnlyList<UpgradeSaveData> saved, IEnumerable<UpgradeTrack> tracks)
    {
        var service = new UpgradeService();

        foreach(UpgradeTrack track in tracks)
        {
            if(!DataManager.instance.TryGetUpgrade(track, out UpgradeTrackInfo info))
            {
                Debug.LogWarning($"[UpgradeLoader] 강화 데이터 {track} 가 없습니다.");
                continue;
            }

            UpgradeSaveData data = Find(saved, track);
            int level = data != null ? data.level : 1;
            int deposited = data != null ? data.deposited : 0;
            service.Register(new UpgradeProgress(info, level, deposited));
        }

        return service;
    }

    private static UpgradeSaveData Find(IReadOnlyList<UpgradeSaveData> saved, UpgradeTrack track)
    {
        string key = track.ToString();

        foreach(UpgradeSaveData data in saved)
        {
            if(data.track == key)
                return data;
        }

        return null;
    }
}