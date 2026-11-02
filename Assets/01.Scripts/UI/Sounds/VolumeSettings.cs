using System;
using UnityEngine;
using UnityEngine.Audio;

public enum SoundChannel { Master, Bgm, Sfx }

public class VolumeSettings
{
    public const int ChannelCount = 3;

    private const string PrefPrefix = "Volume_";
    private const float MinVolume = 0.0001f;
    private const float MinDb = -80f;

    private static readonly string[] ParamNames = { "MasterVolume", "BgmVolume", "SfxVolume" };
    private static readonly float[] Defaults = { 1f, 0.6f, 0.8f };

    private readonly AudioMixer mixer;
    private readonly float[] volumes = new float[ChannelCount];

    public event Action<SoundChannel, float> Changed;

    public VolumeSettings(AudioMixer mixer)
    {
        this.mixer = mixer;
    }

    public float Get(SoundChannel channel) => volumes[(int)channel];

    public void Set(SoundChannel channel, float value)
    {
        int index = (int)channel;
        volumes[index] = Mathf.Clamp01(value);
        PlayerPrefs.SetFloat(PrefPrefix + channel, volumes[index]);
        Apply(index);
        Changed?.Invoke(channel, volumes[index]);
    }

    public void Load()
    {
        for(int i = 0; i < ChannelCount; i++)
        {
            volumes[i] = PlayerPrefs.GetFloat(PrefPrefix + (SoundChannel)i, Defaults[i]);
            Apply(i);
        }
    }

    public void Save()
    {
        PlayerPrefs.Save();
    }

    private void Apply(int index)
    {
        if(mixer == null)
            return;

        if(!mixer.SetFloat(ParamNames[index], ToDecibel(volumes[index])))
            Debug.LogWarning($"[VolumeSettings] 믹서에 '{ParamNames[index]}' 파라미터가 없습니다. Exposed Parameters 이름을 확인하세요.");
        mixer.SetFloat(ParamNames[index], ToDecibel(volumes[index]));
    }

    private static float ToDecibel(float value)
    {
        if(value <= MinVolume)
            return MinDb;

        return Mathf.Log10(value) * 20f;
    }
}