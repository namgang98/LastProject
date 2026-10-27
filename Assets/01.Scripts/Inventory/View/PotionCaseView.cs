using UnityEngine;
using UnityEngine.UI;

public class PotionCaseView : MonoBehaviour
{
    [SerializeField] GameObject panel;
    [SerializeField] Image[] pocketIcons;
    [SerializeField] Button[] pocketButtons;
    [SerializeField] Button takeAllButton;
    [SerializeField] Button closeButton;

    private PotionCaseInstance current;

    private void Awake()
    {
        closeButton.onClick.AddListener(Close);

        foreach(Image icon in pocketIcons)
        {
            icon.preserveAspect = true;
        }

        for(int i = 0; i < pocketButtons.Length; i++)
        {
            int index = i;
            pocketButtons[i].onClick.AddListener(() => TakeOut(index));
        }

        if(takeAllButton != null)
            takeAllButton.onClick.AddListener(TakeAll);

        panel.SetActive(false);
    }

    private void OnEnable()
    {
        EventManager.instance.AddListener<OnItemClicked>(HandleItemClicked);
    }

    private void OnDisable()
    {
        Close();
        if(!EventManager.HasInstance)
            return;

        EventManager.instance.RemoveListener<OnItemClicked>(HandleItemClicked);
    }

    private void OnDestroy()
    {
        Unbind();
    }

    private void HandleItemClicked(OnItemClicked e)
    {
        if(e.Item is not PotionCaseInstance potionCase)
            return;

        Open(potionCase);
    }

    public void Open(PotionCaseInstance potionCase)
    {
        Unbind();
        current = potionCase;
        current.Storage.Changed += Refresh;
        panel.SetActive(true);
        Refresh(current.Storage);
    }

    public void Close()
    {
        Unbind();
        panel.SetActive(false);
    }

    private void Unbind()
    {
        if(current == null)
            return;

        current.Storage.Changed -= Refresh;
        current = null;
    }

    private void TakeOut(int index)
    {
        if(current == null)
            return;

        ItemInstance potion = current.Storage.GetItemAt(new Vector2Int(index, 0));

        if(potion == null)
            return;

        if(TryPlaceOutside(potion))
            return;

        EventManager.instance.PostNotification(new OnFeedback("꺼낼 빈자리가 없습니다"));
    }

    private void TakeAll()
    {
        if(current == null)
            return;

        var potions = new System.Collections.Generic.List<ItemInstance>(current.Storage.Items);

        foreach(ItemInstance potion in potions)
        {
            if(TryPlaceOutside(potion))
                continue;

            EventManager.instance.PostNotification(new OnFeedback("자리가 부족해 일부를 꺼내지 못했습니다"));
            return;
        }
    }

    private bool TryPlaceOutside(ItemInstance potion)
    {
        GridContainer home = current.Owner;

        if(home != null && home.TryAutoPlace(potion))
            return true;

        return ItemTransfer.ToBackpack(GameSession.instance.Backpack, potion);
    }

    private void Refresh(GridContainer storage)
    {
        for(int i = 0; i < pocketIcons.Length; i++)
        {
            ItemInstance potion = storage.GetItemAt(new Vector2Int(i, 0));
            bool has = potion != null;

            pocketIcons[i].gameObject.SetActive(has);

            if(i < pocketButtons.Length)
                pocketButtons[i].interactable = has;

            if(has)
                pocketIcons[i].sprite = potion.Info.Icon;
        }
    }
}