using Application.Contracts.SubUnit;
using Application.Extensions;
using Application.Service.SubUnit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hajzzy.Controllers;

[Route("api/[controller]")]
[ApiController]
public class SubUnitController(ISubUnitService service) : ControllerBase
{
    private readonly ISubUnitService _service = service;

    #region CRUD Operations

    /// <summary>
    /// Get subunit by ID
    /// </summary>
    [HttpGet("{subUnitId}")]
    public async Task<IActionResult> GetById(int subUnitId)
    {
        var result = await _service.GetByIdAsync(subUnitId);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    /// <summary>
    /// Get subunit details by ID (includes amenities and availability)
    /// </summary>
    [HttpGet("{subUnitId}/details")]
    public async Task<IActionResult> GetDetails(int subUnitId)
    {
        var result = await _service.GetDetailsAsync(subUnitId);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    /// <summary>
    /// Get all subunits for a specific unit
    /// </summary>
    [HttpGet("unit/{unitId}")]
    public async Task<IActionResult> GetByUnit(int unitId)
    {
        var result = await _service.GetByUnitAsync(unitId);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    /// <summary>
    /// Create a new subunit
    /// </summary>
    [HttpPost("")]
 
    public async Task<IActionResult> Create([FromBody] CreateSubUnitRequest request)
    {
        var result = await _service.CreateAsync(request);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { subUnitId = result.Value.Id }, result.Value)
            : result.ToProblem();
    }

    /// <summary>
    /// Update an existing subunit
    /// </summary>
    [HttpPut("{subUnitId}")]
 
    public async Task<IActionResult> Update(int subUnitId, [FromBody] UpdateSubUnitRequest request)
    {
        var result = await _service.UpdateAsync(subUnitId, request);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    /// <summary>
    /// Delete a subunit (soft delete by default)
    /// </summary>
    [HttpDelete("{subUnitId}")]
 
    public async Task<IActionResult> Delete(int subUnitId, [FromQuery] bool softDelete = true)
    {
        var result = await _service.DeleteAsync(subUnitId, softDelete);
        return result.IsSuccess
            ? Ok(new { Message = "SubUnit deleted successfully" })
            : result.ToProblem();
    }

    /// <summary>
    /// Restore a soft-deleted subunit
    /// </summary>
    [HttpPost("{subUnitId}/restore")]
 
    public async Task<IActionResult> Restore(int subUnitId)
    {
        var result = await _service.RestoreAsync(subUnitId);
        return result.IsSuccess
            ? Ok(new { Message = "SubUnit restored successfully" })
            : result.ToProblem();
    }

    #endregion

    #region Availability Management

    /// <summary>
    /// Set availability for a subunit
    /// </summary>
    [HttpPost("{subUnitId}/availability")]
 
    public async Task<IActionResult> SetAvailability(
        int subUnitId,
        [FromBody] SetAvailabilityRequest request)
    {
        var result = await _service.SetAvailabilityAsync(subUnitId, request);
        return result.IsSuccess
            ? Ok(new { Message = "Availability set successfully" })
            : result.ToProblem();
    }

    /// <summary>
    /// Toggle subunit availability status
    /// </summary>
    [HttpPost("{subUnitId}/toggle-available")]
 
    public async Task<IActionResult> ToggleAvailable(int subUnitId)
    {
        var result = await _service.ToggleAvailableAsync(subUnitId);
        return result.IsSuccess
            ? Ok(new { Message = "Availability toggled successfully" })
            : result.ToProblem();
    }

    /// <summary>
    /// Get availability for a subunit within a date range
    /// </summary>
    [HttpGet("{subUnitId}/availability")]
    public async Task<IActionResult> GetAvailability(
        int subUnitId,
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        var result = await _service.GetAvailabilityAsync(subUnitId, startDate, endDate);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    /// <summary>
    /// Get available subunits for a unit within a date range
    /// </summary>
    [HttpGet("unit/{unitId}/available")]
    public async Task<IActionResult> GetAvailableSubUnits(
        int unitId,
        [FromQuery] DateTime checkIn,
        [FromQuery] DateTime checkOut)
    {
        var result = await _service.GetAvailableSubUnitsAsync(unitId, checkIn, checkOut);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    #endregion

    #region Image Management

    /// <summary>
    /// Upload images for a subunit
    /// </summary>
    [HttpPost("{subUnitId}/images")]
    [RequestSizeLimit(60_000_000)]
    public async Task<IActionResult> UploadImages(
        int subUnitId,
        [FromForm] List<IFormFile> images)
    {
        var userId = User.GetUserId();
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var result = await _service.UploadImagesAsync(subUnitId, images, userId);
        return result.IsSuccess
            ? Ok(new { Message = $"{images.Count} images uploaded successfully", Images = result.Value })
            : result.ToProblem();
    }

    /// <summary>
    /// Delete a specific image
    /// </summary>
    [HttpDelete("{subUnitId}/images/{imageId}")]
 
    public async Task<IActionResult> DeleteImage(int subUnitId, int imageId)
    {
        var result = await _service.DeleteImageAsync(subUnitId, imageId);
        return result.IsSuccess
            ? Ok(new { Message = "Image deleted successfully" })
            : result.ToProblem();
    }

    /// <summary>
    /// Set primary image for a subunit
    /// </summary>
    [HttpPost("{subUnitId}/images/{imageId}/set-primary")]
 
    public async Task<IActionResult> SetPrimaryImage(int subUnitId, int imageId)
    {
        var result = await _service.SetPrimaryImageAsync(subUnitId, imageId);
        return result.IsSuccess
            ? Ok(new { Message = "Primary image set successfully" })
            : result.ToProblem();
    }

    #endregion

    #region Pricing Management

    /// <summary>
    /// Set special pricing for a subunit
    /// </summary>
    [HttpPost("{subUnitId}/pricing")]
 
    public async Task<IActionResult> SetSpecialPricing(
        int subUnitId,
        [FromBody] SetSpecialPricingRequestsss request)
    {
        var result = await _service.SetSpecialPricingAsync(subUnitId, request);
        return result.IsSuccess
            ? Ok(new { Message = "Special pricing set successfully" })
            : result.ToProblem();
    }

    /// <summary>
    /// Calculate price for a subunit for specific dates
    /// </summary>
    [HttpGet("{subUnitId}/calculate-price")]
    public async Task<IActionResult> CalculatePrice(
        int subUnitId,
        [FromQuery] DateTime checkIn,
        [FromQuery] DateTime checkOut)
    {
        var result = await _service.CalculatePriceAsync(subUnitId, checkIn, checkOut);
        return result.IsSuccess ? Ok(new { Price = result.Value }) : result.ToProblem();
    }

    #endregion

    #region Policy Management

    /// <summary>
    /// Attach a general policy to a subunit
    /// </summary>
    [HttpPost("{subUnitId}/general-policy/{policyId}")]
 
    public async Task<IActionResult> AttachGeneralPolicy(int subUnitId, int policyId)
    {
        var result = await _service.AttachGeneralPolicyAsync(subUnitId, policyId);
        return result.IsSuccess
            ? Ok(new { Message = "General policy attached successfully" })
            : result.ToProblem();
    }

    /// <summary>
    /// Remove a general policy from a subunit
    /// </summary>
    [HttpDelete("{subUnitId}/general-policy/{policyId}")]
 
    public async Task<IActionResult> RemoveGeneralPolicy(int subUnitId, int policyId)
    {
        var result = await _service.RemoveGeneralPolicyAsync(subUnitId, policyId);
        return result.IsSuccess
            ? Ok(new { Message = "General policy removed successfully" })
            : result.ToProblem();
    }

    #endregion
}