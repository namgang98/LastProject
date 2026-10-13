using System.Collections.Generic;
using UnityEngine;

public class CooldownPresenter : MonoBehaviour
{
    [SerializeField] BattleController battle;
    [SerializeField] GridView bagView;
    [SerializeField] GridView pouchView;
    private readonly List<ItemCooldownView> bound = new();


    private void OnEnable()
    {
        battle.OnActionReady += HandleWeaponReady;
        battle.OnBattleEnded += HandleBattleEnded;
    }

    private void OnDisable()
    {
        battle.OnActionReady -= HandleWeaponReady;
        battle.OnBattleEnded -= HandleBattleEnded;
    }

    private void HandleWeaponReady(ItemInstance item, ICooldownSource source)
    {
        if(!bagView.TryGetView(item, out ItemView view) && !pouchView.TryGetView(item, out view))
            return;
        view.Cooldown.Bind(source);
        bound.Add(view.Cooldown);

    }
    private void HandleBattleEnded()
    {
        for(int i = 0; i < bound.Count; i++)
        {
            bound[i].Unbind();
        }

        bound.Clear();
    }
}
