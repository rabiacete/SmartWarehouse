using WarehouseAPI.DTOs.Warehouse;
using WarehouseAPI.Entities;
using WarehouseAPI.Managers.Interfaces;
using WarehouseAPI.Repositories.Interfaces;

namespace WarehouseAPI.Managers;

public class WarehouseManager : IWarehouseManager
{
    private readonly IWarehouseRepository _repo;

    public WarehouseManager(IWarehouseRepository repo)
    {
        _repo = repo;
    }

    public async Task<object> GetPagedAsync(WarehouseListRequestDto request)
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

    public async Task<WarehouseResponseDto?> GetByIdAsync(int id, string companyId)
    {
        var warehouse = await _repo.GetByIdAsync(id, companyId);
        return warehouse == null ? null : MapToDto(warehouse);
    }

    public async Task<(bool Success, string Message, WarehouseResponseDto? Data)> CreateAsync(CreateWarehouseDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.CompanyId))
            return (false, "CompanyId zorunludur.", null);

        if (string.IsNullOrWhiteSpace(dto.Name))
            return (false, "Depo adı zorunludur.", null);

        var warehouse = new Warehouse
        {
            CompanyId = dto.CompanyId,
            Name = dto.Name,
            Location = dto.Location,
            Capacity = dto.Capacity,
            Description = dto.Description,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var created = await _repo.CreateAsync(warehouse);
        return (true, "Depo başarıyla oluşturuldu.", MapToDto(created));
    }

    public async Task<(bool Success, string Message)> UpdateAsync(UpdateWarehouseDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.CompanyId))
            return (false, "CompanyId zorunludur.");

        var existing = await _repo.GetByIdAsync(dto.Id, dto.CompanyId);
        if (existing == null) return (false, "Depo bulunamadı.");

        existing.Name = dto.Name;
        existing.Location = dto.Location;
        existing.Capacity = dto.Capacity;
        existing.Description = dto.Description;
        existing.UpdatedAt = DateTime.UtcNow;

        await _repo.UpdateAsync(existing);
        return (true, "Depo başarıyla güncellendi.");
    }

    public async Task<(bool Success, string Message)> DeleteAsync(DeleteWarehouseDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.CompanyId))
            return (false, "CompanyId zorunludur.");

        var deleted = await _repo.SoftDeleteAsync(dto.Id, dto.CompanyId);
        if (!deleted) return (false, "Depo bulunamadı.");

        return (true, "Depo başarıyla silindi.");
    }

    private static WarehouseResponseDto MapToDto(Warehouse w) => new()
    {
        Id = w.Id,
        CompanyId = w.CompanyId,
        Name = w.Name,
        Location = w.Location,
        Capacity = w.Capacity,
        Description = w.Description,
        CreatedAt = w.CreatedAt,
        UpdatedAt = w.UpdatedAt
    };
}
