using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class VillageView : MonoBehaviour
{
    [SerializeField] Button enterButton;

    private void Awake()
    {
        enterButton.onClick.AddListener(EnterDungeon);
    }

    private void Start()
    {
        GameSession.instance.RefreshGold();
    }

    private void EnterDungeon()
    {
        enterButton.interactable = false;
        GameSession.instance.BeginRun();
        SceneManager.LoadScene(SceneNames.Dungeon);
    }
}