using Application.Abstraction;
using Domain;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace Application.Service.Report;


public class ReportService(ApplicationDbcontext context) : IReportService
{
    private readonly ApplicationDbcontext _context = context;

    #region REVENUE REPORTS

    public async Task<Result<RevenueReportResponse>> GenerateRevenueReportAsync(ReportFilter filter)
    {
        var query = _context.Bookings
            .Include(b => b.Unit)
            .Include(b => b.Payments)
            .AsQueryable();

        query = ApplyDateFilter(query, filter);

        if (filter.UnitId.HasValue)
            query = query.Where(b => b.UnitId == filter.UnitId.Value);

        if (filter.DepartmentId.HasValue)
            query = query.Where(b => b.Unit.CityId == filter.DepartmentId.Value);

        var bookings = await query.ToListAsync();

        var report = new RevenueReportResponse
        {
            GeneratedAt = DateTime.UtcNow.AddHours(3),
            Period = $"{filter.StartDate:yyyy-MM-dd} to {filter.EndDate:yyyy-MM-dd}",

            TotalRevenue = bookings
                .Where(b => b.Status == Domain.BookingStatus.Completed)
                .Sum(b => b.TotalPrice),

            PendingRevenue = bookings
                .Where(b => b.Status == Domain.BookingStatus.Pending ||
                           b.Status == Domain.BookingStatus.Confirmed)
                .Sum(b => b.TotalPrice),

            RefundedAmount = bookings
                .Where(b => b.Status == Domain.BookingStatus.Cancelled)
                .SelectMany(b => b.Payments)
                .Where(p => p.Amount < 0)
                .Sum(p => Math.Abs(p.Amount)),

            NetRevenue = bookings
                .Where(b => b.Status == Domain.BookingStatus.Completed)
                .Sum(b => b.TotalPrice) -
                bookings
                .Where(b => b.Status == Domain.BookingStatus.Cancelled)
                .SelectMany(b => b.Payments)
                .Where(p => p.Amount < 0)
                .Sum(p => Math.Abs(p.Amount)),

            TotalBookings = bookings.Count,
            CompletedBookings = bookings.Count(b => b.Status == Domain.BookingStatus.Completed),
            CancelledBookings = bookings.Count(b => b.Status == Domain.BookingStatus.Cancelled),

            AverageBookingValue = bookings.Any()
                ? bookings.Average(b => b.TotalPrice)
                : 0,

            RevenueByMonth = bookings
                .Where(b => b.Status == Domain.BookingStatus.Completed)
                .GroupBy(b => new { b.CreatedAt.Year, b.CreatedAt.Month })
                .OrderBy(g => g.Key.Year)
                .ThenBy(g => g.Key.Month)
                .ToDictionary(
                    g => $"{g.Key.Year}-{g.Key.Month:D2}",
                    g => g.Sum(b => b.TotalPrice)),

            RevenueByUnit = bookings
                .Where(b => b.Status == Domain.BookingStatus.Completed)
                .GroupBy(b => new { b.Unit.Id, b.Unit.Name })
                .OrderByDescending(g => g.Sum(b => b.TotalPrice))
                .Take(10)
                .ToDictionary(
                    g => g.Key.Name,
                    g => g.Sum(b => b.TotalPrice)),

            PaymentMethodBreakdown = bookings
                .SelectMany(b => b.Payments)
                .Where(p => p.Amount > 0)
                .GroupBy(p => p.PaymentMethod)
                .ToDictionary(
                    g => g.Key.ToString(),
                    g => g.Sum(p => p.Amount))
        };

        return Result.Success(report);
    }

