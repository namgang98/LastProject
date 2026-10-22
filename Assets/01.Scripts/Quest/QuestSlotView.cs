using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuestSlotView : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI nameText;
    [SerializeField] TextMeshProUGUI descText;
    [SerializeField] Image progressFill;
    [SerializeField] TextMeshProUGUI progressText;
    [SerializeField] Image rewardIcon;
    [SerializeField] TextMeshProUGUI rewardText;
    [SerializeField] Button claimButton;
    [SerializeField] TextMeshProUGUI claimLabel;

    private QuestProgress quest;
    private Action<QuestProgress> onClaim;

    private void Awake()
    {
        claimButton.onClick.AddListener(HandleClick);
    }

    public void Bind(QuestProgress quest, QuestBook book, Action<QuestProgress> onClaim)
    {
        this.quest = quest;
        this.onClaim = onClaim;

        QuestInfo info = quest.Info;
        int current = book.GetCurrent(quest);
        int goal = info.Type == QuestType.ReachFloor ? info.Target : info.Count;
        bool ready = book.IsReady(quest);

        nameText.text = info.Name;
        descText.text = info.Description;
        progressText.text = $"{current} / {goal}";
        progressFill.fillAmount = goal > 0 ? (float)current / goal : 0f;

        rewardIcon.gameObject.SetActive(info.HasRewardItem);

        if(info.HasRewardItem)
            rewardIcon.sprite = info.RewardItem.Icon;

        rewardText.text = BuildRewardText(info);

        claimButton.interactable = ready;
        claimLabel.text = GetLabel(info, ready);
    }

    private void HandleClick()
    {
        if(quest == null)
            return;

        onClaim?.Invoke(quest);
    }

    private static string BuildRewardText(QuestInfo info)
    {
        string item = info.HasRewardItem ? $"x{info.RewardCount}" : string.Empty;
        string gold = info.RewardGold > 0 ? $"+{info.RewardGold}G" : string.Empty;
        return $"{item}  {gold}".Trim();
    }

    private static string GetLabel(QuestInfo info, bool ready)
    {
        if(!ready)
            return "진행 중";

        return info.Type == QuestType.Deliver ? "납품" : "수령";
    }
}