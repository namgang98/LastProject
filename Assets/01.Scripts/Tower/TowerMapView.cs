using DG.Tweening;
using System;
using System.Collections;
using UnityEngine;

public class TowerMapView : MonoBehaviour
{
    [SerializeField] GameObject root;
    [SerializeField] CanvasGroup group;
    [SerializeField] TowerSlotView[] slots;

    public event Action<TowerInfo> TowerClicked;

    private void Awake()
    {
        foreach (TowerSlotView slot in slots)
        {
            slot.Clicked += HandleClicked;
        }
    }

    public void Show(TowerProgress progress)
    {
        root.SetActive(true);

        foreach (TowerSlotView slot in slots)
        {
            bool visible = progress.TryGetTower(slot.TowerId, out TowerInfo info) && progress.IsRisen(info);
            slot.gameObject.SetActive(visible);

            if (!visible)
                continue;

            slot.Bind(info, progress.IsCollapsed(info), progress.CanEnter(info));
        }
    }

    public void Hide()
    {
        root.SetActive(false);
    }

    public void SetInteractable(bool interactable)
    {
        group.interactable = interactable;
        group.blocksRaycasts = interactable;
    }

    public IEnumerator Enter(TowerInfo tower)
    {
        foreach (TowerSlotView slot in slots)
        {
            if (slot.Info != tower)
                continue;

            yield return slot.PlayEnter().WaitForCompletion();
            yield break;
        }
    }

    private void HandleClicked(TowerInfo tower)
    {
        TowerClicked?.Invoke(tower);
    }
}