using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;

[Serializable]
public class CutsceneLines
{
    public CutsceneKind kind;
    [TextArea(2, 4)] public string[] intro;
    [TextArea(2, 4)] public string[] outro;
}

public class TowerMapCutscene : MonoBehaviour
{
    [SerializeField] TowerCutsceneSlot[] slots;
    [SerializeField] RectTransform shakeRoot;

    [Header("연출")]
    [SerializeField] float calmTime = 0.6f;
    [SerializeField] float collapseTime = 2.4f;
    [SerializeField] float riseTime = 2f;
    [SerializeField] float riseInterval = 0.7f;
    [SerializeField] float towerShake = 6f;
    [SerializeField] float mapShake = 10f;
    [SerializeField] float holdTime = 1.5f;

    [Header("스토리")]
    [SerializeField] CanvasGroup blackScreen;
    [SerializeField] StoryPlayer story;
    [SerializeField] CutsceneLines[] scripts;
    [SerializeField] float fadeDuration = 1.2f;

    [Header("사운드")]
    [SerializeField] SfxType collapseSfx = SfxType.None;
    [SerializeField] SfxType riseSfx = SfxType.None;

    private CutsceneKind kind;
    private TowerInfo focus;
    private bool finished;

    private void Start()
    {
        Time.timeScale = 1f;
        blackScreen.alpha = 1f;

        GameSession session = GameSession.instance;
        kind = session.PlayingCutscene;
        focus = session.CurrentTower;

        SetupSlots(session.Towers);
        StartCoroutine(PlayRoutine());
    }

    private void Update()
    {
        if(finished || !AdvanceInput.WasSkipPressed())
            return;

        Finish();
    }

    private IEnumerator PlayRoutine()
    {
        CutsceneLines lines = FindLines();

        yield return PlayLines(lines != null ? lines.intro : null);
        yield return Fade(0f);
        yield return new WaitForSecondsRealtime(calmTime);
        yield return PlayEvent();
        yield return new WaitForSecondsRealtime(holdTime);
        yield return Fade(1f);
        yield return PlayLines(lines != null ? lines.outro : null);

        Finish();
    }

    #region 슬롯
    private void SetupSlots(TowerProgress progress)
    {
        foreach(TowerCutsceneSlot slot in slots)
        {
            progress.TryGetTower(slot.TowerId, out TowerInfo info);
            slot.Bind(info);
            slot.SetState(GetStartState(info, progress));
        }
    }

    private TowerSlotState GetStartState(TowerInfo info, TowerProgress progress)
    {
        if(info == null)
            return TowerSlotState.Hidden;

        if(kind == CutsceneKind.TowersRise)
            return info.IsPrologue ? TowerSlotState.Ruin : TowerSlotState.Hidden;

        if(info == focus)
            return TowerSlotState.Standing;

        if(!progress.IsRisen(info))
            return TowerSlotState.Hidden;

        return progress.IsCollapsed(info) ? TowerSlotState.Ruin : TowerSlotState.Standing;
    }

    private TowerCutsceneSlot FindSlot(TowerInfo info)
    {
        foreach(TowerCutsceneSlot slot in slots)
        {
            if(slot.Info == info)
                return slot;
        }

        return null;
    }
    #endregion

    #region 연출
    private IEnumerator PlayEvent()
    {
        if(kind == CutsceneKind.TowersRise)
        {
            yield return RiseThemed();
            yield break;
        }

        yield return Collapse(FindSlot(focus));
    }

    private IEnumerator Collapse(TowerCutsceneSlot slot)
    {
        if(slot == null)
            yield break;

        PlaySfx(collapseSfx);
        ShakeMap(collapseTime);
        yield return slot.PlayCollapse(collapseTime, towerShake).WaitForCompletion();
    }

    private IEnumerator RiseThemed()
    {
        Tween last = null;

        foreach(TowerCutsceneSlot slot in slots)
        {
            if(slot.Info == null || !slot.Info.IsThemed)
                continue;

            PlaySfx(riseSfx);
            ShakeMap(riseTime);
            last = slot.PlayRise(riseTime, towerShake);
            yield return new WaitForSecondsRealtime(riseInterval);
        }

        if(last != null)
            yield return last.WaitForCompletion();
    }

    private void ShakeMap(float duration)
    {
        shakeRoot.DOShakeAnchorPos(duration, mapShake, 25, 90f, false, true)
            .SetUpdate(true)
            .SetLink(shakeRoot.gameObject);
    }

    private IEnumerator Fade(float target)
    {
        yield return blackScreen.DOFade(target, fadeDuration).SetUpdate(true).WaitForCompletion();
    }
    #endregion

    #region 대사
    private CutsceneLines FindLines()
    {
        foreach(CutsceneLines lines in scripts)
        {
            if(lines.kind == kind)
                return lines;
        }

        return null;
    }

    private IEnumerator PlayLines(string[] lines)
    {
        if(lines == null || lines.Length == 0)
            yield break;

        var formatted = new string[lines.Length];

        for(int i = 0; i < lines.Length; i++)
        {
            formatted[i] = lines[i].Replace("{tower}", focus.Name);
        }

        yield return story.Play(formatted);
    }
    #endregion

    private void Finish()
    {
        if(finished)
            return;

        finished = true;
        StopAllCoroutines();
        DOTween.Kill(shakeRoot);
        SceneLoader.instance.Load(kind == CutsceneKind.OriginRise ? SceneNames.TowerRise : SceneNames.Village);
    }

    private void PlaySfx(SfxType type)
    {
        if(type == SfxType.None || !SoundManager.HasInstance)
            return;

        SoundManager.instance.PlaySfx(type);
    }
}