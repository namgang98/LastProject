using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChestUI : MonoBehaviour
{
    [Header("영역")]
    [SerializeField, Min(1f)] float cellSize = 64f;
    [Header("뷰")]
    [SerializeField] VillageBackpackPanel backpackPanel;
    [SerializeField] GridView chestView;
    [SerializeField] DragController dragController;
    [SerializeField] ItemViewPool itemViewPool;

    [Header("탭")]
    [SerializeField] Button[] tabButtons;

    [Header("팝업")]
    [SerializeField] GameObject popup;

    [Header("확장")]
    [SerializeField] Button expandButton;
    [SerializeField] DonationUI expandDonation;

    private InventoryUIContext context;
    private Chest chest;
    private int currentTab;

    private void Awake()
    {
        itemViewPool.Init(dragController);
        context = new InventoryUIContext(cellSize, itemViewPool, dragController);
        dragController.SetContext(context);

        for(int i = 0; i < tabButtons.Length; i++)
        {
            int index = i;
            tabButtons[i].onClick.AddListener(() => ShowTab(index));
        }
        expandButton.onClick.AddListener(expandDonation.Open);
    }
    private void OnEnable()
    {
        EventManager.instance.AddListener<OnBackpackChanged>(HandleBackpackChanged);
        EventManager.instance.AddListener<OnChestExpanded>(HandleChestExpanded);
        EventManager.instance.AddListener<OnItemQuickMove>(HandleQuickMove);
    }

    private void OnDisable()
    {
        if(!EventManager.HasInstance)
            return;
        EventManager.instance.RemoveListener<OnBackpackChanged>(HandleBackpackChanged);
        EventManager.instance.RemoveListener<OnChestExpanded>(HandleChestExpanded);
        EventManager.instance.RemoveListener<OnItemQuickMove>(HandleQuickMove);
    }

    public void Open()
    {
        PopupManager.instance.Open(popup, Close);
        Bind();
    }

    public void Close()
    {
        chestView.Unbind();
        PopupManager.instance.Close(popup);
        GameSession.instance.SaveInVillage();
    }
    private void Bind()
    {
        GameSession session = GameSession.instance;
        chest = session.Chest;

        backpackPanel.Bind(session.Backpack, context);
        backpackPanel.BagSlot.SwapHandler = session.TrySwapBag;
        ShowTab(currentTab);
    }
    public void ShowTab(int index)
    {
        if(index < 0 || index >= chest.Tabs.Count)
            return;

        currentTab = index;
        chestView.Bind(chest.Tabs[index], context);
        chestView.SetCenter(Vector2.zero);
        RefreshTabButtons();
    }

    private void RefreshTabButtons()
    {
        for(int i = 0; i < tabButtons.Length; i++)
        {
            tabButtons[i].gameObject.SetActive(i < chest.Tabs.Count);
            tabButtons[i].interactable = i != currentTab;
        }

        bool canExpand = GameSession.instance.Upgrades.TryGet(UpgradeTrack.Chest, out UpgradeProgress progress) && !progress.IsMaxed;
        expandButton.gameObject.SetActive(canExpand);
    }

    private void HandleBackpackChanged(OnBackpackChanged e)
    {
        if(!popup.activeSelf)
            return;

        backpackPanel.Bind(e.Backpack, context);
        GameSession.instance.SaveInVillage();
    }
    private void HandleChestExpanded(OnChestExpanded e)
    {
        if(!popup.activeSelf)
            return;

        RefreshTabButtons();
    }
    private void HandleQuickMove(OnItemQuickMove e)
    {
        if(!popup.activeSelf || e.Item.Owner == null)
            return;

        Backpack backpack = GameSession.instance.Backpack;
        bool moved = chest.Owns(e.Item.Owner) ? ItemTransfer.ToBackpack(backpack, e.Item) : ItemTransfer.ToChest(chest, e.Item, currentTab);

        if(!moved)
        {
            EventManager.instance.PostNotification(new OnFeedback("옮길 빈자리가 없습니다"));
            return;
        }

        GameSession.instance.SaveInVillage();
    }
    public void StoreAll()
    {
        List<ItemInstance> leftover = chest.StoreAll(GameSession.instance.Backpack.Grid);

        if(leftover.Count > 0)
            EventManager.instance.PostNotification(new OnFeedback($"창고 공간이 부족해 {leftover.Count}개를 넣지 못했습니다"));

        GameSession.instance.SaveInVillage();
    }
}