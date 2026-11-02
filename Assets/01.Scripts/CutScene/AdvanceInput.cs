using UnityEngine.InputSystem;

public class AdvanceInput
{
    public static bool WasPressed()
    {
        Mouse mouse = Mouse.current;
        Keyboard keyboard = Keyboard.current;

        if(mouse != null && mouse.leftButton.wasPressedThisFrame)
            return true;

        if(keyboard == null)
            return false;

        return keyboard.spaceKey.wasPressedThisFrame || keyboard.enterKey.wasPressedThisFrame;
    }

    public static bool WasSkipPressed()
    {
        Keyboard keyboard = Keyboard.current;

        return keyboard != null && keyboard.escapeKey.wasPressedThisFrame;
    }
}
