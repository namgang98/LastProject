using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class SoundManager : Singleton<SoundManager>
{
    [SerializeField] private SoundLibrary library;
    [SerializeField] private AudioMixer mixer;
    [SerializeField] private AudioMixerGroup bgmGroup;
    [SerializeField] private AudioMixerGroup sfxGroup;
    [SerializeField] private int sfxPoolSize = 12;
    [SerializeField] private float bgmFadeTime = 1.5f;
    [SerializeField] private float sfxMinInterval = 0.05f;
    private readonly Dictionary<SfxType, float> lastSfxTime = new();
    private BgmType currentBgm = BgmType.None;
    private readonly AudioSource[] bgmSources = new AudioSource[2];
    private AudioSource[] sfxSources;
    private int bgmIndex;
    private int sfxIndex;
    private Coroutine fadeRoutine;

    public VolumeSettings Volume { get; private set; }

    protected override void Awake()
    {
        base.Awake();

        if(instance != this)
            return;

        CreateSources();
        Volume = new VolumeSettings(mixer);
    }

    private void Start()
    {
        Volume?.Load();
    }

    public void PlayBgm(BgmType type)
    {
        if(type == currentBgm)
            return;

        if(!HasLibrary())
            return;

        if(!library.TryGetBgm(type, out SoundLibrary.BgmEntry entry))
        {
            Debug.LogWarning($"[SoundManager] 등록 안 된 BGM : {type}");
            return;
        }

        currentBgm = type;
        AudioSource from = bgmSources[bgmIndex];
        bgmIndex = 1 - bgmIndex;
        AudioSource to = bgmSources[bgmIndex];

        to.clip = entry.clip;
        to.volume = 0f;
        to.Play();

        StartFade(from, to, entry.volume);
    }
    public void StopBgm()
    {
        currentBgm = BgmType.None;
        StartFade(bgmSources[bgmIndex], null, 0f);
    }

    public void PlaySfx(SfxType type)
    {
        if(type == SfxType.None)
            return;

        if(!HasLibrary())
            return;

        if(!library.TryGetSfx(type, out SoundLibrary.SfxEntry entry))
        {
            Debug.LogWarning($"[SoundManager] 등록 안 된 SFX : {type}");
            return;
        }

        float now = Time.unscaledTime;

        if(lastSfxTime.TryGetValue(type, out float last) && now - last < sfxMinInterval)
            return;

        lastSfxTime[type] = now;

        AudioSource source = sfxSources[sfxIndex];
        sfxIndex = (sfxIndex + 1) % sfxSources.Length;

        source.clip = entry.PickClip();
        source.volume = entry.volume;
        source.pitch = entry.PickPitch();
        source.Play();
    }
    private bool HasLibrary()
    {
        if(library != null)
            return true;

        Debug.LogWarning("[SoundManager] SoundLibrary가 없습니다. 부트 씬에서 시작했는지 확인하세요.");
        return false;
    }

    private void StartFade(AudioSource from, AudioSource to, float target)
    {
        if(fadeRoutine != null)
            StopCoroutine(fadeRoutine);

        fadeRoutine = StartCoroutine(Crossfade(from, to, target));
    }

    private IEnumerator Crossfade(AudioSource from, AudioSource to, float target)
    {
        float fromStart = from.volume;
        float time = 0f;

        while(time < bgmFadeTime)
        {
            time += Time.unscaledDeltaTime;
            float rate = Mathf.Clamp01(time / bgmFadeTime);
            from.volume = Mathf.Lerp(fromStart, 0f, rate);

            if(to != null)
                to.volume = Mathf.Lerp(0f, target, rate);

            yield return null;
        }

        from.Stop();
        fadeRoutine = null;
    }

    private void CreateSources()
    {
        for(int i = 0; i < bgmSources.Length; i++)
            bgmSources[i] = CreateSource($"BGM_{i}", bgmGroup, true);

        sfxSources = new AudioSource[sfxPoolSize];

        for(int i = 0; i < sfxPoolSize; i++)
            sfxSources[i] = CreateSource($"SFX_{i}", sfxGroup, false);
    }

    private AudioSource CreateSource(string sourceName, AudioMixerGroup group, bool loop)
    {
        GameObject child = new(sourceName);
        child.transform.SetParent(transform);

        AudioSource source = child.AddComponent<AudioSource>();
        source.outputAudioMixerGroup = group;
        source.loop = loop;
        source.playOnAwake = false;
        source.spatialBlend = 0f;
        return source;
    }

    private void OnApplicationPause(bool paused)
    {
        if(paused)
            Volume?.Save();
    }

    protected override void OnApplicationQuit()
    {
        Volume?.Save();
        base.OnApplicationQuit();
    }
}