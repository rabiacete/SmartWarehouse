using WarehouseAPI.DTOs.Warehouse;

namespace WarehouseAPI.Managers.Interfaces;

public interface IWarehouseManager
{
    Task<object> GetPagedAsync(WarehouseListRequestDto request);
    Task<WarehouseResponseDto?> GetByIdAsync(int id, string companyId);
    Task<(bool Success, string Message, WarehouseResponseDto? Data)> CreateAsync(CreateWarehouseDto dto);
    Task<(bool Success, string Message)> UpdateAsync(UpdateWarehouseDto dto);
    Task<(bool Success, string Message)> DeleteAsync(DeleteWarehouseDto dto);
}
