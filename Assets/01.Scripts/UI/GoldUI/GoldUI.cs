using TMPro;
using UnityEngine;

public class GoldUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI goldText;

    private void OnEnable()
    {
        EventManager.instance.AddListener<OnChangeGold>(HandleChangeGold);
    }
    private void OnDisable()
    {
        if(!EventManager.HasInstance)
            return;

        EventManager.instance.RemoveListener<OnChangeGold>(HandleChangeGold);
    }

    private void HandleChangeGold(OnChangeGold e)
    {
        goldText.text = $"{e.Gold}";
    }
}

//EventManager.instance.PostNotification(new OnChangeGold(currentGold));