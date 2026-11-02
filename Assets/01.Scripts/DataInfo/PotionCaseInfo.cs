using System.Text;
using UnityEngine;

public class PotionCaseInfo : ContainerInfo
{
    public int Slots { get; }

    public PotionCaseInfo(int id, string name, string iconName, ItemShape shape, ItemGrade grade, int sellPrice, string desc, int slots) : base(id, name, iconName, shape, grade, sellPrice, desc, new Vector2Int(slots, 2))
    {
        Slots = slots;
    }

    public override ItemInstance CreateInstance()
    {
        return new PotionCaseInstance(this);
    }

    public override void AppendTooltip(StringBuilder sb)
    {
        sb.AppendLine($"물약 {Slots}병 보관 · 클릭해서 열기 · 칸을 눌러 꺼내기");
        sb.Append($"판매금액 : {SellPrice}G");
    }
}