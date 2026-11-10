using System;
using System.Collections.Generic;

public class RouletteBet
{
    private readonly Dictionary<int, int> amounts = new();
    private readonly int maxTotal;

    public int Total { get; private set; }

    public event Action Changed;

    public RouletteBet(int maxTotal)
    {
        this.maxTotal = maxTotal;
    }

    public int Get(int multiplier)
    {
        amounts.TryGetValue(multiplier, out int amount);
        return amount;
    }

    public bool TryAdd(int multiplier, int amount, int available)
    {
        int next = Total + amount;

        if(amount <= 0 || next > maxTotal || next > available)
            return false;

        amounts[multiplier] = Get(multiplier) + amount;
        Total = next;
        Changed?.Invoke();
        return true;
    }

    public int GetPayout(int multiplier)
    {
        return Get(multiplier) * (multiplier + 1);
    }

    public void Clear()
    {
        amounts.Clear();
        Total = 0;
        Changed?.Invoke();
    }
}