using WarehouseAPI.DTOs.StockTransaction;
using WarehouseAPI.Entities;

namespace WarehouseAPI.Repositories.Interfaces;

public interface IStockTransactionRepository
{
    Task<(List<StockTransaction> Items, int TotalCount)> GetPagedAsync(StockTransactionListRequestDto request);
    Task<StockTransaction> CreateAsync(StockTransaction transaction);
    Task<int> GetStockByProductAndWarehouseAsync(int productId, int warehouseId, string companyId);
    Task<int> GetTotalStockByProductAsync(int productId, string companyId);
    Task<int> GetTodayTransactionCountAsync(string companyId);
    Task<int> GetTotalStockInAsync(string companyId);
    Task<int> GetTotalStockOutAsync(string companyId);
    Task<List<StockTransaction>> GetRecentAsync(string companyId, int count = 5);
}
