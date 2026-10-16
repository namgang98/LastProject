using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DonationUI : MonoBehaviour
{
    [Header("대상")]
    [SerializeField] UpgradeTrack track = UpgradeTrack.Shop;
    [SerializeField] string title = "상점";

    [Header("팝업")]
    [SerializeField] GameObject popup;

    [Header("표시")]
    [SerializeField] TMP_Text levelText;
    [SerializeField] TMP_Text currentText;
    [SerializeField] TMP_Text nextText;
    [SerializeField] Image gaugeFill;
    [SerializeField] TMP_Text gaugeText;

    [Header("입력")]
    [SerializeField] Slider amountSlider;
    [SerializeField] TMP_InputField amountInput;
    [SerializeField] Button maxButton;
    [SerializeField] Button donateButton;
    [SerializeField] Button closeButton;

    private UpgradeProgress progress;
    private Wallet wallet;
    private int amount;

    private int MaxAmount => progress == null ? 0 : Mathf.Min(wallet.Gold, progress.Remaining);

    private void Awake()
    {
        amountSlider.wholeNumbers = true;
        amountSlider.minValue = 0;
        amountInput.contentType = TMP_InputField.ContentType.IntegerNumber;

        amountSlider.onValueChanged.AddListener(HandleSliderChanged);
        amountInput.onEndEdit.AddListener(HandleInputEnd);
        maxButton.onClick.AddListener(() => SetAmount(MaxAmount));
        donateButton.onClick.AddListener(Donate);
        closeButton.onClick.AddListener(Close);
    }

    private void OnEnable()
    {
        EventManager.instance.AddListener<OnChangeGold>(HandleGoldChanged);
    }

    private void OnDisable()
    {
        if(!EventManager.HasInstance)
            return;

        EventManager.instance.RemoveListener<OnChangeGold>(HandleGoldChanged);
    }

    private void OnDestroy()
    {
        Unsubscribe();
    }

    public void Open()
    {
        GameSession session = GameSession.instance;
        wallet = session.Wallet;

        if(!session.Upgrades.TryGet(track, out UpgradeProgress found))
        {
            Debug.LogWarning($"[DonationUI] {track} 강화 데이터가 없습니다.", this);
            return;
        }

        Unsubscribe();
        progress = found;
        progress.Changed += HandleProgressChanged;

        PopupManager.instance.Open(popup, Close);
        amount = 0;
        Refresh();
    }

    public void Close()
    {
        Unsubscribe();
        PopupManager.instance.Close(popup);
    }

    private void Unsubscribe()
    {
        if(progress == null)
            return;

        progress.Changed -= HandleProgressChanged;
        progress = null;
    }

    private void Refresh()
    {
        if(progress == null)
            return;

        levelText.text = $"{title} Lv.{progress.Level}";
        currentText.text = $"현재 : {progress.Current.Description}";

        if(progress.IsMaxed)
        {
            ShowMaxed();
            return;
        }

        nextText.text = $"다음 : {progress.Next.Description}";
        gaugeFill.fillAmount = (float)progress.Deposited / progress.NextCost;
        gaugeText.text = $"{progress.Deposited} / {progress.NextCost}";

        int max = MaxAmount;
        amountSlider.maxValue = max;
        SetInteractable(max > 0);
        SetAmount(amount);
    }

    private void ShowMaxed()
    {
        nextText.text = "최대 레벨입니다";
        gaugeFill.fillAmount = 1f;
        gaugeText.text = "MAX";
        amountSlider.maxValue = 0;
        SetInteractable(false);
        SetAmount(0);
    }

    private void SetInteractable(bool value)
    {
        amountSlider.interactable = value;
        amountInput.interactable = value;
        maxButton.interactable = value;
    }

    private void SetAmount(int value)
    {
        amount = Mathf.Clamp(value, 0, MaxAmount);
        amountSlider.SetValueWithoutNotify(amount);
        amountInput.SetTextWithoutNotify(amount.ToString());
        donateButton.interactable = amount > 0;
    }

    private void HandleSliderChanged(float value)
    {
        SetAmount(Mathf.RoundToInt(value));
    }

    private void HandleInputEnd(string text)
    {
        if(!int.TryParse(text, out int value))
            value = 0;

        SetAmount(value);
    }

    private void Donate()
    {
        GameSession session = GameSession.instance;
        int used = session.Upgrades.Donate(track, amount, wallet);

        if(used <= 0)
            return;

        amount = 0;
        session.SaveInVillage();
        Refresh();
    }

    private void HandleProgressChanged(UpgradeProgress changed)
    {
        Refresh();
    }

    private void HandleGoldChanged(OnChangeGold e)
    {
        if(!popup.activeSelf)
            return;

        Refresh();
    }
}