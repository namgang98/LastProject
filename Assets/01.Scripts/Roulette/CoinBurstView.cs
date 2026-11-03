using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class CoinBurstView : MonoBehaviour
{
    [SerializeField] RectTransform layer;
    [SerializeField] Image coinTemplate;
    [SerializeField] RectTransform origin;
    [SerializeField] RectTransform target;

    [Header("개수")]
    [SerializeField, Min(1)] int baseCoins = 8;
    [SerializeField, Min(0)] int coinsPerMultiplier = 2;
    [SerializeField, Min(1)] int maxCoins = 40;

    [Header("연출")]
    [SerializeField] float spread = 260f;
    [SerializeField] float jumpPower = 220f;
    [SerializeField] float burstTime = 0.6f;
    [SerializeField] float flyTime = 0.45f;
    [SerializeField] float stagger = 0.02f;

    private readonly List<Image> pool = new();

    private void Awake()
    {
        coinTemplate.gameObject.SetActive(false);
    }

    public void Play(int multiplier)
    {
        int count = Mathf.Clamp(baseCoins + multiplier * coinsPerMultiplier, 1, maxCoins);
        Vector3 start = layer.InverseTransformPoint(origin.position);

        for(int i = 0; i < count; i++)
        {
            Launch(Get(i), start, i * stagger);
        }
    }

    private void Launch(Image coin, Vector3 start, float delay)
    {
        RectTransform rect = coin.rectTransform;
        rect.DOKill();
        coin.DOKill();

        rect.localPosition = start;
        rect.localRotation = Quaternion.identity;
        rect.localScale = Vector3.one * Random.Range(0.7f, 1.1f);
        coin.color = Color.white;
        coin.gameObject.SetActive(true);

        Vector3 land = start + (Vector3)(Random.insideUnitCircle * spread);
        Sequence sequence = DOTween.Sequence()
            .SetDelay(delay)
            .SetUpdate(true)
            .SetLink(coin.gameObject);

        sequence.Append(rect.DOLocalJump(land, jumpPower * Random.Range(0.6f, 1.2f), 1, burstTime).SetEase(Ease.OutQuad));
        sequence.Join(rect.DOLocalRotate(new Vector3(0f, 0f, Random.Range(-720f, 720f)), burstTime, RotateMode.FastBeyond360));
        AppendFinish(sequence, coin);
        sequence.OnComplete(() => coin.gameObject.SetActive(false));
    }

    private void AppendFinish(Sequence sequence, Image coin)
    {
        if(target == null)
        {
            sequence.Append(coin.DOFade(0f, 0.3f));
            return;
        }

        Vector3 end = layer.InverseTransformPoint(target.position);
        sequence.Append(coin.rectTransform.DOLocalMove(end, flyTime).SetEase(Ease.InBack));
        sequence.Join(coin.rectTransform.DOScale(0.4f, flyTime));
    }

    private Image Get(int index)
    {
        while(pool.Count <= index)
        {
            Image coin = Instantiate(coinTemplate, layer);
            coin.raycastTarget = false;
            pool.Add(coin);
        }

        return pool[index];
    }
}