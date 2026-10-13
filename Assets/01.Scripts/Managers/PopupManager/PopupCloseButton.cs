using UnityEngine;
using UnityEngine.UI;

public class PopupCloseButton : MonoBehaviour
{
    [Header("닫기 버튼")]
    [SerializeField] Button closeButton;

    [Header("닫힐 팝업")]
    [SerializeField] GameObject targetPopup;
    private void Awake()
    {
        closeButton.onClick.AddListener(() => PopupManager.instance.Close(targetPopup));
    }
    public void TriggerClose()
    {
        PopupManager.instance.Close(targetPopup);
    }
}
