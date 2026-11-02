using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SellUI : MonoBehaviour
{
    [Header("영역")]
    [SerializeField, Min(1f)] float cellSize = 48f;

    [Header("뷰")]
    [SerializeField] BackpackPanelBase backpackPanel;
    [SerializeField] GridView chestView;
    [SerializeField] SellAreaView sellArea;
    [SerializeField] DragController dragController;
    [SerializeField] ItemViewPool itemViewPool;

    [Header("탭 창고)")]
    [SerializeField] Button[] tabButtons;

    [Header("창고 영역")]
    [SerializeField] RectTransform chestArea;
    [SerializeField, Min(0f)] float chestPadding = 8f;
    
    [Header("판매")]
    [SerializeField] TMP_Text totalText;
    [SerializeField] Button sellButton;
    [SerializeField] Button closeButton;

    [Header("팝업")]
    [SerializeField] GameObject popup;

    private readonly Dictionary<ItemInstance, Origin> origins = new();
    private readonly LootArea basket = new();
    private InventoryUIContext context;
    private Shop shop;
    private int currentTab;

    private void Awake()
    {
        context = new InventoryUIContext(cellSize, itemViewPool, dragController);
        sellArea.CanAccept = CanAccept;
        sellArea.Rejected += HandleRejected;
        sellArea.Entering += RememberOrigin;

        for(int i = 0; i < tabButtons.Length; i++)
        {
            int index = i;
            tabButtons[i].onClick.AddListener(() => ShowTab(index));
        }

        sellButton.onClick.AddListener(Sell);
        closeButton.onClick.AddListener(Close);
    }

    private void OnEnable()
    {
        basket.Changed += HandleBasketChanged;
        EventManager.instance.AddListener<OnItemQuickMove>(HandleQuickMove);
    }

    private void OnDisable()
    {
        basket.Changed -= HandleBasketChanged;
        if(EventManager.HasInstance)
            EventManager.instance.RemoveListener<OnItemQuickMove>(HandleQuickMove);
    }

    public void Open()
    {
        GameSession session = GameSession.instance;
        shop = session.Shop;

        PopupManager.instance.Open(popup, Close);
        dragController.SetContext(context);
        backpackPanel.Bind(session.Backpack, context);
        sellArea.Bind(basket, context);
        ShowTab(currentTab);
        RefreshTotal();
    }

    public void Close()
    {
        ReturnAll();
        chestView.Unbind();
        sellArea.Unbind();
        PopupManager.instance.Close(popup);
        GameSession.instance.SaveInVillage();
    }

    private void ShowTab(int index)
    {
        Chest chest = GameSession.instance.Chest;

        if(index < 0 || index >= chest.Tabs.Count)
            return;

        currentTab = index;
        chestView.Bind(chest.Tabs[index], context);
        chestView.SetCenter(Vector2.zero);
        RefreshTabButtons(chest);
        chestView.SetCenter(Vector2.zero);
        FitChest();
        RefreshTabButtons(chest);
    }

    private void RefreshTabButtons(Chest chest)
    {
        for(int i = 0; i < tabButtons.Length; i++)
        {
            tabButtons[i].gameObject.SetActive(i < chest.Tabs.Count);
            tabButtons[i].interactable = i != currentTab;
        }
    }

    private bool CanAccept(ItemInstance item)
    {
        return shop != null && shop.CanSell(item);
    }

    private void Sell()
    {
        int earned = shop.SellAll(basket, GameSession.instance.Wallet);

        var sold = new List<ItemInstance>();

        foreach(var item in origins.Keys)
        {
            if(!basket.Contains(item))
                sold.Add(item);
        }

        foreach(var item in sold)
        {
            origins.Remove(item);
        }
        if(earned <= 0)
            return;

        Sfx.Play(SfxType.Coins);
        GameSession.instance.SaveInVillage();
    }
    private void RememberOrigin(ItemInstance item)
    {
        origins[item] = new Origin(item.Owner, item.Anchor, item.Rotation);
    }
    private void ReturnAll()
    {
        var items = new List<ItemInstance>();

        foreach(var entry in basket.Entries)
        {
            items.Add(entry.Item);
        }

        foreach(var item in items)
        {
            if(TryReturn(item))
                basket.Remove(item);
            else
                EventManager.instance.PostNotification(new OnFeedback($"{item.Info.Name}을(를) 돌려놓을 자리가 없습니다"));
        }
    }
    private bool TryReturn(ItemInstance item)
    {
        if(origins.TryGetValue(item, out Origin origin) && origin.Grid != null && origin.Grid.TryPlace(item, origin.Anchor, origin.Rotation))
        {
            origins.Remove(item);
            return true;
        }

        if(!GameSession.instance.TryReceive(item))
            return false;

        origins.Remove(item);
        return true;
    }

    private void RefreshTotal()
    {
        if(shop == null)
            return;

        totalText.text = $"{shop.GetSellTotal(basket.Entries)}G";
        sellButton.interactable = basket.Count > 0;
    }

    private void HandleBasketChanged(LootArea changed)
    {
        RefreshTotal();
    }
    private void HandleRejected(ItemInstance item)
    {
        if(item is ContainerInstance)
            EventManager.instance.PostNotification(new OnFeedback("안에 물건이 들어있습니다."));
    }
    private void FitChest()
    {
        Vector2 size = chestView.PixelSize;
        Vector2 room = chestArea.rect.size - Vector2.one * (chestPadding * 2f);
        float scale = Mathf.Min(1f, room.x / size.x, room.y / size.y);

        chestView.transform.localScale = Vector3.one * scale;
    }
    private readonly struct Origin
    {
        public readonly GridContainer Grid;
        public readonly Vector2Int Anchor;
        public readonly int Rotation;

        public Origin(GridContainer grid, Vector2Int anchor, int rotation)
        {
            Grid = grid;
            Anchor = anchor;
            Rotation = rotation;
        }
    }
    private void HandleQuickMove(OnItemQuickMove e)
    {
        if(!popup.activeSelf)
            return;

        if(basket.Contains(e.Item))
        {
            ReturnFromBasket(e.Item);
            return;
        }

        if(e.Item.Owner == null)
            return;

        if(!CanAccept(e.Item))
        {
            HandleRejected(e.Item);
            return;
        }

        RememberOrigin(e.Item);
        e.Item.Owner.Remove(e.Item);
        sellArea.Scatter(e.Item);
    }

    private void ReturnFromBasket(ItemInstance item)
    {
        if(!TryReturn(item))
        {
            EventManager.instance.PostNotification(new OnFeedback($"{item.Info.Name}을(를) 돌려놓을 자리가 없습니다"));
            return;
        }

        basket.Remove(item);
    }
}
