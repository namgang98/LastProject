using System.Collections.Generic;
using UnityEngine;

public class ItemDropper : MonoBehaviour
{
    [SerializeField] int bossEquipRolls = 2;

    [Header("장비 등급 확률 (레전더리, 유니크, 에픽, 레어, 노말)")]
    [SerializeField] float[] gradeWeights = { 0.1f, 0.8f, 5f, 25f, 70f };
    [SerializeField] float[] gradePerStage = { 0.015f, 0.08f, 0.5f, 1f, 0f };

    private IRunBonus runBonus = EmptyRunBonus.Instance;
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

        float materialRate = 1f + runBonus.Sum(BlessingStat.MaterialDrop);
        float equipRate = 1f + runBonus.Sum(BlessingStat.DropChance);
        roller.Roll(e.Monster, currentStage, results, materialRate, equipRate);

        foreach(ItemInfo info in results)
        {
            EventManager.instance.PostNotification(new OnItemDropped(info.CreateInstance(), e.Position));
        }
    }

    private DropRoller CreateRoller()
    {
        var table = new LootTable(DataManager.instance.Items);
        var rarity = new DropRarity(gradeWeights, gradePerStage);
        return new DropRoller(table, rarity, bossEquipRolls);
    }

    public void SetRunBonus(IRunBonus bonus)
    {
        runBonus = bonus ?? EmptyRunBonus.Instance;
    }
    private void HandleStageStarted(OnStageStarted e)
    {
        currentStage = e.Stage;
    }
}