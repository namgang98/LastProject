using System.Collections;
using DG.Tweening;
using UnityEngine;

public class TowerRiseCutscene : MonoBehaviour
{
    [Header("탑")]
    [SerializeField] CrossfadeSpritePlayer tower;
    [SerializeField] Sprite idleFrame;
    [SerializeField] Sprite[] riseFrames;
    [SerializeField] float riseFrameTime = 0.25f;
    [SerializeField] float towerShakeStrength = 0.06f;

    [Header("지진")]
    [SerializeField] Transform cameraRoot;
    [SerializeField] float calmTime = 0.8f;
    [SerializeField] float rumbleTime = 1.6f;
    [SerializeField] float rumbleStrength = 0.05f;
    [SerializeField] float quakeStrength = 0.3f;
    [SerializeField] ParticleSystem[] riseEffects;

    [Header("심연 빛")]
    [SerializeField] SpriteRenderer abyssGlow;
    [SerializeField] float glowAlpha = 0.85f;
    [SerializeField] float glowFadeTime = 1f;
    [SerializeField] CanvasGroup flash;

    [Header("스토리")]
    [SerializeField] CanvasGroup blackScreen;
    [SerializeField] StoryPlayer story;
    [SerializeField, TextArea(2, 4)] string[] introLines;
    [SerializeField, TextArea(2, 4)] string[] outroLines;
    [SerializeField] float fadeDuration = 1.2f;
    [SerializeField] float afterGlowHold = 1.5f;
    [SerializeField] float endHold = 1f;

    [Header("사운드")]
    [SerializeField] SfxType rumbleSfx = SfxType.None;
    [SerializeField] SfxType quakeSfx = SfxType.None;
    [SerializeField] SfxType flashSfx = SfxType.None;

    private bool finished;

    private void Start()
    {
        Time.timeScale = 1f;
        blackScreen.alpha = 1f;
        flash.alpha = 0f;
        SetGlow(0f);
        tower.Show(idleFrame);

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
        yield return story.Play(introLines);
        yield return Fade(0f);
        yield return new WaitForSecondsRealtime(calmTime);
        yield return Rumble();
        yield return Rise();
        yield return Flash();
        yield return new WaitForSecondsRealtime(afterGlowHold);
        yield return Fade(1f);
        yield return story.Play(outroLines);
        yield return new WaitForSecondsRealtime(endHold);

        Finish();
    }

    private IEnumerator Fade(float target)
    {
        yield return blackScreen.DOFade(target, fadeDuration).SetUpdate(true).WaitForCompletion();
    }

    private IEnumerator Rumble()
    {
        PlaySfx(rumbleSfx);

        tower.transform.DOShakePosition(rumbleTime, towerShakeStrength, 40, 90f, false, false)
            .SetUpdate(true)
            .SetLink(tower.gameObject);

        yield return cameraRoot.DOShakePosition(rumbleTime, rumbleStrength, 30, 90f, false, false)
            .SetUpdate(true)
            .WaitForCompletion();
    }

    private IEnumerator Rise()
    {
        PlaySfx(quakeSfx);
        SetEffects(true);

        float length = riseFrames.Length * riseFrameTime;

        cameraRoot.DOShakePosition(length, quakeStrength, 25, 90f, false, true)
            .SetUpdate(true)
            .SetLink(cameraRoot.gameObject);

        yield return tower.Play(riseFrames, riseFrameTime);

        SetEffects(false);
    }

    private IEnumerator Flash()
    {
        PlaySfx(flashSfx);

        yield return flash.DOFade(1f, 0.08f).SetUpdate(true).WaitForCompletion();

        if(abyssGlow != null)
            abyssGlow.DOFade(glowAlpha, glowFadeTime).SetUpdate(true).SetLink(abyssGlow.gameObject);

        yield return flash.DOFade(0f, 0.8f).SetUpdate(true).WaitForCompletion();
    }

    private void Finish()
    {
        if(finished)
            return;

        finished = true;
        StopAllCoroutines();
        DOTween.Kill(cameraRoot);
        DOTween.Kill(tower.transform);
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