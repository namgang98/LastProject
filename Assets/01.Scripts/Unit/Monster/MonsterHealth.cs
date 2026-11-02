using System;
using UnityEngine;

public class MonsterHealth : UnitHealth, IRevivable
{
    [SerializeField] float reviveInvulnerableTime = 1f;

    private float reviveRate;
    private bool revived;
    private float invulnerableUntil;

    public event Action OnRevived;

    protected override bool IsInvulnerable => Time.time < invulnerableUntil;

    public override void Init(int maxHP)
    {
        revived = false;
        invulnerableUntil = 0f;
        base.Init(maxHP);
    }

    public void SetRevive(float rate)
    {
        reviveRate = Mathf.Clamp01(rate);
        revived = false;
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