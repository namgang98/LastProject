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

    public void PlaySfx(SfxType type, float volumeScale = 1f)
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
        source.volume = entry.volume * volumeScale;
        source.pitch = entry.PickPitch();
        source.Play();
    }

    public void PlaySfxRepeat(SfxType type, int count, float interval, float endVolume = 1f)
    {
        if(type == SfxType.None || count <= 0)
            return;

        StartCoroutine(RepeatSfx(type, count, interval, endVolume));
    }

    private IEnumerator RepeatSfx(SfxType type, int count, float interval, float endVolume)
    {
        var wait = new WaitForSeconds(interval);

        for(int i = 0; i < count; i++)
        {
            float rate = count > 1 ? (float)i / (count - 1) : 0f;
            PlaySfx(type, Mathf.Lerp(1f, endVolume, rate));

            if(i < count - 1)
                yield return wait;
        }
    }

    private bool HasLibrary()
    {
        if(library != null)
            return true;

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