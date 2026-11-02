using DG.Tweening;
using System;
using UnityEngine;

[Serializable]
public class FootstepSettings
{
    public SfxType sfx = SfxType.Footsteps;
    public int count = 4;
    public float interval = 0.12f;
    [Range(0f, 1f)] public float endVolume = 0.4f;
}

public class PlayerWalk : MonoBehaviour
{
    
    [Header("이동")]
    [SerializeField] float exitDistance = 6f;
    [SerializeField] float enterDistance = 6f;
    [SerializeField] float walkOutTime = 0.6f;
    [SerializeField] float walkInTime = 0.9f;
    
    [Header("발소리")]
    [SerializeField] FootstepSettings footsteps = new();
    
    private UnitController runner;
    private Vector3 home;
    private Tween tween;
    
    private void OnDisable()
    {
        tween?.Kill();
    }
    
    public void Bind(UnitController unit)
    {
        runner = unit;
        home = unit.transform.localPosition;
    }
    
    public void WalkOut()
    {
        Walk(home + Vector3.right * exitDistance, walkOutTime, footsteps.endVolume, null);
    }
    
    public void WalkIn(Action onArrived)
    {
        runner.transform.localPosition = home + Vector3.left * enterDistance;
        Walk(home, walkInTime, 1f, onArrived);
    }
    
    private void Walk(Vector3 target, float time, float endVolume, Action onArrived)
    {
        tween?.Kill();
        runner.SetMoving(true);
    
        int steps = Mathf.Max(1, Mathf.RoundToInt(time / footsteps.interval));
        SoundManager.instance.PlaySfxRepeat(footsteps.sfx, steps, footsteps.interval, endVolume);
    
        tween = runner.transform.DOLocalMove(target, time)
            .SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                runner.SetMoving(false);
                onArrived?.Invoke();
            })
            .SetLink(runner.gameObject);
    }
}
