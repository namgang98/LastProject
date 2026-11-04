using System.Collections;
using UnityEngine;

public enum TutorialTrigger
{
    OnStart,
    OnEnable,
    Manual
}

public class TutorialSequence : MonoBehaviour
{
    [SerializeField] string key;
    [SerializeField] string requires;
    [SerializeField] TutorialTrigger trigger = TutorialTrigger.OnStart;
    [SerializeField, Min(0f)] float delay = 0.3f;
    [SerializeField] TutorialDirector director;
    [SerializeField, Min(0)] int minCleared;

    private void Start()
    {
        if (trigger == TutorialTrigger.OnStart)
            StartCoroutine(PlayRoutine());
    }

    private void OnEnable()
    {
        if (trigger == TutorialTrigger.OnEnable)
            StartCoroutine(PlayRoutine());
    }

    public void Play()
    {
        StartCoroutine(PlayRoutine());
    }

    private IEnumerator PlayRoutine()
    {
        yield return new WaitForSecondsRealtime(delay);

        if (!IsReady())
            yield break;

        director.TryPlay(key);
    }

    private bool IsReady()
    {
        if(!GameSession.HasInstance)
            return false;

        GameSession session = GameSession.instance;

        if(session.BestCleared < minCleared)
            return false;

        return string.IsNullOrWhiteSpace(requires) || session.Tutorials.IsDone(requires.Trim());
    }
}