using Microsoft.AspNetCore.Mvc;
using WarehouseAPI.DTOs.Product;
using WarehouseAPI.Managers.Interfaces;

namespace WarehouseAPI.Controllers;

// ============================================================
// KURAL UYUM ÖZETİ — ProductController
// ============================================================
// [KURAL-1] HTTP Metod Kuralları  → Sadece GET ve POST kullanıldı
//                                    PUT/DELETE HİÇ KULLANILMADI
// [KURAL-2] Soft Delete          → IsDeleted=true, fiziksel silme yok
// [KURAL-3] EF Core Zorunlu      → Tüm DB işlemleri EF Core ile
// [KURAL-4] EntityState.Modified → Repository.UpdateAsync'te uygulandı
// [KURAL-5] Server-Side Pagination→ GetByCompany endpoint'inde Skip/Take
// [KURAL-6] Naming Convention    → PascalCase (CompanyId, ProductName...)
// [KURAL-MT] Multi-Tenant        → Her endpoint'te CompanyId kontrolü
// ============================================================

[ApiController]
[Route("api/product")]
public class ProductController : ControllerBase
{
    private readonly IProductManager _manager;

    public ProductController(IProductManager manager)
    {
        _manager = manager;
    }

    // [KURAL-1] GET kullanımı ✓
    // [KURAL-5] Server-side pagination: page + pageSize parametreleri backend'de işlenir
    // [KURAL-MT] CompanyId route parametresi ile filtreleme
    [HttpGet("by-company/{companyId}")]
    public async Task<IActionResult> GetByCompany(
        string companyId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25,
        [FromQuery] string? search = null,
        [FromQuery] string? category = null)
    {
        // [KURAL-MT] CompanyId eksikse 400 BadRequest döndür
        if (string.IsNullOrWhiteSpace(companyId))
            return BadRequest(new { success = false, message = "CompanyId zorunludur." });

        var request = new ProductListRequestDto
        {
            CompanyId = companyId,
            Page = page,
            PageSize = pageSize,
            Search = search,
            Category = category
        };

        var result = await _manager.GetPagedAsync(request);
        return Ok(result);
    }

    // [KURAL-1] GET kullanımı ✓
    // [KURAL-MT] CompanyId query parametresi zorunlu, uyuşmazsa 403 Forbid
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id, [FromQuery] string companyId)
    {
        if (string.IsNullOrWhiteSpace(companyId))
            return BadRequest(new { success = false, message = "CompanyId zorunludur." });

        var product = await _manager.GetByIdAsync(id, companyId);
        if (product == null) return NotFound(new { success = false, message = "Ürün bulunamadı." });

        // [KURAL-MT] CompanyId uyuşmuyorsa 403 Forbid
        if (product.CompanyId != companyId) return Forbid();

        return Ok(new { success = true, data = product });
    }

    // [KURAL-1] POST kullanımı ✓ (PUT yasak)
    // [KURAL-MT] CompanyId body'de zorunlu
    [HttpPost("create")]
    public async Task<IActionResult> Create([FromBody] CreateProductDto dto)
    {
        // [KURAL-MT] CompanyId eksikse 400 BadRequest
        if (string.IsNullOrWhiteSpace(dto.CompanyId))
            return BadRequest(new { success = false, message = "CompanyId zorunludur." });

        var (success, message, data) = await _manager.CreateAsync(dto);
        if (!success) return BadRequest(new { success, message });
        return Ok(new { success, message, data });
    }

    // [KURAL-1] POST kullanımı ✓ (PUT/DELETE yasak)
    // [KURAL-MT] CompanyId ve entity eşleşmesi kontrolü
    [HttpPost("update")]
    public async Task<IActionResult> Update([FromBody] UpdateProductDto dto)
    {
        // [KURAL-MT] CompanyId eksikse 400 BadRequest
        if (string.IsNullOrWhiteSpace(dto.CompanyId))
            return BadRequest(new { success = false, message = "CompanyId zorunludur." });

        var existing = await _manager.GetByIdAsync(dto.Id, dto.CompanyId);
        if (existing == null) return NotFound(new { success = false, message = "Ürün bulunamadı." });

        // [KURAL-MT] CompanyId uyuşmuyorsa 403 Forbid
        if (existing.CompanyId != dto.CompanyId) return Forbid();

        var (success, message) = await _manager.UpdateAsync(dto);
        if (!success) return BadRequest(new { success, message });
        return Ok(new { success, message });
    }

    // [KURAL-1] POST kullanımı ✓ (DELETE yasak!)
    // [KURAL-2] Soft delete — IsDeleted=true yapılır, kayıt fiziksel silinmez
    // [KURAL-MT] CompanyId ve entity eşleşmesi kontrolü
    [HttpPost("delete")]
    public async Task<IActionResult> Delete([FromBody] DeleteProductDto dto)
    {
        // [KURAL-MT] CompanyId eksikse 400 BadRequest
        if (string.IsNullOrWhiteSpace(dto.CompanyId))
            return BadRequest(new { success = false, message = "CompanyId zorunludur." });

        var (success, message) = await _manager.DeleteAsync(dto);
        if (!success) return BadRequest(new { success, message });
        return Ok(new { success, message });
    }
}
