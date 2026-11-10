using System.Collections.Generic;
using UnityEngine;

public class TutorialDirector : MonoBehaviour
{
    [SerializeField] TutorialDialogView dialog;
    [SerializeField] TutorialSpotlight spotlight;
    [SerializeField] ConfirmPopup confirm;
    [SerializeField] bool saveOnFinish;

    private readonly Dictionary<string, RectTransform> anchors = new();
    private IReadOnlyList<TutorialLineInfo> lines;
    private string playingKey;
    private int index;
    private bool paused;
    private float savedTimeScale = 1f;

    public bool IsPlaying => lines != null;

    private TutorialLineInfo Current => lines[index];

    private void Awake()
    {
        dialog.NextRequested += HandleNext;
        dialog.SkipRequested += HandleSkip;
        CollectAnchors();
    }

    private void OnEnable()
    {
        EventManager.instance.AddListener<OnTutorialSignal>(HandleSignal);
    }

    private void OnDisable()
    {
        SetPaused(false);

        if (!EventManager.HasInstance)
            return;

        EventManager.instance.RemoveListener<OnTutorialSignal>(HandleSignal);
    }

    private void CollectAnchors()
    {
        anchors.Clear();

        foreach (TutorialAnchor anchor in FindObjectsByType<TutorialAnchor>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (!anchors.TryAdd(anchor.Id, anchor.Rect))
                Debug.LogWarning($"[TutorialDirector] 앵커 Id {anchor.Id} 가 중복됩니다", anchor);
        }
    }

    public bool CanPlay(string key)
    {
        if (IsPlaying || !GameSession.HasInstance)
            return false;

        return !GameSession.instance.Tutorials.IsDone(key);
    }

    public bool TryPlay(string key)
    {
        if (!CanPlay(key) || !DataManager.instance.TryGetTutorial(key, out IReadOnlyList<TutorialLineInfo> found))
            return false;

        lines = found;
        playingKey = key;
        index = -1;
        Advance();
        return true;
    }

    private void Advance()
    {
        index++;

        if (index >= lines.Count)
        {
            Finish(false);
            return;
        }

        Render(Current);
    }

    private void Render(TutorialLineInfo line)
    {
        SetPaused(line.Pause);
        dialog.Show(line.Line, line.Wait == TutorialWait.Click);
        ShowSpotlight(line);
    }

    private void ShowSpotlight(TutorialLineInfo line)
    {
        if (TryGetAnchor(line, out RectTransform focus))
        {
            spotlight.Focus(focus, line.Wait == TutorialWait.Signal);
            return;
        }

        if (line.Wait == TutorialWait.Click)
        {
            spotlight.DimAll();
            return;
        }

        spotlight.Hide();
    }

    private bool TryGetAnchor(TutorialLineInfo line, out RectTransform focus)
    {
        focus = null;

        if (!line.HasFocus)
            return false;

        if (anchors.TryGetValue(line.FocusId, out focus))
            return true;

        Debug.LogWarning($"[TutorialDirector] {line.Key} {line.Order}번 : 앵커 {line.FocusId} 를 찾을 수 없습니다", this);
        return false;
    }

    private void HandleNext()
    {
        if (!IsPlaying || Current.Wait != TutorialWait.Click)
            return;

        Advance();
    }

    private void HandleSignal(OnTutorialSignal e)
    {
        if (!IsPlaying || Current.Wait != TutorialWait.Signal || Current.Signal != e.Key)
            return;

        Advance();
    }

    private void HandleSkip()
    {
        if (!IsPlaying)
            return;

        if (confirm == null)
        {
            Finish(true);
            return;
        }

        confirm.Show("튜토리얼을 모두 건너뛸까요?\n설정에서 다시 볼 수 있어요.", () => Finish(true));
    }

    private void Finish(bool skipAll)
    {
        TutorialBook book = GameSession.instance.Tutorials;

        if (skipAll)
            book.SkipAll();
        else
            book.MarkDone(playingKey);

        lines = null;
        playingKey = null;
        SetPaused(false);
        dialog.Hide();
        spotlight.Hide();

        if (saveOnFinish)
            GameSession.instance.SaveInVillage();
    }

    private void SetPaused(bool value)
    {
        if (paused == value)
            return;

        paused = value;

        if (value)
        {
            savedTimeScale = Time.timeScale;
            Time.timeScale = 0f;
            return;
        }

        Time.timeScale = savedTimeScale;
    }
}