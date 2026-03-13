using Microsoft.AspNetCore.Mvc;
using WarehouseAPI.Managers.Interfaces;

namespace WarehouseAPI.Controllers;

[ApiController]
[Route("api/dashboard")]
public class DashboardController : ControllerBase
{
    private readonly IDashboardManager _manager;

    public DashboardController(IDashboardManager manager)
    {
        _manager = manager;
    }

    [HttpGet("summary/{companyId}")]
    public async Task<IActionResult> GetSummary(string companyId)
    {
        if (string.IsNullOrWhiteSpace(companyId))
            return BadRequest(new { success = false, message = "CompanyId zorunludur." });

        var summary = await _manager.GetSummaryAsync(companyId);
        return Ok(new { success = true, data = summary });
    }
}
