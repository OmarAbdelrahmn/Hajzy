using Application.Abstraction;
using Application.Contracts.UnitRegisteration;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using static Application.Service.UnitRegistration.UnitRegistrationService;

namespace Application.Service.UnitRegistration;

public interface IUnitRegistrationService
{
    /// <summary>
    /// Allows anonymous users to submit a hotel registration request
    /// </summary>
    //Task<Result<int>> SubmitRegistrationAsync(SubmitUnitRegistrationRequest request);

    Task<Result<int>> SubmitRegistrationAsync(SubmitUnitRegistrationRequest request);

    /// <summary>
    /// Upload and process images for existing registration - Can take time
    /// </summary>
    Task<Result<ImageUploadResult>> UploadRegistrationImagesAsync(
        int requestId,
        List<IFormFile> images);

    /// <summary>
    /// Check if email is already in use
    /// </summary>
    Task<Result<bool>> IsEmailAvailableAsync(string email);

    // ============= ADMIN OPERATIONS =============
    Task<Result<ImageProcessingStatusDto>> GetProcessingStatusAsync(int requestId);

    /// <summary>
    /// Get all registration requests with filtering
    /// </summary>
    Task<Result<IEnumerable<UnitRegistrationResponse>>> GetAllRequestsAsync(
        UnitRegistrationListFilter filter);
    Task<Result<IEnumerable<UnitRegistrationResponse>>> GetAllRequestsAsync(
        string userId
         );

    /// <summary>
    /// Get a specific request by ID
    /// </summary>
    Task<Result<UnitRegistrationResponse>> GetRequestByIdAsync(int requestId);

    /// <summary>
    /// Approve a registration request (creates user + unit)
    /// </summary>
    Task<Result<ApprovalResult>> ApproveRequestAsync(
        int requestId,
        string adminUserId);

    /// <summary>
    /// Reject a registration request
    /// </summary>
    Task<Result> RejectRequestAsync(
        int requestId,
        string adminUserId,
        string rejectionReason);

    /// <summary>
    /// Delete/cancel a registration request
    /// </summary>
    Task<Result> DeleteRequestAsync(int requestId);

    /// <summary>
    /// Get statistics about registration requests
    /// </summary>
    Task<Result<UnitRegistrationStatistics>> GetStatisticsAsync();

    /// <summary>
    /// Resend credentials email to approved user
    /// </summary>
    Task<Result> ResendCredentialsEmailAsync(int requestId);
}
