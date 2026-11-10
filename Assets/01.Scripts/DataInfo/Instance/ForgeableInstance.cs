using System.Text;
using UnityEngine;

public class ForgeableInstance : ItemInstance
{
    public int Level { get; private set; }
    public OrbInfo Orb { get; private set; }

    public float Bonus => DataManager.instance.Forge.GetBonus(Level);
    public bool CanSocket => Info is WeaponInfo;

    public ForgeableInstance(ItemInfo info) : base(info)
    {
    }

    internal void SetLevel(int level)
    {
        Level = Mathf.Max(0, level);
    }

    internal void SetOrb(OrbInfo orb)
    {
        if(!CanSocket)
            return;

        Orb = orb;
    }

    public AttackData ApplyTo(AttackData attack)
    {
        if(Orb != null)
            attack = Orb.ApplyTo(attack);

        if(Level <= 0)
            return attack;

        int damage = Mathf.Max(1, Mathf.RoundToInt(attack.Damage * (1f + Bonus)));
        return attack.WithDamage(damage);
    }

    public void ApplyBonus(PlayerStats stats)
    {
        if(Level <= 0 || Info is not ArmorInfo armor)
            return;

        stats.AddMaxHp(Mathf.RoundToInt(armor.MaxHp * Bonus));
    }

	public override void AppendStateTooltip(StringBuilder sb)
	{
		AppendForgeTooltip(sb);
		AppendOrbTooltip(sb);
		base.AppendStateTooltip(sb);
	}

	private void AppendForgeTooltip(StringBuilder sb)
    {
        if(Level <= 0)
            return;

        string stat = Info is ArmorInfo ? "최대 체력" : "데미지";
        sb.AppendLine();
        sb.Append($"강화 +{Level} : {stat} +{Bonus * 100f:0}%");
    }

    private void AppendOrbTooltip(StringBuilder sb)
    {
        if(Orb == null || Info is not WeaponInfo weapon)
            return;

        sb.AppendLine();
        sb.Append($"{Orb.Name} : {ElementText.Describe(Orb.ApplyTo(weapon.Attack))}");
    }
}