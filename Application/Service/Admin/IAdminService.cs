using Application.Abstraction;
using Application.Contracts.AD;
using Application.Contracts.Admin;
using Application.Contracts.Bookin;
using Application.Contracts.Dashboard;


namespace Application.Service.Admin;

public interface IAdminService
{
    Task<Result<PaginatedResponse<UserResponse>>> GetAllUsers(int page = 1, int pageSize = 10);
    Task<Result<UserResponse>> GetUserAsync(string Id);
    Task<Result<UserResponse>> GetUser2Async(string UserName);
    Task<Result<UserResponse>> AddUserAsync(CreateUserRequest request);
    Task<Result> UpdateUserAsync(string UserId, UpdateUserRequest request);
    Task<Result> ToggleStatusAsync(string UserId);
    Task<Result> EndLockOutAsync(string UserId);
    Task<Result> DeletaUserAsync(string UserId);
    Task<Result<PaginatedBookingsResponse>> GetAllBookingsAsync(BookingAdminFilter filter);

    // <summary>
    /// Get comprehensive dashboard data for super admin across all departments
    /// </summary>
    Task<Result<SuperAdminDashboardResponse>> GetDashboardAsync(string userId);

    /// <summary>
    /// Get quick statistics across entire platform
    /// </summary>
    Task<Result<SuperAdminStatsResponse>> GetQuickStatsAsync(string userId);

    /// <summary>
    /// Get platform-wide revenue overview
    /// </summary>
    Task<Result<PlatformRevenueOverviewResponse>> GetPlatformRevenueOverviewAsync(
        string userId,
        DateTime? startDate = null,
        DateTime? endDate = null);

    /// <summary>
    /// Get department performance comparison
    /// </summary>
    Task<Result<IEnumerable<DepartmentPerformanceResponse>>> GetDepartmentPerformanceAsync(
        string userId,
        int? topN = null);

    /// <summary>
    /// Get platform growth metrics
    /// </summary>
    Task<Result<PlatformGrowthMetricsResponse>> GetPlatformGrowthMetricsAsync(
        string userId,
        int months = 12);

    /// <summary>
    /// Get user engagement statistics
    /// </summary>
    Task<Result<UserEngagementStatsResponse>> GetUserEngagementStatsAsync(string userId);

    /// <summary>
    /// Get booking trends across platform
    /// </summary>
    Task<Result<PlatformBookingTrendsResponse>> GetPlatformBookingTrendsAsync(
        string userId,
        DateTime startDate,
        DateTime endDate);

    /// <summary>
    /// Get top performing units across all departments
    /// </summary>
    Task<Result<IEnumerable<TopPerformingUnitResponse>>> GetTopPerformingUnitsAsync(
        string userId,
        int topN = 10);

    /// <summary>
    /// Get registration requests overview
    /// </summary>
    Task<Result<RegistrationRequestsOverviewResponse>> GetRegistrationRequestsOverviewAsync(
        string userId);

    /// <summary>
    /// Get platform financial summary
    /// </summary>
    Task<Result<PlatformFinancialSummaryResponse>> GetPlatformFinancialSummaryAsync(
        string userId,
        DateTime? startDate = null,
        DateTime? endDate = null);
}
