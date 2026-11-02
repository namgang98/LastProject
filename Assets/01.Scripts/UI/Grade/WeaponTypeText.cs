public static class WeaponTypeText
{
    public static string ToKorean(WeaponType type)
    {
        switch(type)
        {
            case WeaponType.Axe:
                return "도끼";
            case WeaponType.Bow:
                return "활";
            case WeaponType.Staff:
                return "지팡이";
            case WeaponType.Dagger:
                return "단검";
            default:
                return "검";
        }
    }
}