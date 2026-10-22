using UnityEngine;

public class QuestBoardBadge : MonoBehaviour
{
    [SerializeField] GameObject badge;

    private void OnEnable()
    {
        EventManager.instance.AddListener<OnPopupCountChanged>(HandlePopupCountChanged);
        Refresh();
    }

    private void OnDisable()
    {
        if(!EventManager.HasInstance)
            return;

        EventManager.instance.RemoveListener<OnPopupCountChanged>(HandlePopupCountChanged);
    }

    private void HandlePopupCountChanged(OnPopupCountChanged e)
    {
        if(e.Count > 0)
            return;

        Refresh();
    }

    private void Refresh()
    {
        QuestBook book = GameSession.instance.Quests;

        foreach(QuestProgress quest in book.All)
        {
            if(book.IsVisible(quest) && book.IsReady(quest))
            {
                badge.SetActive(true);
                return;
            }
        }

        badge.SetActive(false);
    }
}