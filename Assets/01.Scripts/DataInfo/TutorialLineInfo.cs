public enum TutorialWait
{
    Click,
    Signal
}

public class TutorialLineInfo
{
    public string Key { get; }
    public int Order { get; }
    public string Line { get; }
    public TutorialWait Wait { get; }
    public string Signal { get; }
    public string FocusId { get; }
    public bool Pause { get; }

    public bool HasFocus => !string.IsNullOrEmpty(FocusId);

    public TutorialLineInfo(string key, int order, string line, TutorialWait wait, string signal, string focusId, bool pause)
    {
        Key = key;
        Order = order;
        Line = line;
        Wait = wait;
        Signal = signal;
        FocusId = focusId;
        Pause = pause;
    }
}
