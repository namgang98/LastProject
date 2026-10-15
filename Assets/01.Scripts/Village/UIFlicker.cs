using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Graphic))]
public class UIFlicker : MonoBehaviour
{
    [SerializeField] float speed = 6f;
    [SerializeField, Range(0f, 1f)] float alphaMin = 0.6f;
    [SerializeField, Range(0f, 1f)] float alphaMax = 1f;
    [SerializeField] float scaleAmount = 0.05f;
    [SerializeField] bool onlyY = false;

    private Graphic graphic;
    private RectTransform rect;
    private Color baseColor;
    private float seed;

    private void Awake()
    {
        graphic = GetComponent<Graphic>();
        rect = (RectTransform)transform;
        baseColor = graphic.color;
        seed = Random.value * 100f;
    }

    private void Update()
    {
        float n = Mathf.PerlinNoise(seed, Time.time * speed);

        Color c = baseColor;
        c.a = Mathf.Lerp(alphaMin, alphaMax, n);
        graphic.color = c;

        float s = 1f + (n - 0.5f) * 2f * scaleAmount;

        if (onlyY)
        {
            rect.localScale = new Vector3(1f, s, 1f);
            return;
        }

        rect.localScale = new Vector3(s, s, 1f);
    }
}