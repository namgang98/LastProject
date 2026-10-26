using System.Collections;
using DG.Tweening;
using UnityEngine;

public class TowerRiseCutscene : MonoBehaviour
{
    private static readonly int RumbleHash = Animator.StringToHash("Rumble");
    private static readonly int RiseHash = Animator.StringToHash("Rise");
    private static readonly int GlowHash = Animator.StringToHash("Glow");

    [Header("탑")]
    [SerializeField] Animator towerAnimator;

    [Header("지진")]
    [SerializeField] Transform cameraRoot;
    [SerializeField] float calmTime = 1f;
    [SerializeField] float rumbleTime = 1.6f;
    [SerializeField] float rumbleStrength = 0.06f;
    [SerializeField] float quakeStrength = 0.3f;
    [SerializeField] ParticleSystem[] riseEffects;

    [Header("심연 빛")]
    [SerializeField] SpriteRenderer abyssGlow;
    [SerializeField] float glowAlpha = 0.85f;
    [SerializeField] CanvasGroup flash;

    [Header("스토리")]
    [SerializeField] CanvasGroup blackScreen;
    [SerializeField] StoryPlayer story;
    [SerializeField] float afterGlowHold = 1.2f;
    [SerializeField] float fadeDuration = 1.2f;
    [SerializeField] float endHold = 1f;

    [Header("사운드")]
    [SerializeField] SfxType rumbleSfx = SfxType.None;
    [SerializeField] SfxType quakeSfx = SfxType.None;
    [SerializeField] SfxType flashSfx = SfxType.None;

    private bool finished;

    private void Start()
    {
        Time.timeScale = 1f;
        blackScreen.alpha = 0f;
        flash.alpha = 0f;
        SetGlow(0f);

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
        yield return new WaitForSecondsRealtime(calmTime);
        yield return Rumble();
        yield return Rise();
        yield return Flash();
        yield return new WaitForSecondsRealtime(afterGlowHold);
        yield return blackScreen.DOFade(1f, fadeDuration).SetUpdate(true).WaitForCompletion();
        yield return story.Play();
        yield return new WaitForSecondsRealtime(endHold);

        Finish();
    }

    private IEnumerator Rumble()
    {
        PlaySfx(rumbleSfx);
        towerAnimator.Play(RumbleHash, 0, 0f);

        yield return cameraRoot.DOShakePosition(rumbleTime, rumbleStrength, 30, 90f, false, false)
            .SetUpdate(true)
            .WaitForCompletion();
    }

    private IEnumerator Rise()
    {
        PlaySfx(quakeSfx);
        SetEffects(true);
        towerAnimator.Play(RiseHash, 0, 0f);

        yield return null;

        float length = towerAnimator.GetCurrentAnimatorStateInfo(0).length;

        yield return cameraRoot.DOShakePosition(length, quakeStrength, 25, 90f, false, true)
            .SetUpdate(true)
            .WaitForCompletion();

        SetEffects(false);
    }

    private IEnumerator Flash()
    {
        PlaySfx(flashSfx);

        yield return flash.DOFade(1f, 0.08f).SetUpdate(true).WaitForCompletion();

        towerAnimator.Play(GlowHash, 0, 0f);
        SetGlow(glowAlpha);

        yield return flash.DOFade(0f, 0.8f).SetUpdate(true).WaitForCompletion();
    }

    private void Finish()
    {
        if(finished)
            return;

        finished = true;
        StopAllCoroutines();
        DOTween.Kill(cameraRoot);
        SceneLoader.instance.Load(SceneNames.Village);
    }

    private void SetEffects(bool play)
    {
        foreach(ParticleSystem effect in riseEffects)
        {
            if(play)
                effect.Play();
            else
                effect.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }
    }

    private void SetGlow(float alpha)
    {
        if(abyssGlow == null)
            return;

        Color color = abyssGlow.color;
        color.a = alpha;
        abyssGlow.color = color;
    }

    private void PlaySfx(SfxType type)
    {
        if(type == SfxType.None || !SoundManager.HasInstance)
            return;

        SoundManager.instance.PlaySfx(type);
    }
}