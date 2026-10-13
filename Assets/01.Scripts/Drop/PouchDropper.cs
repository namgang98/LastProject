using UnityEngine;

public class PouchDropper : MonoBehaviour
{
    [SerializeField] private PouchDropTable dropTable;

    private void OnEnable()
    {
        EventManager.instance.AddListener<OnMonsterKilled>(HandleMonsterKilled);
    }

    private void OnDisable()
    {
        if(!EventManager.HasInstance)
            return;

        EventManager.instance.RemoveListener<OnMonsterKilled>(HandleMonsterKilled);
    }

    private void HandleMonsterKilled(OnMonsterKilled e)
    {
        if(dropTable == null)
            return;

        if(!dropTable.TryRoll(out PouchInfo pouchinfo))
            return;

        ItemInstance pouch = pouchinfo.CreateInstance();
        EventManager.instance.PostNotification(new OnItemDropped(pouch, e.Position));
    }
}