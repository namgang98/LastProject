public interface IQuestContext
{
    int BestFloor { get; }
    int MaxForgeLevel { get; }
    int CountItem(int itemId);
}