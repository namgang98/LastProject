using UnityEngine;

public class MonsterHealth : UnitHealth
{
    MonsterStats stats;
    private void OnEnable()
    {
        Init(stats.MaxHp);        
    }
    protected override void Die()
    {
    }
    private void OnDisable()
    {
        
    }
}
