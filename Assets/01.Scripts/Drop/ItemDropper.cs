using System.Collections.Generic;
using UnityEngine;

public class ItemDropper : MonoBehaviour
{
    [SerializeField] int bossEquipRolls = 2;

    [Header("장비 등급 확률")]
    [SerializeField] float normalWeight = 70f;
    [SerializeField] float rareWeight = 25f;
    [SerializeField] float epicWeight = 5f;
    [SerializeField] float rarePerStage = 1f;
    [SerializeField] float epicPerStage = 0.5f;

    private readonly List<ItemInfo> results = new();
    private DropRoller roller;
    private int currentStage = 1;

    private void OnEnable()
    {
        EventManager.instance.AddListener<OnMonsterKilled>(HandleMonsterKilled);
        EventManager.instance.AddListener<OnStageStarted>(HandleStageStarted);
    }

    private void OnDisable()
    {
        if(!EventManager.HasInstance)
            return;

        EventManager.instance.RemoveListener<OnMonsterKilled>(HandleMonsterKilled);
        EventManager.instance.RemoveListener<OnStageStarted>(HandleStageStarted);
    }

    private void HandleMonsterKilled(OnMonsterKilled e)
    {
        if(roller == null)
            roller = CreateRoller();

        roller.Roll(e.Monster, currentStage, results);

        foreach(ItemInfo info in results)
        {
            EventManager.instance.PostNotification(new OnItemDropped(info.CreateInstance(), e.Position));
        }
    }

    private DropRoller CreateRoller()
    {
        var table = new LootTable(DataManager.instance.Items);
        var rarity = new DropRarity(normalWeight, rareWeight, epicWeight, rarePerStage, epicPerStage);
        return new DropRoller(table, rarity, bossEquipRolls);
    }

    private void HandleStageStarted(OnStageStarted e)
    {
        currentStage = e.Stage;
    }
}