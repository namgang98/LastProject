public interface IDeathGuard
{
    bool TryPreventDeath(out int reviveHp);
}