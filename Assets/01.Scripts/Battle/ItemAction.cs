using System;
using UnityEngine;
public interface ICooldownSource
{
    float Progress { get; }
}
public interface IBattleEffect
{
    void Execute();
}
public class ItemAction : ICooldownSource
{
    private readonly float windup;
    private bool windupFired;
    private readonly IBattleEffect effect;
    private readonly float cooldown;
    public UnitHealth Owner { get; }
    float timer;

    public event Action OnFired;

    public ItemAction(IBattleEffect effect, float cooldown, float windup, UnitHealth owner)
    {
        this.effect = effect;
        this.cooldown = cooldown;
        this.windup = Mathf.Clamp(windup, 0f, cooldown);
        Owner = owner;
        timer = cooldown;
    }

    public float Progress
    {
        get
        {
            if(cooldown <= 0)
                return 1f;
            
            return 1f - Mathf.Clamp01(timer / cooldown);
        }
    }
    public void Tick(float time)
    {
        timer -= time;

        if(!windupFired && timer <= windup)
        {
            windupFired = true;
            OnFired?.Invoke();
        }

        if(timer > 0f)
            return;

        effect.Execute();
        timer += cooldown;
        windupFired = false;
    }
}
