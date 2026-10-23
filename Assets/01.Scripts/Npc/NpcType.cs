public enum NpcType
{
    Board,
    Merchant,
    Trainer,
    Blacksmith
}

public static class NpcTypeExtensions
{
    public static string ToKorean(this NpcType npc)
    {
        switch(npc)
        {
            case NpcType.Merchant:
                return "상인";
            case NpcType.Trainer:
                return "교관";
            case NpcType.Blacksmith:
                return "대장장이";
            default:
                return "의뢰 게시판";
        }
    }

    public static bool TryGetFavorTrack(this NpcType npc, out UpgradeTrack track)
    {
        switch(npc)
        {
            case NpcType.Merchant:
                track = UpgradeTrack.FavorMerchant;
                return true;
            case NpcType.Trainer:
                track = UpgradeTrack.FavorTrainer;
                return true;
            case NpcType.Blacksmith:
                track = UpgradeTrack.FavorBlacksmith;
                return true;
            default:
                track = default;
                return false;
        }
    }
}
