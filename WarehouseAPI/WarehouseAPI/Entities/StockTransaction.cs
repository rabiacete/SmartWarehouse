namespace WarehouseAPI.Entities;

public enum TransactionType
{
    In = 1,
    Out = 2
}

public class StockTransaction : BaseEntity
{
    public int ProductId { get; set; }
    public int WarehouseId { get; set; }
    public TransactionType TransactionType { get; set; }
    public int Quantity { get; set; }
    public string? Note { get; set; }

    // Navigation
    public Product Product { get; set; } = null!;
    public Warehouse Warehouse { get; set; } = null!;
}
