using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class StageBackgroundView : MonoBehaviour
{
    private SpriteRenderer render;

    private void Awake()
    {
        render = GetComponent<SpriteRenderer>();
    }

    public void Show(Sprite sprite)
    {
        if(sprite == null)
            return;

        render.sprite = sprite;
    }
}