using Application.Abstraction;

namespace Application.Service.Report;

public interface IReportService
{
    Task<Result<RevenueReportResponse>> GenerateRevenueReportAsync(ReportFilter filter);
    Task<Result<byte[]>> ExportRevenueReportToCsvAsync(ReportFilter filter);

    // Booking Reports
    Task<Result<BookingReportResponse>> GenerateBookingReportAsync(ReportFilter filter);
    Task<Result<byte[]>> ExportBookingReportToCsvAsync(ReportFilter filter);

    // Unit Performance Reports
    Task<Result<UnitPerformanceReportResponse>> GenerateUnitPerformanceReportAsync(ReportFilter filter);
    Task<Result<byte[]>> ExportUnitPerformanceReportToCsvAsync(ReportFilter filter);

    // User Activity Reports
    Task<Result<UserActivityReportResponse>> GenerateUserActivityReportAsync(ReportFilter filter);

    // Occupancy Reports
    Task<Result<OccupancyReportResponse>> GenerateOccupancyReportAsync(ReportFilter filter);

    // Department Reports
    Task<Result<DepartmentReportResponse>> GenerateDepartmentReportAsync(ReportFilter filter);

    // Comprehensive Dashboard Report
    Task<Result<DashboardReportResponse>> GenerateDashboardReportAsync(ReportFilter filter);

    // Custom Reports
    Task<Result<byte[]>> GenerateCustomReportAsync(CustomReportRequest request);
}