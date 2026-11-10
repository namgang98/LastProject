using UnityEngine;

public class TutorialSignal : MonoBehaviour
{
    [SerializeField] string key;

    public void Send()
    {
        Post(key);
    }

    public static void Post(string key)
    {
        EventManager.instance.PostNotification(new OnTutorialSignal(key));
    }
}