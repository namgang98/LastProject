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
    }
    private void OnEnable()
    {
        EventManager.instance.AddListener<OnBackpackChanged>(HandleBackpackChanged);
    }

    private void OnDisable()
    {
        if(!EventManager.HasInstance)
            return;
        EventManager.instance.RemoveListener<OnBackpackChanged>(HandleBackpackChanged);
    }

    public void Open()
    {
        PopupManager.instance.Open(popup);   // 먼저 켜야 안쪽 Awake가 실행됨
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
    }

    private void HandleBackpackChanged(OnBackpackChanged e)
    {
        if(!popup.activeSelf)
            return;

        backpackPanel.Bind(e.Backpack, context);
        GameSession.instance.SaveInVillage();
    }
    public void StoreAll()
    {
        List<ItemInstance> leftover = chest.StoreAll(GameSession.instance.Backpack.Grid);

        if(leftover.Count > 0) //피드백팝업만들면 변경
            Debug.LogWarning($"[ChestUI] 창고 공간이 부족해 {leftover.Count}개를 넣지 못했습니다.", this);

        GameSession.instance.SaveInVillage();
    }
}