using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class TutorialAnchor : MonoBehaviour
{
    [SerializeField] string id;

    public string Id => id;
    public RectTransform Rect => (RectTransform)transform;
}