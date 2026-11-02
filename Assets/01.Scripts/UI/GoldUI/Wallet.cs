using System;

public class Wallet
{
    public int Gold { get; private set; }

    public event Action<Wallet> Changed;

    public Wallet(int gold)
    {
        Gold = Math.Max(0, gold);
    }

    public bool TrySpend(int amount)
    {
        if(amount <= 0 || Gold < amount)
            return false;

        Gold -= amount;
        Changed?.Invoke(this);
        return true;
    }

    public void Earn(int amount)
    {
        if(amount <= 0)
            return;

        Gold += amount;
        Changed?.Invoke(this);
    }
}