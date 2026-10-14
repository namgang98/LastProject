using System.Collections.Generic;
using UnityEngine;

public static class InventorySaveMapper
{
    public static BackpackSaveData ToData(Backpack backpack)
    {
        return new BackpackSaveData
        {
            bagId = backpack.Bag.Info.Id,
            grid = ToData(backpack.Grid),
            pouch = ToSingleData(backpack.EquippedPouch)
        };
    }
    private static GridSaveData ToSingleData(ItemInstance item)
    {
        var data = new GridSaveData();

        if(item == null)
            return data;

        Append(data.items, item, -1);
        return data;
    }

    public static GridSaveData ToData(GridContainer grid)
    {
        var data = new GridSaveData();

        foreach(var item in grid.Items)
        {
            Append(data.items, item, -1);
        }

        return data;
    }

    private static void Append(List<ItemSaveData> list, ItemInstance item, int parent)
    {
        int index = list.Count;

        list.Add(new ItemSaveData
        {
            id = item.Info.Id,
            x = item.Anchor.x,
            y = item.Anchor.y,
            rotation = item.Rotation,
            parent = parent
        });

        if(item is not ContainerInstance container)
            return;

        foreach(var child in container.Storage.Items)
        {
            Append(list, child, index);
        }
    }
    public static void Restore(GridSaveData data, GridContainer target, List<string> errors)
    {
        ItemInstance[] instances = CreateAll(data, errors);
        PlaceAll(data, instances, target, 0, errors);
    }
    private static void PlaceAll(GridSaveData data, ItemInstance[] instances, GridContainer root, int start, List<string> errors)
    {
        for(int i = start; i < instances.Length; i++)
        {
            if(instances[i] == null)
                continue;

            GridContainer destination = FindDestination(data.items[i].parent, i, instances, root, errors);

            if(destination == null)
                continue;

            Place(instances[i], data.items[i], destination, errors);
        }
    }

    private static PouchInstance RestorePouch(GridSaveData data, List<string> errors)
    {
        if(data == null || data.items.Count == 0)
            return null;

        ItemInstance[] instances = CreateAll(data, errors);

        if(instances[0] is not PouchInstance pouch)
        {
            errors.Add("착용 주머니 데이터가 주머니가 아닙니다 → 사라짐");
            return null;
        }

        PlaceAll(data, instances, null, 1, errors);
        return pouch;
    }
    private static ItemInstance[] CreateAll(GridSaveData data, List<string> errors)
    {
        var instances = new ItemInstance[data.items.Count];

        for(int i = 0; i < data.items.Count; i++)
        {
            int id = data.items[i].id;

            if(!DataManager.instance.TryGetItem(id, out ItemInfo info))
            {
                errors.Add($"아이템 ID {id} 가 데이터에 없습니다 → 사라짐");
                continue;
            }

            instances[i] = info.CreateInstance();
        }

        return instances;
    }
    private static GridContainer FindDestination(int parent, int index, ItemInstance[] instances, GridContainer target, List<string> errors)
    {
        if(parent == -1)
            return target;

        if(parent < 0 || parent >= index)
        {
            errors.Add($"{index}번 아이템의 번호 {parent}가 잘못되었습니다");
            return null;
        }

        if(instances[parent] is not ContainerInstance container)
        {
            errors.Add($"{index}번 아이템의 부모가 없거나 컨테이너가 아닙니다 → 사라짐");
            return null;
        }

        return container.Storage;
    }
    private static void Place(ItemInstance item, ItemSaveData saved, GridContainer destination, List<string> errors)
    {
        if(destination.TryPlace(item, new Vector2Int(saved.x, saved.y), saved.rotation))
            return;

        if(destination.TryAutoPlace(item))
            return;

        errors.Add($"{item.Info.Name} 이 들어갈 자리가 없습니다 → 사라짐");
    }
    public static bool TryRestore(BackpackSaveData data, List<string> errors, out Backpack backpack)
    {
        backpack = null;
        
        if(!DataManager.instance.TryGetItem(data.bagId, out BagInfo bagInfo))
        {
            errors.Add($"가방 ID {data.bagId} 가 데이터에 없습니다.");
            return false;
        }

        backpack = new Backpack((BagInstance)bagInfo.CreateInstance());
        Restore(data.grid, backpack.Grid, errors);

        PouchInstance pouch = RestorePouch(data.pouch, errors);

        if(pouch != null)
            backpack.TryEquipPouch(pouch);

        return true;
    }

}
