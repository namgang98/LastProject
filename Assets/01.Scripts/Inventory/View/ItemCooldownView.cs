using UnityEngine;
using UnityEngine.UI;

public class ItemCooldownView : MonoBehaviour
{
    [SerializeField] Image fill;
    private ICooldownSource source;


    private void Update()
    {
        if(source == null)
            return;

        fill.fillAmount = source.Progress;
    }

    public void Bind(ICooldownSource source)
    {
        this.source = source;
        fill.enabled = true;
    }
    public void Unbind()
    {
        source = null;
        fill.enabled = false;
    }

    public void SetOrientation(bool horizontal)
    {
        fill.fillMethod = horizontal ? Image.FillMethod.Horizontal : Image.FillMethod.Vertical;

        Vector3 localStart = horizontal ? Vector3.left : Vector3.down;
        Vector3 screenDir = fill.rectTransform.TransformDirection(localStart);
        bool startAtMin = screenDir.x + screenDir.y < 0f;

        if(horizontal)
        {
            fill.fillOrigin = startAtMin ? (int)Image.OriginHorizontal.Left : (int)Image.OriginHorizontal.Right;
            return;
        }

        fill.fillOrigin = startAtMin ? (int)Image.OriginVertical.Bottom : (int)Image.OriginVertical.Top;
    }
}
