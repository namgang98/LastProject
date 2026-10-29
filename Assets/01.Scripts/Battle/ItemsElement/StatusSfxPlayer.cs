using System;
using UnityEngine;

public class StatusSfxPlayer : MonoBehaviour
{
    [Serializable]
    private struct ElementSfx
    {
        public DamageElement element;
        public SfxType sfx;
    }

    [SerializeField]
    ElementSfx[] sounds =
    {
        new ElementSfx { element = DamageElement.Fire, sfx = SfxType.StatusFire },
        new ElementSfx { element = DamageElement.Ice, sfx = SfxType.StatusIce },
        new ElementSfx { element = DamageElement.Poison, sfx = SfxType.StatusPoison },
    };

    private void OnEnable()
    {
        EventManager.instance.AddListener<OnStatusApplied>(HandleApplied);
    }

    private void OnDisable()
    {
        if(!EventManager.HasInstance)
            return;

        EventManager.instance.RemoveListener<OnStatusApplied>(HandleApplied);
    }

    private void HandleApplied(OnStatusApplied e)
    {
        foreach(ElementSfx entry in sounds)
        {
            if(entry.element != e.Status.Element)
                continue;

            Sfx.Play(entry.sfx);
            return;
        }
    }
}