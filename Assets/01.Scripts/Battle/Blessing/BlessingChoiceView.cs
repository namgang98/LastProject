using System;
using System.Collections.Generic;
using UnityEngine;

public class BlessingChoiceView : MonoBehaviour
{
    [SerializeField] GameObject popup;
    [SerializeField] BlessingCardView[] cards;

    private Action<BlessingInfo> onChosen;

    private void Awake()
    {
        foreach(BlessingCardView card in cards)
        {
            card.Clicked += HandleClicked;
        }
    }

    public void Show(IReadOnlyList<BlessingInfo> choices, RunBlessings owned, Action<BlessingInfo> chosen)
    {
        onChosen = chosen;

        for(int i = 0; i < cards.Length; i++)
        {
            bool active = i < choices.Count;
            cards[i].gameObject.SetActive(active);

            if(!active)
                continue;

            cards[i].Bind(choices[i], owned.GetStack(choices[i]));
        }

        PopupManager.instance.Open(popup, null, false);
    }

    private void HandleClicked(BlessingInfo info)
    {
        if(onChosen == null)
            return;

        Action<BlessingInfo> callback = onChosen;
        onChosen = null;
        PopupManager.instance.Close(popup);
        callback(info);
    }
}