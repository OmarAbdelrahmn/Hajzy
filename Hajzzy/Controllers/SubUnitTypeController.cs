using Application.Service.SubUnitType;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hajzzy.Controllers;

[Route("api/[controller]")]
[ApiController]
public class SubUnitTypeController(ISubUnitTypeService service) : ControllerBase
{
    private readonly ISubUnitTypeService _service = service;

    #region CRUD Operations

    /// <summary>
    /// Get sub unit type by ID
    /// </summary>
    [HttpGet("{subUnitTypeId}")]
    public async Task<IActionResult> GetById(int subUnitTypeId)
    {
        var result = await _service.GetByIdAsync(subUnitTypeId);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    /// <summary>
    /// Get sub unit type details with sub units list
    /// </summary>
    [HttpGet("{subUnitTypeId}/details")]
    public async Task<IActionResult> GetDetails(int subUnitTypeId)
    {
        var result = await _service.GetDetailsAsync(subUnitTypeId);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    /// <summary>
    /// Get all sub unit types
    /// </summary>
    [HttpGet("")]
    public async Task<IActionResult> GetAll(int page = 1, int pageSize = 10)
    {
        var result = await _service.GetAllSubUnitTypesAsync(page, pageSize);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    /// <summary>
    /// Create a new sub unit type
    /// </summary>
    [HttpPost("")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> Create([FromBody] CreateSubUnitTypeRequest request)
    {
        var result = await _service.CreateAsync(request);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { subUnitTypeId = result.Value.Id }, result.Value)
            : result.ToProblem();
    }

    /// <summary>
    /// Update an existing sub unit type
    /// </summary>
    [HttpPut("{subUnitTypeId}")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> Update(
        int subUnitTypeId,
        [FromBody] UpdateSubUnitTypeRequest request)
    {
        var result = await _service.UpdateAsync(subUnitTypeId, request);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    /// <summary>
    /// Delete a sub unit type
    /// </summary>
    [HttpDelete("{subUnitTypeId}")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> Delete(int subUnitTypeId)
    {
        var result = await _service.DeleteAsync(subUnitTypeId);
        return result.IsSuccess
            ? Ok(new { Message = "Sub unit type deleted successfully" })
            : result.ToProblem();
    }

    #endregion

    #region Status Management

    /// <summary>
    /// Toggle sub unit type active status
    /// </summary>
    [HttpPost("{subUnitTypeId}/toggle-active")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> ToggleActive(int subUnitTypeId)
    {
        var result = await _service.ToggleActiveAsync(subUnitTypeId);
        return result.IsSuccess
            ? Ok(new { Message = "Sub unit type status toggled successfully" })
            : result.ToProblem();
    }

    #endregion

    #region Filtering & Search

    /// <summary>
    /// Filter sub unit types with pagination
    /// </summary>
    [HttpPost("filter")]
    public async Task<IActionResult> Filter([FromBody] SubUnitTypeFilter filter)
    {
        var result = await _service.FilterAsync(filter);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    /// <summary>
    /// Search sub unit types by keyword
    /// </summary>
    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string keyword)
    {
        if (string.IsNullOrWhiteSpace(keyword))
            return BadRequest(new { Message = "Keyword is required" });

        var result = await _service.SearchAsync(keyword);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    #endregion

    #region Statistics

    /// <summary>
    /// Get sub unit type statistics
    /// </summary>
    [HttpGet("statistics")]
    [Authorize(Roles = "SuperAdmin,CityAdmin")]
    public async Task<IActionResult> GetStatistics()
    {
        var result = await _service.GetStatisticsAsync();
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    #endregion

    #region Validation

    /// <summary>
    /// Check if sub unit type name is unique
    /// </summary>
    [HttpGet("validate/name")]
    public async Task<IActionResult> ValidateName(
        [FromQuery] string name,
        [FromQuery] int? excludeId = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            return BadRequest(new { Message = "Name is required" });

        var result = await _service.IsNameUniqueAsync(name, excludeId);
        return result.IsSuccess
            ? Ok(new { IsUnique = result.Value })
            : result.ToProblem();
    }

    /// <summary>
    /// Check if sub unit type can be deleted
    /// </summary>
    [HttpGet("{subUnitTypeId}/can-delete")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> CanDelete(int subUnitTypeId)
    {
        var result = await _service.CanDeleteAsync(subUnitTypeId);
        return result.IsSuccess
            ? Ok(new { CanDelete = result.Value })
            : result.ToProblem();
    }

    #endregion
}