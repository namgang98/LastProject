using UnityEngine;
using UnityEngine.UI;

public class TutorialResetButton : MonoBehaviour
{
    [SerializeField] Button button;

    private void Awake()
    {
        button.onClick.AddListener(ResetTutorials);
    }

    private void ResetTutorials()
    {
        GameSession.instance.Tutorials.Reset();
        GameSession.instance.SaveInVillage();
        EventManager.instance.PostNotification(new OnFeedback("다음 방문부터 튜토리얼이 다시 나옵니다", FeedbackType.Info));
    }
}