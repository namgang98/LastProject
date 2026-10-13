using DG.Tweening;
using UnityEngine;

public class LockView : MonoBehaviour
{
    [SerializeField] GameObject chains;
    [SerializeField] RectTransform icon;
    [SerializeField] DragController drag;

    private void OnEnable()
    {
        drag.OnDragBlocked += ShakeIcon;
    }
    private void OnDisable()
    {
        drag.OnDragBlocked -= ShakeIcon;
    }

    public void SetLocked(bool b)
    {
        chains.SetActive(b);
    }
    public void ShakeIcon()
    {
        icon.DOComplete();
        icon.DOShakeAnchorPos(0.25f, new Vector2(6f, 0f), 20, 0f).SetLink(icon.gameObject);
    }
}
