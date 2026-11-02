using System.Text;

public enum ConsumeType
{
    Heal,
    Cure,
    Haste,
    ResetPoints,
}
public class ConsumableInfo : ItemInfo
{
    public ConsumeType Kind { get; }
    public int Value { get; }
    public float Duration { get; }

    public ConsumableInfo(int id, string name, string iconName, ItemShape shape, ItemGrade grade, int sellPrice, string desc, ConsumeType kind, int value, float duration) : base(id, name, iconName, shape, grade, sellPrice, desc)
    {
        Kind = kind;
        Value = value;
        Duration = duration;
    }

    public bool IsPotion => Kind != ConsumeType.ResetPoints;

    public override void AppendTooltip(StringBuilder sb)
    {
        sb.Append(Describe());
        base.AppendTooltip(sb);
    }

    private string Describe()
    {
        switch(Kind)
        {
            case ConsumeType.Heal:
                return $"위험할 때 자동 사용\n최대 체력의 {Value}% 회복";
            case ConsumeType.Cure:
                return "중독되면 자동 사용\n독 해제";
            case ConsumeType.Haste:
                return $"보스전 시작 시 자동 사용\n{Duration:0}초 동안 공격 속도 +{Value}%";
            case ConsumeType.ResetPoints:
                return "훈련장에서 사용\n스탯 포인트 초기화";
            default:
                return string.Empty;
        }
    }
}
