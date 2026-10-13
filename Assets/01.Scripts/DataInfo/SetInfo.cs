using System.Collections.Generic;

public class SetInfo
{
    private readonly List<SetBonusInfo> bonuses = new();

    public int Id { get; }
    public string Name { get; }
    public IReadOnlyList<int> Members { get; }
    public IReadOnlyList<SetBonusInfo> Bonuses => bonuses;
    public string Description { get; }

    public SetInfo(int id, string name, IReadOnlyList<int> members, string desc)
    {
        Id = id;
        Name = name;
        Members = members;
        Description = desc;
    }

    public void AddBonus(SetBonusInfo bonus)
    {
        bonuses.Add(bonus);
        bonuses.Sort((a, b) => a.Count.CompareTo(b.Count));
    }

    public int MaxCount => bonuses.Count == 0 ? Members.Count : bonuses[^1].Count;
}
