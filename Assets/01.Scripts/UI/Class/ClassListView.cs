using System;
using System.Collections.Generic;
using UnityEngine;

public class ClassListView : MonoBehaviour
{
    [SerializeField] ClassCardView cardPrefab;
    [SerializeField] Transform cardRoot;
    [SerializeField] bool showFallen = true;

    private readonly List<ClassCardView> cards = new();
    private ClassCardView selected;

    public ClassInfo Selected => selected != null ? selected.Info : null;
    public event Action<ClassInfo> SelectionChanged;

    public void Refresh(GameSession session)
    {
        BuildCards();

        foreach(ClassCardView card in cards)
        {
            ClassAvailability state = session.GetAvailability(card.Info);
            card.gameObject.SetActive(state != ClassAvailability.Fallen || showFallen);
            card.SetState(state, GetStatus(session, card.Info, state));
        }

        Select(FindSelectable(session.CurrentClass));
    }

    private void BuildCards()
    {
        if(cards.Count > 0)
            return;

        var classes = new List<ClassInfo>(DataManager.instance.Classes);
        classes.Sort((a, b) => a.Id.CompareTo(b.Id));

        foreach(ClassInfo info in classes)
        {
            ClassCardView card = Instantiate(cardPrefab, cardRoot);
            card.Bind(info);
            card.Clicked += Select;
            cards.Add(card);
        }
    }

    private static string GetStatus(GameSession session, ClassInfo info, ClassAvailability state)
    {
        switch(state)
        {
            case ClassAvailability.Fallen:
                return "석상이 되었다";
            case ClassAvailability.Locked:
                return $"{info.UnlockFloor}층 해금";
            default:
                return $"Lv.{session.Training.GetProgress(info).Level}";
        }
    }

    private void Select(ClassCardView card)
    {
        if(card != null && card.IsLocked)
            return;

        if(selected != null)
            selected.SetSelected(false);

        selected = card;

        if(selected != null)
            selected.SetSelected(true);

        SelectionChanged?.Invoke(Selected);
    }

    private ClassCardView FindSelectable(ClassInfo preferred)
    {
        ClassCardView first = null;

        foreach(ClassCardView card in cards)
        {
            if(card.IsLocked)
                continue;

            if(card.Info == preferred)
                return card;

            if(first == null)
                first = card;
        }

        return first;
    }
}