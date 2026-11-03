using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OrbSocketUI : MonoBehaviour
{
    [Header("팝업")]
    [SerializeField] GameObject popup;
    [SerializeField] Button closeButton;

    [Header("장비 목록")]
    [SerializeField] Transform itemRoot;
    [SerializeField] SocketSlotView itemPrefab;
    [SerializeField] GameObject noItemText;

    [Header("보주 목록")]
    [SerializeField] Transform orbRoot;
    [SerializeField] OrbSlotView orbPrefab;
    [SerializeField] GameObject noOrbText;

    [Header("선택한 장비")]
    [SerializeField] Image itemIcon;
    [SerializeField] TMP_Text itemNameText;
    [SerializeField] TMP_Text itemOrbText;

    [Header("미리보기")]
    [SerializeField] TMP_Text beforeText;
    [SerializeField] TMP_Text afterText;
    [SerializeField] GameObject replaceWarning;
    [SerializeField] TMP_Text goldText;
    [SerializeField] Button socketButton;
    [SerializeField] TMP_Text socketLabel;
    [SerializeField] Color enoughColor = Color.white;
    [SerializeField] Color lackColor = new(1f, 0.42f, 0.42f);

    private readonly List<ItemInstance> buffer = new();
    private readonly List<ItemInstance> items = new();
    private readonly List<OrbInfo> orbs = new();
    private readonly List<SocketSlotView> itemSlots = new();
    private readonly List<OrbSlotView> orbSlots = new();
    private ItemInstance selectedItem;
    private OrbInfo selectedOrb;

    private void Awake()
    {
        closeButton.onClick.AddListener(Close);
        socketButton.onClick.AddListener(Socket);
        popup.SetActive(false);
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

    public void Open()
    {
        selectedItem = null;
        selectedOrb = null;
        Refresh();
        PopupManager.instance.Open(popup, Close);
    }

    public void Close()
    {
        PopupManager.instance.Close(popup);
    }

    private void Refresh()
    {
        RefreshOrbs();
        RefreshItems();
        RefreshHeader();
        RefreshPreview();
    }

    private void RefreshOrbs()
    {
        GameSession session = GameSession.instance;
        orbs.Clear();

        foreach(OrbInfo orb in DataManager.instance.Orbs)
        {
            if(session.CountItem(orb.Id) > 0)
                orbs.Add(orb);
        }

        if(!orbs.Contains(selectedOrb))
            selectedOrb = orbs.Count > 0 ? orbs[0] : null;

        EnsureSlots(orbSlots, orbPrefab, orbRoot, orbs.Count);

        for(int i = 0; i < orbSlots.Count; i++)
        {
            bool used = i < orbs.Count;
            orbSlots[i].gameObject.SetActive(used);

            if(!used)
                continue;

            orbSlots[i].Bind(orbs[i], session.CountItem(orbs[i].Id), SelectOrb);
            orbSlots[i].SetSelected(orbs[i] == selectedOrb);
        }

        noOrbText.SetActive(orbs.Count == 0);
    }

    private void RefreshItems()
    {
        GameSession.instance.CollectEquipment(buffer);
        items.Clear();

        foreach(ItemInstance item in buffer)
        {
            if(selectedOrb == null || OrbService.CanSocketInto(selectedOrb, item))
                items.Add(item);
        }

        if(!items.Contains(selectedItem))
            selectedItem = items.Count > 0 ? items[0] : null;

        EnsureSlots(itemSlots, itemPrefab, itemRoot, items.Count);

        for(int i = 0; i < itemSlots.Count; i++)
        {
            bool used = i < items.Count;
            itemSlots[i].gameObject.SetActive(used);

            if(!used)
                continue;

            itemSlots[i].Bind(items[i], SelectItem);
            itemSlots[i].SetSelected(items[i] == selectedItem);
        }

        noItemText.SetActive(items.Count == 0);
    }

    private static void EnsureSlots<T>(List<T> slots, T prefab, Transform root, int count) where T : Component
    {
        while(slots.Count < count)
        {
            slots.Add(Instantiate(prefab, root));
        }
    }

    private void SelectItem(ItemInstance item)
    {
        selectedItem = item;
        Refresh();
    }

    private void SelectOrb(OrbInfo orb)
    {
        selectedOrb = orb;
        Refresh();
    }

    private void RefreshHeader()
    {
        bool has = selectedItem != null;
        itemIcon.enabled = has;
        itemNameText.text = has ? selectedItem.Info.Name : "장비를 선택하세요";
        itemOrbText.text = has ? DescribeSockets(selectedItem) : string.Empty;

        if(!has)
            return;

        itemIcon.sprite = selectedItem.Info.Icon;
    }

    private void RefreshPreview()
    {
        GameSession session = GameSession.instance;
        bool hasPair = selectedItem != null && selectedOrb != null;

        beforeText.text = hasPair ? DescribeBefore(selectedItem, selectedOrb) : string.Empty;
        replaceWarning.SetActive(hasPair && IsReplacing(selectedItem, selectedOrb));

        if(!hasPair)
        {
            afterText.text = string.Empty;
            goldText.text = string.Empty;
            socketButton.interactable = false;
            socketLabel.text = "선택 필요";
            return;
        }

        int cost = session.Socket.GetCost(selectedOrb);
        afterText.text = DescribeAfter(selectedItem, selectedOrb);
        goldText.text = cost > 0 ? $"{cost}G" : "무료";
        goldText.color = session.Wallet.Gold >= cost ? enoughColor : lackColor;

        SocketResult check = session.Socket.Check(selectedItem, selectedOrb, session.Wallet, session);
        socketButton.interactable = check == SocketResult.Ready;
        socketLabel.text = ToLabel(check);
    }

    private static string DescribeSockets(ItemInstance item)
    {
        string orb = item is ForgeableInstance forged && forged.Orb != null ? forged.Orb.Name : "없음";
        string ward = item.Ward != null ? " · 망자의 가호" : string.Empty;
        return $"보주 : {orb}{ward}";
    }

    private static string DescribeBefore(ItemInstance item, OrbInfo orb)
    {
        if(!orb.IsElement)
            return item.Ward != null ? "망자의 가호 (1회)" : "가호 없음";

        if(item is not ForgeableInstance forged || item.Info is not WeaponInfo weapon)
            return "속성 없음";

        return ElementText.Describe(forged.ApplyTo(weapon.Attack));
    }

    private static string DescribeAfter(ItemInstance item, OrbInfo orb)
    {
        if(!orb.IsElement)
            return "쓰러져도 이 장비가 남는다 (1회)";

        if(item.Info is not WeaponInfo weapon)
            return string.Empty;

        return ElementText.Describe(orb.ApplyTo(weapon.Attack));
    }

    private static bool IsReplacing(ItemInstance item, OrbInfo orb)
    {
        if(!orb.IsElement || item is not ForgeableInstance forged || item.Info is not WeaponInfo weapon)
            return false;

        AttackData current = forged.ApplyTo(weapon.Attack);
        return current.Element != DamageElement.None && current.Element != orb.Element;
    }

    private void Socket()
    {
        GameSession session = GameSession.instance;
        string itemName = selectedItem.Info.Name;
        string orbName = selectedOrb.Name;
        SocketResult result = session.Socket.TrySocket(selectedItem, selectedOrb, session.Wallet, session);

        if(result == SocketResult.Success)
        {
            Sfx.Play(SfxType.ForgeSuccess);
            session.SaveInVillage();
        }

        FeedbackType type = result == SocketResult.Success ? FeedbackType.Info : FeedbackType.Warning;
        EventManager.instance.PostNotification(new OnFeedback(ToMessage(result, itemName, orbName), type));
        Refresh();
    }

    private static string ToLabel(SocketResult result)
    {
        switch(result)
        {
            case SocketResult.Ready:
                return "합성하기";
            case SocketResult.SameOrb:
                return "이미 깃듦";
            case SocketResult.NotFit:
                return "합성 불가";
            case SocketResult.NoGold:
                return "골드 부족";
            default:
                return "선택 필요";
        }
    }

    private static string ToMessage(SocketResult result, string itemName, string orbName)
    {
        switch(result)
        {
            case SocketResult.Success:
                return $"{itemName}에 {orbName}이(가) 깃들었다!";
            case SocketResult.SameOrb:
                return "이미 같은 보주가 깃들어 있습니다";
            case SocketResult.NotFit:
                return "이 장비에는 깃들일 수 없습니다";
            case SocketResult.NoGold:
                return "골드가 부족합니다";
            case SocketResult.NoOrb:
                return "보주가 없습니다";
            default:
                return "합성할 수 없습니다";
        }
    }

    private void HandleGoldChanged(OnChangeGold e)
    {
        if(!popup.activeSelf)
            return;

        RefreshPreview();
    }
}