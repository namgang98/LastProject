using System.Collections.Generic;
using UnityEngine;

public class GameSession : Singleton<GameSession>
{
    private const string SaveFileName = "save.json";

    [SerializeField] int defaultClassId = 3000;
    [SerializeField] StarterKit kit;
    [SerializeField] Vector2Int chestTabSize = new (10, 8);
    [SerializeField, Min(1)] int startTabCount = 1;

    public Chest Chest { get; private set; }
    private SaveService saveService;
    private SaveData saveData;

    public ClassInfo CurrentClass { get; private set; }
    public Backpack Backpack { get; private set; }
    public Wallet Wallet { get; private set; }

    public bool TryLoad()
    {
        saveService = new SaveService(new FileSaveStorage(SaveFileName));

        if(!saveService.TryLoad(out saveData))
            saveData = new SaveData { classId = defaultClassId };

        if(!DataManager.instance.TryGetClass(saveData.classId, out ClassInfo playerClass))
        {
            Debug.LogError($"[GameSession] 직업 ID {saveData.classId}를 찾을 수 없습니다.", this);
            return false;
        }

        CurrentClass = playerClass;
        Wallet = new Wallet(saveData.gold);
        Wallet.Changed += PostGold;

        if(!TryLoadBackpack())
            return false;
        LoadChest();
        if(!saveData.inRun)
            return true;

        Debug.Log("[GameSession] 런 도중 종료 기록 → 사망 처리", this);
        LoseBackpack();
        Save(false);
        return true;
    }

    public void BeginRun()
    {
        Save(true);
    }
    public void FinishRun(RunEndReason reason)
    {
        if(reason == RunEndReason.Death)
        {
            LoseBackpack();
            Wallet.LoseRun();
        }
        else
        {
            Wallet.CommitRun();
        }

        Save(false);
    }

    public void RefreshGold()
    {
        PostGold(Wallet);
    }

    public void DeleteSave()
    {
        saveService.Delete();
        Debug.Log($"[GameSession] 세이브 삭제 : {Application.persistentDataPath}", this);
    }

    private bool TryLoadBackpack()
    {
        var errors = new List<string>();

        if(saveData.backpack.bagId != 0 && InventorySaveMapper.TryRestore(saveData.backpack, errors, out Backpack restored))
        {
            foreach(var error in errors)
            {
                Debug.LogWarning($"[GameSession] {error}", this);
            }

            Backpack = restored;
            return true;
        }

        if(!kit.TryCreate(out Backpack created))
            return false;

        Backpack = created;
        return true;
    }

    public bool TrySwapBag(BagInstance newBag)
    {
        GridContainer from = newBag.Owner;

        if(from == null || newBag == Backpack.Bag)
            return false;

        Vector2Int anchor = newBag.Anchor;
        int rotation = newBag.Rotation;
        BagInstance oldBag = Backpack.Bag;

        from.Remove(newBag);

        if(!from.TryPlace(oldBag, anchor, rotation) && !from.TryAutoPlace(oldBag))
        {
            from.TryPlace(newBag, anchor, rotation);
            return false;
        }

        Backpack old = Backpack;
        Backpack = new Backpack(newBag);

        PouchInstance pouch = old.UnequipPouch();

        if(pouch != null)
            Backpack.TryEquipPouch(pouch);

        EventManager.instance.PostNotification(new OnBackpackChanged(Backpack));
        return true;
    }
    private void LoseBackpack()
    {
        PouchInstance pouch = Backpack.LoseAllOnDeath();

        if(!kit.TryCreate(out Backpack newPack))
            return;

        if(pouch != null)
            newPack.TryEquipPouch(pouch);

        Backpack = newPack;
    }

    private void LoadChest()
    {
        var errors = new List<string>();
        Chest = new Chest(chestTabSize);
        int count = Mathf.Max(startTabCount, saveData.chestTabs.Count);

        for(int i = 0; i < count; i++)
        {
            GridContainer tab = Chest.AddTab();

            if(i < saveData.chestTabs.Count)
                InventorySaveMapper.Restore(saveData.chestTabs[i], tab, errors);
        }

        foreach(var error in errors)
        {
            Debug.LogWarning($"[GameSession] 창고 : {error}", this);
        }
    }

    private void Save(bool inRun)
    {
        saveData.chestTabs.Clear();
        foreach(var tab in Chest.Tabs)
        {
            saveData.chestTabs.Add(InventorySaveMapper.ToData(tab));
        }
        saveData.classId = CurrentClass.Id;
        saveData.gold = Wallet.Gold;
        saveData.inRun = inRun;
        saveData.backpack = InventorySaveMapper.ToData(Backpack);
        saveService.Save(saveData);
    }
    public void SaveInVillage()
    {
        Save(false);
    }
    private void PostGold(Wallet wallet)
    {
        EventManager.instance.PostNotification(new OnChangeGold(wallet.Gold, wallet.RunGold));
    }
    public void SelectClass(ClassInfo info)
    {
        if(info == null)
            return;
        CurrentClass = info;
    }
}
