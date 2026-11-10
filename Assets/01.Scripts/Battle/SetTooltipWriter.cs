using System.Collections.Generic;
using System.Text;

public static class SetTooltipWriter
{
    private const string ActiveColor = "#9CC47A";
    private const string InactiveColor = "#7A7A7A";
    private const string BonusColor = "#C9A0FF";

    public static void Append(StringBuilder sb, ItemInfo info, HashSet<int> equipped, int extra = 0)
    {
        foreach(SetInfo set in DataManager.instance.GetSetsOf(info.Id))
        {
            int equippedCount = SetCounter.CountEquipped(set, equipped);
            int bonus = equippedCount > 0 ? extra : 0;
            int count = equippedCount + bonus;
            string countText = bonus > 0 ? $"{equippedCount}<color={BonusColor}>+{bonus}</color>" : $"{equippedCount}";

            sb.AppendLine("<size=30%> </size>");
            sb.AppendLine($"<size=80%>[{set.Name}] ({countText}/{set.MaxCount})</size>");

            foreach(var bonusInfo in set.Bonuses)
            {
                string color = count >= bonusInfo.Count ? ActiveColor : InactiveColor;
                sb.AppendLine($"<size=80%><color={color}>{bonusInfo.Count}세트 : {bonusInfo.Description}</color></size>");
            }
        }
    }
}