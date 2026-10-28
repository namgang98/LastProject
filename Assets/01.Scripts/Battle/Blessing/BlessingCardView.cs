using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BlessingCardView : MonoBehaviour
{
    [SerializeField] Button button;
    [SerializeField] Image frame;
    [SerializeField] TMP_Text nameText;
    [SerializeField] TMP_Text descText;
    [SerializeField] TMP_Text stackText;

    private BlessingInfo info;

    public event Action<BlessingInfo> Clicked;

    private void Awake()
    {
        button.onClick.AddListener(HandleClick);
    }

    public void Bind(BlessingInfo blessing, int owned)
    {
        info = blessing;
        Color color = GradeStyle.GetColor(blessing.Grade);

        nameText.text = blessing.Name;
        nameText.color = color;
        frame.color = color;
        descText.text = blessing.Description;
        stackText.text = blessing.MaxStack > 1 ? $"{owned} / {blessing.MaxStack}" : string.Empty;
    }

    private void HandleClick()
    {
        if(info == null)
            return;

        Clicked?.Invoke(info);
    }
}