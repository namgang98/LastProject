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
        if (!GameSession.HasInstance)
            return false;

        return string.IsNullOrEmpty(requires) || GameSession.instance.Tutorials.IsDone(requires);
    }
}