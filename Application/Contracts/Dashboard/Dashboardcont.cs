using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Contracts.Dashboard;

internal class Dashboardcont
{
}

public class SuperAdminDashboardResponse
{
    public PlatformOverview Platform { get; set; } = new();
    public SuperAdminDashboardStats Stats { get; set; } = new();
    public List<DepartmentSummary> TopDepartments { get; set; } = [];
    public List<RecentActivityItem> RecentActivities { get; set; } = [];
    public List<TopPerformingUnitSummary> TopUnits { get; set; } = [];
    public PlatformRevenueOverview RevenueOverview { get; set; } = new();
    public List<PendingRegistrationSummary> PendingRegistrations { get; set; } = [];
    public List<AlertItem> SystemAlerts { get; set; } = [];
}

public class PlatformOverview
{
    public int TotalDepartments { get; set; }
    public int ActiveDepartments { get; set; }
    public int TotalUnits { get; set; }
    public int ActiveUnits { get; set; }
    public int VerifiedUnits { get; set; }
    public int TotalSubUnits { get; set; }
    public int TotalUsers { get; set; }
    public int TotalHotelAdmins { get; set; }
    public int TotalCityAdmins { get; set; }
    public decimal AveragePlatformRating { get; set; }
    public int TotalReviews { get; set; }
}

public class SuperAdminDashboardStats
{
    // Bookings
    public int TodayBookings { get; set; }
    public int WeekBookings { get; set; }
    public int MonthBookings { get; set; }
    public int TotalBookings { get; set; }
    public int ActiveBookings { get; set; }
    public int CompletedBookings { get; set; }
    public int CancelledBookings { get; set; }

    // Check-ins/Check-outs
    public int TodayCheckIns { get; set; }
    public int TodayCheckOuts { get; set; }
    public int CurrentGuests { get; set; }

    // Revenue
    public decimal TodayRevenue { get; set; }
    public decimal WeekRevenue { get; set; }
    public decimal MonthRevenue { get; set; }
    public decimal YearRevenue { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal PendingPayments { get; set; }

    // Occupancy
    public decimal AverageOccupancyRate { get; set; }

    // Registration Requests
    public int PendingRegistrations { get; set; }
    public int ApprovedThisMonth { get; set; }
    public int RejectedThisMonth { get; set; }

    // Growth Metrics
    public decimal BookingGrowthRate { get; set; } // Compared to last period
    public decimal RevenueGrowthRate { get; set; }
    public decimal UserGrowthRate { get; set; }
}

public class DepartmentSummary
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public int TotalUnits { get; set; }
    public int ActiveUnits { get; set; }
    public int TotalBookings { get; set; }
    public decimal Revenue { get; set; }
    public decimal AverageRating { get; set; }
    public decimal OccupancyRate { get; set; }
}

public class RecentActivityItem
{
    public int Id { get; set; }
    public string Type { get; set; } = string.Empty; // Booking, Registration, Review, etc.
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? DepartmentName { get; set; }
    public string? UnitName { get; set; }
    public DateTime Timestamp { get; set; }
    public string? UserName { get; set; }
    public string? Status { get; set; }
}

