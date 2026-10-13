using UnityEngine;
using UnityEngine.Pool;

public class ItemViewPool : MonoBehaviour
{
    [SerializeField] private ItemView prefab;
    [SerializeField, Min(1)] private int defaultCapacity = 16;
    [SerializeField, Min(1)] private int maxSize = 64;

    private ObjectPool<ItemView> pool;
    private DragController drag;

    public void Init(DragController dragController)
    {
        if(pool != null)
            return;

        drag = dragController;
        pool = new ObjectPool<ItemView>(Create, OnGet, OnRelease, OnDestroyView, false, defaultCapacity, maxSize);
    }

    public ItemView Get()
    {
        return pool.Get();
    }

    public void Release(ItemView view)
    {
        pool.Release(view);
    }

    private ItemView Create()
    {
        ItemView view = Instantiate(prefab, transform);
        view.Init(drag);
        return view;
    }

    private void OnGet(ItemView view)
    {
        view.gameObject.SetActive(true);
    }

    private void OnRelease(ItemView view)
    {
        view.ResetView();
        view.transform.SetParent(transform, false);
        view.gameObject.SetActive(false);
    }

    private void OnDestroyView(ItemView view)
    {
        Destroy(view.gameObject);
    }
}
