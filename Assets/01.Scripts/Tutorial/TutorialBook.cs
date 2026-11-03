using System.Collections.Generic;

public class TutorialBook
{
    private const string SkipAllKey = "skip_all";

    private readonly HashSet<string> done;

    public TutorialBook(List<string> saved)
    {
        done = new HashSet<string>(saved);
    }

    public bool IsDone(string key)
    {
        return done.Contains(SkipAllKey) || done.Contains(key);
    }

    public void MarkDone(string key)
    {
        done.Add(key);
    }

    public void SkipAll()
    {
        done.Add(SkipAllKey);
    }

    public void Reset()
    {
        done.Clear();
    }

    public void WriteTo(List<string> target)
    {
        target.Clear();
        target.AddRange(done);
    }
}