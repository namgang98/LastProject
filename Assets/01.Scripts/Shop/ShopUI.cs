using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopUI : MonoBehaviour
{
    [Header("팝업")]
    [SerializeField] GameObject popup;

    [Header("선반")]
    [SerializeField] ShopSlotView[] slots;

    [Header("버튼")]
    [SerializeField] Button rerollButton;
    [SerializeField] TMP_Text rerollText;
    [SerializeField] Button closeButton;

    [Header("구매 확인")]
    [SerializeField] GameObject confirmPanel;
    [SerializeField] TMP_Text confirmText;
    [SerializeField] Button buyButton;
    [SerializeField] Button cancelButton;

    private Shop shop;
    private Wallet wallet;
    private int selected = -1;

    private void Awake()
    {
        for(int i = 0; i < slots.Length; i++)
        {
            slots[i].SetIndex(i);
            slots[i].Clicked += Select;
        }

        rerollButton.onClick.AddListener(Reroll);
        closeButton.onClick.AddListener(Close);
        buyButton.onClick.AddListener(Buy);
        cancelButton.onClick.AddListener(HideConfirm);
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
        if(shop != null)
            shop.Changed -= Refresh;
    }

    public void Open()
    {
        GameSession session = GameSession.instance;
        shop = session.Shop;
        wallet = session.Wallet;

        shop.Changed -= Refresh;
        shop.Changed += Refresh;

        PopupManager.instance.Open(popup, Close);
        HideConfirm();
        Refresh();
    }

    public void Close()
    {
        HideConfirm();

        if(shop != null)
            shop.Changed -= Refresh;

        PopupManager.instance.Close(popup);
        GameSession.instance.SaveInVillage();
    }

    private void Refresh()
    {
        for(int i = 0; i < slots.Length; i++)
        {
            RefreshSlot(i);
        }

        int cost = shop.Pricing.RerollCost;
        rerollText.text = $"리롤 {cost}G";
        rerollButton.interactable = wallet.Gold >= cost;
    }

    private void RefreshSlot(int i)
    {
        if(i >= shop.SlotCount)
        {
            slots[i].ShowLocked();
            return;
        }

        ShopStockInfo stock = shop.Offers[i];

        if(stock == null)
        {
            slots[i].ShowEmpty();
            return;
        }

        int price = shop.Pricing.BuyPrice(stock);
        slots[i].ShowItem(stock.Item, price, wallet.Gold >= price);
    }

    private void Select(int index)
    {
        ShopStockInfo stock = shop.Offers[index];

        if(stock == null)
            return;

        selected = index;
        confirmText.text = $"{stock.Item.Name}\n{shop.Pricing.BuyPrice(stock)}G에 구매할까요?";
        confirmPanel.SetActive(true);
    }

    private void Buy()
    {
        if(selected < 0)
            return;

        GameSession session = GameSession.instance;
        ShopStockInfo stock = shop.Offers[selected];

        if(stock != null && wallet.Gold < shop.Pricing.BuyPrice(stock))
            Feedback("골드가 부족합니다");
        else if(!shop.TryBuy(selected, wallet, session.TryReceive))
            Feedback("가방과 창고에 자리가 없습니다");
        else
            session.SaveInVillage();

        HideConfirm();
    }

    private void Reroll()
    {
        HideConfirm();

        if(!shop.TryReroll(wallet))
            return;

        GameSession.instance.SaveInVillage();
    }

    private void HideConfirm()
    {
        selected = -1;
        confirmPanel.SetActive(false);
    }

    private void HandleGoldChanged(OnChangeGold e)
    {
        if(!popup.activeSelf || shop == null)
            return;

        Refresh();
    }
    private static void Feedback(string message)
    {
        EventManager.instance.PostNotification(new OnFeedback(message));
    }
}