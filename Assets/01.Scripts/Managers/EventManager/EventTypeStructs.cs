using UnityEngine;

public interface IGameEvent { }

public struct OnChangeGold : IGameEvent
{
    public int Gold;
    public int RunGold;
    public OnChangeGold(int gold, int runGold)
    {
        Gold = gold;
        RunGold = runGold;
    }
}

public struct OnMonsterKilled : IGameEvent
{
    public Vector3 Position;
    public MonsterInfo Monster;
    public OnMonsterKilled(Vector3 position, MonsterInfo info)
    { 
        Position = position;
        Monster = info;
    }
}

public struct OnItemDropped : IGameEvent
{
    public ItemInstance Item;
    public Vector3 Position;

    public OnItemDropped(ItemInstance item, Vector3 position)
    {
        Item = item;
        Position = position;
    }
}

public struct OnPouchChanged : IGameEvent
{
    public Backpack Backpack;
    public PouchInstance Pouch;

    public OnPouchChanged(Backpack backpack, PouchInstance pouch)
    {
        Backpack = backpack;
        Pouch = pouch;
    }
}

public struct OnStageStarted : IGameEvent
{
    public int Stage;
    public bool IsRest;
    public OnStageStarted(int stage, bool isRest)
    {
        Stage = stage;
        IsRest = isRest;
    }
}

public enum RunEndReason
{
    Death,
    Return,
}
public struct OnRunEnded : IGameEvent
{

    public int Stage;
    public RunEndReason Reason;

    public OnRunEnded(int stage, RunEndReason reason)
    {
        Stage = stage;
        Reason = reason;
    }
}

public struct OnUnitDamaged : IGameEvent
{
    public Vector3 Position;
    public DamageInfo Info;
    public int SourceId;
    public OnUnitDamaged(Vector3 pos, DamageInfo info, int sourceId)
    {
        Position = pos;
        Info = info;
        SourceId = sourceId;
    }
}

public struct OnUnitHealed : IGameEvent
{
    public Vector3 Position;
    public int Value;

   public OnUnitHealed(Vector3 pos, int value)
    {
        Position = pos;
        Value = value;
    }
}

public struct OnUnitBlocked : IGameEvent
{
    public Vector3 Position;

    public OnUnitBlocked(Vector3 pos)
    {
        Position = pos;
    }
}

public struct OnItemHoverEnter : IGameEvent
{
    public ItemInstance Item;
    public RectTransform Target;

    public OnItemHoverEnter(ItemInstance item, RectTransform target)
    {
        Item = item;
        Target = target;
    }
}
public struct OnItemHoverExit : IGameEvent {}

public struct OnStatusApplied : IGameEvent
{
    public UnitHealth Target;
    public IStatusEffect Status;
    public OnStatusApplied(UnitHealth target, IStatusEffect status)
    {
        Target = target;
        Status = status;
    }

}
public struct OnStatusRemoved : IGameEvent
{
    public UnitHealth Target;
    public DamageElement Element;
    public OnStatusRemoved(UnitHealth target, DamageElement element)
    {
        Target = target;
        Element = element;
    }
}

public struct OnBackpackChanged : IGameEvent
{
    public Backpack Backpack;
    public OnBackpackChanged(Backpack backpack) => Backpack = backpack;
}

public struct OnChestExpanded : IGameEvent
{
    public int TabCount;
    public OnChestExpanded(int tabCount) => TabCount = tabCount;
}

public struct OnItemClicked : IGameEvent
{
    public ItemInstance Item;
    public OnItemClicked(ItemInstance item) => Item = item;
}

public enum FeedbackType
{
    Info,
    Warning,
}

public struct OnFeedback : IGameEvent
{
    public string Message;
    public FeedbackType Type;

    public OnFeedback(string message, FeedbackType type = FeedbackType.Warning)
    {
        Message = message;
        Type = type;
    }
}

public struct OnPopupCountChanged : IGameEvent
{
    public int Count;

    public OnPopupCountChanged(int count)
    {
        Count = count;
    }
}