using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ButtonSound : MonoBehaviour
{
    [SerializeField] private SfxType sfx = SfxType.UiClick;

    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    private void OnEnable()
    {
        button.onClick.AddListener(Play);
    }

    private void OnDisable()
    {
        button.onClick.RemoveListener(Play);
    }

    private void Play()
    {
        SoundManager.instance.PlaySfx(sfx);
    }
}