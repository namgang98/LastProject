using System.Collections.Generic;
using UnityEngine;

public class ItemDropper : MonoBehaviour
{
    [SerializeField] int normalDropCount = 1;
    [SerializeField] int bossDropMultiplier = 2;

    [Header("드랍 확률")]
    [SerializeField] float normalWeight = 70f;
    [SerializeField] float rareWeight = 25f;
    [SerializeField] float epicWeight = 5f;
    [SerializeField] float rarePerStage = 1f;
    [SerializeField] float epicPerStage = 0.5f;

    private LootTable table;
    private DropRarity rarity;
    private int currentStage = 1;

    private void Awake()
    {
        rarity = new DropRarity(normalWeight, rareWeight, epicWeight, rarePerStage, epicPerStage);
    }
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
    private ItemInfo PickItem()
    {
        return table.Pick(rarity.Roll(currentStage));
    }

    private void HandleMonsterKilled(OnMonsterKilled e)
    {
        if(table == null)
            table = new LootTable(DataManager.instance.Items);

        if(table.IsEmpty)
        {
            Debug.LogWarning("[ItemDropper] Normal 등급 아이템이 없습니다.");
            return;
        }

        int dropCount = e.Monster.IsBoss ? normalDropCount * bossDropMultiplier : normalDropCount;

        for(int i = 0; i < dropCount; i++)
        {
            ItemInstance item = PickItem().CreateInstance();
            EventManager.instance.PostNotification(new OnItemDropped(item, e.Position));
        }

    }
    private void HandleStageStarted(OnStageStarted e)
    {
        currentStage = e.Stage;
    }
}