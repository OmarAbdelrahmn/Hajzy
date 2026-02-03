using Application.Abstraction;
using Application.Contracts.Department;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Service.Department;

public interface IDepartmanetService
{
    Task<Result<DepartmentResponse>> GetByIdAsync(int departmentId);
    Task<Result<PaginatedResponse<DepartmentResponse>>> GetAllDepartmentsAsync(int page = 1, int pageSize = 10);
    Task<Result<PaginatedResponse<DepartmentResponse>>> GetDepartmentsByCountryAsync(
        string country, int page = 1, int pageSize = 10);
    
    Task<Result<DepartmentResponse>> CreateAsync(CreateDepartmentRequest request);
    Task<Result<DepartmentResponse>> UpdateAsync(int departmentId, UpdateDepartmentRequest request);
    Task<Result> DeleteAsync(int departmentId, bool softDelete = true);
    Task<Result> RestoreAsync(int departmentId);

    // ============= IMAGE MANAGEMENT =============
    Task<Result<string>> UploadDepartmentImageAsync(int departmentId, IFormFile image, string userId);
    Task<Result> DeleteDepartmentImageAsync(int departmentId);
    Task<Result<string>> GetPresignedImageUrlAsync(string s3Key, int expirationMinutes = 60);
    string GetCloudFrontImageUrl(string s3Key);

    // Admin Management - Updated for multi-admin support
    Task<Result> AttachAdminAsync(int departmentId, string userId, bool setAsPrimary = false);
    Task<Result> SetPrimaryAdminAsync(int departmentId, string userId);
    Task<Result> RemoveAdminAsync(int departmentId, string userId);
    Task<Result> DeactivateAdminAsync(int departmentId, string userId);
    Task<Result> ActivateAdminAsync(int departmentId, string userId);
    Task<Result<DepartmentAdminsResponse>> GetDepartmentAdminsAsync(int departmentId);
    Task<Result<IEnumerable<DepartmentWithAdminsResponse>>> GetDepartmentsWithAdminsAsync();

    // Statistics & Details
    Task<Result<DepartmentDetailsResponse>> GetDepartmentDetailsAsync(int departmentId);
    Task<Result<DepartmentStatisticsResponse>> GetDepartmentStatisticsAsync(int departmentId);
    Task<Result<IEnumerable<DepartmentStatisticsSummary>>> GetAllDepartmentStatisticsAsync();

    // Search & Filter
    Task<Result<IEnumerable<DepartmentResponse>>> FilterAsync(DepartmentFilter filter);
    Task<Result<IEnumerable<DepartmentResponse>>> SearchAsync(string keyword);

    // Validation
    Task<Result<bool>> HasActiveAdminAsync(int departmentId);
    Task<Result<bool>> CanAssignAdminAsync(string userId);
}

public class PaginatedResponse<T>
{
    public IEnumerable<T> Items { get; set; } = [];
    public int TotalPages { get; set; }
    public int CurrentPage { get; set; }
    public int? NextPage { get; set; }
    public int? PrevPage { get; set; }
    public int TotalCount { get; set; }


    private PaginatedResponse<T> CreatePaginatedResponse<T>(
        IEnumerable<T> items,
        int totalCount,
        int page,
        int pageSize)
    {
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        return new PaginatedResponse<T>
        {
            Items = items,
            TotalCount = totalCount,
            TotalPages = totalPages,
            CurrentPage = page,
            NextPage = page < totalPages ? page + 1 : null,
            PrevPage = page > 1 ? page - 1 : null
        };
    } 

}