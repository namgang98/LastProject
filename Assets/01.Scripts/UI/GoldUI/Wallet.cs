using System;

public class Wallet
{
    public int Gold { get; private set; }
    public int RunGold { get; private set; }

    public event Action<Wallet> Changed;
    public Wallet(int gold)
    {
        Gold = Math.Max(0, gold);
    }

    public void AddRunGold(int amount)
    {
        if(amount <= 0)
            return;

        RunGold += amount;
        Changed?.Invoke(this);
    }

    public void CommitRun()
    {
        if(RunGold == 0)
            return;

        Gold += RunGold;
        RunGold = 0;
        Changed?.Invoke(this);
    }

    public void LoseRun()
    {
        if(RunGold == 0)
            return;

        RunGold = 0;
        Changed?.Invoke(this);
    }

    public bool TrySpend(int amount)
    {
        if(amount <= 0 || Gold < amount)
            return false;

        Gold -= amount;
        Changed?.Invoke(this);
        return true;
    }
}
