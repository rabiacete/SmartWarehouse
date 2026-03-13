using WarehouseAPI.DTOs.Warehouse;
using WarehouseAPI.Entities;

namespace WarehouseAPI.Repositories.Interfaces;

public interface IWarehouseRepository
{
    Task<(List<Warehouse> Items, int TotalCount)> GetPagedAsync(WarehouseListRequestDto request);
    Task<Warehouse?> GetByIdAsync(int id, string companyId);
    Task<Warehouse> CreateAsync(Warehouse warehouse);
    Task UpdateAsync(Warehouse warehouse);
    Task<bool> SoftDeleteAsync(int id, string companyId);
}
