using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ConfirmPopup : MonoBehaviour
{
    [SerializeField] GameObject root;
    [SerializeField] TMP_Text messageText;
    [SerializeField] Button okButton;
    [SerializeField] Button cancelButton;

    private Action onConfirm;

    private void Awake()
    {
        okButton.onClick.AddListener(Confirm);
        cancelButton.onClick.AddListener(Hide);
        root.SetActive(false);
    }

    public void Show(string message, Action confirm)
    {
        messageText.text = message;
        onConfirm = confirm;
        PopupManager.instance.Open(root, Hide);
    }

    public void Hide()
    {
        onConfirm = null;
        PopupManager.instance.Close(root);
    }

    private void Confirm()
    {
        Action action = onConfirm;
        Hide();
        action?.Invoke();
    }
}