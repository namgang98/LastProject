using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class DamageTextSpawner : MonoBehaviour
{
    [Header("색")]
    [SerializeField] Color damageColor = Color.red;
    [SerializeField] Color healColor = Color.green;
    [SerializeField] Color blockColor = Color.gray;
    [SerializeField] Color critColor = Color.yellow;
    [SerializeField] Color poisonColor = new(0.55f, 0.85f, 0.3f);
    [SerializeField] Color fireColor = new(1f, 0.55f, 0.15f);

    [Header("배치")]
    [SerializeField] DamageText prefab;
    [SerializeField] float jitterX = 0.3f;
    [SerializeField] float stackSpacing = 0.3f;
    [SerializeField] float stackWindow = 0.2f;
    [SerializeField] float critScale = 1.4f;

    [Header("합치기")]
    [SerializeField] float mergeWindow = 0.25f;

    private struct Bucket
    {
        public DamageText Text;
        public int Total;
        public float Until;
    }

    private readonly Dictionary<long, Bucket> buckets = new();
    private ObjectPool<DamageText> pool;
    private float lastSpawnTime = -1f;
    private int stackCount;

    private void Awake()
    {
        pool = new ObjectPool<DamageText>(
            createFunc: () => Instantiate(prefab, transform),
            actionOnGet: t => t.gameObject.SetActive(true),
            actionOnRelease: t => t.gameObject.SetActive(false),
            actionOnDestroy: t => Destroy(t.gameObject),
            collectionCheck: true,
            defaultCapacity: 10,
            maxSize: 50);
    }

    private void OnEnable()
    {
        EventManager.instance.AddListener<OnUnitDamaged>(HandleUnitDamaged);
        EventManager.instance.AddListener<OnUnitHealed>(HandleUnitHealed);
        EventManager.instance.AddListener<OnUnitBlocked>(HandleUnitBlocked);
    }

    private void OnDisable()
    {
        if(!EventManager.HasInstance)
            return;

        EventManager.instance.RemoveListener<OnUnitDamaged>(HandleUnitDamaged);
        EventManager.instance.RemoveListener<OnUnitHealed>(HandleUnitHealed);
        EventManager.instance.RemoveListener<OnUnitBlocked>(HandleUnitBlocked);
    }

    private void HandleUnitDamaged(OnUnitDamaged e)
    {
        if(e.Info.IsCritical)
        {
            Spawn($"-{e.Info.Amount}!!!", critColor, e.Position, critScale);
            return;
        }

        long key = MakeKey(e.SourceId, e.Info);

        if(buckets.TryGetValue(key, out Bucket bucket) && Time.time <= bucket.Until)
        {
            bucket.Total += e.Info.Amount;
            bucket.Until = Time.time + mergeWindow;
            bucket.Text.Refresh($"-{bucket.Total}");
            buckets[key] = bucket;
            return;
        }

        DamageText text = Spawn($"-{e.Info.Amount}", GetDamageColor(e.Info), e.Position, 1f);
        buckets[key] = new Bucket { Text = text, Total = e.Info.Amount, Until = Time.time + mergeWindow };
    }

    private void HandleUnitHealed(OnUnitHealed e)
    {
        Spawn($"+{e.Value}", healColor, e.Position, 1f);
    }

    private void HandleUnitBlocked(OnUnitBlocked e)
    {
        Spawn("막음", blockColor, e.Position, 1f);
    }

    private DamageText Spawn(string value, Color color, Vector3 pos, float scale)
    {
        stackCount = Time.time - lastSpawnTime <= stackWindow ? stackCount + 1 : 0;
        lastSpawnTime = Time.time;

        Vector3 offset = new(Random.Range(-jitterX, jitterX), stackCount * stackSpacing, 0f);

        DamageText text = pool.Get();
        text.Play(value, color, pos + offset, scale, Release);
        return text;
    }

    private void Release(DamageText text)
    {
        long found = 0;
        bool hasKey = false;

        foreach(var pair in buckets)
        {
            if(pair.Value.Text != text)
                continue;

            found = pair.Key;
            hasKey = true;
            break;
        }

        if(hasKey)
            buckets.Remove(found);

        pool.Release(text);
    }

    private Color GetDamageColor(DamageInfo info)
    {
        if(!info.IsTick)
            return damageColor;

        if(info.Element == DamageElement.Poison)
            return poisonColor;

        if(info.Element == DamageElement.Fire)
            return fireColor;

        return damageColor;
    }

    private static long MakeKey(int sourceId, DamageInfo info)
    {
        int kind = info.IsTick ? (int)info.Element + 1 : 0;
        return ((long)sourceId << 8) | (uint)kind;
    }
}