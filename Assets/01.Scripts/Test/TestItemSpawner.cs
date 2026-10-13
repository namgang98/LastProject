using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// 테스트용 : 전투가 없어도 아이템 드랍을 흉내낸다
// 1키 = 목록 중 랜덤 아이템 드랍, 2키 = 안전 주머니 드랍, 3키 = 바닥 비우기(다음으로 출발)
public class TestItemSpawner : MonoBehaviour
{
    [SerializeField] private List<int> itemIds = new();
    [SerializeField] private int pouchId = 9001;
    [SerializeField] private InventoryUI inventoryUI;

    private void Update()
    {
        if(!DataManager.HasInstance || !DataManager.instance.IsLoaded)
            return;


        Keyboard keyboard = Keyboard.current;

        if(keyboard == null)
            return;

        if(keyboard.digit1Key.wasPressedThisFrame && itemIds.Count > 0)
            Spawn(itemIds[Random.Range(0, itemIds.Count)]);

        if(keyboard.digit2Key.wasPressedThisFrame)
            Spawn(pouchId);

        if(keyboard.digit3Key.wasPressedThisFrame && inventoryUI != null)
            inventoryUI.ClearLoot();
    }

    private void Spawn(int id)
    {
        if(!DataManager.instance.TryGetItem(id, out ItemInfo info))
        {
            Debug.LogWarning($"[TestItemSpawner] ID {id} 가 데이터에 없습니다.", this);
            return;
        }
            EventManager.instance.PostNotification(new OnItemDropped(info.CreateInstance(), Vector3.zero));
    }
}
