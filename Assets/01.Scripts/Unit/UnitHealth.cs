using System;
using UnityEngine;

public interface IHealable
{
    void Heal(int value);
}
public abstract class UnitHealth : MonoBehaviour, IHealable
{
    protected int maxHP;
    protected int nowHP;
    private const int MaxShield = 3;
    private int shieldCount;
    public int CurrentHP => nowHP;
    public int MaxHp => maxHP;


    private bool isDead;
    protected virtual bool IsInvulnerable => false;
    public bool IsDead => isDead;

    public event Action<DamageInfo> OnHit;
    public event Action OnDie;
    public event Action OnBlocked;
    public event Action<int, int> OnHpChanged;
    public event Action<int> OnHealed;
    public event Action OnDodged;

    public virtual void Init(int maxHP)
    {
        this.maxHP = maxHP;
        nowHP = maxHP;
        isDead = false;
        OnHpChanged?.Invoke(nowHP,maxHP);
    }
    public void SetMaxHp(int max)
    {
        if(isDead || max <= 0)
            return;

        maxHP = max;
        nowHP = Mathf.Min(nowHP, maxHP);
        OnHpChanged?.Invoke(nowHP, maxHP);
    }
    public int TakeDamage(DamageInfo info)
    {
        if(isDead || info.Amount <= 0 || IsInvulnerable)
            return 0;

        if(!info.IsTick && TryDodge(info))
        {
            OnDodged?.Invoke();
            return 0;
        }

        if(shieldCount > 0 && !info.IsTick)
        {
            shieldCount--;
            OnBlocked?.Invoke();
            return 0;
        }

        int amount = Mathf.Max(1, ReduceDamage(info.Amount));
        int before = nowHP;
        nowHP = Mathf.Max(nowHP - amount, 0);
        int dealt = before - nowHP;

        OnHit?.Invoke(new DamageInfo(amount, info.IsCritical, info.Element, info.IsTick));
        OnHpChanged?.Invoke(nowHP, maxHP);

        if(nowHP > 0)
            return dealt;

        if(TryCheatDeath())
            return dealt;

        isDead = true;
        OnDie?.Invoke();
        Die();
        return dealt;
    }

    public void Heal(int value)
    {
        if(isDead || IsFullHP() || value <= 0)
            return;

        int before = nowHP;
        nowHP = Mathf.Min(nowHP + value, MaxHp);
        int healed = nowHP - before;

        if(healed <= 0)
            return;

        OnHpChanged?.Invoke(nowHP, maxHP);
        OnHealed?.Invoke(healed);
    }
    protected virtual bool TryDodge(DamageInfo info)
    {
        return false;
    }

    protected virtual int ReduceDamage(int amount)
    {
        return amount;
    }

    public bool IsFullHP()
    {
        return nowHP >= maxHP;
    }
    protected abstract void Die();
    public void AddShield(int count)
    {
        if(isDead || count <= 0)
            return;

        shieldCount = Mathf.Min(shieldCount + count, MaxShield);
    }

    public void ClearShield()
    {
        shieldCount = 0;
    }
    protected virtual bool TryCheatDeath()
    {
        return false;
    }

    protected void ReviveTo(int hp)
    {
        nowHP = Mathf.Clamp(hp, 1, maxHP);
        OnHpChanged?.Invoke(nowHP, maxHP);
    }
}
