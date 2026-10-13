using System;
using UnityEngine;

public class SaveService
{
    private readonly ISaveStorage storage;

    public SaveService(ISaveStorage storage)
    {
        this.storage = storage;
    }

    public bool TryLoad(out SaveData data)
    {
        data = null;

        try
        {
            if(!storage.TryLoad(out string json))
                return false;

            data = JsonUtility.FromJson<SaveData>(json);
        }
        catch(Exception e)
        {
            Debug.LogWarning($"[SaveService] 세이브를 읽지 못했습니다 → 새 게임으로 시작합니다\n{e.Message}");
            data = null;
            return false;
        }

        if(data == null || data.version > SaveData.CurrentVersion)
        {
            Debug.LogWarning($"[SaveService] 지원하지 않는 세이브 버전입니다 ({data?.version})");
            data = null;
            return false;
        }

        return true;
    }

    public bool Save(SaveData data)
    {
        data.version = SaveData.CurrentVersion;

        try
        {
            storage.Save(JsonUtility.ToJson(data, true));
            return true;
        }
        catch(Exception e)
        {
            Debug.LogError($"[SaveService] 저장 실패\n{e.Message}");
            return false;
        }
    }

    public void Delete()
    {
        storage.Delete();
    }
}
