using Application.Service.Report;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Hajzzy.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ReportController(IReportService service) : ControllerBase
{
    private readonly IReportService service = service;

    [HttpGet("revenue")]
    public async Task<IActionResult> GenerateRevenueReportAsync(ReportFilter filter)
    {
        var result = await service.GenerateRevenueReportAsync(filter);

        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }
   
    [HttpGet("booking")]
    public async Task<IActionResult> GenerateBookingReportAsync(ReportFilter filter)
    {
        var result = await service.GenerateBookingReportAsync(filter);

        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }
    
    [HttpGet("unit-performance")]
    public async Task<IActionResult> GenerateUnitPerformanceReportAsync(ReportFilter filter)
    {
        var result = await service.GenerateUnitPerformanceReportAsync(filter);

        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpGet("user-performance")]
    public async Task<IActionResult> GenerateUserActivityReportAsync(ReportFilter filter)
    {
        var result = await service.GenerateUserActivityReportAsync(filter);

        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }
    [HttpGet("occupancy")]
    public async Task<IActionResult> GenerateOccupancyReportAsync(ReportFilter filter)
    {
        var result = await service.GenerateOccupancyReportAsync(filter);

        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }
    [HttpGet("department")]
    public async Task<IActionResult> GenerateDepartmentReportAsync(ReportFilter filter)
    {
        var result = await service.GenerateDepartmentReportAsync(filter);

        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }
    [HttpGet("dashboard")]
    public async Task<IActionResult> GenerateDashboardReportAsync(ReportFilter filter)
    {
        var result = await service.GenerateDashboardReportAsync(filter);

        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }
}
