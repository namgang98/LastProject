using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class NpcQuestButton : MonoBehaviour
{
    [SerializeField] NpcType npc = NpcType.Merchant;
    [SerializeField] QuestBoardUI board;

    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(() => board.Open(npc));
    }
}
