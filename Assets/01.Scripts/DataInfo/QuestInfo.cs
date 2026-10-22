public enum QuestType
{
    ReachFloor,
    Kill,
    KillBoss,
    Deliver
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

    public ItemInfo RewardItem { get; private set; }

    public QuestInfo(int id, string name, string description, QuestType type, int target, int count, int rewardItemId, int rewardCount, int rewardGold, bool repeatable, int prereqId)
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
    }

    public bool HasRewardItem => RewardItem != null && RewardCount > 0;

    internal void SetRewardItem(ItemInfo item)
    {
        RewardItem = item;
    }
}