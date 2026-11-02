using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Tools/Sheet Sync Settings")]
public class SheetSyncSettings : ScriptableObject
{
    [Serializable]
    public class Tab
    {
        public string gid;
        public TextAsset target;
    }

    public string sheetId;
    public List<Tab> tabs = new();
}
