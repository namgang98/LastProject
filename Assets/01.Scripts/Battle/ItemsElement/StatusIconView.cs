using System;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class StatusSlot
{
    public DamageElement element;
    public GameObject root;
    public Image fill;
    [NonSerialized] public IStatusEffect status;
}
public class StatusIconView : MonoBehaviour
{
    [SerializeField] UnitController owner;
    [SerializeField] StatusSlot[] slots;
    private void Awake()
    {
        foreach(StatusSlot slot in slots)
        {
            slot.root.SetActive(false);
        }
    }
    private void OnEnable()
    {
        EventManager.instance.AddListener<OnStatusApplied>(Applied);
        EventManager.instance.AddListener<OnStatusRemoved>(Removed);
    }
    private void OnDisable()
    {
        if(!EventManager.HasInstance)    
            return;
        EventManager.instance.RemoveListener<OnStatusApplied>(Applied);
        EventManager.instance.RemoveListener<OnStatusRemoved>(Removed);
    }
    public void Bind(UnitController unit)
    {
        owner = unit;

        foreach(StatusSlot slot in slots)
        {
            slot.status = null;
            slot.root.SetActive(false);
        }
    }
    private void Applied(OnStatusApplied e)
    {
        if(owner == null || e.Target != owner.Health)
            return;

        StatusSlot slot = Find(e.Status.Element);

        if(slot == null)
            return;

        slot.status = e.Status;
        slot.root.SetActive(true);
    }
    private void Removed(OnStatusRemoved e)
    {
        if(owner == null || e.Target != owner.Health)
            return;

        StatusSlot slot = Find(e.Element);

        if(slot == null)
            return;

        slot.status = null;
        slot.root.SetActive(false);
    }
    private void Update()
    {
        foreach(StatusSlot slot in slots)
        {
            if(slot.status == null)
                continue;

            slot.fill.fillAmount = slot.status.Progress;
        }
    }
    private StatusSlot Find(DamageElement element)
    {
        foreach(StatusSlot slot in slots)
        {
            if(slot.element == element)
                return slot;
        }

        return null;
    }
}
