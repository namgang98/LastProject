using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class TitleView : MonoBehaviour
{
    private const string ResetMessage = "새로 시작하면 지금까지의 기록이 모두 사라집니다.\n계속할까요?";

    [Header("버튼")]
    [SerializeField] Button continueButton;
    [SerializeField] Button newGameButton;
    [SerializeField] Button settingsButton;
    [SerializeField] Button quitButton;
    [SerializeField] TitleMenuButton continueVisual;

    [Header("팝업")]
    [SerializeField] GameObject settingsPanel;
    [SerializeField] ConfirmPopup confirmPopup;

    [Header("연출")]
    [SerializeField] CanvasGroup menuGroup;
    [SerializeField] RectTransform background;
    [SerializeField] float menuDelay = 1f;
    [SerializeField] float menuFadeTime = 0.8f;
    [SerializeField] float driftScale = 1.06f;
    [SerializeField] float driftTime = 25f;

    private void Awake()
    {
        continueButton.onClick.AddListener(Continue);
        newGameButton.onClick.AddListener(NewGame);
        settingsButton.onClick.AddListener(OpenSettings);
        quitButton.onClick.AddListener(Quit);
        settingsPanel.SetActive(false);
    }

    private void Start()
    {
        continueButton.interactable = GameSession.instance.HasSave;
        continueVisual.ResetVisual();
        ShowMenu();
        Drift();
    }

    private void ShowMenu()
    {
        menuGroup.alpha = 0f;
        menuGroup.interactable = false;
        menuGroup.DOFade(1f, menuFadeTime)
            .SetDelay(menuDelay)
            .SetUpdate(true)
            .SetLink(menuGroup.gameObject)
            .OnComplete(() => menuGroup.interactable = true);
    }

    private void Drift()
    {
        background.DOScale(driftScale, driftTime)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo)
            .SetLink(background.gameObject);
    }

    private void Continue()
    {
        SceneLoader.instance.Load(SceneNames.Village);
    }

    private void NewGame()
    {
        if(!GameSession.instance.HasSave)
        {
            StartNewGame();
            return;
        }

        confirmPopup.Show(ResetMessage, StartNewGame);
    }

    private void StartNewGame()
    {
        if(!GameSession.instance.ResetSave())
            return;

        SceneLoader.instance.Load(SceneNames.Village);
    }

    private void OpenSettings()
    {
        PopupManager.instance.Open(settingsPanel);
    }

    private void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}