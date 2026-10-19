using System;
using System.Collections.Generic;
using UnityEngine;

public class PopupManager : Singleton<PopupManager>
{
    [Header("팝업캔버스의 Dim")]
    [SerializeField] GameObject dim;

    private readonly List<Entry> openPopups = new();

    public int OpenCount => openPopups.Count;

    public void Open(GameObject popup, Action onClose = null, bool closeOnEscape = true)
    {
        if(popup == null)
            return;

        int index = IndexOf(popup);

        if(index >= 0)
            openPopups.RemoveAt(index);

        openPopups.Add(new Entry(popup, onClose,closeOnEscape));
        popup.SetActive(true);
        RefreshDim();
    }

    public void Close(GameObject popup)
    {
        if(popup == null)
            return;

        CloseChildren(popup);

        int index = IndexOf(popup);

        if(index >= 0)
            openPopups.RemoveAt(index);

        popup.SetActive(false);
        RefreshDim();
    }

    public void CloseTop()
    {
        if(openPopups.Count == 0)
            return;

        Entry top = openPopups[^1];

        if(!top.CloseOnEscape)
            return;

        RequestClose(top);
    }

    public void CloseAll()
    {
        for(int i = openPopups.Count - 1; i >= 0; i--)
        {
            if(i >= openPopups.Count)
                continue;

            RequestClose(openPopups[i]);
        }

        RefreshDim();
    }

    private void CloseChildren(GameObject parent)
    {
        for(int i = openPopups.Count - 1; i >= 0; i--)
        {
            if(i >= openPopups.Count)
                continue;

            Entry entry = openPopups[i];

            if(entry.Popup == parent || !entry.Popup.transform.IsChildOf(parent.transform))
                continue;

            RequestClose(entry);
        }
    }

    private void RequestClose(Entry entry)
    {
        if(entry.OnClose != null)
        {
            entry.OnClose();
            return;
        }

        Close(entry.Popup);
    }

    private int IndexOf(GameObject popup)
    {
        for(int i = 0; i < openPopups.Count; i++)
        {
            if(openPopups[i].Popup == popup)
                return i;
        }

        return -1;
    }

    private void RefreshDim()
    {
        dim.SetActive(openPopups.Count > 0);
    }

    private readonly struct Entry
    {
        public readonly GameObject Popup;
        public readonly Action OnClose;
        public readonly bool CloseOnEscape;
        public Entry(GameObject popup, Action onClose, bool closeOnEscape)
        {
            Popup = popup;
            OnClose = onClose;
            CloseOnEscape = closeOnEscape;
        }
    }
}