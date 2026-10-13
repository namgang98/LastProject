using UnityEngine;
using UnityEngine.Pool;

public class DamageTextSpawner : MonoBehaviour
{
    [SerializeField] Color damageColor = Color.red;
    [SerializeField] Color healColor = Color.green;
    [SerializeField] Color blockColor = Color.gray;
    [SerializeField] Color critColor = Color.yellow;

    [SerializeField] DamageText prefab;
    [SerializeField] float jitterX = 0.3f;     
    [SerializeField] float stackSpacing = 0.3f;
    [SerializeField] float critScale = 1.4f;

    private int lastFrame = -1;
    private int stackCount;

    ObjectPool<DamageText> pool;

    private void Awake()
    {
        pool = new ObjectPool<DamageText>(
            //풀 채우기
            createFunc: () => Instantiate(prefab, transform),
            //풀로 꺼내오기
            actionOnGet: t => t.gameObject.SetActive(true),
            //풀로 리턴하기
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

        Spawn($"-{e.Info.Amount}", damageColor, e.Position, 1f);
    }
    private void HandleUnitHealed(OnUnitHealed e)
    {
        Spawn($"+{e.Value}", healColor, e.Position, 1f);
    }
    private void HandleUnitBlocked(OnUnitBlocked e)
    {
        Spawn($"막음", blockColor, e.Position, 1f);
    }
    private void Spawn(string value, Color color, Vector3 pos, float scale)
    {
        stackCount = Time.frameCount == lastFrame ? stackCount + 1 : 0;
        lastFrame = Time.frameCount;

        Vector3 offset = new(Random.Range(-jitterX, jitterX), stackCount * stackSpacing, 0f);

        DamageText text = pool.Get();
        text.Play(value, color, pos + offset, scale,pool.Release);
    }
}
