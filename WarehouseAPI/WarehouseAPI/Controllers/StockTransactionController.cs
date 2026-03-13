using Microsoft.AspNetCore.Mvc;
using WarehouseAPI.DTOs.StockTransaction;
using WarehouseAPI.Managers.Interfaces;

namespace WarehouseAPI.Controllers;

// ============================================================
// KURAL UYUM ÖZETİ — StockTransactionController
// ============================================================
// [KURAL-1] HTTP Metod Kuralları  → Sadece GET ve POST kullanıldı
//                                    PUT/DELETE HİÇ KULLANILMADI
// [KURAL-3] EF Core Zorunlu      → Tüm DB işlemleri EF Core ile
// [KURAL-5] Server-Side Pagination→ GetByCompany endpoint'inde Skip/Take
// [KURAL-MT] Multi-Tenant        → Her endpoint'te CompanyId kontrolü
// NOT: Stok hareketleri düzenlenmez ve silinmez (iş kuralı)
//      Bu yüzden update/delete endpoint'i YOKTUR — kasıtlı tasarım.
// ============================================================

[ApiController]
[Route("api/stock-transaction")]
public class StockTransactionController : ControllerBase
{
    private readonly IStockTransactionManager _manager;

    public StockTransactionController(IStockTransactionManager manager)
    {
        _manager = manager;
    }

    // [KURAL-1] GET kullanımı ✓
    // [KURAL-5] Server-side pagination: page + pageSize parametreleri
    // [KURAL-MT] CompanyId zorunlu — başka şirketin hareketi görülmez
    // Ek filtreler: productId, warehouseId, transactionType (In/Out)
    [HttpGet("by-company/{companyId}")]
    public async Task<IActionResult> GetByCompany(
        string companyId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25,
        [FromQuery] int? productId = null,
        [FromQuery] int? warehouseId = null,
        [FromQuery] int? transactionType = null)
    {
        // [KURAL-MT] CompanyId eksikse 400 BadRequest
        if (string.IsNullOrWhiteSpace(companyId))
            return BadRequest(new { success = false, message = "CompanyId zorunludur." });

        var request = new StockTransactionListRequestDto
        {
            CompanyId = companyId,
            Page = page,
            PageSize = pageSize,
            ProductId = productId,
            WarehouseId = warehouseId,
            TransactionType = transactionType.HasValue
                ? (Entities.TransactionType)transactionType.Value
                : null
        };

        var result = await _manager.GetPagedAsync(request);
        return Ok(result);
    }

    // [KURAL-1] POST kullanımı ✓ (PUT yasak)
    // [KURAL-MT] CompanyId body'de zorunlu
    // İş kuralı: Stok çıkışında yeterli stok kontrolü Manager'da yapılır
    [HttpPost("create")]
    public async Task<IActionResult> Create([FromBody] CreateStockTransactionDto dto)
    {
        // [KURAL-MT] CompanyId eksikse 400 BadRequest
        if (string.IsNullOrWhiteSpace(dto.CompanyId))
            return BadRequest(new { success = false, message = "CompanyId zorunludur." });

        var (success, message, data) = await _manager.CreateAsync(dto);
        if (!success) return BadRequest(new { success, message });
        return Ok(new { success, message, data });
    }
}
