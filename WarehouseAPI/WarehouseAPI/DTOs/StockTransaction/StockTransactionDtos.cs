using WarehouseAPI.Entities;

namespace WarehouseAPI.DTOs.StockTransaction;

public class CreateStockTransactionDto
{
    public string CompanyId { get; set; } = string.Empty;
    public int ProductId { get; set; }
    public int WarehouseId { get; set; }
    public TransactionType TransactionType { get; set; }
    public int Quantity { get; set; }
    public string? Note { get; set; }
}

public class StockTransactionListRequestDto
{
    public string CompanyId { get; set; } = string.Empty;
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 25;
    public int? ProductId { get; set; }
    public int? WarehouseId { get; set; }
    public TransactionType? TransactionType { get; set; }
}

public class StockTransactionResponseDto
{
    public int Id { get; set; }
    public string CompanyId { get; set; } = string.Empty;
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductSKU { get; set; } = string.Empty;
    public int WarehouseId { get; set; }
    public string WarehouseName { get; set; } = string.Empty;
    public string TransactionType { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public string? Note { get; set; }
    public DateTime CreatedAt { get; set; }
}
