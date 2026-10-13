using UnityEngine;

public class GameBootStrap : MonoBehaviour
{
    [SerializeField] RunController run;
    [SerializeField] UnitController player;
    [SerializeField] HpBarView hpBar;

    private async Awaitable Start()
    {

        await DataManager.instance.InitAsync();

        run.StartFirstRun();
        hpBar.Bind(player.Health);
    }
}
