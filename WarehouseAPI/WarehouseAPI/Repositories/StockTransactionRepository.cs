using Microsoft.EntityFrameworkCore;
using WarehouseAPI.Data;
using WarehouseAPI.DTOs.StockTransaction;
using WarehouseAPI.Entities;
using WarehouseAPI.Repositories.Interfaces;

namespace WarehouseAPI.Repositories;

public class StockTransactionRepository : IStockTransactionRepository
{
    private readonly AppDbContext _context;

    public StockTransactionRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<(List<StockTransaction> Items, int TotalCount)> GetPagedAsync(StockTransactionListRequestDto request)
    {
        var query = _context.StockTransactions
            .Include(x => x.Product)
            .Include(x => x.Warehouse)
            .Where(x => x.CompanyId == request.CompanyId && !x.IsDeleted);

        if (request.ProductId.HasValue)
            query = query.Where(x => x.ProductId == request.ProductId.Value);

        if (request.WarehouseId.HasValue)
            query = query.Where(x => x.WarehouseId == request.WarehouseId.Value);

        if (request.TransactionType.HasValue)
            query = query.Where(x => x.TransactionType == request.TransactionType.Value);

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<StockTransaction> CreateAsync(StockTransaction transaction)
    {
        _context.StockTransactions.Add(transaction);
        await _context.SaveChangesAsync();
        return transaction;
    }

    public async Task<int> GetStockByProductAndWarehouseAsync(int productId, int warehouseId, string companyId)
    {
        var stockIn = await _context.StockTransactions
            .Where(x => x.ProductId == productId && x.WarehouseId == warehouseId
                     && x.CompanyId == companyId && x.TransactionType == TransactionType.In && !x.IsDeleted)
            .SumAsync(x => (int?)x.Quantity) ?? 0;

        var stockOut = await _context.StockTransactions
            .Where(x => x.ProductId == productId && x.WarehouseId == warehouseId
                     && x.CompanyId == companyId && x.TransactionType == TransactionType.Out && !x.IsDeleted)
            .SumAsync(x => (int?)x.Quantity) ?? 0;

        return stockIn - stockOut;
    }

    public async Task<int> GetTotalStockByProductAsync(int productId, string companyId)
    {
        var stockIn = await _context.StockTransactions
            .Where(x => x.ProductId == productId && x.CompanyId == companyId
                     && x.TransactionType == TransactionType.In && !x.IsDeleted)
            .SumAsync(x => (int?)x.Quantity) ?? 0;

        var stockOut = await _context.StockTransactions
            .Where(x => x.ProductId == productId && x.CompanyId == companyId
                     && x.TransactionType == TransactionType.Out && !x.IsDeleted)
            .SumAsync(x => (int?)x.Quantity) ?? 0;

        return stockIn - stockOut;
    }

    public async Task<int> GetTodayTransactionCountAsync(string companyId)
    {
        var today = DateTime.UtcNow.Date;
        return await _context.StockTransactions
            .Where(x => x.CompanyId == companyId && !x.IsDeleted && x.CreatedAt >= today)
            .CountAsync();
    }

    public async Task<int> GetTotalStockInAsync(string companyId)
    {
        return await _context.StockTransactions
            .Where(x => x.CompanyId == companyId && !x.IsDeleted && x.TransactionType == TransactionType.In)
            .SumAsync(x => (int?)x.Quantity) ?? 0;
    }

    public async Task<int> GetTotalStockOutAsync(string companyId)
    {
        return await _context.StockTransactions
            .Where(x => x.CompanyId == companyId && !x.IsDeleted && x.TransactionType == TransactionType.Out)
            .SumAsync(x => (int?)x.Quantity) ?? 0;
    }

    public async Task<List<StockTransaction>> GetRecentAsync(string companyId, int count = 5)
    {
        return await _context.StockTransactions
            .Include(x => x.Product)
            .Include(x => x.Warehouse)
            .Where(x => x.CompanyId == companyId && !x.IsDeleted)
            .OrderByDescending(x => x.CreatedAt)
            .Take(count)
            .ToListAsync();
    }
}