    public async Task<Result<byte[]>> ExportRevenueReportToCsvAsync(ReportFilter filter)
    {
        var report = await GenerateRevenueReportAsync(filter);
        if (!report.IsSuccess)
            return Result.Failure<byte[]>(report.Error);

        var csv = new StringBuilder();
        csv.AppendLine("Revenue Report");
        csv.AppendLine($"Period,{report.Value.Period}");
        csv.AppendLine($"Generated,{report.Value.GeneratedAt}");
        csv.AppendLine();

        csv.AppendLine("Summary");
        csv.AppendLine("Metric,Value");
        csv.AppendLine($"Total Revenue,{report.Value.TotalRevenue:C}");
        csv.AppendLine($"Pending Revenue,{report.Value.PendingRevenue:C}");
        csv.AppendLine($"Refunded Amount,{report.Value.RefundedAmount:C}");
        csv.AppendLine($"Net Revenue,{report.Value.NetRevenue:C}");
        csv.AppendLine($"Total Bookings,{report.Value.TotalBookings}");
        csv.AppendLine($"Completed Bookings,{report.Value.CompletedBookings}");
        csv.AppendLine($"Cancelled Bookings,{report.Value.CancelledBookings}");
        csv.AppendLine($"Average Booking Value,{report.Value.AverageBookingValue:C}");
        csv.AppendLine();

        csv.AppendLine("Revenue by Month");
        csv.AppendLine("Month,Revenue");
        foreach (var item in report.Value.RevenueByMonth)
        {
            csv.AppendLine($"{item.Key},{item.Value:F2}");
        }
        csv.AppendLine();

        csv.AppendLine("Top 10 Units by Revenue");
        csv.AppendLine("Unit,Revenue");
        foreach (var item in report.Value.RevenueByUnit)
        {
            csv.AppendLine($"{item.Key},{item.Value:F2}");
        }

        return Result.Success(Encoding.UTF8.GetBytes(csv.ToString()));
    }

    #endregion

    #region BOOKING REPORTS

    public async Task<Result<BookingReportResponse>> GenerateBookingReportAsync(ReportFilter filter)
    {
        var query = _context.Bookings
            .Include(b => b.Unit)
            .Include(b => b.User)
            .AsQueryable();

        query = ApplyDateFilter(query, filter);

        if (filter.UnitId.HasValue)
            query = query.Where(b => b.UnitId == filter.UnitId.Value);

        if (filter.DepartmentId.HasValue)
            query = query.Where(b => b.Unit.CityId == filter.DepartmentId.Value);

        var bookings = await query.ToListAsync();

        var report = new BookingReportResponse
        {
            GeneratedAt = DateTime.UtcNow.AddHours(3),
            Period = $"{filter.StartDate:yyyy-MM-dd} to {filter.EndDate:yyyy-MM-dd}",

            TotalBookings = bookings.Count,

            BookingsByStatus = bookings
                .GroupBy(b => b.Status)
                .ToDictionary(g => g.Key.ToString(), g => g.Count()),

            BookingsByMonth = bookings
                .GroupBy(b => new { b.CreatedAt.Year, b.CreatedAt.Month })
                .OrderBy(g => g.Key.Year)
                .ThenBy(g => g.Key.Month)
                .ToDictionary(
                    g => $"{g.Key.Year}-{g.Key.Month:D2}",
                    g => g.Count()),

            AverageLengthOfStay = bookings.Any()
                ? bookings.Average(b => b.NumberOfNights)
                : 0,

            AverageGuestsPerBooking = bookings.Any()
                ? bookings.Average(b => b.NumberOfGuests)
                : 0,

            TopUnits = bookings
                .GroupBy(b => new { b.Unit.Id, b.Unit.Name })
                .OrderByDescending(g => g.Count())
                .Take(10)
                .Select(g => new UnitBookingSummary
                {
                    UnitId = g.Key.Id,
                    UnitName = g.Key.Name,
                    BookingCount = g.Count(),
                    Revenue = g.Where(b => b.Status == Domain.BookingStatus.Completed)
                             .Sum(b => b.TotalPrice)
                })
                .ToList(),

            CancellationRate = bookings.Any()
                ? (decimal)bookings.Count(b => b.Status == Domain.BookingStatus.Cancelled) / bookings.Count * 100
                : 0,

            BookingLeadTime = bookings.Any()
                ? bookings.Average(b => (b.CheckInDate - b.CreatedAt).TotalDays)
                : 0
        };

        return Result.Success(report);
    }

