using System.Collections.Generic;
using UnityEngine;

public enum DisplayMode
{
    Fullscreen,
    Borderless,
    Windowed
}

public class DisplaySettings
{
    private const int MinHeight = 720;
    private const float TargetAspect = 16f / 9f;

    private static readonly FullScreenMode[] Modes =
    {
        FullScreenMode.ExclusiveFullScreen,
        FullScreenMode.FullScreenWindow,
        FullScreenMode.Windowed
    };

    private readonly List<Vector2Int> resolutions = new();
    public IReadOnlyList<Vector2Int> Resolutions => resolutions;

    public DisplaySettings()
    {
        foreach(Resolution r in Screen.resolutions)
        {
            var size = new Vector2Int(r.width, r.height);

            if(r.height < MinHeight || !IsTargetAspect(size) || resolutions.Contains(size))
                continue;

            resolutions.Add(size);
        }

        resolutions.Sort(CompareSize);

        if(resolutions.Count == 0)
            resolutions.Add(new Vector2Int(Screen.width, Screen.height));
    }

    public DisplayMode CurrentMode
    {
        get
        {
            if(Screen.fullScreenMode == FullScreenMode.ExclusiveFullScreen)
                return DisplayMode.Fullscreen;

            if(Screen.fullScreenMode == FullScreenMode.Windowed)
                return DisplayMode.Windowed;

            return DisplayMode.Borderless;
        }
    }

    public int CurrentResolutionIndex()
    {
        int best = 0;
        int bestDiff = int.MaxValue;

        for(int i = 0; i < resolutions.Count; i++)
        {
            int diff = Mathf.Abs(resolutions[i].x - Screen.width) + Mathf.Abs(resolutions[i].y - Screen.height);

            if(diff >= bestDiff)
                continue;

            best = i;
            bestDiff = diff;
        }

        return best;
    }

    public void Apply(DisplayMode mode, int resolutionIndex)
    {
        int index = mode == DisplayMode.Borderless ? resolutions.Count - 1 : resolutionIndex;
        Vector2Int size = resolutions[Mathf.Clamp(index, 0, resolutions.Count - 1)];

        Screen.SetResolution(size.x, size.y, Modes[(int)mode]);
    }

    private static bool IsTargetAspect(Vector2Int size)
    {
        return Mathf.Abs((float)size.x / size.y - TargetAspect) < 0.01f;
    }

    private static int CompareSize(Vector2Int a, Vector2Int b)
    {
        if(a.x != b.x)
            return a.x.CompareTo(b.x);

        return a.y.CompareTo(b.y);
    }
}