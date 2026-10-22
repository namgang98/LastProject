using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuestBoardUI : MonoBehaviour
{
    [SerializeField] GameObject popup;
    [SerializeField] Transform listRoot;
    [SerializeField] QuestSlotView slotPrefab;
    [SerializeField] Button closeButton;
    [SerializeField] GameObject emptyText;

    private readonly List<QuestSlotView> slots = new();
    private readonly List<QuestProgress> visible = new();

    private void Awake()
    {
        closeButton.onClick.AddListener(Close);
        popup.SetActive(false);
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

    private void Refresh()
    {
        QuestBook book = GameSession.instance.Quests;
        visible.Clear();

        foreach(QuestProgress quest in book.All)
        {
            if(book.IsVisible(quest))
                visible.Add(quest);
        }

        visible.Sort((a, b) => CompareQuest(book, a, b));

        EnsureSlotCount(visible.Count);

        for(int i = 0; i < slots.Count; i++)
        {
            bool used = i < visible.Count;
            slots[i].gameObject.SetActive(used);

            if(used)
                slots[i].Bind(visible[i], book, HandleClaim);
        }

        if(emptyText != null)
            emptyText.SetActive(visible.Count == 0);
    }

    private void HandleClaim(QuestProgress quest)
    {
        string name = quest.Info.Name;
        QuestClaimResult result = GameSession.instance.TryClaimQuest(quest);

        if(result == QuestClaimResult.NoSpace)
            EventManager.instance.PostNotification(new OnFeedback("가방과 창고에 자리가 없습니다"));
        else if(result == QuestClaimResult.Ok)
            EventManager.instance.PostNotification(new OnFeedback($"퀘스트 완료: {name}", FeedbackType.Info));

        Refresh();
    }

    private void EnsureSlotCount(int count)
    {
        while (slots.Count < count)
        {
            slots.Add(Instantiate(slotPrefab, listRoot));
        }
    }

    private static int CompareQuest(QuestBook book, QuestProgress a, QuestProgress b)
    {
        bool readyA = book.IsReady(a);
        bool readyB = book.IsReady(b);

        if(readyA != readyB)
            return readyA ? -1 : 1;

        return a.Info.Id.CompareTo(b.Info.Id);
    }
}