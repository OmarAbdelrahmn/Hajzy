using Application.Contracts.Unit;
using Application.Service.UnitType;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hajzzy.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UnitTypeController(IUnitTypeService service) : ControllerBase
{
    private readonly IUnitTypeService _service = service;

    #region CRUD Operations

    /// <summary>
    /// Get unit type by ID
    /// </summary>
    [HttpGet("{unitTypeId}")]
    public async Task<IActionResult> GetById(int unitTypeId)
    {
        var result = await _service.GetByIdAsync(unitTypeId);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    /// <summary>
    /// Get unit type details with units list
    /// </summary>
    [HttpGet("{unitTypeId}/details")]
    public async Task<IActionResult> GetDetails(int unitTypeId)
    {
        var result = await _service.GetDetailsAsync(unitTypeId);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    /// <summary>
    /// Get all unit types
    /// </summary>
    [HttpGet("")]
    public async Task<IActionResult> GetAll([FromQuery] bool includeInactive = false)
    {
        var result = await _service.GetAllAsync(includeInactive);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    /// <summary>
    /// Create a new unit type
    /// </summary>
    [HttpPost("")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> Create([FromBody] CreateUnitTypeRequest request)
    {
        var result = await _service.CreateAsync(request);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { unitTypeId = result.Value.Id }, result.Value)
            : result.ToProblem();
    }

    /// <summary>
    /// Update an existing unit type
    /// </summary>
    [HttpPut("{unitTypeId}")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> Update(
        int unitTypeId,
        [FromBody] UpdateUnitTypeRequest request)
    {
        var result = await _service.UpdateAsync(unitTypeId, request);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    /// <summary>
    /// Delete a unit type
    /// </summary>
    [HttpDelete("{unitTypeId}")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> Delete(int unitTypeId)
    {
        var result = await _service.DeleteAsync(unitTypeId);
        return result.IsSuccess
            ? Ok(new { Message = "Unit type deleted successfully" })
            : result.ToProblem();
    }

    #endregion

    #region Status Management

    /// <summary>
    /// Toggle unit type active status
    /// </summary>
    [HttpPost("{unitTypeId}/toggle-active")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> ToggleActive(int unitTypeId)
    {
        var result = await _service.ToggleActiveAsync(unitTypeId);
        return result.IsSuccess
            ? Ok(new { Message = "Unit type status toggled successfully" })
            : result.ToProblem();
    }

    #endregion

    #region Filtering & Search

    /// <summary>
    /// Filter unit types with pagination
    /// </summary>
    [HttpPost("filter")]
    public async Task<IActionResult> Filter([FromBody] UnitTypeFilter filter)
    {
        var result = await _service.FilterAsync(filter);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    /// <summary>
    /// Search unit types by keyword
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
    /// Get unit type statistics
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
    /// Check if unit type name is unique
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
    /// Check if unit type can be deleted
    /// </summary>
    [HttpGet("{unitTypeId}/can-delete")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> CanDelete(int unitTypeId)
    {
        var result = await _service.CanDeleteAsync(unitTypeId);
        return result.IsSuccess
            ? Ok(new { CanDelete = result.Value })
            : result.ToProblem();
    }

    #endregion
}