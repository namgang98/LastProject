using UnityEngine;

public class VillageView : MonoBehaviour
{
    private void Start()
    {
        GameSession.instance.RefreshGold();
    }
}