using System;
using UnityEngine;

public enum ForgeResult
{
    Ready,
    Success,
    Failed,
    Downgraded,
    Maxed,
    NoGold,
    NoMaterial,
    NotForgeable
}

public class ForgeService
{
    private readonly ForgeTable table;
    private readonly Func<float> discount;
    private readonly Func<float> roll;

    public ForgeService(ForgeTable table, Func<float> discount, Func<float> roll = null)
    {
        this.table = table;
        this.discount = discount;
        this.roll = roll ?? (() => UnityEngine.Random.value);
    }

    public int MaxLevel => table.MaxLevel;

    public float GetBonus(int level)
    {
        return table.GetBonus(level);
    }

    public bool TryGetNext(ForgeableInstance item, out ForgeLevelInfo next)
    {
        next = null;

        if(item == null)
            return false;

        return table.TryGetLevel(item.Level + 1, out next);
    }

    public int GetCost(ForgeableInstance item, ForgeLevelInfo next)
    {
        float rate = GradeRate(item.Info.Grade) * (1f - Mathf.Clamp01(discount()));
        return Mathf.Max(1, Mathf.RoundToInt(next.Cost * rate));
    }

    public ForgeResult Check(ForgeableInstance item, Wallet wallet, IMaterialStorage storage)
    {
        if(item == null)
            return ForgeResult.NotForgeable;

        if(!TryGetNext(item, out ForgeLevelInfo next))
            return ForgeResult.Maxed;

        if(wallet.Gold < GetCost(item, next))
            return ForgeResult.NoGold;

        foreach(ItemCost cost in next.Materials)
        {
            if(storage.CountItem(cost.ItemId) < cost.Count)
                return ForgeResult.NoMaterial;
        }

        return ForgeResult.Ready;
    }

    public ForgeResult TryForge(ForgeableInstance item, Wallet wallet, IMaterialStorage storage)
    {
        ForgeResult check = Check(item, wallet, storage);

        if(check != ForgeResult.Ready)
            return check;

        TryGetNext(item, out ForgeLevelInfo next);

        if(!wallet.TrySpend(GetCost(item, next)))
            return ForgeResult.NoGold;

        foreach(ItemCost cost in next.Materials)
        {
            storage.TakeItems(cost.ItemId, cost.Count);
        }

        if(roll() < next.Chance)
        {
            item.SetLevel(next.Level);
            return ForgeResult.Success;
        }

        if(!next.Downgrade)
            return ForgeResult.Failed;

        item.SetLevel(item.Level - 1);
        return ForgeResult.Downgraded;
    }

    private static float GradeRate(ItemGrade grade)
    {
        switch(grade)
        {
            case ItemGrade.Rare:
                return 1.5f;
            case ItemGrade.Epic:
                return 2f;
            default:
                return 1f;
        }
    }
}