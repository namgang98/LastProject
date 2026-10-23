using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class PopupEscapeCloser : MonoBehaviour
{
    [SerializeField] UnityEvent onEscapeIdle;

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;

        if(keyboard == null || !keyboard.escapeKey.wasPressedThisFrame)
            return;

        if(!PopupManager.HasInstance || IsTyping())
            return;

        if(PopupManager.instance.OpenCount > 0)
        {
            PopupManager.instance.CloseTop();
            return;
        }

        onEscapeIdle?.Invoke();
    }

    private static bool IsTyping()
    {
        if(EventSystem.current == null)
            return false;

        GameObject selected = EventSystem.current.currentSelectedGameObject;

        return selected != null && selected.TryGetComponent(out TMP_InputField input) && input.isFocused;
    }
}