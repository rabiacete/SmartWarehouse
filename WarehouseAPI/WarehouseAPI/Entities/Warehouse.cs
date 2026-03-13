namespace WarehouseAPI.Entities;

public class Warehouse : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public string? Description { get; set; }

    // Navigation
    public ICollection<StockTransaction> StockTransactions { get; set; } = new List<StockTransaction>();
}
