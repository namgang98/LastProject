public class QuestProgress
{
    public QuestInfo Info { get; }
    public int Progress { get; private set; }
    public int ClaimCount { get; private set; }

    public QuestProgress(QuestInfo info, int progress, int claimCount)
    {
        Info = info;
        Progress = progress;
        ClaimCount = claimCount;
    }

    public bool IsFinished => !Info.Repeatable && ClaimCount > 0;

    public bool AddProgress(int amount)
    {
        bool wasReady = Progress >= Info.Count;
        Progress = System.Math.Min(Progress + amount, Info.Count);
        return !wasReady && Progress >= Info.Count;
    }

    public void MarkClaimed()
    {
        ClaimCount++;
        Progress = 0;
    }
}