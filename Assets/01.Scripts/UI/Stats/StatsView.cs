using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public interface IPlayerStatsSource
{
    Backpack Backpack { get; }
    RunBlessings Blessings { get; }
    PlayerStats BuildStats();
}
public class StatsView : MonoBehaviour
{
    [SerializeField] GameObject popup;
    [SerializeField] Button openButton;
    [SerializeField] Button closeButton;
    [SerializeField] RunController source;

    [Header("내용")]
    [SerializeField] TMP_Text statText;
    [SerializeField] TMP_Text weaponText;
    [SerializeField] TMP_Text blessingText;

    private static readonly WeaponType[] Types = (WeaponType[])System.Enum.GetValues(typeof(WeaponType));
    private readonly StringBuilder sb = new();
    private IPlayerStatsSource stats;

    private void Awake()
    {
        stats = source;
        openButton.onClick.AddListener(Open);
        closeButton.onClick.AddListener(Close);
    }

    public void Open()
    {
        if(stats == null)
            return;

        Refresh();
        popup.SetActive(true);
    }

    public void Close()
    {
        popup.SetActive(false);
    }

    private void Refresh()
    {
        PlayerStats current = stats.BuildStats();

        statText.text = BuildStatText(current);
        weaponText.text = BuildWeaponText(current);
        blessingText.text = BuildBlessingText();
    }

    private string BuildStatText(PlayerStats current)
    {
        sb.Clear();
        sb.AppendLine($"최대 체력  {current.MaxHp}");
        AppendPercent("공격력", current.DamageBonus, "+");
        AppendPercent("쿨타임", current.CooldownReduction, "-");
        AppendPercent("치명타 확률", current.CritChanceBonus, "+");
        AppendPercent("속성 확률", current.ElementChanceBonus, "+");

        if(current.CritMultiplierBonus != 0f)
            sb.AppendLine($"치명타 배율  +{current.CritMultiplierBonus:0.##}");

        foreach(WeaponType type in Types)
        {
            AppendPercent($"{WeaponTypeText.ToKorean(type)} 데미지", current.GetTypeDamageBonus(type), "+");
            AppendPercent($"{WeaponTypeText.ToKorean(type)} 쿨타임", current.GetTypeCooldownReduction(type), "-");
        }

        return sb.ToString();
    }

    private string BuildWeaponText(PlayerStats current)
    {
        sb.Clear();

        foreach(ItemInstance item in stats.Backpack.EquippedItems())
        {
            if(item.Info is not WeaponInfo weapon)
                continue;

            AttackData attack = item is ForgeableInstance forged ? forged.ApplyTo(weapon.Attack) : weapon.Attack;
            AttackData final = current.Apply(weapon.Type, attack);
            float average = final.Damage * (1f + final.CritChance * (final.CritMultiplier - 1f));
            float dps = final.Cooldown > 0f ? average / final.Cooldown : 0f;

            sb.AppendLine($"<b>{weapon.Name}</b>");
            sb.AppendLine($"  데미지 {final.Damage}  쿨타임 {final.Cooldown:0.##}초");
            sb.AppendLine($"  치명타 {final.CritChance * 100f:0}% (x{final.CritMultiplier:0.##})  초당 {dps:0.#}");
        }

        if(sb.Length == 0)
            sb.Append("장착한 무기가 없습니다");

        return sb.ToString();
    }

    private string BuildBlessingText()
    {
        sb.Clear();
        RunBlessings blessings = stats.Blessings;

        if(blessings == null || blessings.Stacks.Count == 0)
            return "받은 축복이 없습니다";

        foreach(var pair in blessings.Stacks)
        {
            string stack = pair.Key.MaxStack > 1 ? $" x{pair.Value}" : string.Empty;
            sb.AppendLine($"<b>{pair.Key.Name}</b>{stack}");
            sb.AppendLine($"  {pair.Key.Description}");
        }

        return sb.ToString();
    }

    private void AppendPercent(string label, float value, string sign)
    {
        if(Mathf.Approximately(value, 0f))
            return;

        float shown = sign == "-" ? -value : value;
        sb.AppendLine($"{label}  {(shown > 0f ? "+" : "")}{shown * 100f:0.#}%");
    }
}