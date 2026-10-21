using System;
using System.Collections.Generic;
using UnityEngine;

public enum BgmType
{
    None = 0,
    Camp = 1,
    Dungeon = 2,
}

public enum SfxType
{
    None = 0,

    UiClick = 100,
    UiBuy = 101,
    UiSell = 102,

    //Class
    SwordAttack = 200,
    BowAttack = 201,
    Die = 202,
    AxeAttack = 203,
    Footsteps = 204,
    FootstepsWood = 205,

    //Monster
    GoblinAttack = 500,
    MushroomAttack = 501,
    SkeletonAttack = 502,
}

[CreateAssetMenu(fileName = "SoundLibrary", menuName = "Sound/SoundLibrary")]
public class SoundLibrary : ScriptableObject
{
    [Serializable]
    public class BgmEntry
    {
        public BgmType type;
        public AudioClip clip;
        [Range(0f, 1f)] public float volume = 0.8f;
    }

    [Serializable]
    public class SfxEntry
    {
        public SfxType type;
        public AudioClip[] clips;
        [Range(0f, 1f)] public float volume = 1f;
        [Range(0f, 0.3f)] public float pitchRandom = 0f;

        public AudioClip PickClip()
        {
            return clips[UnityEngine.Random.Range(0, clips.Length)];
        }

        public float PickPitch()
        {
            return 1f + UnityEngine.Random.Range(-pitchRandom, pitchRandom);
        }
    }

    [SerializeField] private List<BgmEntry> bgms = new();
    [SerializeField] private List<SfxEntry> sfxs = new();

    private Dictionary<BgmType, BgmEntry> bgmTable;
    private Dictionary<SfxType, SfxEntry> sfxTable;

    public bool TryGetBgm(BgmType type, out BgmEntry entry)
    {
        if(bgmTable == null)
            Build();

        return bgmTable.TryGetValue(type, out entry);
    }

    public bool TryGetSfx(SfxType type, out SfxEntry entry)
    {
        if(sfxTable == null)
            Build();

        return sfxTable.TryGetValue(type, out entry);
    }

    private void Build()
    {
        bgmTable = new Dictionary<BgmType, BgmEntry>();
        sfxTable = new Dictionary<SfxType, SfxEntry>();

        foreach(BgmEntry entry in bgms)
        {
            if(entry.type == BgmType.None || entry.clip == null)
            {
                Debug.LogWarning($"[SoundLibrary] 비어있는 BGM 항목 : {entry.type}");
                continue;
            }

            if(!bgmTable.TryAdd(entry.type, entry))
                Debug.LogWarning($"[SoundLibrary] 중복 BGM : {entry.type}");
        }

        foreach(SfxEntry entry in sfxs)
        {
            if(entry.type == SfxType.None || entry.clips == null || entry.clips.Length == 0)
            {
                Debug.LogWarning($"[SoundLibrary] 비어있는 SFX 항목 : {entry.type}");
                continue;
            }

            if(!sfxTable.TryAdd(entry.type, entry))
                Debug.LogWarning($"[SoundLibrary] 중복 SFX : {entry.type}");
        }
    }

    private void OnValidate()
    {
        bgmTable = null;
        sfxTable = null;
    }
}