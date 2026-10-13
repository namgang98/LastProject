using System;
using System.Collections.Generic;

public class EventManager : Singleton<EventManager>
{
    private readonly Dictionary<Type, Delegate> events = new();

    public void AddListener<T>(Action<T> listener) where T : struct, IGameEvent
    {
        Type type = typeof(T);

        if(events.TryGetValue(type, out Delegate existing))
            events[type] = Delegate.Combine(existing, listener);
        else
            events[type] = listener;
    }
    public void RemoveListener<T>(Action<T> listener) where T : struct, IGameEvent
    {
        Type type = typeof(T);

        if(!events.TryGetValue(type, out Delegate existing))
            return;

        Delegate result = Delegate.Remove(existing, listener);

        if(result == null)
            events.Remove(type);
        else
            events[type] = result;
    }

    public void PostNotification<T>(T gameEvent) where T : struct, IGameEvent
    {
        Type type = typeof(T);

        if(events.TryGetValue(type, out Delegate handler))
            (handler as Action<T>)?.Invoke(gameEvent);
    }
}
