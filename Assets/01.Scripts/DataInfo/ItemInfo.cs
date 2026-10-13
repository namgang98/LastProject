using System.Text;
using UnityEngine;
public enum ItemGrade
{
    Base, //가방 기본
    Normal,
    Rare,
    Epic,
}
public abstract class ItemInfo
{
    public int Id { get; }
    public string Name { get; }
    public string IconName { get; }
    public int SellPrice { get; }
    public ItemShape Shape { get; }
    public ItemGrade Grade { get; }

    public string Description { get; }

    public Sprite Icon { get; private set; }

    protected ItemInfo(int id, string name, string iconName, ItemShape shape, ItemGrade grade, int sellPrice, string desc)
    {
        Id = id;
        Name = name;
        IconName = iconName;
        Shape = shape;
        Grade = grade;
        SellPrice = sellPrice;
        Description = desc;
    }

    public virtual bool CanStoreInPouch => true;
    public virtual bool CanStoreInBag => true;
    public virtual bool IsLootable => false;

    public virtual ItemInstance CreateInstance()
    {
        return new ItemInstance(this);
    }
    public virtual void AppendTooltip(StringBuilder sb)
    {
        sb.AppendLine();
        sb.Append($"판매금액 : {SellPrice}G");
    }

    public void SetIcon(Sprite icon)
    {
        if(Icon != null)
            return;
        Icon = icon;
    }
}
