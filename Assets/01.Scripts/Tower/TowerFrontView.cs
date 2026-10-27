using System;
using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TowerFrontView : MonoBehaviour
{
    [SerializeField] GameObject root;
    [SerializeField] Button doorButton;
    [SerializeField] Image leftDoor;
    [SerializeField] Image rightDoor;
    [SerializeField] RectTransform zoomTarget;
    [SerializeField] TMP_Text nameText;

    [Header("문 연출")]
    [SerializeField] float openTime = 0.8f;
    [SerializeField] Color openedTint = new(0.35f, 0.3f, 0.3f, 1f);
    [SerializeField] float zoomDelay = 0.4f;
    [SerializeField] float zoomScale = 1.3f;
    [SerializeField] float zoomTime = 1f;
    [SerializeField] SfxType openSfx = SfxType.None;

    public event Action DoorClicked;

    private void Awake()
    {
        doorButton.onClick.AddListener(HandleDoorClick);
    }

    public void Show(TowerInfo tower)
    {
        root.SetActive(true);
        nameText.text = tower != null ? tower.Name : string.Empty;
        ResetDoors();
    }

    public void Hide()
    {
        root.SetActive(false);
    }

    public void SetInteractable(bool interactable)
    {
        doorButton.interactable = interactable;
    }

    public IEnumerator Open()
    {
        doorButton.interactable = false;
        PlaySfx(openSfx);
        Swing(leftDoor);
        Swing(rightDoor);

        yield return new WaitForSecondsRealtime(zoomDelay);

        yield return zoomTarget.DOScale(zoomScale, zoomTime)
            .SetEase(Ease.InQuad)
            .SetUpdate(true)
            .SetLink(zoomTarget.gameObject)
            .WaitForCompletion();
    }

    private void Swing(Image door)
    {
        door.rectTransform.DOScaleX(0f, openTime).SetEase(Ease.InOutSine).SetUpdate(true).SetLink(door.gameObject);
        door.DOColor(openedTint, openTime).SetUpdate(true).SetLink(door.gameObject);
    }

    private void ResetDoors()
    {
        ResetDoor(leftDoor);
        ResetDoor(rightDoor);
        zoomTarget.DOKill();
        zoomTarget.localScale = Vector3.one;
    }

    private static void ResetDoor(Image door)
    {
        door.rectTransform.DOKill();
        door.DOKill();
        door.rectTransform.localScale = Vector3.one;
        door.color = Color.white;
    }

    private void HandleDoorClick()
    {
        DoorClicked?.Invoke();
    }

    private static void PlaySfx(SfxType type)
    {
        if (type == SfxType.None || !SoundManager.HasInstance)
            return;

        SoundManager.instance.PlaySfx(type);
    }
}