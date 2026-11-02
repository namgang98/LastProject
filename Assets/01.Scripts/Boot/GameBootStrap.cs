using UnityEngine;
using UnityEngine.SceneManagement;

public class GameBootStrap : MonoBehaviour
{
    private async Awaitable Start()
    {
        await DataManager.instance.InitAsync();

        if(!GameSession.instance.TryLoad())
        {
            Debug.LogError("[GameBootStrap] 세션을 불러오지 못했습니다.", this);
            return;
        }

        SceneLoader.instance.Load(SceneNames.Village);
    }
}
