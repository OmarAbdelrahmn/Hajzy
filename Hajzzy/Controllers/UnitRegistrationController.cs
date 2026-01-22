using Application.Contracts.UnitRegisteration;
using Application.Extensions;
using Application.Service.UnitRegistration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Hajzzy.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UnitRegistrationController(IUnitRegistrationService service) : ControllerBase
{
    private readonly IUnitRegistrationService _service = service;

    // ============= PUBLIC ENDPOINTS (Anonymous) =============

    /// <summary>
    /// Submit a new unit registration request (Anonymous)
    /// </summary>
    [HttpPost("submit")]
    [AllowAnonymous]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> SubmitRegistration(
        [FromForm] SubmitUnitRegistrationRequest request)
    {
        var result = await _service.SubmitRegistrationAsync(request);

        if (!result.IsSuccess)
            return result.ToProblem();

        return Ok(new
        {
            Message = "Registration request submitted successfully. " +
                      "You will receive an email once it's reviewed.",
            RequestId = result.Value
        });
    }

    /// <summary>
    /// Check if an email is available (Anonymous)
    /// </summary>
    [HttpGet("check-email")]
    [AllowAnonymous]
    public async Task<IActionResult> CheckEmailAvailability([FromQuery] string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return BadRequest(new { Message = "Email is required" });

        var result = await _service.IsEmailAvailableAsync(email);

        return result.IsSuccess
            ? Ok(new { Available = result.Value })
            : result.ToProblem();
    }

    // ============= ADMIN ENDPOINTS =============

    /// <summary>
    /// Get all registration requests with filtering (Admin only)
    /// </summary>
    [HttpPost("admin/list")]
 
    public async Task<IActionResult> GetAllRequests(
        [FromBody] UnitRegistrationListFilter filter)
    {
        var result = await _service.GetAllRequestsAsync(filter);

        return result.IsSuccess
            ? Ok(result.Value)
            : result.ToProblem();
    }

    //[HttpGet("department-admin/list")]
    //public async Task<IActionResult> GetAllRequests(
    //    string userId)
    //{
    //    var result = await _service.GetAllRequestsAsync(userId);

    //    return result.IsSuccess
    //        ? Ok(result.Value)
    //        : result.ToProblem();
    //}

    /// <summary>
    /// Get a specific registration request (Admin only)
    /// </summary>
    [HttpGet("admin/{requestId}")]
 
    public async Task<IActionResult> GetRequestById(int requestId)
    {
        var result = await _service.GetRequestByIdAsync(requestId);

        return result.IsSuccess
            ? Ok(result.Value)
            : result.ToProblem();
    }

    /// <summary>
    /// Approve a registration request (Admin only)
    /// Creates user account and unit
    /// </summary>
    [HttpPost("admin/{requestId}/approve")]
 
    public async Task<IActionResult> ApproveRequest(int requestId)
    {
        var adminUserId = User.GetUserId();
        if (string.IsNullOrEmpty(adminUserId))
            return Unauthorized();

        var result = await _service.ApproveRequestAsync(requestId, adminUserId);

        if (!result.IsSuccess)
            return result.ToProblem();

        return Ok(new
        {
            Message = "Registration request approved successfully",
            Data = result.Value
        });
    }

    /// <summary>
    /// Reject a registration request (Admin only)
    /// </summary>
    [HttpPost("admin/{requestId}/reject")]
 
    public async Task<IActionResult> RejectRequest(
        int requestId,
        [FromBody] RejectRequestDto dto)
    {
        var adminUserId = User.GetUserId();
        if (string.IsNullOrEmpty(adminUserId))
            return Unauthorized();

        if (string.IsNullOrWhiteSpace(dto.RejectionReason))
            return BadRequest(new { Message = "Rejection reason is required" });

        var result = await _service.RejectRequestAsync(
            requestId,
            adminUserId,
            dto.RejectionReason);

        return result.IsSuccess
            ? Ok(new { Message = "Registration request rejected successfully" })
            : result.ToProblem();
    }

    /// <summary>
    /// Delete a registration request (Admin only)
    /// </summary>
    [HttpDelete("admin/{requestId}")]
 
    public async Task<IActionResult> DeleteRequest(int requestId)
    {
        var result = await _service.DeleteRequestAsync(requestId);

        return result.IsSuccess
            ? Ok(new { Message = "Registration request deleted successfully" })
            : result.ToProblem();
    }

    /// <summary>
    /// Get registration statistics (Admin only)
    /// </summary>
    [HttpGet("admin/statistics")]
 
    public async Task<IActionResult> GetStatistics()
    {
        var result = await _service.GetStatisticsAsync();

        return result.IsSuccess
            ? Ok(result.Value)
            : result.ToProblem();
    }

    /// <summary>
    /// Resend credentials email to approved user (Admin only)
    /// </summary>
    [HttpPost("admin/{requestId}/resend-credentials")]
 
    public async Task<IActionResult> ResendCredentials(int requestId)
    {
        var result = await _service.ResendCredentialsEmailAsync(requestId);

        return result.IsSuccess
            ? Ok(new { Message = "Credentials email sent successfully" })
            : result.ToProblem();
    }


    // ============= City ADMIN ENDPOINTS =============
    /// <summary>
    /// Get all registration requests with filtering (Admin only)
    /// </summary>
    [HttpPost("department-admin/list")]

    public async Task<IActionResult> DepartmentAdminGetAllRequests(
        [FromBody] DAUnitRegistrationListFilter filter , CancellationToken ct = default)
    {
        var userId = User.GetUserId()!;
        var result = await _service.DepartmentAdmin_GetAllRequestsAsync(filter, userId,ct);

        return result.IsSuccess
            ? Ok(result.Value)
            : result.ToProblem();
    }

    [HttpGet("department-admin/list")]
    public async Task<IActionResult> DepartmentAdminGetAllRequests(CancellationToken ct = default)
    {
        var userId = User.GetUserId()!;
        var result = await _service.DepartmentAdmin_GetAllRequestsAsync(userId,ct);

        return result.IsSuccess
            ? Ok(result.Value)
            : result.ToProblem();
    }

    /// <summary>
    /// Get a specific registration request (Admin only)
    /// </summary>
    [HttpGet("department-admin/{requestId}")]

    public async Task<IActionResult> DepartmentAdminGetRequestById(int requestId, CancellationToken ct = default)
    {
        var userId = User.GetUserId()!;
        var result = await _service.DepartmentAdmin_GetRequestByIdAsync(requestId, userId,ct);

        return result.IsSuccess
            ? Ok(result.Value)
            : result.ToProblem();
    }

    /// <summary>
    /// Approve a registration request (Admin only)
    /// Creates user account and unit
    /// </summary>
    [HttpPost("department-admin/{requestId}/approve")]

    public async Task<IActionResult> DepartmentAdminApproveRequest(int requestId, CancellationToken ct = default)
    {
        var userId = User.GetUserId()!;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var result = await _service.DepartmentAdmin_ApproveRequestAsync(requestId, userId,ct);

        if (!result.IsSuccess)
            return result.ToProblem();

        return Ok(new
        {
            Message = "Registration request approved successfully",
            Data = result.Value
        });
    }

    /// <summary>
    /// Reject a registration request (Admin only)
    /// </summary>
    [HttpPost("department-admin/{requestId}/reject")]

    public async Task<IActionResult> DepartmentAdminRejectRequest(
        int requestId,
        [FromBody] RejectRequestDto dto , CancellationToken ct = default)
    {
        var userId = User.GetUserId()!;


        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        if (string.IsNullOrWhiteSpace(dto.RejectionReason))
            return BadRequest(new { Message = "Rejection reason is required" });

        var result = await _service.DepartmentAdmin_RejectRequestAsync(
            requestId,
            userId,
            dto.RejectionReason,ct);

        return result.IsSuccess
            ? Ok(new { Message = "Registration request rejected successfully" })
            : result.ToProblem();
    }

    /// <summary>
    /// Delete a registration request (Admin only)
    /// </summary>
    [HttpDelete("department-admin/{requestId}")]

    public async Task<IActionResult> DepartmentAdminDeleteRequest(int requestId, CancellationToken ct = default)
    {
        var userId = User.GetUserId()!;

        var result = await _service.DepartmentAdmin_DeleteRequestAsync(requestId , userId, ct);

        return result.IsSuccess
            ? Ok(new { Message = "Registration request deleted successfully" })
            : result.ToProblem();
    }

    /// <summary>
    /// Get registration statistics (Admin only)
    /// </summary>
    [HttpGet("department-admin/statistics")]

    public async Task<IActionResult> DepartmentAdminGetStatistics(CancellationToken ct = default)
    {
        var userId = User.GetUserId()!;

        var result = await _service.DepartmentAdmin_GetStatisticsAsync(userId,ct);

        return result.IsSuccess
            ? Ok(result.Value)
            : result.ToProblem();
    }



}



// DTO for rejection
public record RejectRequestDto
{
    public string RejectionReason { get; init; } = string.Empty;
}
