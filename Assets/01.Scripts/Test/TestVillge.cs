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

        if(keyboard.digit6Key.wasPressedThisFrame)
            SpawnTest();

        if(keyboard.digit7Key.wasPressedThisFrame)
            DonateTest();

        if(keyboard.digit8Key.wasPressedThisFrame)
            TrainTest();

        if(keyboard.digit9Key.wasPressedThisFrame)
            PointTest();
    }
    private void DonateTest()
    {
        GameSession s = GameSession.instance;
        int used = s.Upgrades.Donate(UpgradeTrack.Shop, 100, s.Wallet);

        if(!s.Upgrades.TryGet(UpgradeTrack.Shop, out UpgradeProgress p))
            return;

        Debug.Log($"기부 {used}G → 상점 Lv.{p.Level} ({p.Deposited}/{p.NextCost}) / 칸 {s.Shop.SlotCount} / 남은 골드 {s.Wallet.Gold}");
    }
    private void SpawnTest()
    {
        if(itemIds.Count == 0)
        {
            Debug.LogWarning("[TestVillge] itemIds가 비어 있습니다.", this);
            return;
        }

        int id = itemIds[Random.Range(0, itemIds.Count)];

        if(!DataManager.instance.TryGetItem(id, out ItemInfo info))
            return;

        GameSession.instance.Chest.Tabs[0].TryAutoPlace(info.CreateInstance());
        GameSession.instance.SaveInVillage();
        Debug.Log($"창고 아이템 수 : {GameSession.instance.Chest.Tabs[0].Items.Count}");
    }
    private void TrainTest()
    {
        GameSession s = GameSession.instance;
        bool ok = s.Training.TryTrain(s.CurrentClass, s.Wallet);
        ClassProgress p = s.Training.GetProgress(s.CurrentClass);
        Debug.Log($"훈련 {(ok ? "성공" : "실패")} → Lv.{p.Level} / 남은 포인트 {p.FreePoints} / 골드 {s.Wallet.Gold}");
        s.SaveInVillage();
    }

    private void PointTest()
    {
        GameSession s = GameSession.instance;
        bool ok = s.Training.TryAddPoint(s.CurrentClass, TrainingStat.MaxHp);
        ClassProgress p = s.Training.GetProgress(s.CurrentClass);
        Debug.Log($"체력 포인트 {(ok ? "+1" : "실패")} → 체력 {p.GetPoints(TrainingStat.MaxHp)}점 / 남은 {p.FreePoints}");
        s.SaveInVillage();
    }

}
