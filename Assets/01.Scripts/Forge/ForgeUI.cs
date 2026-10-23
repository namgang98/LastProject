using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ForgeUI : MonoBehaviour
{
    [Header("팝업")]
    [SerializeField] GameObject popup;
    [SerializeField] Button closeButton;

    [Header("목록")]
    [SerializeField] Transform listRoot;
    [SerializeField] ForgeSlotView slotPrefab;
    [SerializeField] GameObject emptyText;

    [Header("상세")]
    [SerializeField] GameObject detailRoot;
    [SerializeField] Image itemIcon;
    [SerializeField] TMP_Text nameText;
    [SerializeField] TMP_Text levelText;
    [SerializeField] TMP_Text statText;
    [SerializeField] TMP_Text chanceText;
    [SerializeField] GameObject warning;
    [SerializeField] TMP_Text warningText;
    [SerializeField] TMP_Text goldText;
    [SerializeField] Transform materialRoot;
    [SerializeField] MaterialCostView materialPrefab;
    [SerializeField] Button forgeButton;
    [SerializeField] TMP_Text forgeLabel;
    [SerializeField] Color enoughColor = Color.white;
    [SerializeField] Color lackColor = new(1f, 0.42f, 0.42f);

    private readonly List<ForgeableInstance> items = new();
    private readonly List<ForgeSlotView> slots = new();
    private readonly List<MaterialCostView> rows = new();
    private ForgeableInstance selected;

    private void Awake()
    {
        closeButton.onClick.AddListener(Close);
        forgeButton.onClick.AddListener(Forge);
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
        selected = null;
        RefreshList();
        PopupManager.instance.Open(popup, Close);
    }

    public void Close()
    {
        PopupManager.instance.Close(popup);
    }

    private void RefreshList()
    {
        GameSession.instance.CollectForgeables(items);

        if(selected == null || !items.Contains(selected))
            selected = items.Count > 0 ? items[0] : null;

        while(slots.Count < items.Count)
        {
            slots.Add(Instantiate(slotPrefab, listRoot));
        }

        for(int i = 0; i < slots.Count; i++)
        {
            bool used = i < items.Count;
            slots[i].gameObject.SetActive(used);

            if(!used)
                continue;

            slots[i].Bind(items[i], Select);
            slots[i].SetSelected(items[i] == selected);
        }

        if(emptyText != null)
            emptyText.SetActive(items.Count == 0);

        RefreshDetail();
    }

    private void Select(ForgeableInstance item)
    {
        selected = item;

        for(int i = 0; i < items.Count; i++)
        {
            slots[i].SetSelected(items[i] == selected);
        }

        RefreshDetail();
    }

    private void RefreshDetail()
    {
        detailRoot.SetActive(selected != null);

        if(selected == null)
            return;

        GameSession session = GameSession.instance;
        ForgeService forge = session.Forge;
        string stat = selected.Info is ArmorInfo ? "최대 체력" : "데미지";

        itemIcon.sprite = selected.Info.Icon;
        nameText.text = selected.Info.Name;

        if(!forge.TryGetNext(selected, out ForgeLevelInfo next))
        {
            ShowMaxed(stat);
            return;
        }

        int cost = forge.GetCost(selected, next);
        bool risky = next.Downgrade;

        levelText.text = $"+{selected.Level}  →  +{next.Level}";
        statText.text = $"{stat} +{selected.Bonus * 100f:0}%  →  +{next.Bonus * 100f:0}%";
        chanceText.text = $"성공 확률 {next.Chance * 100f:0}%";
        warning.SetActive(risky);

        if(risky)
            warningText.text = $"실패하면 +{selected.Level - 1}(으)로 떨어집니다";

        goldText.text = $"{session.Wallet.Gold} / {cost}G";
        goldText.color = session.Wallet.Gold >= cost ? enoughColor : lackColor;

        ShowMaterials(next.Materials, session);

        bool ready = forge.Check(selected, session.Wallet, session) == ForgeResult.Ready;
        forgeButton.interactable = ready;
        forgeLabel.text = ready ? "강화하기" : "재료 부족";
    }

    private void ShowMaxed(string stat)
    {
        levelText.text = $"+{selected.Level}  (최대)";
        statText.text = $"{stat} +{selected.Bonus * 100f:0}%";
        chanceText.text = string.Empty;
        warning.SetActive(false);
        goldText.text = string.Empty;
        ShowMaterials(null, null);
        forgeButton.interactable = false;
        forgeLabel.text = "MAX";
    }

    private void ShowMaterials(IReadOnlyList<ItemCost> materials, IMaterialStorage storage)
    {
        int count = materials == null ? 0 : materials.Count;

        while(rows.Count < count)
        {
            rows.Add(Instantiate(materialPrefab, materialRoot));
        }

        for(int i = 0; i < rows.Count; i++)
        {
            bool used = i < count;
            rows[i].gameObject.SetActive(used);

            if(used)
                rows[i].Bind(materials[i], storage.CountItem(materials[i].ItemId));
        }
    }

    private void Forge()
    {
        GameSession session = GameSession.instance;
        string name = selected.Info.Name;
        ForgeResult result = session.Forge.TryForge(selected, session.Wallet, session);

        EventManager.instance.PostNotification(new OnFeedback(ToMessage(result, name, selected.Level), ToFeedbackType(result)));

        if(result == ForgeResult.Success || result == ForgeResult.Failed || result == ForgeResult.Downgraded)
            session.SaveInVillage();

        RefreshList();
    }

    private static string ToMessage(ForgeResult result, string name, int level)
    {
        switch(result)
        {
            case ForgeResult.Success:
                return $"강화 성공! {name} +{level}";
            case ForgeResult.Failed:
                return "강화 실패... 재료만 사라졌다";
            case ForgeResult.Downgraded:
                return $"강화 실패! {name}이(가) +{level}(으)로 떨어졌다";
            case ForgeResult.NoGold:
                return "골드가 부족합니다";
            case ForgeResult.NoMaterial:
                return "재료가 부족합니다";
            case ForgeResult.Maxed:
                return "이미 최대 강화입니다";
            default:
                return "강화할 수 없습니다";
        }
    }

    private static FeedbackType ToFeedbackType(ForgeResult result)
    {
        return result == ForgeResult.Success ? FeedbackType.Info : FeedbackType.Warning;
    }

    private void HandleGoldChanged(OnChangeGold e)
    {
        if(!popup.activeSelf)
            return;

        RefreshDetail();
    }
}