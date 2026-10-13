using System.Collections.Generic;
using UnityEngine;

public class GameSession : Singleton<GameSession>
{
    private const string SaveFileName = "save.json";

    [SerializeField] int defaultClassId = 3000;
    [SerializeField] StarterKit kit;

    private SaveService saveService;
    private SaveData saveData;

    public ClassInfo CurrentClass { get; private set; }
    public Backpack Backpack { get; private set; }
    public Wallet Wallet { get; private set; }

    public bool TryLoad()
    {
        saveService = new SaveService(new FileSaveStorage(SaveFileName));

        if (!saveService.TryLoad(out saveData))
            saveData = new SaveData { classId = defaultClassId };

        if (!DataManager.instance.TryGetClass(saveData.classId, out ClassInfo playerClass))
        {
            Debug.LogError($"[GameSession] 직업 ID {saveData.classId}를 찾을 수 없습니다.", this);
            return false;
        }

        CurrentClass = playerClass;
        Wallet = new Wallet(saveData.gold);
        Wallet.Changed += PostGold;

        if (!TryLoadBackpack())
            return false;

        if (!saveData.inRun)
            return true;

        Debug.Log("[GameSession] 런 도중 종료 기록 → 사망 처리", this);
        LoseBackpack();
        Save(false);
        return true;
    }

    public void ReturnVillage()
    {
        Save(true);
    }
    public void FinishRun(RunEndReason reason)
    {
        if (reason == RunEndReason.Death)
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

        if (saveData.backpack.bagId != 0 && InventorySaveMapper.TryRestore(saveData.backpack, errors, out Backpack restored))
        {
            foreach (var error in errors)
            {
                Debug.LogWarning($"[GameSession] {error}", this);
            }

            Backpack = restored;
            return true;
        }

        if (!kit.TryCreate(out Backpack created))
            return false;

        Backpack = created;
        return true;
    }

    private void LoseBackpack()
    {
        PouchInstance pouch = Backpack.LoseAllOnDeath();

        if (!kit.TryCreate(out Backpack newPack))
            return;

        if (pouch != null && !newPack.Grid.TryAutoPlace(pouch))
            Debug.LogWarning("[GameSession] 새 가방에 안전 주머니를 넣지 못했습니다.", this);

        Backpack = newPack;
    }

    private void Save(bool inRun)
    {
        saveData.classId = CurrentClass.Id;
        saveData.gold = Wallet.Gold;
        saveData.inRun = inRun;
        saveData.backpack = InventorySaveMapper.ToData(Backpack);
        saveService.Save(saveData);
    }

    private void PostGold(Wallet wallet)
    {
        EventManager.instance.PostNotification(new OnChangeGold(wallet.Gold, wallet.RunGold));
    }
}
