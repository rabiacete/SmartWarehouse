using WarehouseAPI.DTOs.StockTransaction;
using WarehouseAPI.DTOs.Dashboard;

namespace WarehouseAPI.Managers.Interfaces;

public interface IStockTransactionManager
{
    Task<object> GetPagedAsync(StockTransactionListRequestDto request);
    Task<(bool Success, string Message, StockTransactionResponseDto? Data)> CreateAsync(CreateStockTransactionDto dto);
}

public interface IDashboardManager
{
    Task<DashboardSummaryDto> GetSummaryAsync(string companyId);
}
