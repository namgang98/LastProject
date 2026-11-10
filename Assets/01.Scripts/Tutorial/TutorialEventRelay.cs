using UnityEngine;

public class TutorialEventRelay : MonoBehaviour
{
    [SerializeField] string monsterKilled = "MonsterKilled";

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
        TutorialSignal.Post(monsterKilled);
    }
}