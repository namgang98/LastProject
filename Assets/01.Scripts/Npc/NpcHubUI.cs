using UnityEngine;
using UnityEngine.UI;

public class NpcHubUI : MonoBehaviour
{
    [SerializeField] GameObject popup;
    [SerializeField] Button closeButton;
    [SerializeField] QuestBoardUI board;
    [SerializeField] NpcCardView[] cards;

    private void Awake()
    {
        closeButton.onClick.AddListener(Close);

        foreach(NpcCardView card in cards)
        {
            card.Bind(HandleSelect);
        }
    }

    private void OnEnable()
    {
        EventManager.instance.AddListener<OnPopupCountChanged>(HandlePopupCountChanged);
    }

    private void OnDisable()
    {
        if(!EventManager.HasInstance)
            return;

        EventManager.instance.RemoveListener<OnPopupCountChanged>(HandlePopupCountChanged);
    }

    public void Open()
    {
        Refresh();
        PopupManager.instance.Open(popup, Close);
    }

    public void Close()
    {
        PopupManager.instance.Close(popup);
    }

    private void HandleSelect(NpcType npc)
    {
        board.Open(npc);
    }

    private void HandlePopupCountChanged(OnPopupCountChanged e)
    {
        if(!popup.activeSelf)
            return;

        Refresh();
    }

    private void Refresh()
    {
        GameSession session = GameSession.instance;

        foreach(NpcCardView card in cards)
        {
            card.Refresh(session.Quests, session.Favor);
        }
    }
}