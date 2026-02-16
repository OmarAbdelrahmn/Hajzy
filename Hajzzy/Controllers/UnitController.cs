using Application.Contracts.Unit;
using Application.Extensions;
using Application.Service.Unit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hajzzy.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UnitController(IUnitService service) : ControllerBase
{
    private readonly IUnitService _service = service;

    #region CRUD Operations

    /// <summary>
    /// Get unit by ID
    /// </summary>
    [HttpGet("{unitId}")]
    public async Task<IActionResult> GetById(int unitId)
    {
        var result = await _service.GetByIdAsync(unitId);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    /// <summary>
    /// Get unit details by ID (includes rooms, amenities, reviews, bookings)
    /// </summary>
    [HttpGet("{unitId}/details")]
    public async Task<IActionResult> GetDetails(int unitId)
    {
        var result = await _service.GetDetailsAsync(unitId);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    /// <summary>
    /// Get all units with filtering and pagination
    /// </summary>
    [HttpPost("filter")]
    public async Task<IActionResult> GetAll([FromBody] UnitFilter filter)
    {
        var result = await _service.GetAllComprehensiveAsync(filter);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost("filter-with-pagenation")]
    public async Task<IActionResult> GetAll2([FromBody] UnitFilter filter)
    {
        var result = await _service.FilterUnitsAsync(filter);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost("departmint/filter")]
    public async Task<IActionResult> GetAlld([FromBody] UnitFilter filter)
    {
        var UserId = User.GetUserId();

        var result = await _service.GetAllByUserDepartmentAsync(UserId, filter);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }
    [HttpPost("hotel/filter")]
    public async Task<IActionResult> GetAllh([FromBody] UnitFilter filter)
    {

        var UserId = User.GetUserId();
        var result = await _service.GetAllByHotelAdminAsync(UserId, filter);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    /// <summary>
    /// Update an existing unit
    /// </summary>
    [HttpPut("{unitId}")]

    public async Task<IActionResult> Update(int unitId, [FromBody] UpdateUnitRequest request)
    {
        var result = await _service.UpdateAsync(unitId, request);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    /// <summary>
    /// Delete a unit (soft delete by default)
    /// </summary>
    [HttpDelete("{unitId}")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> Delete(int unitId, [FromQuery] bool softDelete = true)
    {
        var result = await _service.DeleteAsync(unitId, softDelete);
        return result.IsSuccess
            ? Ok(new { Message = "Unit deleted successfully" })
            : result.ToProblem();
    }

    /// <summary>
    /// Restore a soft-deleted unit
    /// </summary>
    [HttpPost("{unitId}/restore")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> Restore(int unitId)
    {
        var result = await _service.RestoreAsync(unitId);
        return result.IsSuccess
            ? Ok(new { Message = "Unit restored successfully" })
            : result.ToProblem();
    }

    #endregion

    #region Status Management

    /// <summary>
    /// Toggle unit active status
    /// </summary>
    [HttpPost("{unitId}/toggle-active")]

    public async Task<IActionResult> ToggleActive(int unitId)
    {
        var result = await _service.ToggleActiveAsync(unitId);
        return result.IsSuccess
            ? Ok(new { Message = "Unit status toggled successfully" })
            : result.ToProblem();
    }

    /// <summary>
    /// Verify a unit (Admin only)
    /// </summary>
    [HttpPost("{unitId}/verify")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> Verify(int unitId)
    {
        var adminUserId = User.GetUserId();
        if (string.IsNullOrEmpty(adminUserId))
            return Unauthorized();

        var result = await _service.VerifyUnitAsync(unitId, adminUserId);
        return result.IsSuccess
            ? Ok(new { Message = "Unit verified successfully" })
            : result.ToProblem();
    }

    /// <summary>
    /// Unverify a unit (Admin only)
    /// </summary>
    [HttpPost("{unitId}/unverify")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> Unverify(int unitId, [FromBody] UnverifyUnitRequest request)
    {
        var adminUserId = User.GetUserId();
        if (string.IsNullOrEmpty(adminUserId))
            return Unauthorized();

        var result = await _service.UnverifyUnitAsync(unitId, adminUserId, request.Reason);
        return result.IsSuccess
            ? Ok(new { Message = "Unit unverified successfully" })
            : result.ToProblem();
    }

    #endregion

    #region Image Management

    /// <summary>
    /// Upload images for a unit
    /// </summary>
    [HttpPost("{unitId}/images")]
    [RequestSizeLimit(60_000_000)]
    public async Task<IActionResult> UploadImages(
        int unitId,
        [FromForm] List<IFormFile> images
        )
    {
        var userId = User.GetUserId();
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var result = await _service.UploadImagesAsync(unitId, images, userId);
        return result.IsSuccess
            ? Ok(new { Message = $"{images.Count} images uploaded successfully", Images = result.Value })
            : result.ToProblem();
    }

    /// <summary>
    /// Delete a specific image
    /// </summary>
    [HttpDelete("{unitId}/images/{imageId}")]

    public async Task<IActionResult> DeleteImage(int unitId, int imageId)
    {
        var result = await _service.DeleteImageAsync(unitId, imageId);
        return result.IsSuccess
            ? Ok(new { Message = "Image deleted successfully" })
            : result.ToProblem();
    }

    /// <summary>
    /// Set primary image for a unit
    /// </summary>
    [HttpPost("{unitId}/images/{imageId}/set-primary")]

    public async Task<IActionResult> SetPrimaryImage(int unitId, int imageId)
    {
        var result = await _service.SetPrimaryImageAsync(unitId, imageId);
        return result.IsSuccess
            ? Ok(new { Message = "Primary image set successfully" })
            : result.ToProblem();
    }

    /// <summary>
    /// Reorder unit images
    /// </summary>
    [HttpPut("{unitId}/images/reorder")]

    public async Task<IActionResult> ReorderImages(int unitId, [FromBody] ReorderImagesRequests request)
    {
        var result = await _service.ReorderImagesAsync(unitId, request.ImageIds);
        return result.IsSuccess
            ? Ok(new { Message = "Images reordered successfully" })
            : result.ToProblem();
    }

    #endregion

    #region Admin Management

    /// <summary>
    /// Attach an admin to a unit
    /// </summary>
    [HttpPost("{unitId}/admins/{userId}")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> AttachAdmin(int unitId, string userId)
    {
        var result = await _service.AttachAdminAsync(unitId, userId);
        return result.IsSuccess
            ? Ok(new { Message = "Admin attached successfully" })
            : result.ToProblem();
    }

    /// <summary>
    /// Remove an admin from a unit
    /// </summary>
    [HttpDelete("{unitId}/admins/{userId}")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> RemoveAdmin(int unitId, string userId)
    {
        var result = await _service.RemoveAdminAsync(unitId, userId);
        return result.IsSuccess
            ? Ok(new { Message = "Admin removed successfully" })
            : result.ToProblem();
    }

    /// <summary>
    /// Get all admins for a unit
    /// </summary>
    [HttpGet("{unitId}/admins")]

    public async Task<IActionResult> GetUnitAdmins(int unitId)
    {
        var result = await _service.GetUnitAdminsAsync(unitId);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    /// <summary>
    /// Get all units managed by a specific admin
    /// </summary>
    [HttpGet("admin/{userId}/units")]

    public async Task<IActionResult> GetAdminUnits(string userId)
    {
        var result = await _service.GetAdminUnitsAsync(userId);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    #endregion

    #region Policy Management

    /// <summary>
    /// Attach a cancellation policy to a unit
    /// </summary>
    [HttpPost("{unitId}/cancellation-policy/{policyId}")]

    public async Task<IActionResult> AttachCancellationPolicy(int unitId, int policyId)
    {
        var result = await _service.AttachCancellationPolicyAsync(unitId, policyId);
        return result.IsSuccess
            ? Ok(new { Message = "Cancellation policy attached successfully" })
            : result.ToProblem();
    }

    /// <summary>
    /// Remove cancellation policy from a unit
    /// </summary>
    [HttpDelete("{unitId}/cancellation-policy")]

    public async Task<IActionResult> RemoveCancellationPolicy(int unitId)
    {
        var result = await _service.RemoveCancellationPolicyAsync(unitId);
        return result.IsSuccess
            ? Ok(new { Message = "Cancellation policy removed successfully" })
            : result.ToProblem();
    }

    /// <summary>
    /// Attach a general policy to a unit
    /// </summary>
    [HttpPost("{unitId}/general-policy/{policyId}")]

    public async Task<IActionResult> AttachGeneralPolicy(int unitId, int policyId)
    {
        var result = await _service.AttachGeneralPolicyAsync(unitId, policyId);
        return result.IsSuccess
            ? Ok(new { Message = "General policy attached successfully" })
            : result.ToProblem();
    }

    /// <summary>
    /// Remove a general policy from a unit
    /// </summary>
    [HttpDelete("{unitId}/general-policy/{policyId}")]

    public async Task<IActionResult> RemoveGeneralPolicy(int unitId, int policyId)
    {
        var result = await _service.RemoveGeneralPolicyAsync(unitId, policyId);
        return result.IsSuccess
            ? Ok(new { Message = "General policy removed successfully" })
            : result.ToProblem();
    }

    /// <summary>
    /// Get all policies for a unit
    /// </summary>
    [HttpGet("{unitId}/policies")]
    public async Task<IActionResult> GetUnitPolicies(int unitId)
    {
        var result = await _service.GetUnitPoliciesAsync(unitId);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    #endregion

    #region Statistics & Search

    /// <summary>
    /// Get unit statistics
    /// </summary>
    [HttpGet("{unitId}/statistics")]

    public async Task<IActionResult> GetStatistics(int unitId)
    {
        var result = await _service.GetStatisticsAsync(unitId);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    /// <summary>
    /// Search units by keyword
    /// </summary>
    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string keyword)
    {
        if (string.IsNullOrWhiteSpace(keyword))
            return BadRequest(new { Message = "Keyword is required" });

        var result = await _service.SearchAsync(keyword);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    /// <summary>
    /// Get units by department/city
    /// </summary>
    [HttpGet("department/{departmentId}")]
    public async Task<IActionResult> GetByDepartment(int departmentId)
    {
        var result = await _service.GetByDepartmentAsync(departmentId);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    /// <summary>
    /// Get units by type
    /// </summary>
    [HttpGet("type/{typeId}")]
    public async Task<IActionResult> GetByType(int typeId)
    {
        var result = await _service.GetByTypeAsync(typeId);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    /// <summary>
    /// Get nearby units based on location
    /// </summary>
    [HttpGet("nearby")]
    public async Task<IActionResult> GetNearby(
        [FromQuery] decimal latitude,
        [FromQuery] decimal longitude,
        [FromQuery] int radiusKm = 10)
    {
        var result = await _service.GetNearbyAsync(latitude, longitude, radiusKm);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    #endregion

    #region Validation

    /// <summary>
    /// Check if user can manage a unit
    /// </summary>
    [HttpGet("{unitId}/can-manage")]
    [Authorize]
    public async Task<IActionResult> CanUserManage(int unitId)
    {
        var userId = User.GetUserId();
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var result = await _service.CanUserManageUnitAsync(userId, unitId);
        return result.IsSuccess ? Ok(new { CanManage = result.Value }) : result.ToProblem();
    }

    /// <summary>
    /// Check if unit is available for specific dates
    /// </summary>
    [HttpGet("{unitId}/availability")]
    public async Task<IActionResult> CheckAvailability(
        int unitId,
        [FromQuery] DateTime checkIn,
        [FromQuery] DateTime checkOut)
    {
        var result = await _service.IsUnitAvailableAsync(unitId, checkIn, checkOut);
        return result.IsSuccess ? Ok(new { IsAvailable = result.Value }) : result.ToProblem();
    }

    #endregion
    [HttpPost("{unitId}/toggle-featured")]
    [Authorize(Roles = "SuperAdmin,CityAdmin")]
    public async Task<IActionResult> ToggleFeatured(int unitId)
    {
        var result = await _service.ToggleFeaturedAsync(unitId);
        return result.IsSuccess
            ? Ok(new { message = "Featured status toggled successfully" })
            : result.ToProblem();
    }

    /// <summary>
    /// Set unit featured status (Admin only)
    /// </summary>
    [HttpPut("{unitId}/featured")]
    [Authorize(Roles = "SuperAdmin,CityAdmin")]
    public async Task<IActionResult> SetFeatured(int unitId, [FromBody] SetFeaturedRequest request)
    {
        var result = await _service.SetFeaturedAsync(unitId, request.IsFeatured);
        return result.IsSuccess
            ? Ok(new { message = $"Unit featured status set to {request.IsFeatured}" })
            : result.ToProblem();
    }

    /// <summary>
    /// Get all featured units (Public endpoint)
    /// </summary>
    [HttpGet("featured")]
    [AllowAnonymous]
    public async Task<IActionResult> GetFeaturedUnits([FromQuery] UnitFilter filter)
    {
        var result = await _service.GetFeaturedUnitsAsync(filter);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }
}

// ============= REQUEST DTO =============
public record SetFeaturedRequest(bool IsFeatured);
// ============= REQUEST MODELS =============

public record UnverifyUnitRequest
{
    public string Reason { get; init; } = string.Empty;
}

public record ReorderImagesRequests
{
    public List<int> ImageIds { get; init; } = new();
}