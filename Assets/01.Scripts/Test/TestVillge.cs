using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class TestVillge : MonoBehaviour
{
    [SerializeField] private List<int> itemIds = new();
    void Update()
    {
        if(!DataManager.HasInstance || !DataManager.instance.IsLoaded)
            return;


        Keyboard keyboard = Keyboard.current;
        if(!keyboard.digit6Key.wasPressedThisFrame)
            return;
        if(itemIds.Count == 0)
        {
            Debug.LogWarning("[TestVillge] itemIds가 비어 있습니다.", this);
            return;
        }
        
        int id = itemIds[Random.Range(0, itemIds.Count)];

        if(DataManager.instance.TryGetItem(id, out ItemInfo info))
        {
            GameSession.instance.Chest.Tabs[0].TryAutoPlace(info.CreateInstance());
            GameSession.instance.SaveInVillage();
            Debug.Log($"창고 아이템 수 : {GameSession.instance.Chest.Tabs[0].Items.Count}");
        }
    }
}
