using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FavorView : MonoBehaviour
{
    [SerializeField] TMP_Text levelText;
    [SerializeField] TMP_Text effectText;
    [SerializeField] Image gaugeFill;
    [SerializeField] TMP_Text gaugeText;

    private UpgradeProgress progress;
    private NpcType npc;

    private void OnDisable()
    {
        Unbind();
    }

    public void Show(NpcType target)
    {
        Unbind();
        npc = target;

        if(!GameSession.instance.Favor.TryGet(npc, out progress))
        {
            gameObject.SetActive(false);
            return;
        }

        gameObject.SetActive(true);
        progress.Changed += HandleChanged;
        Refresh();
    }

    private void Unbind()
    {
        if(progress == null)
            return;

        progress.Changed -= HandleChanged;
        progress = null;
    }

    private void HandleChanged(UpgradeProgress changed)
    {
        Refresh();
    }

    private void Refresh()
    {
        levelText.text = $"{npc.ToKorean()} 우호도 Lv.{progress.Level}";
        effectText.text = progress.Current.Description;

        if(progress.IsMaxed)
        {
            gaugeFill.fillAmount = 1f;
            gaugeText.text = "MAX";
            return;
        }

        gaugeFill.fillAmount = (float)progress.Deposited / progress.NextCost;
        gaugeText.text = $"{progress.Deposited} / {progress.NextCost}";
    }
}
