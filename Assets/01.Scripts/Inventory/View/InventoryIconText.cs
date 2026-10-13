public class InventoryUIContext
{
    public float CellSize { get; }
    public ItemViewPool Pool { get; }
    public DragController Drag { get; }

    public InventoryUIContext(float cellSize, ItemViewPool pool, DragController drag)
    {
        CellSize = cellSize;
        Pool = pool;
        Drag = drag;
    }
}