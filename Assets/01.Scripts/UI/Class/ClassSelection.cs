using System.Collections.Generic;

public class ClassSelection
{
    private readonly ClassTraining training;
    private readonly TowerProgress towers;

    public ClassInfo Current { get; private set; }
    public bool HasPlayable => GetAvailability(Current) == ClassAvailability.Available;
    public ClassSelection(ClassTraining training, TowerProgress towers, ClassInfo saved, ClassInfo fallback)
    {
        this.training = training;
        this.towers = towers;
        Current = saved;

        if(!training.IsUnlocked(saved) && fallback != null)
            Current = fallback;

        EnsurePlayable();
    }

    public ClassAvailability GetAvailability(ClassInfo info)
    {
        if(towers.IsFallen(info))
            return ClassAvailability.Fallen;

        if(!training.IsUnlocked(info))
            return ClassAvailability.Locked;

        return ClassAvailability.Available;
    }

    public bool Select(ClassInfo info)
    {
        if(info == null || GetAvailability(info) != ClassAvailability.Available)
            return false;

        Current = info;
        return true;
    }

    public void EnsurePlayable()
    {
        if(GetAvailability(Current) == ClassAvailability.Available)
            return;

        var classes = new List<ClassInfo>(DataManager.instance.Classes);
        classes.Sort((a, b) => a.Id.CompareTo(b.Id));

        foreach(ClassInfo info in classes)
        {
            if(Select(info))
                return;
        }
    }
    public ClassInfo FindNewcomer()
    {
        ClassInfo newcomer = null;

        foreach(ClassInfo info in DataManager.instance.Classes)
        {
            if(GetAvailability(info) != ClassAvailability.Locked)
                continue;

            if(newcomer == null || info.Id < newcomer.Id)
                newcomer = info;
        }

        return newcomer;
    }
}