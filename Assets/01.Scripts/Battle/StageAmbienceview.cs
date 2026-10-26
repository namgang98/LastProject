using System.Collections.Generic;
using UnityEngine;

public class StageAmbienceView : MonoBehaviour
{
    private readonly Dictionary<GameObject, GameObject> spawned = new();
    private GameObject currentPrefab;
    private GameObject currentInstance;

    public void Show(GameObject prefab)
    {
        if(prefab == currentPrefab)
            return;

        if(currentInstance != null)
            currentInstance.SetActive(false);

        currentPrefab = prefab;
        currentInstance = null;

        if(prefab == null)
            return;

        if(!spawned.TryGetValue(prefab, out GameObject instance))
        {
            instance = Instantiate(prefab, transform);
            spawned.Add(prefab, instance);
        }

        instance.SetActive(true);
        currentInstance = instance;
    }
}