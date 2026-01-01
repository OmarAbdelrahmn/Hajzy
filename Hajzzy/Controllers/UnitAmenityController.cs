using Application.Contracts.Aminety;
using Application.Service.UnitAmenity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hajzzy.Controllers;

[Route("api/units/{unitId}/amenities")]
[ApiController]
public class UnitAmenityController(IUnitAmenityService service) : ControllerBase
{
    private readonly IUnitAmenityService _service = service;

    #region Query Operations

    /// <summary>
    /// Get all amenities for a specific unit
    /// </summary>
    [HttpGet("")]
    public async Task<IActionResult> GetUnitAmenities(int unitId)
    {
        var result = await _service.GetUnitAmenitiesAsync(unitId);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    #endregion

    #region Attach Operations

    /// <summary>
    /// Attach a single amenity to a unit
    /// </summary>
    [HttpPost("{amenityId}")]
 
    public async Task<IActionResult> AttachAmenity(int unitId, int amenityId)
    {
        var result = await _service.AttachAmenityAsync(unitId, amenityId);
        return result.IsSuccess
            ? Ok(new { Message = "Amenity attached successfully" })
            : result.ToProblem();
    }

    /// <summary>
    /// Attach multiple amenities to a unit
    /// </summary>
    [HttpPost("batch")]
 
    public async Task<IActionResult> AttachAmenities(int unitId, [FromBody] AttachAmenitiesRequest request)
    {
        var result = await _service.AttachAmenitiesAsync(unitId, request.AmenityIds);
        return result.IsSuccess
            ? Ok(new { Message = $"{request.AmenityIds.Count} amenities attached successfully" })
            : result.ToProblem();
    }

    #endregion

    #region Update Operations

    /// <summary>
    /// Update unit amenities (replace all)
    /// </summary>
    [HttpPut("")]
 
    public async Task<IActionResult> UpdateAmenities(int unitId, [FromBody] UpdateUnitAmenitiesRequest request)
    {
        var result = await _service.UpdateAmenitiesAsync(unitId, request);
        return result.IsSuccess
            ? Ok(new { Message = "Unit amenities updated successfully" })
            : result.ToProblem();
    }

    /// <summary>
    /// Toggle availability of a specific amenity in a unit
    /// </summary>
    [HttpPatch("{amenityId}/toggle")]
 
    public async Task<IActionResult> ToggleAvailability(int unitId, int amenityId)
    {
        var result = await _service.ToggleAvailabilityAsync(unitId, amenityId);
        return result.IsSuccess
            ? Ok(new { Message = "Amenity availability toggled successfully" })
            : result.ToProblem();
    }

    #endregion

    #region Remove Operations

    /// <summary>
    /// Remove a single amenity from a unit
    /// </summary>
    [HttpDelete("{amenityId}")]
 
    public async Task<IActionResult> RemoveAmenity(int unitId, int amenityId)
    {
        var result = await _service.RemoveAmenityAsync(unitId, amenityId);
        return result.IsSuccess
            ? Ok(new { Message = "Amenity removed successfully" })
            : result.ToProblem();
    }

    /// <summary>
    /// Remove all amenities from a unit
    /// </summary>
    [HttpDelete("")]
 
    public async Task<IActionResult> RemoveAllAmenities(int unitId)
    {
        var result = await _service.RemoveAllAmenitiesAsync(unitId);
        return result.IsSuccess
            ? Ok(new { Message = "All amenities removed successfully" })
            : result.ToProblem();
    }

    #endregion
}