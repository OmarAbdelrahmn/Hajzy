using Application.Contracts.Department;
using Application.Extensions;
using Application.Service.Department;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Hajzzy.Controllers;

[Route("api/[controller]")]
[ApiController]
public class DepartmentController(IDepartmanetService service) : ControllerBase
{
    private readonly IDepartmanetService service = service;


    #region CRUD Operations

    [HttpGet("{departmentId}")]
     
    public async Task<IActionResult> GetById(int departmentId)
    {
        var response = await service.GetByIdAsync(departmentId);
        return response.IsSuccess ?
            Ok(response.Value) :
            response.ToProblem();
    }

    [HttpGet("")]
     
    public async Task<IActionResult> GetAll([FromQuery] bool includeInactive = false)
    {
        var response = await service.GetAllAsync(includeInactive);
        return response.IsSuccess ?
            Ok(response.Value) :
            response.ToProblem();
    }

    [HttpPost("")]
     
    public async Task<IActionResult> Create([FromBody] CreateDepartmentRequest request)
    {
        var response = await service.CreateAsync(request);
        return response.IsSuccess ?
            Ok(response.Value) :
            response.ToProblem();
    }

    [HttpPut("{departmentId}")]
     
    public async Task<IActionResult> Update(int departmentId, [FromBody] UpdateDepartmentRequest request)
    {
        var response = await service.UpdateAsync(departmentId, request);
        return response.IsSuccess ?
            Ok(response.Value) :
            response.ToProblem();
    }

    [HttpDelete("{departmentId}")]
     
    public async Task<IActionResult> Delete(int departmentId, [FromQuery] bool softDelete = true)
    {
        var response = await service.DeleteAsync(departmentId, softDelete);
        return response.IsSuccess ?
            Ok(new { Message = "Department deleted successfully." }) :
            response.ToProblem();
    }

    [HttpPost("{departmentId}/restore")]
     
    public async Task<IActionResult> Restore(int departmentId)
    {
        var response = await service.RestoreAsync(departmentId);
        return response.IsSuccess ?
            Ok(new { Message = "Department restored successfully." }) :
            response.ToProblem();
    }

    #endregion

    #region Admin Management

    [HttpPost("{departmentId}/admins/{userId}")]
     
    public async Task<IActionResult> AttachAdmin(int departmentId, string userId, [FromQuery] bool setAsPrimary = false)
    {
        var response = await service.AttachAdminAsync(departmentId, userId, setAsPrimary);
        return response.IsSuccess ?
            Ok(new { Message = "Admin attached successfully." }) :
            response.ToProblem();
    }

    [HttpPut("{departmentId}/admins/{userId}/set-primary")]
     
    public async Task<IActionResult> SetPrimaryAdmin(int departmentId, string userId)
    {
        var response = await service.SetPrimaryAdminAsync(departmentId, userId);
        return response.IsSuccess ?
            Ok(new { Message = "Primary admin updated successfully." }) :
            response.ToProblem();
    }

    [HttpDelete("{departmentId}/admins/{userId}")]
     
    public async Task<IActionResult> RemoveAdmin(int departmentId, string userId)
    {
        var response = await service.RemoveAdminAsync(departmentId, userId);
        return response.IsSuccess ?
            Ok(new { Message = "Admin removed successfully." }) :
            response.ToProblem();
    }

    [HttpPut("{departmentId}/admins/{userId}/deactivate")]
     
    public async Task<IActionResult> DeactivateAdmin(int departmentId, string userId)
    {
        var response = await service.DeactivateAdminAsync(departmentId, userId);
        return response.IsSuccess ?
            Ok(new { Message = "Admin deactivated successfully." }) :
            response.ToProblem();
    }

    [HttpPut("{departmentId}/admins/{userId}/activate")]
     
    public async Task<IActionResult> ActivateAdmin(int departmentId, string userId)
    {
        var response = await service.ActivateAdminAsync(departmentId, userId);
        return response.IsSuccess ?
            Ok(new { Message = "Admin activated successfully." }) :
            response.ToProblem();
    }

    [HttpGet("{departmentId}/admins")]
     
    public async Task<IActionResult> GetDepartmentAdmins(int departmentId)
    {
        var response = await service.GetDepartmentAdminsAsync(departmentId);
        return response.IsSuccess ?
            Ok(response.Value) :
            response.ToProblem();
    }

    [HttpGet("with-admins")]
     
