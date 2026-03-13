namespace WarehouseAPI.DTOs.Dashboard;

public class DashboardSummaryDto
{
    public int TotalProducts { get; set; }
    public int TotalWarehouses { get; set; }
    public int TodayTransactions { get; set; }
    public int LowStockCount { get; set; }
    public int TotalStockIn { get; set; }
    public int TotalStockOut { get; set; }
    public List<LowStockProductDto> LowStockProducts { get; set; } = new();
    public List<RecentTransactionDto> RecentTransactions { get; set; } = new();
}

public class LowStockProductDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty;
    public int CurrentStock { get; set; }
    public int MinStockLevel { get; set; }
}

public class RecentTransactionDto
{
    public int Id { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string WarehouseName { get; set; } = string.Empty;
    public string TransactionType { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public DateTime CreatedAt { get; set; }
}
