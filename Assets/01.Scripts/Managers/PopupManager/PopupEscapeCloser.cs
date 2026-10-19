using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class PopupEscapeCloser : MonoBehaviour
{
    private void Update()
    {
        Keyboard keyboard = Keyboard.current;

        if (keyboard == null || !keyboard.escapeKey.wasPressedThisFrame)
            return;

        if (!PopupManager.HasInstance || IsTyping())
            return;

        PopupManager.instance.CloseTop();
    }

    private static bool IsTyping()
    {
        if (EventSystem.current == null)
            return false;

        GameObject selected = EventSystem.current.currentSelectedGameObject;

        return selected != null && selected.TryGetComponent(out TMP_InputField input) && input.isFocused;
    }
}