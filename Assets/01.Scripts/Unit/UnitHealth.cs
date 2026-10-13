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
    public bool IsDead => isDead;

    public event Action<DamageInfo> OnHit;
    public event Action OnDie;
    public event Action OnBlocked;
    public event Action<int, int> OnHpChanged;
    public event Action<int> OnHealed;
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
    public void TakeDamage(DamageInfo info)
    {
        if(isDead || info.Amount <= 0)
            return;

        if (shieldCount > 0 && !info.IsTick)
        {
            shieldCount--;
            OnBlocked?.Invoke();
            return;
        }

        nowHP = Mathf.Max(nowHP - info.Amount, 0);
        OnHit?.Invoke(info);
        OnHpChanged?.Invoke(nowHP, maxHP);

        if(nowHP > 0)
            return;

        isDead = true;
        OnDie?.Invoke();
        Die();
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
}
