using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuestBoardUI : MonoBehaviour
{
    [SerializeField] GameObject popup;
    [SerializeField] Transform listRoot;
    [SerializeField] QuestSlotView slotPrefab;
    [SerializeField] Button closeButton;
    [SerializeField] GameObject emptyText;

    [Header("NPC 의뢰")]
    [SerializeField] TMP_Text titleText;
    [SerializeField] FavorView favorView;
    [SerializeField] GameObject dim;

    private readonly List<QuestSlotView> slots = new();
    private readonly List<QuestProgress> visible = new();
    private NpcType npc = NpcType.Board;

    private void Awake()
    {
        closeButton.onClick.AddListener(Close);
        popup.SetActive(false);
    }

    public void Open()
    {
        Open(NpcType.Board);
    }

    public void Open(NpcType target)
    {
        npc = target;

        if(titleText != null)
            titleText.text = npc == NpcType.Board ? npc.ToKorean() : $"{npc.ToKorean()}의 의뢰";

        if(favorView != null)
            favorView.Show(npc);

        Refresh();
        PopupManager.instance.Open(popup, Close);

        if(dim != null)
            dim.SetActive(true);

        Sfx.Play(SfxType.BookOpen);
    }

    public void Close()
    {
        PopupManager.instance.Close(popup);

        if(dim != null)
            dim.SetActive(false);
    }

    private void Refresh()
    {
        QuestBook book = GameSession.instance.Quests;
        visible.Clear();

        foreach(QuestProgress quest in book.All)
        {
            if(quest.Info.Npc == npc && book.IsVisible(quest))
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
        int favor = quest.Info.Favor;
        QuestClaimResult result = GameSession.instance.TryClaimQuest(quest, out int levelUps);

        if(result == QuestClaimResult.NoSpace)
            EventManager.instance.PostNotification(new OnFeedback("가방과 창고에 자리가 없습니다"));
        else if(result == QuestClaimResult.Ok)
        {
            Sfx.Play(SfxType.QuestComplete);
            PostClaimFeedback(name, favor, levelUps);
        }

        Refresh();
    }

    private void PostClaimFeedback(string name, int favor, int levelUps)
    {
        string text = favor > 0 ? $"퀘스트 완료: {name} (우호도 +{favor})" : $"퀘스트 완료: {name}";
        EventManager.instance.PostNotification(new OnFeedback(text, FeedbackType.Info));

        if(levelUps <= 0)
            return;

        int level = GameSession.instance.Favor.GetLevel(npc);
        EventManager.instance.PostNotification(new OnFeedback($"{npc.ToKorean()}와(과) 더 가까워졌다! 우호도 Lv.{level}", FeedbackType.Info));
    }

    private void EnsureSlotCount(int count)
    {
        while(slots.Count < count)
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