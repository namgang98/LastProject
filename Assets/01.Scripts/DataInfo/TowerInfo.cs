public class TowerInfo
{
    public int Id { get; }
    public string Name { get; }
    public string Description { get; }
    public int TopFloor { get; }
    public int AbyssFloor { get; }
    public bool IsPrologue { get; }

    public bool HasAbyss => AbyssFloor > TopFloor;
    public int LastFloor => HasAbyss ? AbyssFloor : TopFloor;

    public TowerInfo(int id, string name, string description, int topFloor, int abyssFloor, bool isPrologue)
    {
        Id = id;
        Name = name;
        Description = description;
        TopFloor = topFloor;
        AbyssFloor = abyssFloor;
        IsPrologue = isPrologue;
    }
}
