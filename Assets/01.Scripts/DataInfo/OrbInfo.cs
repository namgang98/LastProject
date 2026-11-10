using System.Text;
using UnityEngine;

public enum OrbKind
{
    Element,
    Ward,
}

public class OrbInfo : ItemInfo
{
    private const float SameElementRate = 0.5f;

    public OrbKind Kind { get; }
    public DamageElement Element { get; }
    public float ElementChance { get; }
    public int ElementValue { get; }
    public float ElementDuration { get; }
    public int Cost { get; }

    public bool IsElement => Kind == OrbKind.Element;

    public OrbInfo(int id, string name, string iconName, ItemShape shape, ItemGrade grade, int sellPrice, string desc, OrbKind kind, DamageElement element, float elementChance, int elementValue, float elementDuration, int cost) : base(id, name, iconName, shape, grade, sellPrice, desc)
    {
        Kind = kind;
        Element = element;
        ElementChance = elementChance;
        ElementValue = elementValue;
        ElementDuration = elementDuration;
        Cost = cost;
    }

    public AttackData ApplyTo(AttackData attack)
    {
        if(!IsElement)
            return attack;

        if(attack.Element != Element)
            return attack.WithElement(Element, ElementChance, ElementValue, ElementDuration);

        float chance = Mathf.Min(1f, attack.ElementChance + ElementChance * SameElementRate);
        int value = attack.ElementValue + Mathf.CeilToInt(ElementValue * SameElementRate);
        float duration = Mathf.Max(attack.ElementDuration, ElementDuration);
        return attack.WithElement(Element, chance, value, duration);
    }

    public override void AppendTooltip(StringBuilder sb)
    {
        sb.Append(IsElement ? "보주 · 대장간에서 무기에 깃들일 수 있다" : "보주 · 대장간에서 장비에 깃들이면 쓰러져도 그 장비가 한 번 남는다");
        base.AppendTooltip(sb);
    }
}
