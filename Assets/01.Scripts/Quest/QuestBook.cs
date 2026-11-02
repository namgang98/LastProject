using System;
using System.Collections.Generic;
using System.Text;
public enum QuestClaimResult
{
    Ok,
    NotReady,
    NoSpace
}
public class QuestBook
{
    private readonly Dictionary<int, QuestProgress> table = new();
    private readonly List<QuestProgress> list = new();
    private readonly Func<int> bestFloor;
    private readonly Func<int, int> countItem;
    private readonly IQuestContext context;

    public IReadOnlyList<QuestProgress> All => list;
    public event Action Changed;

    public QuestBook(IEnumerable<QuestInfo> infos, List<QuestSaveData> saved, IQuestContext context)
    {
        this.context = context;

        var savedTable = new Dictionary<int, QuestSaveData>();

        foreach(QuestSaveData data in saved)
        {
            savedTable[data.id] = data;
        }

        foreach(QuestInfo info in infos)
        {
            savedTable.TryGetValue(info.Id, out QuestSaveData data);
            var progress = new QuestProgress(info, data != null ? data.progress : 0, data != null ? data.claimed : 0);
            table.Add(info.Id, progress);
            list.Add(progress);
        }

        list.Sort((a, b) => a.Info.Id.CompareTo(b.Info.Id));
    }

    public bool IsVisible(QuestProgress quest)
    {
        if(quest.IsFinished)
            return false;

        if(quest.Info.PrereqId == 0)
            return true;

        return table.TryGetValue(quest.Info.PrereqId, out QuestProgress prereq) && prereq.ClaimCount > 0;
    }

    public bool IsReady(QuestProgress quest)
    {
        switch(quest.Info.Type)
        {
            case QuestType.ReachFloor:
                return context.BestFloor >= quest.Info.Target;
            case QuestType.ForgeLevel:
                return context.MaxForgeLevel >= quest.Info.Target;
            case QuestType.Deliver:
                return context.CountItem(quest.Info.Target) >= quest.Info.Count;
            default:
                return quest.Progress >= quest.Info.Count;
        }
    }

    public int GetCurrent(QuestProgress quest)
    {
        switch(quest.Info.Type)
        {
            case QuestType.ReachFloor:
                return Math.Min(context.BestFloor, quest.Info.Target);
            case QuestType.ForgeLevel:
                return Math.Min(context.MaxForgeLevel, quest.Info.Target);
            case QuestType.Deliver:
                return Math.Min(context.CountItem(quest.Info.Target), quest.Info.Count);
            default:
                return quest.Progress;
        }
    }

    public bool HasReady(NpcType npc)
    {
        foreach(QuestProgress quest in list)
        {
            if(quest.Info.Npc == npc && IsVisible(quest) && IsReady(quest))
                return true;
        }

        return false;
    }

    public void ReportKill(MonsterInfo monster, List<QuestProgress> newlyReady)
    {
        foreach(QuestProgress quest in list)
        {
            if(!IsVisible(quest) || !Matches(quest.Info, monster))
                continue;

            if(quest.AddProgress(1))
                newlyReady.Add(quest);
        }

        Changed?.Invoke();
    }

    public void MarkClaimed(QuestProgress quest)
    {
        quest.MarkClaimed();
        Changed?.Invoke();
    }

    public void WriteTo(List<QuestSaveData> saved)
    {
        saved.Clear();

        foreach(QuestProgress quest in list)
        {
            if(quest.Progress == 0 && quest.ClaimCount == 0)
                continue;

            saved.Add(new QuestSaveData { id = quest.Info.Id, progress = quest.Progress, claimed = quest.ClaimCount });
        }
    }

    private static bool Matches(QuestInfo info, MonsterInfo monster)
    {
        if(info.Type == QuestType.Kill)
            return info.Target == 0 || info.Target == monster.Id;

        if(info.Type == QuestType.KillBoss)
            return monster.IsBoss && (info.Target == 0 || info.Target == monster.Id);

        return false;
    }
    public int CountVisible(NpcType npc)
    {
        int count = 0;

        foreach(QuestProgress quest in list)
        {
            if(quest.Info.Npc == npc && IsVisible(quest))
                count++;
        }

        return count;
    }

    public bool HasAnyReady()
    {
        foreach(QuestProgress quest in list)
        {
            if(IsVisible(quest) && IsReady(quest))
                return true;
        }

        return false;
    }

    public void AppendItemNeeds(int itemId, StringBuilder sb)
    {
        foreach(QuestProgress quest in list)
        {
            if(quest.Info.Type != QuestType.Deliver || quest.Info.Target != itemId || !IsVisible(quest))
                continue;

            sb.AppendLine();
            sb.Append($"<color=#E8C66A>퀘스트 · {quest.Info.Name} ({GetCurrent(quest)}/{quest.Info.Count})</color>");
        }
    }
}