    public async Task<Result<byte[]>> ExportBookingReportToCsvAsync(ReportFilter filter)
    {
        var report = await GenerateBookingReportAsync(filter);
        if (!report.IsSuccess)
            return Result.Failure<byte[]>(report.Error);

        var csv = new StringBuilder();
        csv.AppendLine("Booking Report");
        csv.AppendLine($"Period,{report.Value.Period}");
        csv.AppendLine($"Generated,{report.Value.GeneratedAt}");
        csv.AppendLine();

        csv.AppendLine("Summary");
        csv.AppendLine($"Total Bookings,{report.Value.TotalBookings}");
        csv.AppendLine($"Average Length of Stay,{report.Value.AverageLengthOfStay:F2} nights");
        csv.AppendLine($"Average Guests per Booking,{report.Value.AverageGuestsPerBooking:F2}");
        csv.AppendLine($"Cancellation Rate,{report.Value.CancellationRate:F2}%");
        csv.AppendLine($"Booking Lead Time,{report.Value.BookingLeadTime:F2} days");
        csv.AppendLine();

        csv.AppendLine("Bookings by Status");
        csv.AppendLine("Status,Count");
        foreach (var item in report.Value.BookingsByStatus)
        {
            csv.AppendLine($"{item.Key},{item.Value}");
        }
        csv.AppendLine();

        csv.AppendLine("Top 10 Units");
        csv.AppendLine("Unit,Bookings,Revenue");
        foreach (var item in report.Value.TopUnits)
        {
            csv.AppendLine($"{item.UnitName},{item.BookingCount},{item.Revenue:F2}");
        }

        return Result.Success(Encoding.UTF8.GetBytes(csv.ToString()));
    }

    #endregion

    #region UNIT PERFORMANCE REPORTS

    public async Task<Result<UnitPerformanceReportResponse>> GenerateUnitPerformanceReportAsync(
        ReportFilter filter)
    {
        var query = _context.Units
            .Include(u => u.Bookings)
            .Include(u => u.Reviews)
            .Include(u => u.Rooms)
            .Where(u => !u.IsDeleted)
            .AsQueryable();

        if (filter.UnitId.HasValue)
            query = query.Where(u => u.Id == filter.UnitId.Value);

        if (filter.DepartmentId.HasValue)
            query = query.Where(u => u.CityId == filter.DepartmentId.Value);

        var units = await query.ToListAsync();

        var unitPerformances = new List<UnitPerformanceData>();

        foreach (var unit in units)
        {
            var relevantBookings = unit.Bookings
                .Where(b => b.CreatedAt >= filter.StartDate && b.CreatedAt <= filter.EndDate)
                .ToList();

            var completedBookings = relevantBookings
                .Where(b => b.Status == Domain.BookingStatus.Completed)
                .ToList();

            var totalRoomNights = unit.Rooms.Count *
                                 (DateOnly.FromDateTime(filter.EndDate ?? DateTime.UtcNow).Day - DateOnly.FromDateTime(filter.StartDate ?? DateTime.UtcNow).Day);

            var bookedRoomNights = relevantBookings
                .Where(b => b.Status != Domain.BookingStatus.Cancelled)
                .Sum(b => b.NumberOfNights);

            unitPerformances.Add(new UnitPerformanceData
            {
                UnitId = unit.Id,
                UnitName = unit.Name,
                TotalBookings = relevantBookings.Count,
                CompletedBookings = completedBookings.Count,
                CancelledBookings = relevantBookings
                    .Count(b => b.Status == Domain.BookingStatus.Cancelled),
                Revenue = completedBookings.Sum(b => b.TotalPrice),
                AverageRating = unit.AverageRating,
                TotalReviews = unit.Reviews.Count,
                OccupancyRate = totalRoomNights > 0
                    ? (decimal)bookedRoomNights / totalRoomNights * 100
                    : 0,
                AverageBookingValue = completedBookings.Any()
                    ? completedBookings.Average(b => b.TotalPrice)
                    : 0,
                TotalRooms = unit.Rooms.Count
            });
        }

        var report = new UnitPerformanceReportResponse
        {
            GeneratedAt = DateTime.UtcNow.AddHours(3),
            Period = $"{filter.StartDate:yyyy-MM-dd} to {filter.EndDate:yyyy-MM-dd}",
            TotalUnits = unitPerformances.Count,
            UnitPerformances = unitPerformances.OrderByDescending(u => u.Revenue).ToList()
        };

        return Result.Success(report);
    }

