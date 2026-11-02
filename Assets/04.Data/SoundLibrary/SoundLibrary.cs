using System;
using System.Collections.Generic;
using UnityEngine;

public enum BgmType
{
    None = 0,
    Camp = 1,
    Dungeon = 2,
    Catacomb = 3,
}

public enum SfxType
{
    None = 0,

    [InspectorName("UI/UiClick")] UiClick = 100,
    [InspectorName("UI/UiBuy")] UiBuy = 101,
    [InspectorName("UI/UiSell")] UiSell = 102,
    [InspectorName("UI/BlessingPick")] BlessingPick = 103,
    [InspectorName("UI/QuestComplete")] QuestComplete = 104,
    [InspectorName("UI/ForgeSuccess")] ForgeSuccess = 105,
    [InspectorName("UI/ForgeFail")] ForgeFail = 106,
    [InspectorName("UI/PotionCaseOpen")] PotionCaseOpen = 107,
    [InspectorName("UI/ItemPick")] ItemPick = 108,
    [InspectorName("UI/ItemDrop")] ItemDrop = 109,
    [InspectorName("UI/Coins")] Coins = 110,
    [InspectorName("UI/BookOpen")] BookOpen = 111,
    [InspectorName("UI/BookFlip")] BookFlip = 112,

    //Class
    [InspectorName("직업/공격/SwordAttack")] SwordAttack = 200,
    [InspectorName("직업/공격/BowAttack")] BowAttack = 201,
    [InspectorName("직업/피격/Die")] Die = 202,
    [InspectorName("직업/공격/AxeAttack")] AxeAttack = 203,
    [InspectorName("직업/이동/Footsteps")] Footsteps = 204,
    [InspectorName("직업/이동/FootstepsWood")] FootstepsWood = 205,
    [InspectorName("직업/공격/DaggerAttack")] DaggerAttack = 206,
    [InspectorName("직업/공격/StaffAttack")] StaffAttack = 207,
    [InspectorName("직업/공격/GreatswordAttack")] GreatswordAttack = 208,
    [InspectorName("직업/피격/HurtRogue")] HurtRogue = 209,
    [InspectorName("직업/피격/HurtElf")] HurtElf = 210,
    [InspectorName("직업/물약/PotionDrink")] PotionDrink = 211,
    [InspectorName("직업/이동/Run")] Run = 212,
    [InspectorName("직업/피격/HurtMale")] HurtMale = 213,
    [InspectorName("직업/피격/HurtMage")] HurtMage = 214,

    //Battle
    [InspectorName("전투/FloorPoison")] FloorPoison = 300,
    [InspectorName("전투/RestEnter")] RestEnter = 301,
    [InspectorName("전투/StatusFire")] StatusFire = 302,
    [InspectorName("전투/StatusIce")] StatusIce = 303,
    [InspectorName("전투/StatusPoison")] StatusPoison = 304,

    //Tower
    [InspectorName("탑/TowerRumble")] TowerRumble = 400,
    [InspectorName("탑/TowerCollapse")] TowerCollapse = 401,
    [InspectorName("탑/TowerRise")] TowerRise = 402,
    [InspectorName("탑/TowerDoorOpen")] TowerDoorOpen = 403,

    //Monster
    [InspectorName("몬스터/공격/GoblinAttack")] GoblinAttack = 500,
    [InspectorName("몬스터/공격/MushroomAttack")] MushroomAttack = 501,
    [InspectorName("몬스터/공격/SkeletonAttack")] SkeletonAttack = 502,
    [InspectorName("몬스터/공격/BluntAttack")] BluntAttack = 503,
    [InspectorName("몬스터/공격/FireAttack")] FireAttack = 504,
    [InspectorName("몬스터/공격/IceAttack")] IceAttack = 505,
    [InspectorName("몬스터/공격/SlimeAttack")] SlimeAttack = 506,
    [InspectorName("몬스터/공격/WhipAttack")] WhipAttack = 507,
    [InspectorName("몬스터/공격/BeastAttack")] BeastAttack = 508,
    [InspectorName("몬스터/공격/GruntAttack")] GruntAttack = 509,
    [InspectorName("몬스터/공격/BugAttack")] BugAttack = 510,
    [InspectorName("몬스터/죽음/MonsterDeath")] MonsterDeath = 550,
    [InspectorName("몬스터/죽음/WraithDeath")] WraithDeath = 551,
    [InspectorName("몬스터/죽음/BoneDeath")] BoneDeath = 552,
    [InspectorName("몬스터/죽음/RatDeath")] RatDeath = 553,
    [InspectorName("몬스터/기타/BossRoar")] BossRoar = 554,
    [InspectorName("몬스터/기타/Revive")] Revive = 555,
    [InspectorName("몬스터/죽음/GoblinDeath")] GoblinDeath = 556,
    [InspectorName("몬스터/죽음/BigDeath")] BigDeath = 557,
    [InspectorName("몬스터/기타/Stomp")] Stomp = 558,
    [InspectorName("몬스터/기타/MonsterHit")] MonsterHit = 559,
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