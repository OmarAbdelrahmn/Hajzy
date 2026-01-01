using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Contracts.Department;

public class Departmentrecords
{
}
public record CreateDepartmentRequest(
    string Name,
    string Country,
    string? Description,
    string? ImageUrl,
    decimal Latitude,
    decimal Longitude,
    bool IsActive = true
);

public record UpdateDepartmentRequest(
    string? Name = null,
    string? Country = null,
    string? Description = null,
    string? ImageUrl = null,
    decimal? Latitude = null,
    decimal? Longitude = null,
    bool? IsActive = null
);

public record DepartmentResponse(
    int Id,
    string Name,
    string Country,
    string? Description,
    string? ImageUrl,
    decimal Latitude,
    decimal Longitude,
    bool IsActive,
    int TotalUnits,
    int TotalBookings,
    decimal? AverageRating,
    bool IsDeleted,
    DateTime CreatedAt,
    DateTime? DeletedAt,
    int AdminCount,
    bool HasActiveAdmin
);

public record DepartmentAdminsResponse(
    int DepartmentId,
    string DepartmentName,
    int TotalAdmins,
    int ActiveAdmins,
    List<DepartmentAdminInfo> Admins
);

public record DepartmentWithAdminsResponse(
    int Id,
    string Name,
    string Country,
    bool IsActive,
    int TotalAdmins,
    int ActiveAdmins,
    List<AdminSummary> Admins
);

public record AdminSummary(
    string UserId,
    string Name,
    string Email,
    bool IsActive,
    bool IsPrimary,
    DateTime AssignedAt
);

public record DepartmentDetailsResponse(
    int Id,
    string Name,
    string Country,
    string? Description,
    string? ImageUrl,
    decimal Latitude,
    decimal Longitude,
    bool IsActive,
    int TotalUnits,
    int TotalBookings,
    decimal? AverageRating,
    DateTime CreatedAt,
    List<DepartmentAdminInfo> Admins,
    DepartmentAdminInfo? PrimaryAdmin,
    List<UnitSummary> Units,
    DepartmentMetrics Metrics
);

public record DepartmentAdminInfo(
    string UserId,
    string FullName,
    string Email,
    string? PhoneNumber,
    bool IsActive,
    bool IsPrimary,
    DateTime AssignedAt
);

public record UnitSummary(
    int Id,
    string Name,
    string UnitType,
    bool IsActive,
    bool IsVerified,
    decimal AverageRating,
    int TotalReviews
);

public record DepartmentMetrics(
    int ActiveUnits,
    int InactiveUnits,
    int VerifiedUnits,
    int UnverifiedUnits,
    int PendingBookings,
    int ConfirmedBookings,
    int CompletedBookings,
    decimal TotalRevenue,
    decimal AverageBookingValue
);

public record DepartmentStatisticsResponse(
    int DepartmentId,
    string DepartmentName,
    int TotalUnits,
    int TotalBookings,
    decimal? AverageRating,
    int TotalReviews,
    decimal TotalRevenue,
    int ActiveUnits,
    int InactiveUnits,
    Dictionary<string, int> UnitsByType,
    Dictionary<string, int> BookingsByStatus,
    Dictionary<string, decimal> MonthlyRevenue
);

public record DepartmentStatisticsSummary(
    int Id,
    string Name,
    string Country,
    int TotalUnits,
    int TotalBookings,
    decimal? AverageRating,
    bool HasActiveAdmin,
    string? AdminName
);