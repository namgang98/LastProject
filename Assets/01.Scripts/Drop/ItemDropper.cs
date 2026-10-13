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

    private readonly Dictionary<ItemGrade, List<ItemInfo>> looTable = new();
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
    private void BuildTable()
    {
        foreach (var item in DataManager.instance.Items)
        {
            if(!item.IsLootable)
                continue;

            if(!looTable.TryGetValue(item.Grade, out List<ItemInfo> list))
            {
                list = new List<ItemInfo>();
                looTable.Add(item.Grade, list);
            }

            list.Add(item);
        }
    }

    private ItemInfo PickItem()
    {
        ItemGrade grade = rarity.Roll(currentStage);

        if(!looTable.TryGetValue(grade, out List<ItemInfo> list))
            list = looTable[ItemGrade.Normal];

        return list[Random.Range(0, list.Count)];
    }

    private void HandleMonsterKilled(OnMonsterKilled e)
    {
        if(looTable.Count == 0)
            BuildTable();

        if(!looTable.ContainsKey(ItemGrade.Normal))
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