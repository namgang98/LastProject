using UnityEngine;

public static class GradeStyle
{
    private static readonly Color NormalColor = new(0.85f, 0.85f, 0.85f, 1f);
    private static readonly Color RareColor = new(0.35f, 0.6f, 1f, 1f);
    private static readonly Color EpicColor = new(0.75f, 0.4f, 1f, 1f);
    private static readonly Color UniqueColor = new(1f, 0.65f, 0.15f, 1f);
    private static readonly Color LegendaryColor = new(1f, 0.25f, 0.25f, 1f);

    public static Color GetColor(ItemGrade grade)
    {
        switch(grade)
        {
            case ItemGrade.Rare:
                return RareColor;
            case ItemGrade.Epic:
                return EpicColor;
            case ItemGrade.Unique:
                return UniqueColor;
            case ItemGrade.Legendary:
                return LegendaryColor;
            default:
                return NormalColor;
        }
    }

    public static Color GetColor(ItemGrade grade, float alpha)
    {
        Color color = GetColor(grade);
        color.a = alpha;
        return color;
    }

    public static string ToKorean(ItemGrade grade)
    {
        switch(grade)
        {
            case ItemGrade.Rare:
                return "레어";
            case ItemGrade.Epic:
                return "에픽";
            case ItemGrade.Unique:
                return "유니크";
            case ItemGrade.Legendary:
                return "레전더리";
            default:
                return "노말";
        }
    }
}