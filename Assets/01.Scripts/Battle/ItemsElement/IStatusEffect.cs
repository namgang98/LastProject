public interface IStatusEffect
{
    UnitHealth Target { get; }
    DamageElement Element { get; }
    float Duration { get; }
    float Progress { get; }
    float SpeedMultiplier { get; }
    bool Tick(float deltaTime);   // false면 끝남 → 목록에서 제거
    void Refresh(float duration);
}