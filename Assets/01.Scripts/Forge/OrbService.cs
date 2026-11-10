using System;
using UnityEngine;

public enum SocketResult
{
    Ready,
    Success,
    NoTarget,
    NoOrb,
    NotFit,
    SameOrb,
    NoGold
}

public class OrbService
{
    private readonly Func<float> discount;

    public OrbService(Func<float> discount)
    {
        this.discount = discount;
    }

    public int GetCost(OrbInfo orb)
    {
        return Mathf.Max(0, Mathf.RoundToInt(orb.Cost * (1f - Mathf.Clamp01(discount()))));
    }

    public static bool CanSocketInto(OrbInfo orb, ItemInstance item)
    {
        if(!orb.IsElement)
            return item.CanWard;

        return item is ForgeableInstance forged && forged.CanSocket;
    }

    private static bool HasSame(OrbInfo orb, ItemInstance item)
    {
        if(!orb.IsElement)
            return item.Ward != null;

        return item is ForgeableInstance forged && forged.Orb == orb;
    }

    public SocketResult Check(ItemInstance item, OrbInfo orb, Wallet wallet, IMaterialStorage storage)
    {
        if(item == null)
            return SocketResult.NoTarget;

        if(orb == null || storage.CountItem(orb.Id) <= 0)
            return SocketResult.NoOrb;

        if(!CanSocketInto(orb, item))
            return SocketResult.NotFit;

        if(HasSame(orb, item))
            return SocketResult.SameOrb;

        if(wallet.Gold < GetCost(orb))
            return SocketResult.NoGold;

        return SocketResult.Ready;
    }

    public SocketResult TrySocket(ItemInstance item, OrbInfo orb, Wallet wallet, IMaterialStorage storage)
    {
        SocketResult check = Check(item, orb, wallet, storage);

        if(check != SocketResult.Ready)
            return check;

        if(!TryPay(wallet, GetCost(orb)))
            return SocketResult.NoGold;

        storage.TakeItems(orb.Id, 1);
        Attach(orb, item);
        return SocketResult.Success;
    }

    private static bool TryPay(Wallet wallet, int cost)
    {
        if(cost <= 0)
            return true;

        return wallet.TrySpend(cost);
    }

    private static void Attach(OrbInfo orb, ItemInstance item)
    {
        if(!orb.IsElement)
        {
            item.SetWard(orb);
            return;
        }

        ((ForgeableInstance)item).SetOrb(orb);
    }
}