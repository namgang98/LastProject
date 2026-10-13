using UnityEngine;
using UnityEngine.UI;

public class PopupOpenButton : MonoBehaviour
{
    [Header("열기 버튼")]
    [SerializeField] Button openButton;

    [Header("열릴 팝업")]
    [SerializeField] GameObject targetPopup;

    private void Awake()
    {
        openButton.onClick.AddListener(() => PopupManager.instance.Open(targetPopup));
    }
    public void TriggerOpen()
    {
        PopupManager.instance.Open(targetPopup);
    }
}

