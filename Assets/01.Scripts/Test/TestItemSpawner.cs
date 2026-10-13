using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// 테스트용 : 전투가 없어도 아이템 드랍을 흉내낸다
// 1키 = 목록 중 랜덤 아이템 드랍, 2키 = 안전 주머니 드랍, 3키 = 바닥 비우기(다음으로 출발)
public class TestItemSpawner : MonoBehaviour
{
    [SerializeField] private RunController run;
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

        if(keyboard.digit4Key.wasPressedThisFrame && run != null)
            DebugKey(run.CurrentBackpack);

        if (keyboard.digit5Key.wasPressedThisFrame && run != null)
            GameSession.instance.DeleteSave();
    }
    private void DebugKey(Backpack backpack)
    {
        var errors = new List<string>();
        string json = JsonUtility.ToJson(InventorySaveMapper.ToData(backpack), true);
        Debug.Log(json);

        var data = JsonUtility.FromJson<BackpackSaveData>(json);

        if(!InventorySaveMapper.TryRestore(data, errors, out Backpack copy))
        {
            Debug.LogWarning("[TestItemSpawner] 가방 복원 실패");
            return;
        }

        int originPouch = backpack.PouchStorage?.Items.Count ?? 0;
        int copyPouch = copy.PouchStorage?.Items.Count ?? 0;

        Debug.Log($"가방 {backpack.Grid.Items.Count} → {copy.Grid.Items.Count} / 주머니 {originPouch} → {copyPouch} / 경고 {errors.Count}");

        foreach(var error in errors)
        {
            Debug.LogWarning(error);
        }
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
