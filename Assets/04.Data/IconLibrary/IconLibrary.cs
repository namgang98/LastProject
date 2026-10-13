using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Icon/Icon Library", fileName = "IconLibrary")]
public class IconLibrary : ScriptableObject
{
    [SerializeField] List<Sprite> icons = new();

    private Dictionary<string, Sprite> table;

    public void Init()
    {
        table = new Dictionary<string, Sprite>(icons.Count);

        foreach(var icon in icons)
        {
            if(icon == null)
                continue;

            if(!table.TryAdd(icon.name, icon))
            {
                Debug.LogWarning($"[IconLibrary] 이름이 같은 아이콘이 있습니다 : {icon.name}", this);
            }
        }

    }
    public bool TryGet(string iconName, out Sprite icon)
    {
        icon = null;

        if(table == null)
            return false;

        return table.TryGetValue(iconName, out icon);
    }

}
