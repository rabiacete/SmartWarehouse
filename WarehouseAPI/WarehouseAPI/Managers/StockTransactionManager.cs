using WarehouseAPI.DTOs.Dashboard;
using WarehouseAPI.DTOs.StockTransaction;
using WarehouseAPI.Entities;
using WarehouseAPI.Managers.Interfaces;
using WarehouseAPI.Repositories.Interfaces;

namespace WarehouseAPI.Managers;

public class StockTransactionManager : IStockTransactionManager
{
    private readonly IStockTransactionRepository _repo;
    private readonly IProductRepository _productRepo;
    private readonly IWarehouseRepository _warehouseRepo;

    public StockTransactionManager(
        IStockTransactionRepository repo,
        IProductRepository productRepo,
        IWarehouseRepository warehouseRepo)
    {
        _repo = repo;
        _productRepo = productRepo;
        _warehouseRepo = warehouseRepo;
    }

    public async Task<object> GetPagedAsync(StockTransactionListRequestDto request)
    {
        var (items, totalCount) = await _repo.GetPagedAsync(request);
        var dtos = items.Select(MapToDto).ToList();

        return new
        {
            success = true,
            data = dtos,
            totalCount,
            page = request.Page,
            pageSize = request.PageSize,
            totalPages = (int)Math.Ceiling((double)totalCount / request.PageSize)
        };
    }

    public async Task<(bool Success, string Message, StockTransactionResponseDto? Data)> CreateAsync(CreateStockTransactionDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.CompanyId))
            return (false, "CompanyId zorunludur.", null);

        if (dto.Quantity <= 0)
            return (false, "Miktar 0'dan büyük olmalıdır.", null);

        // Ürün kontrolü
        var product = await _productRepo.GetByIdAsync(dto.ProductId, dto.CompanyId);
        if (product == null)
            return (false, "Ürün bulunamadı veya bu şirkete ait değil.", null);

        // Depo kontrolü
        var warehouse = await _warehouseRepo.GetByIdAsync(dto.WarehouseId, dto.CompanyId);
        if (warehouse == null)
            return (false, "Depo bulunamadı veya bu şirkete ait değil.", null);

        // Çıkış işleminde yeterli stok kontrolü
        if (dto.TransactionType == TransactionType.Out)
        {
            var currentStock = await _repo.GetStockByProductAndWarehouseAsync(dto.ProductId, dto.WarehouseId, dto.CompanyId);
            if (currentStock < dto.Quantity)
                return (false, $"Yetersiz stok. {warehouse.Name} deposunda bu ürün için mevcut stok: {currentStock}", null);
        }

        var transaction = new StockTransaction
        {
            CompanyId = dto.CompanyId,
            ProductId = dto.ProductId,
            WarehouseId = dto.WarehouseId,
            TransactionType = dto.TransactionType,
            Quantity = dto.Quantity,
            Note = dto.Note,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var created = await _repo.CreateAsync(transaction);
        created.Product = product;
        created.Warehouse = warehouse;

        return (true, dto.TransactionType == TransactionType.In ? "Stok girişi başarıyla kaydedildi." : "Stok çıkışı başarıyla kaydedildi.", MapToDto(created));
    }

    private static StockTransactionResponseDto MapToDto(StockTransaction t) => new()
    {
        Id = t.Id,
        CompanyId = t.CompanyId,
        ProductId = t.ProductId,
        ProductName = t.Product?.ProductName ?? "",
        ProductSKU = t.Product?.SKU ?? "",
        WarehouseId = t.WarehouseId,
        WarehouseName = t.Warehouse?.Name ?? "",
        TransactionType = t.TransactionType == TransactionType.In ? "Giriş" : "Çıkış",
        Quantity = t.Quantity,
        Note = t.Note,
        CreatedAt = t.CreatedAt
    };
}

public class DashboardManager : IDashboardManager
{
    private readonly IStockTransactionRepository _transactionRepo;
    private readonly IProductRepository _productRepo;
    private readonly IWarehouseRepository _warehouseRepo;

    public DashboardManager(
        IStockTransactionRepository transactionRepo,
        IProductRepository productRepo,
        IWarehouseRepository warehouseRepo)
    {
        _transactionRepo = transactionRepo;
        _productRepo = productRepo;
        _warehouseRepo = warehouseRepo;
    }

    public async Task<DashboardSummaryDto> GetSummaryAsync(string companyId)
    {
        var allProductsRequest = new DTOs.Product.ProductListRequestDto { CompanyId = companyId, Page = 1, PageSize = 1000 };
        var (products, totalProducts) = await _productRepo.GetPagedAsync(allProductsRequest);

        var allWarehousesRequest = new DTOs.Warehouse.WarehouseListRequestDto { CompanyId = companyId, Page = 1, PageSize = 1000 };
        var (_, totalWarehouses) = await _warehouseRepo.GetPagedAsync(allWarehousesRequest);

        // Düşük stoklu ürünler
        var lowStockProducts = new List<LowStockProductDto>();
        foreach (var product in products)
        {
            var stock = await _transactionRepo.GetTotalStockByProductAsync(product.Id, companyId);
            if (stock <= product.MinStockLevel)
            {
                lowStockProducts.Add(new LowStockProductDto
                {
                    ProductId = product.Id,
                    ProductName = product.ProductName,
                    SKU = product.SKU,
                    CurrentStock = stock,
                    MinStockLevel = product.MinStockLevel
                });
            }
        }

        var recentTransactions = await _transactionRepo.GetRecentAsync(companyId, 5);

        return new DashboardSummaryDto
        {
            TotalProducts = totalProducts,
            TotalWarehouses = totalWarehouses,
            TodayTransactions = await _transactionRepo.GetTodayTransactionCountAsync(companyId),
            LowStockCount = lowStockProducts.Count,
            TotalStockIn = await _transactionRepo.GetTotalStockInAsync(companyId),
            TotalStockOut = await _transactionRepo.GetTotalStockOutAsync(companyId),
            LowStockProducts = lowStockProducts,
            RecentTransactions = recentTransactions.Select(t => new RecentTransactionDto
            {
                Id = t.Id,
                ProductName = t.Product?.ProductName ?? "",
                WarehouseName = t.Warehouse?.Name ?? "",
                TransactionType = t.TransactionType == TransactionType.In ? "Giriş" : "Çıkış",
                Quantity = t.Quantity,
                CreatedAt = t.CreatedAt
            }).ToList()
        };
    }
}
