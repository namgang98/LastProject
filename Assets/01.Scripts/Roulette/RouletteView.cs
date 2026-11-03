using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class RouletteView : MonoBehaviour
{
    [SerializeField] GameObject panel;
    [SerializeField] RouletteWheelView wheel;
    [SerializeField] RouletteBetButton[] betButtons;

    [Header("칩")]
    [SerializeField] Button[] chipButtons;
    [SerializeField] int[] chipValues = { 10, 100, 500 };
    [SerializeField] GameObject[] chipDims;

    [Header("버튼")]
    [SerializeField] Button clearButton;
    [SerializeField] Button spinButton;
    [SerializeField] Button closeButton;

    [Header("표시")]
    [SerializeField] TMP_Text totalText;
    [SerializeField] TMP_Text resultText;
    [SerializeField] CoinBurstView coinBurst;

    [Header("규칙")]
    [SerializeField, Min(0.5f)] float messageTime = 2f;
    [SerializeField, Min(1)] int maxBet = 1000;
    [SerializeField] int[] slots =
    {
        1, 2, 3, 1, 5, 2, 1, 3, 10, 1, 2, 1, 5, 20, 3, 2, 1, 1, 2, 3, 1, 5, 2, 1, 3,
        10, 1, 2, 1, 5, 3, 2, 1, 1, 2, 3, 1, 5, 20, 2, 1, 3, 10, 1, 2, 1, 5, 3, 2, 1
    };

    private RouletteTable table;
    private RouletteBet bet;
    private int chipIndex;
    private bool spinning;

    private Wallet Wallet => GameSession.instance.Wallet;

    private void Awake()
    {
        table = new RouletteTable(slots);
        bet = new RouletteBet(maxBet);
        bet.Changed += RefreshBets;

        foreach(RouletteBetButton button in betButtons)
        {
            button.Clicked += PlaceBet;
        }

        for(int i = 0; i < chipButtons.Length; i++)
        {
            int index = i;
            chipButtons[i].onClick.AddListener(() => SelectChip(index));
        }

        clearButton.onClick.AddListener(ClearBet);
        spinButton.onClick.AddListener(Spin);
        closeButton.onClick.AddListener(Close);
        panel.SetActive(false);
    }

    public void Open()
    {
        PopupManager.instance.Open(panel, Close);
        resultText.text = string.Empty;
        SelectChip(chipIndex);
        RefreshBets();
    }

    public void Close()
    {
        if(spinning)
            return;

        bet.Clear();
        PopupManager.instance.Close(panel);
    }

    private void SelectChip(int index)
    {
        chipIndex = index;

        for(int i = 0; i < chipButtons.Length; i++)
        {
            bool selected = i == index;

            if(i < chipDims.Length && chipDims[i] != null)
                chipDims[i].SetActive(selected);
        }
    }

    private void PlaceBet(int multiplier)
    {
        if(spinning)
            return;

        if(bet.TryAdd(multiplier, chipValues[chipIndex], Wallet.Gold))
        {
            ShowMessage(string.Empty);
            return;
        }

        ShowMessage($"골드가 부족하거나 최대 {maxBet}골드까지 걸 수 있습니다", messageTime);
    }

    private void ClearBet()
    {
        if(spinning)
            return;

        bet.Clear();
    }

    private void Spin()
    {
        if(spinning || bet.Total <= 0)
            return;

        if(!Wallet.TrySpend(bet.Total))
        {
            ShowMessage("골드가 부족합니다", messageTime);
            return;
        }

        spinning = true;
        SetInteractable(false);
        resultText.text = string.Empty;

        int index = table.Spin();
        wheel.Spin(index, table.SlotCount, () => Finish(index));
        Sfx.Play(SfxType.RouletteSpin);
    }

    private void Finish(int index)
    {
        int multiplier = table.GetMultiplier(index);
        int payout = bet.GetPayout(multiplier);

        Wallet.Earn(payout);
        if(payout > 0 && coinBurst != null)
            coinBurst.Play(multiplier);

        resultText.text = payout > 0 ? $"x{multiplier} 당첨! +{payout}" : $"x{multiplier}... 꽝";
        Sfx.Play(payout > 0 ? SfxType.Coins : SfxType.UiClick);

        bet.Clear();
        GameSession.instance.SaveInVillage();
        spinning = false;
        SetInteractable(true);
    }

    private void RefreshBets()
    {
        foreach(RouletteBetButton button in betButtons)
        {
            button.SetAmount(bet.Get(button.Multiplier));
        }

        totalText.text = $"{bet.Total} / {maxBet}";
    }

    private void SetInteractable(bool interactable)
    {
        foreach(RouletteBetButton button in betButtons)
        {
            button.SetInteractable(interactable);
        }

        foreach(Button chip in chipButtons)
        {
            chip.interactable = interactable;
        }

        clearButton.interactable = interactable;
        spinButton.interactable = interactable;
        closeButton.interactable = interactable;
    }
    private void ShowMessage(string message, float hideAfter = 0f)
    {
        resultText.DOKill();
        resultText.text = message;

        if(hideAfter <= 0f)
            return;

        DOVirtual.DelayedCall(hideAfter, () => resultText.text = string.Empty)
            .SetTarget(resultText)
            .SetLink(resultText.gameObject);
    }
}