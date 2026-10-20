using UnityEngine;
using UnityEngine.UI;

public class PotionCaseView : MonoBehaviour
{
    [SerializeField] GameObject panel;
    [SerializeField] Image[] pocketIcons;
    [SerializeField] Button closeButton;

    private PotionCaseInstance current;

    private void Awake()
    {
        closeButton.onClick.AddListener(Close);

        foreach(Image icon in pocketIcons)
        {
            icon.preserveAspect = true;
            icon.raycastTarget = false;
        }

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

    private void Refresh(GridContainer storage)
    {
        for(int i = 0; i < pocketIcons.Length; i++)
        {
            ItemInstance potion = storage.GetItemAt(new Vector2Int(i, 0));
            bool has = potion != null;

            pocketIcons[i].gameObject.SetActive(has);

            if(has)
                pocketIcons[i].sprite = potion.Info.Icon;
        }
    }
}