    public async Task<IActionResult> GetDepartmentsWithAdmins()
    {
        var response = await service.GetDepartmentsWithAdminsAsync();
        return response.IsSuccess ?
            Ok(response.Value) :
            response.ToProblem();
    }

    #endregion

    #region Statistics & Details

    [HttpGet("{departmentId}/details")]
     
    public async Task<IActionResult> GetDepartmentDetails(int departmentId)
    {
        var response = await service.GetDepartmentDetailsAsync(departmentId);
        return response.IsSuccess ?
            Ok(response.Value) :
            response.ToProblem();
    }

    [HttpGet("{departmentId}/statistics")]
     
    public async Task<IActionResult> GetDepartmentStatistics(int departmentId)
    {
        var response = await service.GetDepartmentStatisticsAsync(departmentId);
        return response.IsSuccess ?
            Ok(response.Value) :
            response.ToProblem();
    }

    [HttpGet("statistics")]
     
    public async Task<IActionResult> GetAllDepartmentStatistics()
    {
        var response = await service.GetAllDepartmentStatisticsAsync();
        return response.IsSuccess ?
            Ok(response.Value) :
            response.ToProblem();
    }

    #endregion

    #region Search & Filter

    [HttpPost("filter")]
     
    public async Task<IActionResult> Filter([FromBody] DepartmentFilter filter)
    {
        var response = await service.FilterAsync(filter);
        return response.IsSuccess ?
            Ok(response.Value) :
            response.ToProblem();
    }

    [HttpGet("search")]
     
    public async Task<IActionResult> Search([FromQuery] string keyword)
    {
        if (string.IsNullOrWhiteSpace(keyword))
            return BadRequest(new { Message = "Keyword is required." });

        var response = await service.SearchAsync(keyword);
        return response.IsSuccess ?
            Ok(response.Value) :
            response.ToProblem();
    }

    [HttpGet("by-country/{country}")]
     
    public async Task<IActionResult> GetByCountry(string country)
    {
        var response = await service.GetByCountryAsync(country);
        return response.IsSuccess ?
            Ok(response.Value) :
            response.ToProblem();
    }

    #endregion

    #region Validation

    [HttpGet("{departmentId}/has-active-admin")]
     
    public async Task<IActionResult> HasActiveAdmin(int departmentId)
    {
        var response = await service.HasActiveAdminAsync(departmentId);
        return response.IsSuccess ?
            Ok(response.Value) :
            response.ToProblem();
    }

    [HttpGet("can-assign-admin/{userId}")]
     
    public async Task<IActionResult> CanAssignAdmin(string userId)
    {
        var response = await service.CanAssignAdminAsync(userId);
        return response.IsSuccess ?
            Ok(response.Value) :
            response.ToProblem();
    }

    #endregion


    #region Image Management

    /// <summary>
    /// Upload image for a department
    /// </summary>
    [HttpPost("{departmentId}/image")]
    [RequestSizeLimit(10_000_000)] // 10MB
    public async Task<IActionResult> UploadImage(
        [FromRoute]int departmentId,
        [FromForm] UploadImageDto dto)
    {
        var userId = User.GetUserId();
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var result = await service.UploadDepartmentImageAsync(departmentId, dto.Image, userId);

        return result.IsSuccess
            ? Ok(new { Message = "Department image uploaded successfully", ImageUrl = result.Value })
            : result.ToProblem();
    }

    /// <summary>
    /// Delete department image
    /// </summary>
    [HttpDelete("{departmentId}/image")]
    public async Task<IActionResult> DeleteImage(int departmentId)
    {
        var result = await service.DeleteDepartmentImageAsync(departmentId);

        return result.IsSuccess
            ? Ok(new { Message = "Department image deleted successfully" })
            : result.ToProblem();
    }

    /// <summary>
    /// Get presigned URL for department image
    /// </summary>
    [HttpGet("{departmentId}/image/presigned-url")]
    public async Task<IActionResult> GetPresignedUrl(
        int departmentId,
        [FromQuery] string s3Key,
        [FromQuery] int expirationMinutes = 60)
    {
        var result = await service.GetPresignedImageUrlAsync(s3Key, expirationMinutes);

        return result.IsSuccess
            ? Ok(new { Url = result.Value, ExpiresIn = expirationMinutes })
            : result.ToProblem();
    }

    #endregion
}

public class UploadImageDto
{
    public IFormFile Image { get; set; }
}