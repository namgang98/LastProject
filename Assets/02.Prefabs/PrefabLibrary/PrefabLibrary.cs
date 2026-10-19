using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Prefab/PrefabLibrary", fileName = "PrefabLibrary")]
public class PrefabLibrary : ScriptableObject
{
    [SerializeField] List<GameObject> prefabs = new();

    private Dictionary<string, GameObject> table;

    public void Init()
    {
        table = new Dictionary<string, GameObject>(prefabs.Count);

        foreach(var prefab in prefabs)
        {
            if(prefab == null)
                continue;

            if(!table.TryAdd(prefab.name, prefab))
            {
                Debug.LogWarning($"[PrefabLibrary]의 이름이 같은 프리팹이 있습니다 : {prefab.name}", this);
            }
        }

    }
    public bool TryGet(string iconName, out GameObject prefab)
    {
        prefab = null;

        if(table == null)
            return false;

        return table.TryGetValue(iconName, out prefab);
    }

}
