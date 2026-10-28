using System;
using UnityEngine;

public class PlayerHealth : UnitHealth, IRevivable
{
    [SerializeField] float reviveInvulnerableTime = 1f;
    [SerializeField, Range(0f, 0.9f)] float maxReduction = 0.75f;
    [SerializeField, Range(0f, 0.9f)] float maxDodge = 0.5f;

    private float damageReduction;
    private float dodgeChance;
    private float reviveRate;
    private bool revived;
    private float invulnerableUntil;

    public event Action OnRevived;

    protected override bool IsInvulnerable => Time.time < invulnerableUntil;

    public void SetDefense(float reduction, float dodge)
    {
        damageReduction = Mathf.Clamp(reduction, 0f, maxReduction);
        dodgeChance = Mathf.Clamp(dodge, 0f, maxDodge);
    }

    public void SetRevive(float rate)
    {
        reviveRate = Mathf.Clamp01(rate);
    }

    protected override bool TryDodge(DamageInfo info)
    {
        return dodgeChance > 0f && UnityEngine.Random.value < dodgeChance;
    }

    protected override int ReduceDamage(int amount)
    {
        return Mathf.RoundToInt(amount * (1f - damageReduction));
    }

    protected override bool TryCheatDeath()
    {
        if(revived || reviveRate <= 0f)
            return false;

        revived = true;
        invulnerableUntil = Time.time + reviveInvulnerableTime;
        ReviveTo(Mathf.RoundToInt(MaxHp * reviveRate));
        OnRevived?.Invoke();
        return true;
    }

    protected override void Die()
    {
    }
}