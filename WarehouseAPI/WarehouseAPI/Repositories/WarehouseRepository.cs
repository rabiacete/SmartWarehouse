using Microsoft.EntityFrameworkCore;
using WarehouseAPI.Data;
using WarehouseAPI.DTOs.Warehouse;
using WarehouseAPI.Entities;
using WarehouseAPI.Repositories.Interfaces;

namespace WarehouseAPI.Repositories;

public class WarehouseRepository : IWarehouseRepository
{
    private readonly AppDbContext _context;

    public WarehouseRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<(List<Warehouse> Items, int TotalCount)> GetPagedAsync(WarehouseListRequestDto request)
    {
        var query = _context.Warehouses
            .Where(x => x.CompanyId == request.CompanyId && !x.IsDeleted);

        if (!string.IsNullOrWhiteSpace(request.Search))
            query = query.Where(x => x.Name.Contains(request.Search) || x.Location.Contains(request.Search));

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<Warehouse?> GetByIdAsync(int id, string companyId)
    {
        return await _context.Warehouses
            .FirstOrDefaultAsync(x => x.Id == id && x.CompanyId == companyId && !x.IsDeleted);
    }

    public async Task<Warehouse> CreateAsync(Warehouse warehouse)
    {
        _context.Warehouses.Add(warehouse);
        await _context.SaveChangesAsync();
        return warehouse;
    }

    // [KURAL-3] EF Core zorunlu — Raw SQL yasak
    // [KURAL-4] EntityState.Modified zorunlu
    public async Task UpdateAsync(Warehouse warehouse)
    {
        _context.Entry(warehouse).State = EntityState.Modified; // KURAL-4
        await _context.SaveChangesAsync();
    }

    // [KURAL-2] Soft delete — IsDeleted=true, fiziksel silme yok
    // [KURAL-4] EntityState.Modified zorunlu
    public async Task<bool> SoftDeleteAsync(int id, string companyId)
    {
        var warehouse = await _context.Warehouses
            .FirstOrDefaultAsync(x => x.Id == id && x.CompanyId == companyId && !x.IsDeleted);

        if (warehouse == null) return false;

        warehouse.IsDeleted = true;
        warehouse.UpdatedAt = DateTime.UtcNow;
        _context.Entry(warehouse).State = EntityState.Modified; // KURAL-4
        await _context.SaveChangesAsync();
        return true;
    }
}
