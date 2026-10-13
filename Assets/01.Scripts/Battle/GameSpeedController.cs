using UnityEngine;

public class GameSpeedController : MonoBehaviour
{
    [SerializeField] float fastSpeed = 4f;
    public float Speed => isFast ? fastSpeed : 1f;
    
    private bool isFast;
    private bool paused;

    public void ToggleSpeed()
    {
        isFast = !isFast;
        Apply();
    }

    public void SetPaused(bool value)
    {
        paused = value;
        Apply();
    }

    private void Apply()
    {
        Time.timeScale = paused ? 0f : Speed;
    }

    private void OnDestroy()
    {
        Time.timeScale = 1f;
    }
}
