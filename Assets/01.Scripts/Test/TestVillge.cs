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
        if(keyboard.digit7Key.wasPressedThisFrame)
            DonateTest();

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
    private void DonateTest()
    {
        GameSession s = GameSession.instance;
        int used = s.Upgrades.Donate(UpgradeTrack.Shop, 100, s.Wallet);

        if(!s.Upgrades.TryGet(UpgradeTrack.Shop, out UpgradeProgress p))
            return;

        Debug.Log($"기부 {used}G → 상점 Lv.{p.Level} ({p.Deposited}/{p.NextCost}) / 칸 {s.Shop.SlotCount} / 남은 골드 {s.Wallet.Gold}");
    }
}
