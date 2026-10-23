using UnityEngine;
using UnityEngine.Pool;

[RequireComponent(typeof(RectTransform))]
public class CloudEmitter : MonoBehaviour
{
    [Header("구름 프리팹 / 이미지")]
    [SerializeField] CloudParticle prefab;
    [SerializeField] Sprite[] sprites;

    [Header("생성")]
    [SerializeField] FloatRange interval = new(5f, 10f);
    [SerializeField] int prewarmCount = 4;
    [SerializeField] int maxCount = 8;

    [Header("모양 / 움직임")]
    [SerializeField] FloatRange scale = new(0.7f, 1.3f);
    [SerializeField] FloatRange alpha = new(0.6f, 1f);
    [SerializeField] FloatRange speed = new(10f, 25f);
    [SerializeField, Range(0f, 1f)] float minHeightRate = 0.1f;
    [SerializeField, Range(0f, 1f)] float maxHeightRate = 0.9f;

    private RectTransform area;
    private ObjectPool<CloudParticle> pool;
    private float timer;

    private void Awake()
    {
        area = (RectTransform)transform;
        pool = new ObjectPool<CloudParticle>(CreateCloud, OnGetCloud, OnReleaseCloud, OnDestroyCloud, false, maxCount, maxCount);
    }

    private void OnEnable()
    {
        timer = interval.Random();

        for(int i = 0; i < prewarmCount; i++)
            Spawn(true);
    }

    private void OnDisable()
    {
        pool.Clear();

        for(int i = transform.childCount - 1; i >= 0; i--)
            Destroy(transform.GetChild(i).gameObject);
    }

    private void Update()
    {
        timer -= Time.deltaTime;

        if(timer > 0f)
            return;

        timer = interval.Random();
        Spawn(false);
    }

    private void Spawn(bool prewarm)
    {
        if(pool.CountActive >= maxCount || sprites.Length == 0)
            return;

        CloudParticle cloud = pool.Get();
        float size = scale.Random();

        cloud.Setup(sprites[Random.Range(0, sprites.Length)], size, Random.value < 0.5f, alpha.Random(), speed.Random() * size, Release);

        Rect rect = area.rect;
        float half = cloud.Width * 0.5f;
        float fromX = rect.xMin - half;
        float toX = rect.xMax + half;
        float x = prewarm ? Random.Range(fromX, toX) : fromX;
        float y = Mathf.Lerp(rect.yMin, rect.yMax, Random.Range(minHeightRate, maxHeightRate));

        cloud.Place(fromX, toX, x, y);
    }

    private void Release(CloudParticle cloud)
    {
        pool.Release(cloud);
    }

    private CloudParticle CreateCloud()
    {
        return Instantiate(prefab, transform);
    }

    private void OnGetCloud(CloudParticle cloud)
    {
        cloud.gameObject.SetActive(true);
        cloud.transform.SetAsLastSibling();
    }

    private void OnReleaseCloud(CloudParticle cloud)
    {
        cloud.gameObject.SetActive(false);
    }

    private void OnDestroyCloud(CloudParticle cloud)
    {
        Destroy(cloud.gameObject);
    }
}
