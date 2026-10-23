using UnityEngine;

public class QuestBoardBadge : MonoBehaviour
{
    [SerializeField] GameObject badge;
    [SerializeField] NpcType npc = NpcType.Board;
    [SerializeField] bool checkAllNpc;

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
        badge.SetActive(checkAllNpc ? book.HasAnyReady() : book.HasReady(npc));
    }
}