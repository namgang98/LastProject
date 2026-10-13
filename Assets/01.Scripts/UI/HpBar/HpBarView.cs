using UnityEngine;
using UnityEngine.UI;

public class HpBarView : MonoBehaviour
{
    [SerializeField] Image fillImage;
    private UnitHealth target;
    
    public void Bind(UnitHealth health)
    {
        if(target != null)
            target.OnHpChanged -= Refresh;

        target = health;
        target.OnHpChanged += Refresh;

        Refresh(health.CurrentHP, health.MaxHp);

    }

    private void Refresh(int current, int max)
    {
        if(max <= 0)
            return;

        fillImage.fillAmount = (float)current / max;
    }

    private void OnDestroy()
    {
        if(target != null)
            target.OnHpChanged -= Refresh;
    }
}
