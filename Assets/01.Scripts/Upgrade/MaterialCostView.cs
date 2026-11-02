using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MaterialCostView : MonoBehaviour
{
    [SerializeField] Image icon;
    [SerializeField] TMP_Text nameText;
    [SerializeField] TMP_Text countText;
    [SerializeField] Color enoughColor = Color.white;
    [SerializeField] Color lackColor = new(1f, 0.42f, 0.42f);

    public void Bind(ItemCost cost, int have)
    {
        icon.sprite = cost.Item.Icon;
        nameText.text = cost.Item.Name;
        countText.text = $"{have} / {cost.Count}";
        countText.color = have >= cost.Count ? enoughColor : lackColor;
    }
}
