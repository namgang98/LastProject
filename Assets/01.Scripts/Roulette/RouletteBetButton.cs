using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RouletteBetButton : MonoBehaviour
{
    [SerializeField] Button button;
    [SerializeField] TMP_Text multiplierText;
    [SerializeField] TMP_Text amountText;
    [SerializeField, Min(1)] int multiplier = 1;

    public int Multiplier => multiplier;

    public event Action<int> Clicked;

    private void Awake()
    {
        multiplierText.text = $"x{multiplier}";
        button.onClick.AddListener(() => Clicked?.Invoke(multiplier));
        SetAmount(0);
    }

    public void SetAmount(int amount)
    {
        amountText.text = amount > 0 ? amount.ToString() : string.Empty;
    }

    public void SetInteractable(bool interactable)
    {
        button.interactable = interactable;
    }
}