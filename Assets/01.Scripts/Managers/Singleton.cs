using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;

    private static bool _applicationIsQuitting = false;

    public static bool HasInstance => !_applicationIsQuitting && _instance != null;

    public static T instance
    {
        get
        {
            if(_applicationIsQuitting)
            {
                Debug.LogWarning($"[Singleton] {typeof(T)} 인스턴스는 종료 중이라 접근할 수 없습니다.");
                return null;
            }

            if(_instance == null)
            {
                _instance = FindFirstObjectByType<T>();

                if(_instance == null)
                {
                    GameObject obj = new(typeof(T).Name);
                    _instance = obj.AddComponent<T>();

                    if(_instance is Singleton<T> singleton && singleton.isDontDestroy)
                        DontDestroyOnLoad(obj);
                }
            }
                return _instance;
        }
    }

    [SerializeField] protected bool isDontDestroy = true;

    protected virtual void Awake()
    {
        if(_instance == null)
        {
            _instance = this as T;

            if(isDontDestroy)
            {
                DontDestroyOnLoad(this.gameObject);
            }
        }
        else if(_instance != this)
        {
            Destroy(this.gameObject);
        }
    }

    protected virtual void OnDestroy()
    {
        if(_instance == this)
        {
            _instance = null;
        }
    }

    protected virtual void OnApplicationQuit()
    {
        _applicationIsQuitting = true;
    }
}