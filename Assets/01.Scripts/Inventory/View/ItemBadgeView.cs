using UnityEngine;
using UnityEngine.UI;

public class ItemBadgeView : MonoBehaviour
{
    [SerializeField] Image orbBadge;
    [SerializeField] Image orbIcon;
    [SerializeField] Image wardBadge;
    [SerializeField] Image wardIcon;

    public void Show(ItemInstance item)
    {
        OrbInfo orb = item is ForgeableInstance forged ? forged.Orb : null;
        SetBadge(orbBadge, orbIcon, orb);
        SetBadge(wardBadge, wardIcon, item.Ward);
    }

    public void Hide()
    {
        orbBadge.gameObject.SetActive(false);
        wardBadge.gameObject.SetActive(false);
    }

    private static void SetBadge(Image badge, Image icon, OrbInfo orb)
    {
        bool has = orb != null;
        badge.gameObject.SetActive(has);

        if(!has)
            return;

        icon.sprite = orb.Icon;
    }
}