public static class ElementText
{
    public static string Name(DamageElement element)
    {
        switch(element)
        {
            case DamageElement.Poison:
                return "독";
            case DamageElement.Fire:
                return "화염";
            case DamageElement.Ice:
                return "빙결";
            default:
                return "없음";
        }
    }

    public static string Describe(AttackData attack)
    {
        if(attack.Element == DamageElement.None)
            return "속성 없음";

        string effect = attack.Element == DamageElement.Ice ? $"감속 {attack.ElementValue}%" : $"위력 {attack.ElementValue}";
        return $"{Name(attack.Element)} {attack.ElementChance * 100f:0}% · {effect} · {attack.ElementDuration:0.#}초";
    }
}