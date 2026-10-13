using System.Collections.Generic;

public static class SetCounter
{
    public static HashSet<int> CollectEquippedIds(Backpack backpack)
    {
        var equipped = new HashSet<int> { backpack.Bag.Info.Id };

        foreach(var item in backpack.EquippedItems())
        {
            equipped.Add(item.Info.Id);
        }

        return equipped;
    }

    public static int CountEquipped(SetInfo set, HashSet<int> equipped)
    {
        int count = 0;

        foreach(int memberId in set.Members)
        {
            if(equipped.Contains(memberId))
                count++;
        }

        return count;
    }
    public static Dictionary<SetInfo, int> CountActiveSets(Backpack backpack)
    {
        HashSet<int> equipped = CollectEquippedIds(backpack);
        Dictionary<SetInfo, int> result = new();
        foreach(int id in equipped)
        {
            foreach(SetInfo set in DataManager.instance.GetSetsOf(id))
            {
                if(result.ContainsKey(set))
                    continue;

                result.Add(set, CountEquipped(set, equipped));
            }
        }
        return result;
    }
}