namespace WarehouseAPI.DTOs.Warehouse;

public class CreateWarehouseDto
{
    public string CompanyId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public string? Description { get; set; }
}

public class UpdateWarehouseDto
{
    public int Id { get; set; }
    public string CompanyId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public string? Description { get; set; }
}

public class DeleteWarehouseDto
{
    public int Id { get; set; }
    public string CompanyId { get; set; } = string.Empty;
}

public class WarehouseListRequestDto
{
    public string CompanyId { get; set; } = string.Empty;
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 25;
    public string? Search { get; set; }
}

public class WarehouseResponseDto
{
    public int Id { get; set; }
    public string CompanyId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