    public async Task<Result<byte[]>> ExportUnitPerformanceReportToCsvAsync(ReportFilter filter)
    {
        var report = await GenerateUnitPerformanceReportAsync(filter);
        if (!report.IsSuccess)
            return Result.Failure<byte[]>(report.Error);

        var csv = new StringBuilder();
        csv.AppendLine("Unit Performance Report");
        csv.AppendLine($"Period,{report.Value.Period}");
        csv.AppendLine($"Generated,{report.Value.GeneratedAt}");
        csv.AppendLine();

        csv.AppendLine("Unit,Total Bookings,Completed,Cancelled,Revenue,Avg Rating,Reviews,Occupancy %,Avg Booking Value,Rooms");

        foreach (var unit in report.Value.UnitPerformances)
        {
            csv.AppendLine($"{unit.UnitName},{unit.TotalBookings},{unit.CompletedBookings}," +
                          $"{unit.CancelledBookings},{unit.Revenue:F2},{unit.AverageRating:F2}," +
                          $"{unit.TotalReviews},{unit.OccupancyRate:F2},{unit.AverageBookingValue:F2}," +
                          $"{unit.TotalRooms}");
        }

        return Result.Success(Encoding.UTF8.GetBytes(csv.ToString()));
    }

    #endregion

    #region USER ACTIVITY REPORTS

    public async Task<Result<UserActivityReportResponse>> GenerateUserActivityReportAsync(
        ReportFilter filter)
    {
        var bookings = await _context.Bookings
            .Include(b => b.User)
            .Where(b => b.CreatedAt >= filter.StartDate && b.CreatedAt <= filter.EndDate)
            .ToListAsync();

        var report = new UserActivityReportResponse
        {
            GeneratedAt = DateTime.UtcNow.AddHours(3),
            Period = $"{filter.StartDate:yyyy-MM-dd} to {filter.EndDate:yyyy-MM-dd}",

            TotalUsers = bookings.Select(b => b.UserId).Distinct().Count(),

            NewUsers = await _context.Users
                .Where(u => u.CreatedAt >= filter.StartDate && u.CreatedAt <= filter.EndDate)
                .CountAsync(),

            ActiveUsers = bookings
                .Where(b => b.Status != Domain.BookingStatus.Cancelled)
                .Select(b => b.UserId)
                .Distinct()
                .Count(),

            TopUsers = bookings
                .GroupBy(b => new { b.UserId, b.User.FullName })
                .OrderByDescending(g => g.Count())
                .Take(10)
                .Select(g => new UserActivitySummary
                {
                    UserId = g.Key.UserId,
                    UserName = g.Key.FullName ?? "N/A",
                    BookingCount = g.Count(),
                    TotalSpent = g.Where(b => b.Status == Domain.BookingStatus.Completed)
                                  .Sum(b => b.TotalPrice)
                })
                .ToList(),

            UsersByBookingCount = bookings
                .GroupBy(b => b.UserId)
                .GroupBy(g => g.Count() switch
                {
                    1 => "1 booking",
                    >= 2 and <= 5 => "2-5 bookings",
                    >= 6 and <= 10 => "6-10 bookings",
                    _ => "10+ bookings"
                })
                .ToDictionary(g => g.Key, g => g.Count())
        };

        return Result.Success(report);
    }

    #endregion

    #region OCCUPANCY REPORTS

