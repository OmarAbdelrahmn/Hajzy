using Application.Contracts.Aminety;
using Application.Service.SubUnitAmenity;
using Microsoft.AspNetCore.Mvc;

namespace Hajzzy.Controllers;

[Route("api/subunits/{subUnitId}/amenities")]
[ApiController]
public class SubUnitAmenityController(ISubUnitAmenityService service) : ControllerBase
{
    private readonly ISubUnitAmenityService _service = service;

    #region Query Operations

    /// <summary>
    /// Get all amenities for a specific subunit
    /// </summary>
    [HttpGet("")]
    public async Task<IActionResult> GetSubUnitAmenities(int subUnitId)
    {
        var result = await _service.GetSubUnitAmenitiesAsync(subUnitId);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    #endregion

    #region Attach Operations

    /// <summary>
    /// Attach a single amenity to a subunit
    /// </summary>
    [HttpPost("{amenityId}")]

    public async Task<IActionResult> AttachAmenity(int subUnitId, int amenityId)
    {
        var result = await _service.AttachAmenityAsync(subUnitId, amenityId);
        return result.IsSuccess
            ? Ok(new { Message = "Amenity attached successfully" })
            : result.ToProblem();
    }

    /// <summary>
    /// Attach multiple amenities to a subunit
    /// </summary>
    [HttpPost("batch")]

    public async Task<IActionResult> AttachAmenities(int subUnitId, [FromBody] AttachAmenitiesRequest request)
    {
        var result = await _service.AttachAmenitiesAsync(subUnitId, request.AmenityIds);
        return result.IsSuccess
            ? Ok(new { Message = $"{request.AmenityIds.Count} amenities attached successfully" })
            : result.ToProblem();
    }

    /// <summary>
    /// Copy amenities from parent unit to subunit
    /// </summary>
    [HttpPost("copy-from-unit")]

    public async Task<IActionResult> CopyFromUnit(int subUnitId)
    {
        var result = await _service.CopyFromUnitAsync(subUnitId);
        return result.IsSuccess
            ? Ok(new { Message = "Amenities copied from unit successfully" })
            : result.ToProblem();
    }

    #endregion

    #region Update Operations

    /// <summary>
    /// Update subunit amenities (replace all)
    /// </summary>
    [HttpPut("")]

    public async Task<IActionResult> UpdateAmenities(int subUnitId, [FromBody] UpdateSubUnitAmenitiesRequest request)
    {
        var result = await _service.UpdateAmenitiesAsync(subUnitId, request);
        return result.IsSuccess
            ? Ok(new { Message = "SubUnit amenities updated successfully" })
            : result.ToProblem();
    }

    /// <summary>
    /// Toggle availability of a specific amenity in a subunit
    /// </summary>
    [HttpPatch("{amenityId}/toggle")]

    public async Task<IActionResult> ToggleAvailability(int subUnitId, int amenityId)
    {
        var result = await _service.ToggleAvailabilityAsync(subUnitId, amenityId);
        return result.IsSuccess
            ? Ok(new { Message = "Amenity availability toggled successfully" })
            : result.ToProblem();
    }

    #endregion

    #region Remove Operations

    /// <summary>
    /// Remove a single amenity from a subunit
    /// </summary>
    [HttpDelete("{amenityId}")]

    public async Task<IActionResult> RemoveAmenity(int subUnitId, int amenityId)
    {
        var result = await _service.RemoveAmenityAsync(subUnitId, amenityId);
        return result.IsSuccess
            ? Ok(new { Message = "Amenity removed successfully" })
            : result.ToProblem();
    }

    /// <summary>
    /// Remove all amenities from a subunit
    /// </summary>
    [HttpDelete("")]

    public async Task<IActionResult> RemoveAllAmenities(int subUnitId)
    {
        var result = await _service.RemoveAllAmenitiesAsync(subUnitId);
        return result.IsSuccess
            ? Ok(new { Message = "All amenities removed successfully" })
            : result.ToProblem();
    }

    #endregion
}