public class TopPerformingUnitSummary
{
    public int UnitId { get; set; }
    public string UnitName { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
    public string UnitType { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
    public int BookingCount { get; set; }
    public decimal Rating { get; set; }
    public decimal OccupancyRate { get; set; }
}

public class PlatformRevenueOverview
{
    public decimal TodayRevenue { get; set; }
    public decimal WeekRevenue { get; set; }
    public decimal MonthRevenue { get; set; }
    public decimal YearRevenue { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal PendingPayments { get; set; }
    public decimal RefundedAmount { get; set; }
    public List<MonthlyRevenueData> MonthlyTrend { get; set; } = [];
    public List<DepartmentRevenueData> RevenueByDepartment { get; set; } = [];
}

public class MonthlyRevenueData
{
    public string Month { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
    public int BookingCount { get; set; }
    public decimal AverageBookingValue { get; set; }
}

public class DepartmentRevenueData
{
    public int DepartmentId { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
    public int BookingCount { get; set; }
    public decimal Percentage { get; set; }
}

public class PendingRegistrationSummary
{
    public int Id { get; set; }
    public string UnitName { get; set; } = string.Empty;
    public string OwnerName { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
    public DateTime SubmittedAt { get; set; }
    public int DaysWaiting { get; set; }
}

public class AlertItem
{
    public int Id { get; set; }
    public string Type { get; set; } = string.Empty; // Warning, Error, Info
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty; // High, Medium, Low
    public DateTime CreatedAt { get; set; }
    public bool IsResolved { get; set; }
}

// ============= STATS RESPONSE =============

public class SuperAdminStatsResponse
{
    // Platform Overview
    public int TotalDepartments { get; set; }
    public int ActiveDepartments { get; set; }
    public int InactiveDepartments { get; set; }

    // Units
    public int TotalUnits { get; set; }
    public int ActiveUnits { get; set; }
    public int InactiveUnits { get; set; }
    public int VerifiedUnits { get; set; }
    public int UnverifiedUnits { get; set; }
    public int FeaturedUnits { get; set; }
    public int TotalSubUnits { get; set; }
    public int AvailableSubUnits { get; set; }

    // Users
    public int TotalUsers { get; set; }
    public int ActiveUsers { get; set; }
    public int TotalGuests { get; set; }
    public int TotalHotelAdmins { get; set; }
    public int TotalCityAdmins { get; set; }
    public int NewUsersThisMonth { get; set; }

    // Bookings
    public int TotalBookings { get; set; }
    public int ActiveBookings { get; set; }
    public int CompletedBookings { get; set; }
    public int CancelledBookings { get; set; }
    public int PendingBookings { get; set; }
    public int BookingsThisMonth { get; set; }
    public int BookingsThisWeek { get; set; }
    public int BookingsToday { get; set; }

    // Revenue
    public decimal TotalRevenue { get; set; }
    public decimal MonthRevenue { get; set; }
    public decimal WeekRevenue { get; set; }
    public decimal TodayRevenue { get; set; }
    public decimal PendingRevenue { get; set; }
    public decimal RefundedAmount { get; set; }
    public decimal AverageBookingValue { get; set; }

    // Reviews
    public int TotalReviews { get; set; }
    public decimal AveragePlatformRating { get; set; }
    public int ReviewsThisMonth { get; set; }

    // Occupancy
    public decimal AverageOccupancyRate { get; set; }
    public decimal CurrentOccupancyRate { get; set; }

    // Registration Requests
    public int TotalRegistrationRequests { get; set; }
    public int PendingRegistrations { get; set; }
    public int ApprovedRegistrations { get; set; }
    public int RejectedRegistrations { get; set; }

    // Coupons & Offers
    public int ActiveCoupons { get; set; }
    public int ActiveOffers { get; set; }
    public int ActiveAds { get; set; }
}

// ============= PLATFORM REVENUE OVERVIEW =============

public class PlatformRevenueOverviewResponse
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal AverageRevenuePerDay { get; set; }
    public decimal AverageRevenuePerBooking { get; set; }
    public int TotalBookings { get; set; }
    public List<DailyRevenueData> DailyRevenue { get; set; } = [];
    public List<DepartmentRevenueData> RevenueByDepartment { get; set; } = [];
    public List<UnitTypeRevenueData> RevenueByUnitType { get; set; } = [];
    public PaymentMethodBreakdown PaymentMethods { get; set; } = new();
}

public class DailyRevenueData
{
    public DateTime Date { get; set; }
    public decimal Revenue { get; set; }
    public int BookingCount { get; set; }
}

public class UnitTypeRevenueData
{
    public string UnitType { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
    public int BookingCount { get; set; }
    public decimal Percentage { get; set; }
}

public class PaymentMethodBreakdown
{
    public decimal CreditCard { get; set; }
    public decimal BankTransfer { get; set; }
    public decimal Cash { get; set; }
    public decimal Wallet { get; set; }
    public decimal Other { get; set; }
}

// ============= DEPARTMENT PERFORMANCE =============

public class DepartmentPerformanceResponse
{
    public int DepartmentId { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public int TotalUnits { get; set; }
    public int ActiveUnits { get; set; }
    public int TotalBookings { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal AverageRating { get; set; }
    public decimal OccupancyRate { get; set; }
    public decimal RevenueGrowth { get; set; } // Percentage change
    public decimal BookingGrowth { get; set; } // Percentage change
    public int Rank { get; set; }
}

// ============= PLATFORM GROWTH METRICS =============

public class PlatformGrowthMetricsResponse
{
    public List<MonthlyGrowthData> MonthlyGrowth { get; set; } = [];
    public GrowthSummary Summary { get; set; } = new();
}

public class MonthlyGrowthData
{
    public string Month { get; set; } = string.Empty;
    public int NewUsers { get; set; }
    public int NewUnits { get; set; }
    public int NewBookings { get; set; }
    public decimal Revenue { get; set; }
    public int ActiveUsers { get; set; }
}

public class GrowthSummary
{
    public decimal UserGrowthRate { get; set; }
    public decimal UnitGrowthRate { get; set; }
    public decimal BookingGrowthRate { get; set; }
    public decimal RevenueGrowthRate { get; set; }
    public int TotalNewUsersThisYear { get; set; }
    public int TotalNewUnitsThisYear { get; set; }
}

// ============= USER ENGAGEMENT STATS =============

public class UserEngagementStatsResponse
{
    public int TotalRegisteredUsers { get; set; }
    public int ActiveUsersLast30Days { get; set; }
    public int ActiveUsersLast7Days { get; set; }
    public int ActiveUsersToday { get; set; }
    public decimal DailyActiveUserRate { get; set; }
    public decimal WeeklyActiveUserRate { get; set; }
    public decimal MonthlyActiveUserRate { get; set; }
    public int AverageBookingsPerUser { get; set; }
    public int ReturningCustomers { get; set; }
    public decimal ReturnCustomerRate { get; set; }
    public List<UserActivityData> UserActivityTrend { get; set; } = [];
}

public class UserActivityData
{
    public DateTime Date { get; set; }
    public int ActiveUsers { get; set; }
    public int NewUsers { get; set; }
    public int BookingsMade { get; set; }
}

// ============= PLATFORM BOOKING TRENDS =============

public class PlatformBookingTrendsResponse
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int TotalBookings { get; set; }
    public decimal AverageBookingsPerDay { get; set; }
    public List<DailyBookingData> DailyTrend { get; set; } = [];
    public List<MonthlyBookingData> MonthlyTrend { get; set; } = [];
    public BookingSourceBreakdown Sources { get; set; } = new();
    public BookingStatusBreakdown StatusBreakdown { get; set; } = new();
}

public class DailyBookingData
{
    public DateTime Date { get; set; }
    public int BookingCount { get; set; }
    public decimal Revenue { get; set; }
    public decimal AverageBookingValue { get; set; }
}

public class MonthlyBookingData
{
    public string Month { get; set; } = string.Empty;
    public int BookingCount { get; set; }
    public decimal Revenue { get; set; }
    public decimal AverageBookingValue { get; set; }
    public decimal GrowthRate { get; set; }
}

public class BookingSourceBreakdown
{
    public int Direct { get; set; }
    public int Mobile { get; set; }
    public int Web { get; set; }
    public int API { get; set; }
}

public class BookingStatusBreakdown
{
    public int Pending { get; set; }
    public int Confirmed { get; set; }
    public int CheckedIn { get; set; }
    public int Completed { get; set; }
    public int Cancelled { get; set; }
}

// ============= TOP PERFORMING UNITS =============

public class TopPerformingUnitResponse
{
    public int UnitId { get; set; }
    public string UnitName { get; set; } = string.Empty;
    public int DepartmentId { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public string UnitType { get; set; } = string.Empty;
    public decimal TotalRevenue { get; set; }
    public int TotalBookings { get; set; }
    public decimal AverageRating { get; set; }
    public int TotalReviews { get; set; }
    public decimal OccupancyRate { get; set; }
    public decimal AverageBookingValue { get; set; }
    public bool IsVerified { get; set; }
    public bool IsFeatured { get; set; }
    public int Rank { get; set; }
}

// ============= REGISTRATION REQUESTS OVERVIEW =============

public class RegistrationRequestsOverviewResponse
{
    public int TotalRequests { get; set; }
    public int PendingRequests { get; set; }
    public int ApprovedRequests { get; set; }
    public int RejectedRequests { get; set; }
    public int RequestsThisMonth { get; set; }
    public int RequestsThisWeek { get; set; }
    public decimal AverageProcessingTimeHours { get; set; }
    public List<DepartmentRequestData> RequestsByDepartment { get; set; } = [];
    public List<RecentRegistrationRequest> RecentRequests { get; set; } = [];
}

public class DepartmentRequestData
{
    public int DepartmentId { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public int TotalRequests { get; set; }
    public int PendingRequests { get; set; }
    public int ApprovedRequests { get; set; }
    public int RejectedRequests { get; set; }
}

public class RecentRegistrationRequest
{
    public int Id { get; set; }
    public string UnitName { get; set; } = string.Empty;
    public string OwnerName { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime SubmittedAt { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public int DaysWaiting { get; set; }
}

// ============= PLATFORM FINANCIAL SUMMARY =============

public class PlatformFinancialSummaryResponse
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    // Revenue
    public decimal TotalRevenue { get; set; }
    public decimal CompletedBookingsRevenue { get; set; }
    public decimal PendingRevenue { get; set; }

    // Refunds & Cancellations
    public decimal TotalRefunded { get; set; }
    public int TotalCancellations { get; set; }
    public decimal CancellationRate { get; set; }

    // Payment Status
    public decimal FullyPaidAmount { get; set; }
    public decimal PartiallyPaidAmount { get; set; }
    public decimal UnpaidAmount { get; set; }

    // Averages
    public decimal AverageBookingValue { get; set; }
    public decimal AverageRevenuePerUnit { get; set; }
    public decimal AverageRevenuePerDepartment { get; set; }

    // Growth
    public decimal RevenueGrowth { get; set; }
    public decimal BookingGrowth { get; set; }

    // Top Contributors
    public List<TopRevenueDepartment> TopDepartments { get; set; } = [];
    public List<TopRevenueUnit> TopUnits { get; set; } = [];
}

public class TopRevenueDepartment
{
    public int DepartmentId { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
    public decimal Percentage { get; set; }
}

public class TopRevenueUnit
{
    public int UnitId { get; set; }
    public string UnitName { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
    public decimal Percentage { get; set; }
}