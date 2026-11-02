using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : Singleton<SceneLoader>
{
    [SerializeField] CanvasGroup curtain;
    [SerializeField] float fadeOut = 0.4f;
    [SerializeField] float fadeIn = 0.4f;

    public bool IsLoading { get; private set; }

    protected override void Awake()
    {
        base.Awake();

        if(instance != this)
            return;

        curtain.alpha = 0f;
        curtain.blocksRaycasts = false;
    }

    public void Load(string sceneName)
    {
        if(IsLoading)
            return;

        StartCoroutine(LoadRoutine(sceneName));
    }

    private IEnumerator LoadRoutine(string sceneName)
    {
        IsLoading = true;
        curtain.blocksRaycasts = true;

        yield return curtain.DOFade(1f, fadeOut).SetUpdate(true).WaitForCompletion();

        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);

        while(!operation.isDone)
            yield return null;

        yield return curtain.DOFade(0f, fadeIn).SetUpdate(true).WaitForCompletion();

        curtain.blocksRaycasts = false;
        IsLoading = false;
    }
}