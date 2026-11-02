using System.Collections.Generic;

public class StatusEffects
{
    private readonly List<IStatusEffect> list = new();

    public void Apply(IStatusEffect status)
    {
        foreach(IStatusEffect existing in list)
        {
            if(existing.Target != status.Target || existing.Element != status.Element)
                continue;

            existing.Refresh(status.Duration);
            return;
        }

        list.Add(status);
        EventManager.instance.PostNotification(new OnStatusApplied(status.Target, status));
    }
    public float GetSpeedMultiplier(UnitHealth target)
    {
        float multiplier = 1f;

        foreach(IStatusEffect status in list)
        {
            if(status.Target != target)
                continue;

            multiplier *= status.SpeedMultiplier;
        }

        return multiplier;
    }

    public void Tick(float dt)
    {
        for(int i = list.Count - 1; i >= 0; i--)
        {
            if(i >= list.Count)
                continue;

            IStatusEffect status = list[i];
            bool alive = status.Tick(dt);

            if(i >= list.Count || list[i] != status)
                return;

            if(alive)
                continue;

            list.RemoveAt(i);
            EventManager.instance.PostNotification(new OnStatusRemoved(status.Target, status.Element));
        }
    }
    public void Clear()
    {
        if(EventManager.HasInstance)
        {
            foreach(IStatusEffect status in list)
            {
                EventManager.instance.PostNotification(new OnStatusRemoved(status.Target, status.Element));
            }
        }
        list.Clear();
    }
    public bool Has(UnitHealth target, DamageElement element)
    {
        foreach(IStatusEffect status in list)
        {
            if(status.Target == target && status.Element == element)
                return true;
        }

        return false;
    }

    public bool Remove(UnitHealth target, DamageElement element)
    {
        for(int i = list.Count - 1; i >= 0; i--)
        {
            IStatusEffect status = list[i];

            if(status.Target != target || status.Element != element)
                continue;

            list.RemoveAt(i);
            EventManager.instance.PostNotification(new OnStatusRemoved(target, element));
            return true;
        }

        return false;
    }
}
