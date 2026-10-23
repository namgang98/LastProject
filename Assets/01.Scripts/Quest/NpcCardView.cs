using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NpcCardView : MonoBehaviour
{
    [SerializeField] NpcType npc;
    [SerializeField] TMP_Text nameText;
    [SerializeField] GameObject favorGroup;
    [SerializeField] TMP_Text favorText;
    [SerializeField] TMP_Text questCountText;
    [SerializeField] GameObject readyBadge;
    [SerializeField] Button button;

    public void Bind(Action<NpcType> onSelect)
    {
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => onSelect?.Invoke(npc));
    } 

    public void Refresh(QuestBook book, NpcFavor favor)
    {
        nameText.text = npc.ToKorean();
        readyBadge.SetActive(book.HasReady(npc));

        int count = book.CountVisible(npc);
        questCountText.text = count > 0 ? $"의뢰 {count}" : "의뢰 없음";

        int level = favor.GetLevel(npc);
        favorGroup.SetActive(level > 0);

        if(level > 0)
            favorText.text = $"Lv.{level}";
    }
}
