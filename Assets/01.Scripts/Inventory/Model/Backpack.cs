using System.Collections.Generic;

public class Backpack
{
    public GridContainer Grid => Bag.Storage;
    public BagInstance Bag { get; }
    public PouchInstance EquippedPouch { get; private set; }

    // 가방 옆에 표시할 주머니 공간 (없으면 null)
    public GridContainer PouchStorage => EquippedPouch?.Storage;

    public Backpack(BagInstance bag)
    {
        Bag = bag;
        Grid.Changed += OnGridChanged;
    }

    // 메인 그리드가 바뀔 때마다 주머니가 들어왔는지 / 빠졌는지 확인
    private void OnGridChanged(GridContainer grid)
    {
        PouchInstance found = FindPouch();

        if(found == EquippedPouch)
            return;

        EquippedPouch = found;
        EventManager.instance.PostNotification(new OnPouchChanged(this, EquippedPouch));
    }

    private PouchInstance FindPouch()
    {
        foreach(var item in Grid.Items)
        {
            if(item is PouchInstance pouch)
                return pouch;
        }

        return null;
    }

    public List<ItemInstance> ExtractAll()
    {
        return Grid.TakeAll();
    }

    public PouchInstance LoseAllOnDeath()
    {
        PouchInstance survived = EquippedPouch;
        Grid.TakeAll();
        return survived;
    }

    public IEnumerable<ItemInstance> EquippedItems()
    {
        foreach(var item in Grid.Items)
            yield return item;

        if(PouchStorage == null)
            yield break;

        foreach(var item in PouchStorage.Items)
            yield return item;
    }
}