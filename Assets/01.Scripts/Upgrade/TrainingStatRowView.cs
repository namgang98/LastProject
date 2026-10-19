using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TrainingStatRowView : MonoBehaviour
{
    [SerializeField] TrainingStat stat;
    [SerializeField] TMP_Text nameText;
    [SerializeField] TMP_Text valueText;
    [SerializeField] Image[] pips;
    [SerializeField] Color onColor = new(0.69f, 0.21f, 0.16f, 1f);
    [SerializeField] Color offColor = new(0.12f, 0.1f, 0.1f, 1f);
    [SerializeField] Button plusButton;

    public TrainingStat Stat => stat;
    public event Action<TrainingStat> PlusClicked;

    private void Awake()
    {
        plusButton.onClick.AddListener(() => PlusClicked?.Invoke(stat));
    }

    public void Show(TrainingStatInfo info, int points, bool canAdd)
    {
        gameObject.SetActive(true);
        nameText.text = info.Description;
        valueText.text = info.FormatValue(points);

        for(int i = 0; i < pips.Length; i++)
        {
            bool used = i < info.MaxPoints;
            pips[i].gameObject.SetActive(used);

            if(!used)
                continue;

            pips[i].color = i < points ? onColor : offColor;
        }

        plusButton.interactable = canAdd;
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}