    public async Task<Result<OccupancyReportResponse>> GenerateOccupancyReportAsync(
        ReportFilter filter)
    {
        var units = await _context.Units
            .Include(u => u.Rooms)
            .Include(u => u.Bookings)
                .ThenInclude(b => b.BookingRooms)
            .Where(u => !u.IsDeleted)
            .ToListAsync();

        var totalDays = (DateOnly.FromDateTime(filter.EndDate ?? DateTime.UtcNow).Day - DateOnly.FromDateTime(filter.StartDate ?? DateTime.UtcNow).Day);
        var totalRoomNights = units.Sum(u => u.Rooms.Count) * totalDays;

        var bookedRoomNights = units
            .SelectMany(u => u.Bookings)
            .Where(b => b.CheckInDate >= filter.StartDate &&
                       b.CheckOutDate <= filter.EndDate &&
                       b.Status != Domain.BookingStatus.Cancelled)
            .Sum(b => b.NumberOfNights * b.BookingRooms.Count);

        var report = new OccupancyReportResponse
        {
            GeneratedAt = DateTime.UtcNow.AddHours(3),
            Period = $"{filter.StartDate:yyyy-MM-dd} to {filter.EndDate:yyyy-MM-dd}",
            OverallOccupancyRate = totalRoomNights > 0
                ? (decimal)bookedRoomNights / totalRoomNights * 100
                : 0,
            TotalRoomNights = totalRoomNights,
            BookedRoomNights = bookedRoomNights,
            AvailableRoomNights = totalRoomNights - bookedRoomNights,

            OccupancyByUnit = units
                .Select(u =>
                {
                    var unitRoomNights = u.Rooms.Count * totalDays;
                    var unitBookedNights = u.Bookings
                        .Where(b => b.CheckInDate >= filter.StartDate &&
                                   b.CheckOutDate <= filter.EndDate &&
                                   b.Status != Domain.BookingStatus.Cancelled)
                        .Sum(b => b.NumberOfNights * b.BookingRooms.Count);

                    return new UnitOccupancyData
                    {
                        UnitId = u.Id,
                        UnitName = u.Name,
                        TotalRoomNights = unitRoomNights,
                        BookedRoomNights = unitBookedNights,
                        OccupancyRate = unitRoomNights > 0
                            ? (decimal)unitBookedNights / unitRoomNights * 100
                            : 0
                    };
                })
                .OrderByDescending(u => u.OccupancyRate)
                .ToList()
        };

        return Result.Success(report);
    }

    #endregion

    #region DEPARTMENT REPORTS

    public async Task<Result<DepartmentReportResponse>> GenerateDepartmentReportAsync(
        ReportFilter filter)
    {
        var departments = await _context.Departments
            .Include(d => d.Units)
                .ThenInclude(u => u.Bookings)
            .Where(d => !d.IsDeleted)
            .ToListAsync();

        if (filter.DepartmentId.HasValue)
            departments = departments.Where(d => d.Id == filter.DepartmentId.Value).ToList();

        var departmentData = new List<DepartmentPerformanceData>();

        foreach (var dept in departments)
        {
            var relevantBookings = dept.Units
                .SelectMany(u => u.Bookings)
                .Where(b => b.CreatedAt >= filter.StartDate && b.CreatedAt <= filter.EndDate)
                .ToList();

            var completedBookings = relevantBookings
                .Where(b => b.Status == Domain.BookingStatus.Completed)
                .ToList();

            departmentData.Add(new DepartmentPerformanceData
            {
                DepartmentId = dept.Id,
                DepartmentName = dept.Name,
                TotalUnits = dept.Units.Count(u => !u.IsDeleted),
                ActiveUnits = dept.Units.Count(u => !u.IsDeleted && u.IsActive),
                TotalBookings = relevantBookings.Count,
                CompletedBookings = completedBookings.Count,
                Revenue = completedBookings.Sum(b => b.TotalPrice),
                AverageRating = dept.AverageRating,
                TotalReviews = dept.Units.Sum(u => u.TotalReviews)
            });
        }

        var report = new DepartmentReportResponse
        {
            GeneratedAt = DateTime.UtcNow.AddHours(3),
            Period = $"{filter.StartDate:yyyy-MM-dd} to {filter.EndDate:yyyy-MM-dd}",
            TotalDepartments = departmentData.Count,
            DepartmentPerformances = departmentData.OrderByDescending(d => d.Revenue).ToList()
        };

        return Result.Success(report);
    }

    #endregion

    #region DASHBOARD REPORT

