using DG.Tweening;
using System;
using UnityEngine;
public interface IRevivable
{
    event Action OnRevived;
    void SetRevive(float rate);
}

public class ReviveEffect : MonoBehaviour
{
    [SerializeField] MonsterHealth health;
    [SerializeField] SpriteRenderer body;
    [SerializeField] Color flashColor = new Color(0.6f, 0.4f, 1f);
    [SerializeField] float flashTime = 0.12f;
    [SerializeField] int flashCount = 3;
    [SerializeField] float punchScale = 0.15f;
    [SerializeField] ParticleSystem soulBurst;
    [SerializeField] SfxType reviveSfx = SfxType.None;

    private void OnEnable()
    {
        health.OnRevived += Play;
    }

    private void OnDisable()
    {
        health.OnRevived -= Play;
    }

    private void Play()
    {
        body.DOKill();
        body.color = Color.white;
        body.DOColor(flashColor, flashTime).SetLoops(flashCount * 2, LoopType.Yoyo).SetLink(gameObject);
        transform.DOPunchScale(Vector3.one * punchScale, 0.4f, 6, 0.5f).SetLink(gameObject);

        if(soulBurst != null)
            soulBurst.Play();

        if(reviveSfx != SfxType.None)
            SoundManager.instance.PlaySfx(reviveSfx);
    }
}
