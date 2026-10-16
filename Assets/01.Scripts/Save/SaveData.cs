using System;
using System.Collections.Generic;
using UnityEngine;
[Serializable]
public class SaveData
{
    public const int CurrentVersion = 1;

    public int version = CurrentVersion;
    public int classId;
    public int gold;
    public bool inRun;
    public BackpackSaveData backpack = new();
    public List<GridSaveData> chestTabs = new();
    public List<int> shopOffers = new();
    public List<UpgradeSaveData> upgrades = new();
}

[Serializable]
public class BackpackSaveData
{
    public int bagId;
    public GridSaveData grid = new();
    public GridSaveData pouch = new();
}

[Serializable]
public class GridSaveData
{
    public List<ItemSaveData> items = new();
}

[Serializable]
public class ItemSaveData
{
    public int id;
    public int x;
    public int y;
    public int rotation;
    public int parent = -1;
}

[Serializable]
public class UpgradeSaveData
{
    public string track;
    public int level = 1;
    public int deposited;
}