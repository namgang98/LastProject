using UnityEngine;

public class FavorGate : MonoBehaviour
{
    [SerializeField] NpcType npc = NpcType.Merchant;
    [SerializeField, Min(1)] int requiredLevel = 3;
    [SerializeField] GameObject target;

    private void OnEnable()
    {
        Refresh();
    }

    public void Refresh()
    {
        target.SetActive(IsUnlocked());
    }

    private bool IsUnlocked()
    {
        if(!GameSession.HasInstance)
            return false;

        return GameSession.instance.Favor.GetLevel(npc) >= requiredLevel;
    }
}