using System.Collections.Generic;
using UnityEngine;

public class PopupManager : Singleton<PopupManager>
{
    [Header("팝업캔버스의 Dim")]
    [SerializeField] GameObject dim;

    private List<GameObject> openPopups = new();
    public void Open(GameObject popup)
    {
        if(popup == null)
            return;

        if(!openPopups.Contains(popup))
            openPopups.Add(popup);

        dim.SetActive(true);
        popup.SetActive(true);
    }
    public void Close(GameObject popup)
    {
        if(popup == null)
            return;

        popup.SetActive(false);
        openPopups.Remove(popup);
        if(openPopups.Count == 0)
            dim.SetActive(false);
    }
    public void CloseAll()
    {
        foreach(var p in openPopups)
            p.SetActive(false);

        openPopups.Clear();
        dim.SetActive(false);
    }
}
