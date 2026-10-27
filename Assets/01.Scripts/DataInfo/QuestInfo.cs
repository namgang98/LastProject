public enum QuestType
{
    ReachFloor,
    Kill,
    KillBoss,
    Deliver,
    ForgeLevel
}

public class QuestInfo
{
    public int Id { get; }
    public string Name { get; }
    public string Description { get; }
    public QuestType Type { get; }
    public int Target { get; }
    public int Count { get; }
    public int RewardItemId { get; }
    public int RewardCount { get; }
    public int RewardGold { get; }
    public bool Repeatable { get; }
    public int PrereqId { get; }
    public NpcType Npc { get; }
    public int Favor { get; }

    public ItemInfo RewardItem { get; private set; }

    public QuestInfo(int id, string name, string description, QuestType type, int target, int count, int rewardItemId, int rewardCount, int rewardGold, bool repeatable, int prereqId, NpcType npc, int favor)
    {
        Id = id;
        Name = name;
        Description = description;
        Type = type;
        Target = target;
        Count = count;
        RewardItemId = rewardItemId;
        RewardCount = rewardCount;
        RewardGold = rewardGold;
        Repeatable = repeatable;
        PrereqId = prereqId;
        Npc = npc;
        Favor = favor;
    }

    public bool HasRewardItem => RewardItem != null && RewardCount > 0;
    public bool UsesTargetAsGoal => Type == QuestType.ReachFloor || Type == QuestType.ForgeLevel;
    public int Goal => UsesTargetAsGoal ? Target : Count;

    internal void SetRewardItem(ItemInfo item)
    {
        RewardItem = item;
    }
}