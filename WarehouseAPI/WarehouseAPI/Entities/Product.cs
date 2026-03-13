namespace WarehouseAPI.Entities;

public class Product : BaseEntity
{
    public string ProductName { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty; // adet, kg, kutu vb.
    public string? Description { get; set; }
    public int MinStockLevel { get; set; } = 0;

    // Navigation
    public ICollection<StockTransaction> StockTransactions { get; set; } = new List<StockTransaction>();
}
