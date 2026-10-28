using System;

[Serializable]
public class BlessingGradeOdds
{
    public float normal = 70f;
    public float rare = 25f;
    public float epic = 5f;
    public float unique = 1.3f;
    public float legendary = 0.2f;
    public float Get(ItemGrade grade)
    {
        switch(grade)
        {
            case ItemGrade.Rare:
                return rare;
            case ItemGrade.Epic:
                return epic;
            case ItemGrade.Normal:
                return normal;
            case ItemGrade.Unique:
                return unique;
            case ItemGrade.Legendary:
                return legendary;
            default:
                return 0f;
        }
    }
}