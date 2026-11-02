public interface IMaterialStorage
{
    int CountItem(int itemId);
    void TakeItems(int itemId, int amount);
}
