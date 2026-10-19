using System.Text;

public class MaterialInfo : ItemInfo
{
    public MaterialInfo(int id, string name, string iconName, ItemShape shape, ItemGrade grade, int sellPrice, string desc) : base(id, name, iconName, shape, grade, sellPrice, desc)
    {
    }

    public override void AppendTooltip(StringBuilder sb)
    {
        sb.Append("기타 아이템 · 상인에게 팔 수 있다");
        base.AppendTooltip(sb);
    }
}