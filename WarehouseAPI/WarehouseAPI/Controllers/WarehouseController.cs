using Microsoft.AspNetCore.Mvc;
using WarehouseAPI.DTOs.Warehouse;
using WarehouseAPI.Managers.Interfaces;

namespace WarehouseAPI.Controllers;

// ============================================================
// KURAL UYUM ÖZETİ — WarehouseController
// ============================================================
// [KURAL-1] HTTP Metod Kuralları  → Sadece GET ve POST kullanıldı
//                                    PUT/DELETE HİÇ KULLANILMADI
// [KURAL-2] Soft Delete          → IsDeleted=true, fiziksel silme yok
// [KURAL-3] EF Core Zorunlu      → Tüm DB işlemleri EF Core ile
// [KURAL-4] EntityState.Modified → Repository.UpdateAsync'te uygulandı
// [KURAL-5] Server-Side Pagination→ GetByCompany endpoint'inde Skip/Take
// [KURAL-6] Naming Convention    → PascalCase (CompanyId, Name...)
// [KURAL-MT] Multi-Tenant        → Her endpoint'te CompanyId kontrolü
// ============================================================

[ApiController]
[Route("api/warehouse")]
public class WarehouseController : ControllerBase
{
    private readonly IWarehouseManager _manager;

    public WarehouseController(IWarehouseManager manager)
    {
        _manager = manager;
    }

    // [KURAL-1] GET kullanımı ✓
    // [KURAL-5] Server-side pagination: page + pageSize + arama desteği
    // [KURAL-MT] CompanyId zorunlu — başka şirketin deposu görülmez
    [HttpGet("by-company/{companyId}")]
    public async Task<IActionResult> GetByCompany(
        string companyId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25,
        [FromQuery] string? search = null)
    {
        // [KURAL-MT] CompanyId eksikse 400 BadRequest
        if (string.IsNullOrWhiteSpace(companyId))
            return BadRequest(new { success = false, message = "CompanyId zorunludur." });

        var request = new WarehouseListRequestDto
        {
            CompanyId = companyId,
            Page = page,
            PageSize = pageSize,
            Search = search
        };

        var result = await _manager.GetPagedAsync(request);
        return Ok(result);
    }

    // [KURAL-1] GET kullanımı ✓
    // [KURAL-MT] CompanyId uyuşmuyorsa 403 Forbid
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id, [FromQuery] string companyId)
    {
        // [KURAL-MT] CompanyId eksikse 400 BadRequest
        if (string.IsNullOrWhiteSpace(companyId))
            return BadRequest(new { success = false, message = "CompanyId zorunludur." });

        var warehouse = await _manager.GetByIdAsync(id, companyId);
        if (warehouse == null) return NotFound(new { success = false, message = "Depo bulunamadı." });

        // [KURAL-MT] CompanyId uyuşmuyorsa 403 Forbid
        if (warehouse.CompanyId != companyId) return Forbid();

        return Ok(new { success = true, data = warehouse });
    }

    // [KURAL-1] POST kullanımı ✓ (PUT yasak)
    // [KURAL-MT] CompanyId body'de zorunlu
    [HttpPost("create")]
    public async Task<IActionResult> Create([FromBody] CreateWarehouseDto dto)
    {
        // [KURAL-MT] CompanyId eksikse 400 BadRequest
        if (string.IsNullOrWhiteSpace(dto.CompanyId))
            return BadRequest(new { success = false, message = "CompanyId zorunludur." });

        var (success, message, data) = await _manager.CreateAsync(dto);
        if (!success) return BadRequest(new { success, message });
        return Ok(new { success, message, data });
    }

    // [KURAL-1] POST kullanımı ✓ (PUT yasak)
    // [KURAL-4] EntityState.Modified → Repository katmanında uygulandı
    // [KURAL-MT] CompanyId uyuşmazlık kontrolü
    [HttpPost("update")]
    public async Task<IActionResult> Update([FromBody] UpdateWarehouseDto dto)
    {
        // [KURAL-MT] CompanyId eksikse 400 BadRequest
        if (string.IsNullOrWhiteSpace(dto.CompanyId))
            return BadRequest(new { success = false, message = "CompanyId zorunludur." });

        var existing = await _manager.GetByIdAsync(dto.Id, dto.CompanyId);
        if (existing == null) return NotFound(new { success = false, message = "Depo bulunamadı." });

        // [KURAL-MT] CompanyId uyuşmuyorsa 403 Forbid
        if (existing.CompanyId != dto.CompanyId) return Forbid();

        var (success, message) = await _manager.UpdateAsync(dto);
        if (!success) return BadRequest(new { success, message });
        return Ok(new { success, message });
    }

    // [KURAL-1] POST kullanımı ✓ (DELETE yasak!)
    // [KURAL-2] Soft delete — Manager → Repository → IsDeleted=true, EntityState.Modified
    // [KURAL-MT] CompanyId kontrolü
    [HttpPost("delete")]
    public async Task<IActionResult> Delete([FromBody] DeleteWarehouseDto dto)
    {
        // [KURAL-MT] CompanyId eksikse 400 BadRequest
        if (string.IsNullOrWhiteSpace(dto.CompanyId))
            return BadRequest(new { success = false, message = "CompanyId zorunludur." });

        var (success, message) = await _manager.DeleteAsync(dto);
        if (!success) return BadRequest(new { success, message });
        return Ok(new { success, message });
    }
}
