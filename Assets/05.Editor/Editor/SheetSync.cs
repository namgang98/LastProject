using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

public static class SheetSync
{
    private const string UrlFormat = "https://docs.google.com/spreadsheets/d/{0}/export?format=csv&gid={1}";
    private static bool isSyncing;

    [MenuItem("Tools/시트 동기화")]
    private static async void Sync()
    {
        if(isSyncing)
            return;

        if(!TryLoadSettings(out SheetSyncSettings settings))
            return;

        isSyncing = true;
        int success = 0;

        try
        {
            foreach(var tab in settings.tabs)
            {
                if(await TryDownload(settings.sheetId, tab))
                    success++;
            }
        }
        finally
        {
            isSyncing = false;
            AssetDatabase.Refresh();
        }

        Debug.Log($"[SheetSync] 완료 : {success} / {settings.tabs.Count} 탭");
    }

    private static bool TryLoadSettings(out SheetSyncSettings settings)
    {
        settings = null;
        string[] guids = AssetDatabase.FindAssets("t:SheetSyncSettings");

        if(guids.Length == 0)
        {
            Debug.LogError("[SheetSync] SheetSyncSettings 에셋이 없습니다. Create → Tools → Sheet Sync Settings 로 만들어주세요.");
            return false;
        }

        string path = AssetDatabase.GUIDToAssetPath(guids[0]);
        settings = AssetDatabase.LoadAssetAtPath<SheetSyncSettings>(path);

        if(string.IsNullOrEmpty(settings.sheetId))
        {
            Debug.LogError("[SheetSync] sheetId 가 비어 있습니다.", settings);
            return false;
        }

        return true;
    }

    private static async Awaitable<bool> TryDownload(string sheetId, SheetSyncSettings.Tab tab)
    {
        if(tab.target == null)
        {
            Debug.LogWarning($"[SheetSync] gid {tab.gid} 의 CSV 파일이 연결되지 않았습니다.");
            return false;
        }

        string path = AssetDatabase.GetAssetPath(tab.target);
        string url = string.Format(UrlFormat, sheetId, tab.gid);

        using var request = UnityWebRequest.Get(url);
        await request.SendWebRequest();

        if(request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"[SheetSync] {tab.target.name} 실패 : {request.error}");
            return false;
        }

        string text = request.downloadHandler.text;

        if(text.StartsWith("<"))
        {
            Debug.LogError($"[SheetSync] {tab.target.name} : CSV 대신 웹페이지가 왔습니다. 시트 공유 설정(링크가 있는 모든 사용자)을 확인해주세요.");
            return false;
        }

        File.WriteAllText(path, text, new UTF8Encoding(true));
        Debug.Log($"[SheetSync] {tab.target.name} ← gid {tab.gid}");
        return true;
    }
}