    public async Task<Result<DashboardReportResponse>> GenerateDashboardReportAsync(
        ReportFilter filter)
    {
        // Aggregate all key metrics
        var revenueReport = await GenerateRevenueReportAsync(filter);
        var bookingReport = await GenerateBookingReportAsync(filter);
        var occupancyReport = await GenerateOccupancyReportAsync(filter);
        var userReport = await GenerateUserActivityReportAsync(filter);

        if (!revenueReport.IsSuccess || !bookingReport.IsSuccess ||
            !occupancyReport.IsSuccess || !userReport.IsSuccess)
        {
            return Result.Failure<DashboardReportResponse>(
                new Error("ReportFailed", "Failed to generate dashboard report", 500));
        }

        var dashboard = new DashboardReportResponse
        {
            GeneratedAt = DateTime.UtcNow.AddHours(3),
            Period = $"{filter.StartDate:yyyy-MM-dd} to {filter.EndDate:yyyy-MM-dd}",

            // Financial Metrics
            TotalRevenue = revenueReport.Value.TotalRevenue,
            NetRevenue = revenueReport.Value.NetRevenue,
            PendingRevenue = revenueReport.Value.PendingRevenue,

            // Booking Metrics
            TotalBookings = bookingReport.Value.TotalBookings,
            CompletedBookings = revenueReport.Value.CompletedBookings,
            CancelledBookings = revenueReport.Value.CancelledBookings,
            CancellationRate = bookingReport.Value.CancellationRate,
            AverageBookingValue = revenueReport.Value.AverageBookingValue,

            // Occupancy Metrics
            OverallOccupancyRate = occupancyReport.Value.OverallOccupancyRate,

            // User Metrics
            TotalUsers = userReport.Value.TotalUsers,
            ActiveUsers = userReport.Value.ActiveUsers,
            NewUsers = userReport.Value.NewUsers,

            // Trend Data
            RevenueByMonth = revenueReport.Value.RevenueByMonth,
            BookingsByMonth = bookingReport.Value.BookingsByMonth
        };

        return Result.Success(dashboard);
    }

    #endregion

    #region CUSTOM REPORTS

    public async Task<Result<byte[]>> GenerateCustomReportAsync(CustomReportRequest request)
    {
        // This allows for flexible custom reporting based on user requirements
        var csv = new StringBuilder();
        csv.AppendLine($"Custom Report: {request.ReportName}");
        csv.AppendLine($"Generated: {DateTime.UtcNow.AddHours(3)}");
        csv.AppendLine();

        // Implementation based on request.ReportType
        // This can be extended to support various custom report types

        return Result.Success(Encoding.UTF8.GetBytes(csv.ToString()));
    }

    #endregion

    #region HELPER METHODS

    private IQueryable<Domain.Entities.Booking> ApplyDateFilter(
        IQueryable<Domain.Entities.Booking> query,
        ReportFilter filter)
    {
        if (filter.StartDate.HasValue)
            query = query.Where(b => b.CreatedAt >= filter.StartDate.Value);

        if (filter.EndDate.HasValue)
            query = query.Where(b => b.CreatedAt <= filter.EndDate.Value);

        return query;
    }

    #endregion
}

// ============= REQUEST & RESPONSE MODELS =============

public record ReportFilter
{
    public DateTime? StartDate { get; init; } = DateTime.UtcNow.AddMonths(-1);
    public DateTime? EndDate { get; init; } = DateTime.UtcNow;
    public int? UnitId { get; init; }
    public int? DepartmentId { get; init; }
}

public record CustomReportRequest
{
    public string ReportName { get; init; } = string.Empty;
    public string ReportType { get; init; } = string.Empty;
    public Dictionary<string, string> Parameters { get; init; } = new();
}

// Revenue Report
public class RevenueReportResponse
{
    public DateTime GeneratedAt { get; set; }
    public string Period { get; set; } = string.Empty;
    public decimal TotalRevenue { get; set; }
    public decimal PendingRevenue { get; set; }
    public decimal RefundedAmount { get; set; }
    public decimal NetRevenue { get; set; }
    public int TotalBookings { get; set; }
    public int CompletedBookings { get; set; }
    public int CancelledBookings { get; set; }
    public decimal AverageBookingValue { get; set; }
    public Dictionary<string, decimal> RevenueByMonth { get; set; } = new();
    public Dictionary<string, decimal> RevenueByUnit { get; set; } = new();
    public Dictionary<string, decimal> PaymentMethodBreakdown { get; set; } = new();
}

