using System.Collections.Generic;
using System.Text;

public static class SetTooltipWriter
{
    private const string ActiveColor = "#9CC47A";
    private const string InactiveColor = "#7A7A7A";

    public static void Append(StringBuilder sb, ItemInfo info, HashSet<int> equipped)
    {
        foreach(SetInfo set in DataManager.instance.GetSetsOf(info.Id))
        {
            int count = SetCounter.CountEquipped(set, equipped);
            sb.AppendLine("<size=30%> </size>");
            sb.AppendLine($"<size=80%>[{set.Name}] ({count}/{set.MaxCount})</size>");
            foreach(var bonus in set.Bonuses)
            {
                string color = count >= bonus.Count ? ActiveColor : InactiveColor;
                sb.AppendLine($"<size=80%><color={color}>{bonus.Count}세트 : {bonus.Description}</color></size>");
            }
        }
    }
}