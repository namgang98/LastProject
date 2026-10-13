using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "Backpack/Starter Kit", fileName = "StarterKit")]
public class StarterKit : ScriptableObject
{
    [SerializeField] private int bagId = 100;
    [SerializeField] private List<int> itemIds = new() { 1001 };

    public bool TryCreate(out Backpack backpack)
    {
        backpack = null;

        if(!DataManager.instance.TryGetItem(bagId, out BagInfo bagInfo))
        {
            Debug.LogError($"[StarterKit] 시작 가방 ID {bagId} 가 데이터에 없습니다.", this);
            return false;
        }

        backpack = new Backpack((BagInstance)bagInfo.CreateInstance());

        foreach(int id in itemIds)
        {
            if(!DataManager.instance.TryGetItem(id, out ItemInfo itemInfo))
            {
                Debug.LogWarning($"[StarterKit] 기본 검 ID {id} 가 데이터에 없습니다.", this);
                continue;
            }
            ItemInstance item = itemInfo.CreateInstance();

            if(!backpack.Grid.TryAutoPlace(item))
            {
                Debug.LogWarning($"[StarterKit] {itemInfo.Name}이 들어갈 자리가없습니다.");
            }
        }

        return true;
    }
}