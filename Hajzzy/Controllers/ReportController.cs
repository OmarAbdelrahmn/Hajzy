using Application.Service.Report;
using Microsoft.AspNetCore.Mvc;

namespace Hajzzy.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ReportController(IReportService service) : ControllerBase
{
    private readonly IReportService service = service;

    [HttpPost("revenue")]
    public async Task<IActionResult> GenerateRevenueReportAsync(ReportFilter filter)
    {
        var result = await service.GenerateRevenueReportAsync(filter);

        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost("booking")]
    public async Task<IActionResult> GenerateBookingReportAsync(ReportFilter filter)
    {
        var result = await service.GenerateBookingReportAsync(filter);

        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost("unit-performance")]
    public async Task<IActionResult> GenerateUnitPerformanceReportAsync(ReportFilter filter)
    {
        var result = await service.GenerateUnitPerformanceReportAsync(filter);

        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost("user-performance")]
    public async Task<IActionResult> GenerateUserActivityReportAsync(ReportFilter filter)
    {
        var result = await service.GenerateUserActivityReportAsync(filter);

        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }
    [HttpPost("occupancy")]
    public async Task<IActionResult> GenerateOccupancyReportAsync(ReportFilter filter)
    {
        var result = await service.GenerateOccupancyReportAsync(filter);

        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }
    [HttpPost("department")]
    public async Task<IActionResult> GenerateDepartmentReportAsync(ReportFilter filter)
    {
        var result = await service.GenerateDepartmentReportAsync(filter);

        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }
    [HttpPost("dashboard")]
    public async Task<IActionResult> GenerateDashboardReportAsync(ReportFilter filter)
    {
        var result = await service.GenerateDashboardReportAsync(filter);

        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }
}
