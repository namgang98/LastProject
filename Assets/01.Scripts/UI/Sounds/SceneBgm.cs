using UnityEngine;

public class SceneBgm : MonoBehaviour
{
    [SerializeField] private BgmType bgm = BgmType.Camp;

    private void Start()
    {
        SoundManager.instance.PlayBgm(bgm);
    }
}