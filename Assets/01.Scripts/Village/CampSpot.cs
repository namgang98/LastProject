using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class CampSpot : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] GameObject highlight;   
    [SerializeField] GameObject nameLabel;   
    [SerializeField] UnityEvent onClick;

    private void Awake()
    {
        SetHover(false);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        SetHover(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        SetHover(false);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        onClick.Invoke();
    }

    private void SetHover(bool on)
    {
        if(highlight != null)
            highlight.SetActive(on);

        if(nameLabel != null)
            nameLabel.SetActive(on);
    }
}