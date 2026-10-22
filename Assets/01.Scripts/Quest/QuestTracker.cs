using System.Collections.Generic;
using UnityEngine;

public class QuestTracker : MonoBehaviour
{
    private readonly List<QuestProgress> newlyReady = new();

    private void OnEnable()
    {
        EventManager.instance.AddListener<OnMonsterKilled>(HandleMonsterKilled);
    }

    private void OnDisable()
    {
        if(!EventManager.HasInstance)
            return;

        EventManager.instance.RemoveListener<OnMonsterKilled>(HandleMonsterKilled);
    }

    private void HandleMonsterKilled(OnMonsterKilled e)
    {
        newlyReady.Clear();
        GameSession.instance.Quests.ReportKill(e.Monster, newlyReady);

        foreach(QuestProgress quest in newlyReady)
        {
            EventManager.instance.PostNotification(new OnFeedback($"퀘스트 완료: {quest.Info.Name} (마을 게시판에서 보상 수령)", FeedbackType.Info));
        }
    }
}