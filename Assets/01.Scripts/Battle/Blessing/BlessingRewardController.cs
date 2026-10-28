using System;
using System.Collections.Generic;
using UnityEngine;

public class BlessingRewardController : MonoBehaviour
{
    [Header("등급 확률")] 
    [SerializeField] BlessingGradeOdds normalOdds = new() { normal = 68f, rare = 25f, epic = 5.5f, unique = 1.3f, legendary = 0.2f };
    [SerializeField] BlessingGradeOdds bossOdds = new() { normal = 40f, rare = 38f, epic = 16f, unique = 5f, legendary = 1f };
    [SerializeField, Min(1)] int bossInterval = 10;

    [SerializeField] BlessingChoiceView view;
    [SerializeField, Min(1)] int interval = 5;
    [SerializeField, Min(1)] int choiceCount = 3;

    private readonly List<BlessingInfo> choices = new();
    private RunBlessings blessings;
    private BlessingPicker picker;
    private Action pending;

    public void Bind(RunBlessings target)
    {
        blessings = target;
        picker = new BlessingPicker(DataManager.instance.Blessings);
    }

    public bool TryOffer(int stage, Action onDone)
    {
        if(blessings == null || stage % interval != 0)
            return false;

        BlessingGradeOdds odds = stage % bossInterval == 0 ? bossOdds : normalOdds;
        picker.Pick(blessings, choiceCount, odds, choices);

        if(choices.Count == 0)
            return false;

        pending = onDone;
        view.Show(choices, blessings, HandleChosen);
        return true;
    }

    private void HandleChosen(BlessingInfo chosen)
    {
        blessings.TryAdd(chosen);

        Action done = pending;
        pending = null;
        done?.Invoke();
    }
}