// Booking Report
public class BookingReportResponse
{
    public DateTime GeneratedAt { get; set; }
    public string Period { get; set; } = string.Empty;
    public int TotalBookings { get; set; }
    public Dictionary<string, int> BookingsByStatus { get; set; } = new();
    public Dictionary<string, int> BookingsByMonth { get; set; } = new();
    public double AverageLengthOfStay { get; set; }
    public double AverageGuestsPerBooking { get; set; }
    public List<UnitBookingSummary> TopUnits { get; set; } = new();
    public decimal CancellationRate { get; set; }
    public double BookingLeadTime { get; set; }
}

public class UnitBookingSummary
{
    public int UnitId { get; set; }
    public string UnitName { get; set; } = string.Empty;
    public int BookingCount { get; set; }
    public decimal Revenue { get; set; }
}

// Unit Performance Report
public class UnitPerformanceReportResponse
{
    public DateTime GeneratedAt { get; set; }
    public string Period { get; set; } = string.Empty;
    public int TotalUnits { get; set; }
    public List<UnitPerformanceData> UnitPerformances { get; set; } = new();
}

public class UnitPerformanceData
{
    public int UnitId { get; set; }
    public string UnitName { get; set; } = string.Empty;
    public int TotalBookings { get; set; }
    public int CompletedBookings { get; set; }
    public int CancelledBookings { get; set; }
    public decimal Revenue { get; set; }
    public decimal AverageRating { get; set; }
    public int TotalReviews { get; set; }
    public decimal OccupancyRate { get; set; }
    public decimal AverageBookingValue { get; set; }
    public int TotalRooms { get; set; }
}

// User Activity Report
public class UserActivityReportResponse
{
    public DateTime GeneratedAt { get; set; }
    public string Period { get; set; } = string.Empty;
    public int TotalUsers { get; set; }
    public int NewUsers { get; set; }
    public int ActiveUsers { get; set; }
    public List<UserActivitySummary> TopUsers { get; set; } = new();
    public Dictionary<string, int> UsersByBookingCount { get; set; } = new();
}

public class UserActivitySummary
{
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public int BookingCount { get; set; }
    public decimal TotalSpent { get; set; }
}

// Occupancy Report
public class OccupancyReportResponse
{
    public DateTime GeneratedAt { get; set; }
    public string Period { get; set; } = string.Empty;
    public decimal OverallOccupancyRate { get; set; }
    public int TotalRoomNights { get; set; }
    public int BookedRoomNights { get; set; }
    public int AvailableRoomNights { get; set; }
    public List<UnitOccupancyData> OccupancyByUnit { get; set; } = new();
}

public class UnitOccupancyData
{
    public int UnitId { get; set; }
    public string UnitName { get; set; } = string.Empty;
    public int TotalRoomNights { get; set; }
    public int BookedRoomNights { get; set; }
    public decimal OccupancyRate { get; set; }
}

// Department Report
public class DepartmentReportResponse
{
    public DateTime GeneratedAt { get; set; }
    public string Period { get; set; } = string.Empty;
    public int TotalDepartments { get; set; }
    public List<DepartmentPerformanceData> DepartmentPerformances { get; set; } = new();
}

public class DepartmentPerformanceData
{
    public int DepartmentId { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public int TotalUnits { get; set; }
    public int ActiveUnits { get; set; }
    public int TotalBookings { get; set; }
    public int CompletedBookings { get; set; }
    public decimal Revenue { get; set; }
    public decimal? AverageRating { get; set; }
    public int TotalReviews { get; set; }
}

// Dashboard Report
public class DashboardReportResponse
{
    public DateTime GeneratedAt { get; set; }
    public string Period { get; set; } = string.Empty;

    // Financial
    public decimal TotalRevenue { get; set; }
    public decimal NetRevenue { get; set; }
    public decimal PendingRevenue { get; set; }

    // Bookings
    public int TotalBookings { get; set; }
    public int CompletedBookings { get; set; }
    public int CancelledBookings { get; set; }
    public decimal CancellationRate { get; set; }
    public decimal AverageBookingValue { get; set; }

    // Occupancy
    public decimal OverallOccupancyRate { get; set; }

    // Users
    public int TotalUsers { get; set; }
    public int ActiveUsers { get; set; }
    public int NewUsers { get; set; }

    // Trends
    public Dictionary<string, decimal> RevenueByMonth { get; set; } = new();
    public Dictionary<string, int> BookingsByMonth { get; set; } = new();
}