using Application.Abstraction;
using Application.Contracts.UnitRegisteration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Service.UnitRegistration;

public interface IUnitRegistrationService
{
    /// <summary>
    /// Allows anonymous users to submit a hotel registration request
    /// </summary>
    Task<Result<int>> SubmitRegistrationAsync(SubmitUnitRegistrationRequest request);

    /// <summary>
    /// Check if email is already in use
    /// </summary>
    Task<Result<bool>> IsEmailAvailableAsync(string email);

    // ============= ADMIN OPERATIONS =============

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


    // ============= City ADMIN OPERATIONS =============


    /// <summary>
    /// Get all registration requests with filtering for city Admin
    /// </summary>
    Task<Result<IEnumerable<DAUnitRegistrationResponse>>> DepartmentAdmin_GetAllRequestsAsync(
        DAUnitRegistrationListFilter filter ,string UserId , CancellationToken ct);


    /// <summary>
    /// Get a specific request by ID
    /// </summary>
    Task<Result<DAUnitRegistrationResponse>> DepartmentAdmin_GetRequestByIdAsync(int requestId , string userId, CancellationToken ct);

    /// <summary>
    /// Approve a registration request (creates user + unit)
    /// </summary>
    Task<Result<DAapprovalResult>> DepartmentAdmin_ApproveRequestAsync(
        int requestId,
        string UserId, CancellationToken ct);

    /// <summary>
    /// Reject a registration request
    /// </summary>
    Task<Result> DepartmentAdmin_RejectRequestAsync(
        int requestId,
        string UserId,
        string rejectionReason, CancellationToken ct);

    /// <summary>
    /// Delete/cancel a registration request
    /// </summary>
    Task<Result> DepartmentAdmin_DeleteRequestAsync(int requestId, string userId, CancellationToken ct);

    /// <summary>
    /// Get statistics about registration requests
    /// </summary>
    Task<Result<DAUnitRegistrationStatistics>> DepartmentAdmin_GetStatisticsAsync(string userId, CancellationToken ct);

    
}
