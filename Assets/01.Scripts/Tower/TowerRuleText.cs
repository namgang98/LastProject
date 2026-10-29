using System.Text;

public static class TowerRuleText
{
    public static string Build(TowerInfo info, bool collapsed)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"<size=120%><b>{info.Name}</b></size>");
        sb.AppendLine($"<size=80%>{info.Description}</size>");

        if(collapsed)
        {
            sb.Append("<color=#9a9a9a>무너진 탑</color>");
            return sb.ToString();
        }

        if(info.Rules.Count == 0)
        {
            sb.Append("특수 규칙 없음");
            return sb.ToString();
        }

        foreach(TowerRuleInfo rule in info.Rules)
        {
            sb.AppendLine($"<color=#e07a5f>· {Describe(rule)}</color>");
        }

        return sb.ToString().TrimEnd();
    }

    public static string Describe(TowerRuleInfo rule)
    {
        switch(rule.Type)
        {
            case TowerRuleType.MonsterRevive:
                return $"몬스터가 처음 쓰러지면 체력 {Percent(rule.Value)}로 부활";
            case TowerRuleType.MonsterDamage:
                return $"몬스터 공격력 +{Percent(rule.Value)}";
            case TowerRuleType.MonsterLifesteal:
                return $"몬스터가 준 피해의 {Percent(rule.Value)} 흡혈";
            case TowerRuleType.MonsterRegen:
                return $"몬스터가 매초 최대 체력 {Percent(rule.Value)} 회복";
            case TowerRuleType.IceAttack:
                return $"몬스터 공격 시 {Percent(rule.Value)} 확률로 빙결 (둔화 {rule.Value2:0}%, {rule.Duration:0.#}초)";
            case TowerRuleType.PoisonAttack:
                return $"몬스터 공격 시 {Percent(rule.Value)} 확률로 중독 ({rule.Duration:0.#}초)";
            case TowerRuleType.PlayerCooldown:
                return $"내 무기 쿨타임 +{Percent(rule.Value)}";
            case TowerRuleType.FloorPoison:
                return $"층 이동 시 최대 체력 {Percent(rule.Value)} 독 피해 (10층마다 +{Percent(rule.Value2)})";
            default:
                return rule.Type.ToString();
        }
    }

    private static string Percent(float value)
    {
        return $"{value * 100f:0.#}%";
    }
}