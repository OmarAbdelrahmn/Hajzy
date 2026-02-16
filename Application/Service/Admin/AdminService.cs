using Application.Abstraction;
using Application.Abstraction.Errors;
using Application.Contracts.AD;
using Application.Contracts.Admin;
using Application.Contracts.Bookin;
using Application.Contracts.Dashboard;
using Application.Service.Roles;
using Domain;
using Domain.Entities;
using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Application.Service.Admin;

public class AdminService(UserManager<ApplicationUser> manager, ApplicationDbcontext dbcontext, IRoleService roleService) : IAdminService
{
    private readonly IRoleService roleService = roleService;

    public async Task<Result<UserResponse>> AddUserAsync(CreateUserRequest request)
    {
        var EmailIsexist = await manager.Users.AnyAsync(c => c.Email == request.Email);

        if (EmailIsexist)
            return Result.Failure<UserResponse>(UserErrors.EmailAlreadyExist);

        var allowedroles = await roleService.GetRolesAsync();

        if (!allowedroles.Value.Any(r => r.Name == request.Role))
            return Result.Failure<UserResponse>(RolesErrors.InvalidRoles);

        var user = request.Adapt<ApplicationUser>();
        user.UserName = request.Email;
        user.EmailConfirmed = true;

        user.Address = request.UserAddress;
        user.FullName = request.UserFullName;

        var result = await manager.CreateAsync(user, request.Password);

        if (result.Succeeded)
        {
            await manager.AddToRoleAsync(user, request.Role);

            var response = (user, request.Role).Adapt<UserResponse>();

            return Result.Success(response);
        }

        var error = result.Errors.First();
        return Result.Failure<UserResponse>(new Error(error.Code, error.Description, StatusCodes.Status400BadRequest));
    }

    public async Task<Result> DeletaUserAsync(string UserId)
    {
        if (await manager.FindByIdAsync(UserId) is not { } user)
            return Result.Failure(UserErrors.UserNotFound);

        var result = await manager.DeleteAsync(user);

        if (result.Succeeded)
            return Result.Success();

        var error = result.Errors.First();

        return Result.Failure(new Error(error.Code, error.Description, StatusCodes.Status400BadRequest));

    }

    public async Task<Result> EndLockOutAsync(string UserId)
    {
        if (await manager.FindByIdAsync(UserId) is not { } user)
            return Result.Failure(UserErrors.UserNotFound);

        var result = await manager.SetLockoutEndDateAsync(user, null);

        if (result.Succeeded)
            return Result.Success();

        var error = result.Errors.First();
        return Result.Failure(new Error(error.Code, error.Description, StatusCodes.Status400BadRequest));
    }

    public async Task<Result<PaginatedResponse<UserResponse>>> GetAllUsers(int page = 1, int pageSize = 10)
    {
        var query = from u in dbcontext.Users
                    join ur in dbcontext.UserRoles on u.Id equals ur.UserId
                    join r in dbcontext.Roles on ur.RoleId equals r.Id into roles
                    select new
                    {
                        u.Id,
                        u.FullName,
                        u.Address,
                        u.Email,
                        u.IsDisable,
                        u.EmailConfirmed,
                        u.PhoneNumber,
                        role = roles.Select(r => r.Name!).FirstOrDefault(),
                        u.AvatarUrl,
                        u.Bio,
                        u.DateOfBirth,
                        u.Nationality,
                        u.CreatedAt,
                        u.LastLoginAt,
                        u.City,
                        u.Country
                    };

        var totalCount = await query.CountAsync();

        var users = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .GroupBy(x => new
            {
                x.Id,
                x.FullName,
                x.Address,
                x.Email,
                x.IsDisable,
                x.EmailConfirmed,
                x.PhoneNumber,
                x.AvatarUrl,
                x.Bio,
                x.DateOfBirth,
                x.Nationality,
                x.CreatedAt,
                x.LastLoginAt,
                x.City,
                x.Country
            })
            .Select(c => new UserResponse(
                c.Key.Id,
                c.Key.FullName,
                c.Key.Address,
                c.Key.Email,
                c.Key.IsDisable,
                c.Select(x => x.role).FirstOrDefault()!,
                c.Key.EmailConfirmed,
                c.Key.PhoneNumber,
                c.Key.AvatarUrl,
                c.Key.Bio,
                c.Key.DateOfBirth,
                c.Key.Nationality,
                c.Key.CreatedAt,
                c.Key.LastLoginAt,
                c.Key.City,
                c.Key.Country
            ))
            .ToListAsync();

        if (users.Count == 0)
            return Result.Failure<PaginatedResponse<UserResponse>>(UserErrors.UserNotFound);

        var paginatedResult = CreatePaginatedResponse(
            users, totalCount, page, pageSize);

        return Result.Success(paginatedResult);
    }

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
    public async Task<Result<UserResponse>> GetUser2Async(string UserName)
    {
        if (await manager.FindByNameAsync(UserName) is not { } user)
            return Result.Failure<UserResponse>(UserErrors.UserNotFound);

        var userroles = await manager.GetRolesAsync(user);

        var response = (user, userroles).Adapt<UserResponse>();

        return Result.Success(response);
    }

    public async Task<Result<UserResponse>> GetUserAsync(string Id)
    {
        if (await manager.FindByIdAsync(Id) is not { } user)
            return Result.Failure<UserResponse>(UserErrors.UserNotFound);

        var userroles = await manager.GetRolesAsync(user);

        var response = new UserResponse
        (
            Id: user.Id,
            FullName: user.FullName!,
            Address: user.Address!,
            Email: user.Email!,
            IsDisable: user.IsDisable,
            Role: userroles.FirstOrDefault()!,
            user.EmailConfirmed,
            PhoneNumber: user.PhoneNumber!,
            AvatarUrl: user.AvatarUrl,
            Bio: user.Bio,
            DateOfBirth: user.DateOfBirth,
            user.Nationality,
            user.CreatedAt,
            user.LastLoginAt,
            user.City,
            user.Country
            );

        return Result.Success(response);
    }

    public async Task<Result> ToggleStatusAsync(string UserId)
    {
        if (await manager.FindByIdAsync(UserId) is not { } user)
            return Result.Failure(UserErrors.UserNotFound);

        user.IsDisable = !user.IsDisable;

        var result = await manager.UpdateAsync(user);
        if (result.Succeeded)
            return Result.Success();

        var error = result.Errors.First();
        return Result.Failure(new Error(error.Code, error.Description, StatusCodes.Status400BadRequest));
    }

    public async Task<Result> UpdateUserAsync(string userId, UpdateUserRequest request)
    {
        using var transaction = await dbcontext.Database.BeginTransactionAsync();
        try
        {
            // Find user
            if (await manager.FindByIdAsync(userId) is not { } user)
                return Result.Failure(UserErrors.UserNotFound);

            // Check duplicate email only if email changed
            if (user.Email != request.Email)
            {
                var duplicatedEmail = await manager.Users
                    .AnyAsync(c => c.Email == request.Email && c.Id != userId);
                if (duplicatedEmail)
                    return Result.Failure(UserErrors.EmailAlreadyExist);
            }

            // Validate role
            var allowedRoles = await roleService.GetRolesAsync();
            if (!allowedRoles.Value.Any(r => r.Name == request.Role))
                return Result.Failure(RolesErrors.InvalidRoles);

            // Update user properties explicitly
            user.Email = request.Email;
            user.NormalizedEmail = request.Email.ToUpperInvariant();
            user.UserName = request.Email;
            user.NormalizedUserName = request.Email.ToUpperInvariant();
            user.FullName = request.UserFullName;
            user.Address = request.UserAddress;

            // Update user first
            var updateResult = await manager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                var error = updateResult.Errors.First();
                return Result.Failure(new Error(error.Code, error.Description,
                    StatusCodes.Status400BadRequest));
            }

            // Update password if provided
            if (!string.IsNullOrEmpty(request.Password))
            {
                await manager.RemovePasswordAsync(user);
                var passwordResult = await manager.AddPasswordAsync(user, request.Password);
                if (!passwordResult.Succeeded)
                {
                    await transaction.RollbackAsync();
                    return Result.Failure(new Error("PasswordUpdateFailed",
                        passwordResult.Errors.First().Description,
                        StatusCodes.Status400BadRequest));
                }
            }

            // Update roles
            var currentRoles = await manager.GetRolesAsync(user);
            await manager.RemoveFromRolesAsync(user, currentRoles);
            var roleResult = await manager.AddToRoleAsync(user, request.Role);
            if (!roleResult.Succeeded)
            {
                await transaction.RollbackAsync();
                return Result.Failure(new Error("RoleUpdateFailed",
                    roleResult.Errors.First().Description,
                    StatusCodes.Status400BadRequest));
            }

            await transaction.CommitAsync();
            return Result.Success();
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<Result<PaginatedBookingsResponse>> GetAllBookingsAsync(BookingAdminFilter filter)
    {
        var query = dbcontext.Bookings
            .Include(b => b.Unit)
                .ThenInclude(u => u.City)
            .Include(b => b.Unit)
                .ThenInclude(u => u.UnitType)
            .Include(b => b.User)
            .Include(b => b.BookingRooms)
                .ThenInclude(br => br.Room)
                    .ThenInclude(r => r.SubUnitType)
            .Include(b => b.Payments)
            .Where(b => !b.IsDeleted)
            .AsQueryable();

        // Apply filters
        if (filter.BookingType.HasValue)
            query = query.Where(b => b.BookingType == filter.BookingType.Value);

        if (filter.Status.HasValue)
            query = query.Where(b => b.Status == filter.Status.Value);

        if (filter.PaymentStatus.HasValue)
            query = query.Where(b => b.PaymentStatus == filter.PaymentStatus.Value);

        if (filter.UnitId.HasValue)
            query = query.Where(b => b.UnitId == filter.UnitId.Value);

        if (filter.CityId.HasValue)
            query = query.Where(b => b.Unit.CityId == filter.CityId.Value);

        if (!string.IsNullOrWhiteSpace(filter.UserId))
            query = query.Where(b => b.UserId == filter.UserId);

        if (!string.IsNullOrWhiteSpace(filter.BookingNumber))
            query = query.Where(b => b.BookingNumber.Contains(filter.BookingNumber));

        if (filter.CheckInFrom.HasValue)
            query = query.Where(b => b.CheckInDate >= filter.CheckInFrom.Value);

        if (filter.CheckInTo.HasValue)
            query = query.Where(b => b.CheckInDate <= filter.CheckInTo.Value);

        if (filter.CreatedFrom.HasValue)
            query = query.Where(b => b.CreatedAt >= filter.CreatedFrom.Value);

        if (filter.CreatedTo.HasValue)
            query = query.Where(b => b.CreatedAt <= filter.CreatedTo.Value);

        // Get total count before pagination
        var totalCount = await query.CountAsync();

        // Apply sorting
        query = filter.SortBy?.ToLower() switch
        {
            "bookingdate" => filter.SortDescending
                ? query.OrderByDescending(b => b.CreatedAt)
                : query.OrderBy(b => b.CreatedAt),
            "checkin" => filter.SortDescending
                ? query.OrderByDescending(b => b.CheckInDate)
                : query.OrderBy(b => b.CheckInDate),
            "totalprice" => filter.SortDescending
                ? query.OrderByDescending(b => b.TotalPrice)
                : query.OrderBy(b => b.TotalPrice),
            "status" => filter.SortDescending
                ? query.OrderByDescending(b => b.Status)
                : query.OrderBy(b => b.Status),
            _ => query.OrderByDescending(b => b.CreatedAt)
        };

        // Apply pagination
        var bookings = await query
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .AsNoTracking()
            .ToListAsync();

        // Get coupon information for all bookings
        var bookingIds = bookings.Select(b => b.Id).ToList();
        var bookingCoupons = await dbcontext.Set<BookingCoupon>()
            .Include(bc => bc.Coupon)
            .Where(bc => bookingIds.Contains(bc.BookingId))
            .AsNoTracking()
            .ToListAsync();

        // Map to response
        var bookingResponses = bookings.Select(booking =>
        {
            var bookingCoupon = bookingCoupons.FirstOrDefault(bc => bc.BookingId == booking.Id);

            return new AdminBookingResponse
            {
                // Basic Info
                Id = booking.Id,
                BookingNumber = booking.BookingNumber,
                BookingType = booking.BookingType.ToString(),
                Status = booking.Status.ToString(),
                PaymentStatus = booking.PaymentStatus.ToString(),

                // Unit Info
                UnitId = booking.UnitId,
                UnitName = booking.Unit.Name,
                UnitAddress = booking.Unit.Address,
                UnitType = booking.Unit.UnitType.Name,
                CityName = booking.Unit.City.Name,
                CityId = booking.Unit.CityId,

                // SubUnit Info (for SubUnit bookings)
                SubUnits = booking.BookingType == BookingType.SubUnitBooking
                    ? booking.BookingRooms.Select(br => new Contracts.Bookin.SubUnitSummary
                    {
                        SubUnitId = br.RoomId,
                        RoomNumber = br.Room.RoomNumber,
                        SubUnitType = br.Room.SubUnitType?.Name ?? "N/A",
                        PricePerNight = br.PricePerNight,
                        NumberOfNights = br.NumberOfNights,
                        Subtotal = br.PricePerNight * br.NumberOfNights,
                        MaxOccupancy = br.Room.MaxOccupancy,
                        Bedrooms = br.Room.Bedrooms,
                        Bathrooms = br.Room.Bathrooms,
                        Size = br.Room.Size
                    }).ToList()
                    : new List<Contracts.Bookin.SubUnitSummary>(),

                // Guest Info
                UserId = booking.UserId,
                GuestName = booking.User.FullName ?? "N/A",
                GuestEmail = booking.User.Email ?? "N/A",
                GuestPhone = booking.User.PhoneNumber,

                // Booking Details
                CheckInDate = booking.CheckInDate,
                CheckOutDate = booking.CheckOutDate,
                NumberOfGuests = booking.NumberOfGuests,
                NumberOfNights = booking.NumberOfNights,

                // Financial Info
                TotalPrice = booking.TotalPrice,
                PaidAmount = booking.PaidAmount,
                RemainingAmount = booking.TotalPrice - booking.PaidAmount,

                // Coupon Info
                CouponCode = bookingCoupon?.Coupon?.Code,
                CouponDiscount = bookingCoupon?.DiscountApplied,

                // Payment Info
                Payments = booking.Payments?.Select(p => new PaymentSummary
                {
                    Id = p.Id,
                    TransactionId = p.TransactionId,
                    Amount = p.Amount,
                    PaymentMethod = p.PaymentMethod.ToString(),
                    PaymentDate = p.PaymentDate,
                    Status = p.Status.ToString(),
                    Notes = p.Notes
                }).OrderByDescending(p => p.PaymentDate).ToList() ?? new List<PaymentSummary>(),

                // Additional Info
                SpecialRequests = booking.SpecialRequests,
                CancellationReason = booking.CancellationReason,
                CancelledAt = booking.CancelledAt,

                // Timestamps
                CreatedAt = booking.CreatedAt,
                UpdatedAt = booking.UpdatedAt
            };
        }).ToList();

        // Calculate statistics
        var statistics = new BookingStatistics
        {
            TotalBookings = totalCount,
            TotalRevenue = bookings.Sum(b => b.TotalPrice),
            PaidRevenue = bookings.Sum(b => b.PaidAmount),
            PendingRevenue = bookings.Where(b => b.Status != BookingStatus.Cancelled)
                .Sum(b => b.TotalPrice - b.PaidAmount),

            PendingCount = bookings.Count(b => b.Status == BookingStatus.Pending),
            ConfirmedCount = bookings.Count(b => b.Status == BookingStatus.Confirmed),
            CheckedInCount = bookings.Count(b => b.Status == BookingStatus.CheckedIn),
            CompletedCount = bookings.Count(b => b.Status == BookingStatus.Completed),
            CancelledCount = bookings.Count(b => b.Status == BookingStatus.Cancelled),

            UnitBookingsCount = bookings.Count(b => b.BookingType == BookingType.UnitBooking),
            SubUnitBookingsCount = bookings.Count(b => b.BookingType == BookingType.SubUnitBooking),

            AverageBookingValue = bookings.Any() ? bookings.Average(b => b.TotalPrice) : 0,
            AverageNightsPerBooking = bookings.Any() ? bookings.Average(b => b.NumberOfNights) : 0
        };

        var response = new PaginatedBookingsResponse
        {
            Bookings = bookingResponses,
            TotalCount = totalCount,
            Page = filter.Page,
            PageSize = filter.PageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)filter.PageSize),
            Statistics = statistics
        };

        return Result.Success(response);
    }


    #region DASHBOARD & OVERVIEW

    public async Task<Result<SuperAdminDashboardResponse>> GetDashboardAsync(string userId)
    {
        try
        {
            var today = DateTime.UtcNow.AddHours(3).Date;
            var thisMonth = new DateTime(today.Year, today.Month, 1);
            var thisWeek = today.AddDays(-(int)today.DayOfWeek);

            // Call individual methods to get detailed data
            var revenueOverviewResult = await GetPlatformRevenueOverviewAsync(userId, thisMonth, today);
            var departmentPerformanceResult = await GetDepartmentPerformanceAsync(userId, 5);
            var growthMetricsResult = await GetPlatformGrowthMetricsAsync(userId, 12);
            var bookingTrendsResult = await GetPlatformBookingTrendsAsync(userId, today.AddDays(-30), today);
            var registrationRequestsResult = await GetRegistrationRequestsOverviewAsync(userId);
            var financialSummaryResult = await GetPlatformFinancialSummaryAsync(userId, thisMonth, today);
            var topUnitsResult = await GetTopPerformingUnitsAsync(userId, 5);

            // Platform Overview
            var platformOverview = new PlatformOverview
            {
                TotalDepartments = await dbcontext.Departments.CountAsync(d => !d.IsDeleted),
                ActiveDepartments = await dbcontext.Departments.CountAsync(d => !d.IsDeleted && d.IsActive),
                TotalUnits = await dbcontext.Units.CountAsync(u => !u.IsDeleted),
                ActiveUnits = await dbcontext.Units.CountAsync(u => !u.IsDeleted && u.IsActive),
                VerifiedUnits = await dbcontext.Units.CountAsync(u => !u.IsDeleted && u.IsVerified),
                TotalSubUnits = await dbcontext.SubUnits.CountAsync(s => !s.IsDeleted),
                TotalUsers = await manager.Users.CountAsync(),
                TotalHotelAdmins = (await manager.GetUsersInRoleAsync("HotelAdmin")).Count,
                TotalCityAdmins = (await manager.GetUsersInRoleAsync("CityAdmin")).Count,
                AveragePlatformRating = await dbcontext.Units
                    .Where(u => !u.IsDeleted && u.TotalReviews > 0)
                    .AverageAsync(u => (decimal?)u.AverageRating) ?? 0,
                TotalReviews = await dbcontext.Reviews.CountAsync()
            };

            // Dashboard Stats (using data from financial summary and booking trends)
            var allBookings = await dbcontext.Bookings.ToListAsync();
            var stats = new SuperAdminDashboardStats
            {
                TodayBookings = allBookings.Count(b => b.CreatedAt.Date == today),
                WeekBookings = allBookings.Count(b => b.CreatedAt >= thisWeek),
                MonthBookings = allBookings.Count(b => b.CreatedAt >= thisMonth),
                TotalBookings = allBookings.Count,
                ActiveBookings = allBookings.Count(b => b.Status == BookingStatus.Confirmed ||
                    b.Status == BookingStatus.CheckedIn),
                CompletedBookings = allBookings.Count(b => b.Status == BookingStatus.Completed),
                CancelledBookings = allBookings.Count(b => b.Status == BookingStatus.Cancelled),
                TodayCheckIns = allBookings.Count(b => b.CheckInDate.Date == today &&
                    b.Status == BookingStatus.Confirmed),
                TodayCheckOuts = allBookings.Count(b => b.CheckOutDate.Date == today &&
                    b.Status == BookingStatus.CheckedIn),
                CurrentGuests = allBookings.Count(b => b.Status == BookingStatus.CheckedIn),
                AverageOccupancyRate = await CalculatePlatformOccupancyRateAsync(today.AddDays(-30), today)
            };

            // Use financial summary data if available
            if (financialSummaryResult.IsSuccess)
            {
                var financialSummary = financialSummaryResult.Value;
                stats.MonthRevenue = financialSummary.TotalRevenue;
                stats.PendingPayments = financialSummary.PendingRevenue;
                stats.RevenueGrowthRate = financialSummary.RevenueGrowth;
                stats.BookingGrowthRate = financialSummary.BookingGrowth;
            }

            // Use revenue overview data if available
            if (revenueOverviewResult.IsSuccess)
            {
                var revenueOverview = revenueOverviewResult.Value;
                stats.TodayRevenue = revenueOverview.TotalRevenue; // Approximate
                stats.WeekRevenue = revenueOverview.TotalRevenue;   // Approximate
                stats.YearRevenue = allBookings.Where(b => b.CreatedAt.Year == today.Year &&
                    b.Status == BookingStatus.Completed).Sum(b => b.TotalPrice);
                stats.TotalRevenue = allBookings.Where(b => b.Status == BookingStatus.Completed)
                    .Sum(b => b.TotalPrice);
            }

            // Use registration requests data if available
            if (registrationRequestsResult.IsSuccess)
            {
                var regRequests = registrationRequestsResult.Value;
                stats.PendingRegistrations = regRequests.PendingRequests;
                stats.ApprovedThisMonth = regRequests.ApprovedRequests;
                stats.RejectedThisMonth = regRequests.RejectedRequests;
            }

            // Use growth metrics for user growth rate
            if (growthMetricsResult.IsSuccess)
            {
                var growthMetrics = growthMetricsResult.Value;
                stats.UserGrowthRate = growthMetrics.Summary.UserGrowthRate;
            }

            // Top Departments from department performance
            var topDepartments = new List<DepartmentSummary>();
            if (departmentPerformanceResult.IsSuccess)
            {
                topDepartments = departmentPerformanceResult.Value.Select(dp => new DepartmentSummary
                {
                    Id = dp.DepartmentId,
                    Name = dp.DepartmentName,
                    Country = dp.Country,
                    ImageUrl = null,
                    TotalUnits = dp.TotalUnits,
                    ActiveUnits = dp.ActiveUnits,
                    TotalBookings = dp.TotalBookings,
                    Revenue = dp.TotalRevenue,
                    AverageRating = dp.AverageRating,
                    OccupancyRate = dp.OccupancyRate
                }).ToList();
            }

            // Top Units
            var topUnitsList = new List<TopPerformingUnitSummary>();
            if (topUnitsResult.IsSuccess)
            {
                topUnitsList = topUnitsResult.Value.Select(u => new TopPerformingUnitSummary
                {
                    UnitId = u.UnitId,
                    UnitName = u.UnitName,
                    DepartmentName = u.DepartmentName,
                    UnitType = u.UnitType,
                    Revenue = u.TotalRevenue,
                    BookingCount = u.TotalBookings,
                    Rating = u.AverageRating,
                    OccupancyRate = u.OccupancyRate
                }).ToList();
            }

            // Revenue Overview
            var revenueOverviewData = new PlatformRevenueOverview();
            if (revenueOverviewResult.IsSuccess)
            {
                var revData = revenueOverviewResult.Value;
                revenueOverviewData = new PlatformRevenueOverview
                {
                    TodayRevenue = stats.TodayRevenue,
                    WeekRevenue = stats.WeekRevenue,
                    MonthRevenue = stats.MonthRevenue,
                    YearRevenue = stats.YearRevenue,
                    TotalRevenue = stats.TotalRevenue,
                    PendingPayments = stats.PendingPayments,
                    RefundedAmount = financialSummaryResult.IsSuccess ?
                        financialSummaryResult.Value.TotalRefunded : 0,
                    MonthlyTrend = revData.DailyRevenue.GroupBy(d => new { d.Date.Year, d.Date.Month })
                        .Select(g => new MonthlyRevenueData
                        {
                            Month = $"{g.Key.Year}-{g.Key.Month:D2}",
                            Revenue = g.Sum(x => x.Revenue),
                            BookingCount = g.Sum(x => x.BookingCount),
                            AverageBookingValue = g.Sum(x => x.Revenue) / g.Sum(x => x.BookingCount)
                        }).ToList(),
                    RevenueByDepartment = revData.RevenueByDepartment
                };
            }

            // Pending Registrations
            var pendingRegs = new List<PendingRegistrationSummary>();
            if (registrationRequestsResult.IsSuccess)
            {
                pendingRegs = registrationRequestsResult.Value.RecentRequests
                    .Where(r => r.Status == "Pending")
                    .Take(10)
                    .Select(r => new PendingRegistrationSummary
                    {
                        Id = r.Id,
                        UnitName = r.UnitName,
                        OwnerName = r.OwnerName,
                        DepartmentName = r.DepartmentName,
                        SubmittedAt = r.SubmittedAt,
                        DaysWaiting = r.DaysWaiting
                    }).ToList();
            }

            // Recent Activities (last 20)
            var recentActivities = new List<RecentActivityItem>();

            // Recent bookings
            var recentBookings = await dbcontext.Bookings
                .Include(b => b.Unit).ThenInclude(u => u.City)
                .Include(b => b.User)
                .OrderByDescending(b => b.CreatedAt)
                .Take(10)
                .ToListAsync();

            recentActivities.AddRange(recentBookings.Select(b => new RecentActivityItem
            {
                Id = b.Id,
                Type = "Booking",
                Title = $"New booking #{b.BookingNumber}",
                Description = $"Booking for {b.NumberOfGuests} guests, {b.NumberOfNights} nights",
                DepartmentName = b.Unit.City.Name,
                UnitName = b.Unit.Name,
                Timestamp = b.CreatedAt,
                UserName = b.User.FullName,
                Status = b.Status.ToString()
            }));

            // Recent reviews
            var recentReviews = await dbcontext.Reviews
                .Include(r => r.Unit).ThenInclude(u => u.City)
                .Include(r => r.User)
                .OrderByDescending(r => r.CreatedAt)
                .Take(10)
                .ToListAsync();

            recentActivities.AddRange(recentReviews.Select(r => new RecentActivityItem
            {
                Id = r.Id,
                Type = "Review",
                Title = $"{r.Rating}★ Review",
                Description = r.Comment?.Substring(0, Math.Min(100, r.Comment.Length)) ?? "",
                DepartmentName = r.Unit.City.Name,
                UnitName = r.Unit.Name,
                Timestamp = r.CreatedAt,
                UserName = r.User.FullName,
                Status = "Published"
            }));

            recentActivities = recentActivities.OrderByDescending(a => a.Timestamp).Take(20).ToList();

            // System Alerts
            var systemAlerts = GenerateSystemAlerts(stats, registrationRequestsResult.Value);

            var dashboard = new SuperAdminDashboardResponse
            {
                Platform = platformOverview,
                Stats = stats,
                TopDepartments = topDepartments,
                RecentActivities = recentActivities,
                TopUnits = topUnitsList,
                RevenueOverview = revenueOverviewData,
                PendingRegistrations = pendingRegs,
                SystemAlerts = systemAlerts
            };

            return Result.Success(dashboard);
        }
        catch (Exception ex)
        {
            return Result.Failure<SuperAdminDashboardResponse>(
                new Error("DashboardFailed", "Failed to load dashboard", 500));
        }
    }

    private List<AlertItem> GenerateSystemAlerts(
        SuperAdminDashboardStats stats,
        RegistrationRequestsOverviewResponse? regRequests)
    {
        var systemAlerts = new List<AlertItem>();
        var alertId = 1;

        // Alert for pending registrations
        if (stats.PendingRegistrations > 10)
        {
            systemAlerts.Add(new AlertItem
            {
                Id = alertId++,
                Type = "Warning",
                Title = "High Pending Registrations",
                Message = $"{stats.PendingRegistrations} registration requests pending review",
                Severity = "Medium",
                CreatedAt = DateTime.UtcNow.AddHours(3),
                IsResolved = false
            });
        }

        // Alert for low occupancy
        if (stats.AverageOccupancyRate < 50)
        {
            systemAlerts.Add(new AlertItem
            {
                Id = alertId++,
                Type = "Info",
                Title = "Low Platform Occupancy",
                Message = $"Average occupancy rate is {stats.AverageOccupancyRate:F1}%",
                Severity = "Low",
                CreatedAt = DateTime.UtcNow.AddHours(3),
                IsResolved = false
            });
        }

        // Alert for high cancellation rate
        var cancellationRate = stats.TotalBookings > 0 ?
            ((decimal)stats.CancelledBookings / stats.TotalBookings * 100) : 0;
        if (cancellationRate > 15)
        {
            systemAlerts.Add(new AlertItem
            {
                Id = alertId++,
                Type = "Warning",
                Title = "High Cancellation Rate",
                Message = $"Cancellation rate is {cancellationRate:F1}%",
                Severity = "High",
                CreatedAt = DateTime.UtcNow.AddHours(3),
                IsResolved = false
            });
        }

        // Alert for negative revenue growth
        if (stats.RevenueGrowthRate < -10)
        {
            systemAlerts.Add(new AlertItem
            {
                Id = alertId++,
                Type = "Error",
                Title = "Declining Revenue",
                Message = $"Revenue decreased by {Math.Abs(stats.RevenueGrowthRate):F1}% this month",
                Severity = "High",
                CreatedAt = DateTime.UtcNow.AddHours(3),
                IsResolved = false
            });
        }

        // Alert for long pending registration times
        if (regRequests != null && regRequests.AverageProcessingTimeHours > 72)
        {
            systemAlerts.Add(new AlertItem
            {
                Id = alertId++,
                Type = "Warning",
                Title = "Slow Registration Processing",
                Message = $"Average processing time is {regRequests.AverageProcessingTimeHours:F1} hours",
                Severity = "Medium",
                CreatedAt = DateTime.UtcNow.AddHours(3),
                IsResolved = false
            });
        }

        // Alert for high pending payments
        if (stats.PendingPayments > stats.MonthRevenue * 0.3m)
        {
            systemAlerts.Add(new AlertItem
            {
                Id = alertId++,
                Type = "Warning",
                Title = "High Pending Payments",
                Message = $"${stats.PendingPayments:N2} in pending payments",
                Severity = "Medium",
                CreatedAt = DateTime.UtcNow.AddHours(3),
                IsResolved = false
            });
        }

        return systemAlerts;
    }

    public async Task<Result<SuperAdminStatsResponse>> GetQuickStatsAsync(string userId)
    {
        try
        {
            var today = DateTime.UtcNow.AddHours(3).Date;
            var thisMonth = new DateTime(today.Year, today.Month, 1);
            var thisWeek = today.AddDays(-(int)today.DayOfWeek);

            var stats = new SuperAdminStatsResponse
            {
                // Departments
                TotalDepartments = await dbcontext.Departments.CountAsync(d => !d.IsDeleted),
                ActiveDepartments = await dbcontext.Departments.CountAsync(d => !d.IsDeleted && d.IsActive),
                InactiveDepartments = await dbcontext.Departments.CountAsync(d => !d.IsDeleted && !d.IsActive),

                // Units
                TotalUnits = await dbcontext.Units.CountAsync(u => !u.IsDeleted),
                ActiveUnits = await dbcontext.Units.CountAsync(u => !u.IsDeleted && u.IsActive),
                InactiveUnits = await dbcontext.Units.CountAsync(u => !u.IsDeleted && !u.IsActive),
                VerifiedUnits = await dbcontext.Units.CountAsync(u => !u.IsDeleted && u.IsVerified),
                UnverifiedUnits = await dbcontext.Units.CountAsync(u => !u.IsDeleted && !u.IsVerified),
                FeaturedUnits = await dbcontext.Units.CountAsync(u => !u.IsDeleted && u.IsFeatured),
                TotalSubUnits = await dbcontext.SubUnits.CountAsync(s => !s.IsDeleted),
                AvailableSubUnits = await dbcontext.SubUnits.CountAsync(s => !s.IsDeleted && s.IsAvailable),

                // Users
                TotalUsers = await dbcontext.Users.CountAsync(),
                ActiveUsers = await dbcontext.Users.CountAsync(u => !u.IsDisable),
                TotalGuests = await dbcontext.Bookings.Select(b => b.UserId).Distinct().CountAsync(),
                TotalHotelAdmins = (await manager.GetUsersInRoleAsync("HotelAdmin")).Count,
                TotalCityAdmins = (await manager.GetUsersInRoleAsync("CityAdmin")).Count,
                NewUsersThisMonth = await dbcontext.Users.CountAsync(u => u.CreatedAt >= thisMonth),

                // Bookings
                TotalBookings = await dbcontext.Bookings.CountAsync(),
                ActiveBookings = await dbcontext.Bookings.CountAsync(b =>
                    b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.CheckedIn),
                CompletedBookings = await dbcontext.Bookings.CountAsync(b => b.Status == BookingStatus.Completed),
                CancelledBookings = await dbcontext.Bookings.CountAsync(b => b.Status == BookingStatus.Cancelled),
                PendingBookings = await dbcontext.Bookings.CountAsync(b => b.Status == BookingStatus.Pending),
                BookingsThisMonth = await dbcontext.Bookings.CountAsync(b => b.CreatedAt >= thisMonth),
                BookingsThisWeek = await dbcontext.Bookings.CountAsync(b => b.CreatedAt >= thisWeek),
                BookingsToday = await dbcontext.Bookings.CountAsync(b => b.CreatedAt.Date == today),

                // Revenue
                TotalRevenue = await dbcontext.Bookings
                    .Where(b => b.Status == BookingStatus.Completed).SumAsync(b => b.TotalPrice),
                MonthRevenue = await dbcontext.Bookings
                    .Where(b => b.CreatedAt >= thisMonth && b.Status == BookingStatus.Completed)
                    .SumAsync(b => b.TotalPrice),
                WeekRevenue = await dbcontext.Bookings
                    .Where(b => b.CreatedAt >= thisWeek && b.Status == BookingStatus.Completed)
                    .SumAsync(b => b.TotalPrice),
                TodayRevenue = await dbcontext.Bookings
                    .Where(b => b.CreatedAt.Date == today && b.Status == BookingStatus.Completed)
                    .SumAsync(b => b.TotalPrice),
                PendingRevenue = await dbcontext.Bookings
                    .Where(b => b.PaymentStatus == PaymentStatus.Pending ||
                        b.PaymentStatus == PaymentStatus.PartiallyPaid)
                    .SumAsync(b => b.TotalPrice - b.PaidAmount),
                RefundedAmount = await dbcontext.Bookings
                    .Where(b => b.Status == BookingStatus.Cancelled)
                    .SumAsync(b => b.PaidAmount)
            };

            var totalBookingsForAvg = await dbcontext.Bookings
                .Where(b => b.Status == BookingStatus.Completed).CountAsync();
            stats.AverageBookingValue = totalBookingsForAvg > 0 ?
                stats.TotalRevenue / totalBookingsForAvg : 0;

            // Reviews
            stats.TotalReviews = await dbcontext.Reviews.CountAsync();
            stats.AveragePlatformRating = await dbcontext.Units
                .Where(u => !u.IsDeleted && u.TotalReviews > 0)
                .AverageAsync(u => (decimal?)u.AverageRating) ?? 0;
            stats.ReviewsThisMonth = await dbcontext.Reviews
                .CountAsync(r => r.CreatedAt >= thisMonth);

            // Occupancy
            stats.AverageOccupancyRate = await CalculatePlatformOccupancyRateAsync(
                today.AddDays(-30), today);
            stats.CurrentOccupancyRate = await CalculatePlatformOccupancyRateAsync(today, today.AddDays(1));

            // Registration Requests
            stats.TotalRegistrationRequests = await dbcontext.Set<UnitRegistrationRequest>().CountAsync();
            stats.PendingRegistrations = await dbcontext.Set<UnitRegistrationRequest>()
                .CountAsync(r => r.Status == RegistrationRequestStatus.Pending);
            stats.ApprovedRegistrations = await dbcontext.Set<UnitRegistrationRequest>()
                .CountAsync(r => r.Status == RegistrationRequestStatus.Approved);
            stats.RejectedRegistrations = await dbcontext.Set<UnitRegistrationRequest>()
                .CountAsync(r => r.Status == RegistrationRequestStatus.Rejected);

            // Coupons & Offers
            stats.ActiveCoupons = await dbcontext.Coupons.CountAsync(c => c.IsActive);
            stats.ActiveOffers = await dbcontext.Offers.CountAsync(o => o.IsActive && !o.IsDeleted);
            stats.ActiveAds = await dbcontext.Ads.CountAsync(a => a.IsActive && !a.IsDeleted);

            return Result.Success(stats);
        }
        catch (Exception ex)
        {
            return Result.Failure<SuperAdminStatsResponse>(
                new Error("StatsFailed", "Failed to retrieve statistics", 500));
        }
    }

    public async Task<Result<PlatformRevenueOverviewResponse>> GetPlatformRevenueOverviewAsync(
        string userId,
        DateTime? startDate = null,
        DateTime? endDate = null)
    {
        try
        {
            var start = startDate ?? DateTime.UtcNow.AddHours(3).AddMonths(-1).Date;
            var end = endDate ?? DateTime.UtcNow.AddHours(3).Date;

            var bookings = await dbcontext.Bookings
                .Include(b => b.Unit).ThenInclude(u => u.City)
                .Include(b => b.Unit).ThenInclude(u => u.UnitType)
                .Where(b => b.CreatedAt >= start && b.CreatedAt <= end &&
                    b.Status == BookingStatus.Completed)
                .ToListAsync();

            var totalRevenue = bookings.Sum(b => b.TotalPrice);
            var days = (end - start).Days;
            var avgPerDay = days > 0 ? totalRevenue / days : 0;

            var dailyRevenue = bookings
                .GroupBy(b => b.CreatedAt.Date)
                .Select(g => new DailyRevenueData
                {
                    Date = g.Key,
                    Revenue = g.Sum(b => b.TotalPrice),
                    BookingCount = g.Count()
                })
                .OrderBy(d => d.Date)
                .ToList();

            var revenueByDept = bookings
                .GroupBy(b => new { b.Unit.CityId, b.Unit.City.Name })
                .Select(g => new DepartmentRevenueData
                {
                    DepartmentId = g.Key.CityId,
                    DepartmentName = g.Key.Name,
                    Revenue = g.Sum(b => b.TotalPrice),
                    BookingCount = g.Count()
                })
                .OrderByDescending(d => d.Revenue)
                .ToList();

            foreach (var dept in revenueByDept)
            {
                dept.Percentage = totalRevenue > 0 ? (dept.Revenue / totalRevenue * 100) : 0;
            }

            var revenueByType = bookings
                .GroupBy(b => b.Unit.UnitType.Name)
                .Select(g => new UnitTypeRevenueData
                {
                    UnitType = g.Key,
                    Revenue = g.Sum(b => b.TotalPrice),
                    BookingCount = g.Count()
                })
                .OrderByDescending(u => u.Revenue)
                .ToList();

            foreach (var type in revenueByType)
            {
                type.Percentage = totalRevenue > 0 ? (type.Revenue / totalRevenue * 100) : 0;
            }

            var payments = await dbcontext.Set<Payment>()
                .Include(p => p.Booking)
                .Where(p => p.Booking.CreatedAt >= start && p.Booking.CreatedAt <= end)
                .ToListAsync();

            var paymentMethods = new PaymentMethodBreakdown
            {
                CreditCard = payments.Where(p => p.PaymentMethod == PaymentMethod.CreditCard)
                    .Sum(p => p.Amount),
                BankTransfer = payments.Where(p => p.PaymentMethod == PaymentMethod.BankTransfer)
                    .Sum(p => p.Amount),
                Cash = payments.Where(p => p.PaymentMethod == PaymentMethod.Cash)
                    .Sum(p => p.Amount),
                Other = 0
            };

            var response = new PlatformRevenueOverviewResponse
            {
                StartDate = start,
                EndDate = end,
                TotalRevenue = totalRevenue,
                AverageRevenuePerDay = avgPerDay,
                AverageRevenuePerBooking = bookings.Count > 0 ? totalRevenue / bookings.Count : 0,
                TotalBookings = bookings.Count,
                DailyRevenue = dailyRevenue,
                RevenueByDepartment = revenueByDept,
                RevenueByUnitType = revenueByType,
                PaymentMethods = paymentMethods
            };

            return Result.Success(response);
        }
        catch (Exception ex)
        {
            return Result.Failure<PlatformRevenueOverviewResponse>(
                new Error("RevenueFailed", "Failed to retrieve revenue overview", 500));
        }
    }

    public async Task<Result<IEnumerable<DepartmentPerformanceResponse>>> GetDepartmentPerformanceAsync(
        string userId,
        int? topN = null)
    {
        try
        {
            var departments = await dbcontext.Departments
                .Include(d => d.Units.Where(u => !u.IsDeleted))
                    .ThenInclude(u => u.Bookings)
                .Where(d => !d.IsDeleted)
                .ToListAsync();

            var thisMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
            var lastMonth = thisMonth.AddMonths(-1);

            var performances = departments.Select(d =>
            {
                var allBookings = d.Units.SelectMany(u => u.Bookings).ToList();
                var thisMonthBookings = allBookings.Count(b => b.CreatedAt >= thisMonth);
                var lastMonthBookings = allBookings.Count(b => b.CreatedAt >= lastMonth &&
                    b.CreatedAt < thisMonth);

                var thisMonthRevenue = allBookings.Where(b => b.CreatedAt >= thisMonth &&
                    b.Status == BookingStatus.Completed).Sum(b => b.TotalPrice);
                var lastMonthRevenue = allBookings.Where(b => b.CreatedAt >= lastMonth &&
                    b.CreatedAt < thisMonth && b.Status == BookingStatus.Completed).Sum(b => b.TotalPrice);

                return new DepartmentPerformanceResponse
                {
                    DepartmentId = d.Id,
                    DepartmentName = d.Name,
                    Country = d.Country,
                    TotalUnits = d.Units.Count,
                    ActiveUnits = d.Units.Count(u => u.IsActive),
                    TotalBookings = allBookings.Count,
                    TotalRevenue = allBookings.Where(b => b.Status == BookingStatus.Completed)
                        .Sum(b => b.TotalPrice),
                    AverageRating = d.AverageRating ?? 0,
                    OccupancyRate = 0, // Calculate if needed
                    RevenueGrowth = lastMonthRevenue > 0 ?
                        ((thisMonthRevenue - lastMonthRevenue) / lastMonthRevenue * 100) : 0,
                    BookingGrowth = lastMonthBookings > 0 ?
                        ((decimal)(thisMonthBookings - lastMonthBookings) / lastMonthBookings * 100) : 0
                };
            })
            .OrderByDescending(p => p.TotalRevenue)
            .ToList();

            // Assign ranks
            for (int i = 0; i < performances.Count; i++)
            {
                performances[i].Rank = i + 1;
            }

            var result = topN.HasValue ? performances.Take(topN.Value) : performances;
            return Result.Success<IEnumerable<DepartmentPerformanceResponse>>(result);
        }
        catch (Exception ex)
        {
            return Result.Failure<IEnumerable<DepartmentPerformanceResponse>>(
                new Error("PerformanceFailed", "Failed to retrieve department performance", 500));
        }
    }

    public async Task<Result<PlatformGrowthMetricsResponse>> GetPlatformGrowthMetricsAsync(
        string userId,
        int months = 12)
    {
        try
        {
            var today = DateTime.UtcNow.AddHours(3).Date;
            var startDate = today.AddMonths(-months);

            var monthlyData = new List<MonthlyGrowthData>();

            for (int i = 0; i < months; i++)
            {
                var monthStart = today.AddMonths(-i).AddDays(-today.Day + 1);
                var monthEnd = monthStart.AddMonths(1);

                var newUsers = await dbcontext.Users
                    .CountAsync(u => u.CreatedAt >= monthStart && u.CreatedAt < monthEnd);

                var newUnits = await dbcontext.Units
                    .CountAsync(u => u.CreatedAt >= monthStart && u.CreatedAt < monthEnd && !u.IsDeleted);

                var newBookings = await dbcontext.Bookings
                    .CountAsync(b => b.CreatedAt >= monthStart && b.CreatedAt < monthEnd);

                var revenue = await dbcontext.Bookings
                    .Where(b => b.CreatedAt >= monthStart && b.CreatedAt < monthEnd &&
                        b.Status == BookingStatus.Completed)
                    .SumAsync(b => b.TotalPrice);

                var activeUsers = await dbcontext.Bookings
                    .Where(b => b.CreatedAt >= monthStart && b.CreatedAt < monthEnd)
                    .Select(b => b.UserId)
                    .Distinct()
                    .CountAsync();

                monthlyData.Add(new MonthlyGrowthData
                {
                    Month = monthStart.ToString("yyyy-MM"),
                    NewUsers = newUsers,
                    NewUnits = newUnits,
                    NewBookings = newBookings,
                    Revenue = revenue,
                    ActiveUsers = activeUsers
                });
            }

            monthlyData.Reverse();

            // Calculate growth rates
            var summary = new GrowthSummary();
            if (monthlyData.Count >= 2)
            {
                var current = monthlyData[^1];
                var previous = monthlyData[^2];

                summary.UserGrowthRate = previous.NewUsers > 0 ?
                    ((decimal)(current.NewUsers - previous.NewUsers) / previous.NewUsers * 100) : 0;
                summary.UnitGrowthRate = previous.NewUnits > 0 ?
                    ((decimal)(current.NewUnits - previous.NewUnits) / previous.NewUnits * 100) : 0;
                summary.BookingGrowthRate = previous.NewBookings > 0 ?
                    ((decimal)(current.NewBookings - previous.NewBookings) / previous.NewBookings * 100) : 0;
                summary.RevenueGrowthRate = previous.Revenue > 0 ?
                    ((current.Revenue - previous.Revenue) / previous.Revenue * 100) : 0;
            }

            var yearStart = new DateTime(today.Year, 1, 1);
            summary.TotalNewUsersThisYear = await dbcontext.Users
                .CountAsync(u => u.CreatedAt >= yearStart);
            summary.TotalNewUnitsThisYear = await dbcontext.Units
                .CountAsync(u => u.CreatedAt >= yearStart && !u.IsDeleted);

            var response = new PlatformGrowthMetricsResponse
            {
                MonthlyGrowth = monthlyData,
                Summary = summary
            };

            return Result.Success(response);
        }
        catch (Exception ex)
        {
            return Result.Failure<PlatformGrowthMetricsResponse>(
                new Error("GrowthFailed", "Failed to retrieve growth metrics", 500));
        }
    }

    public async Task<Result<UserEngagementStatsResponse>> GetUserEngagementStatsAsync(string userId)
    {
        try
        {
            var today = DateTime.UtcNow.AddHours(3).Date;
            var last30Days = today.AddDays(-30);
            var last7Days = today.AddDays(-7);

            var totalUsers = await dbcontext.Users.CountAsync();
            var activeUsers30 = await dbcontext.Bookings
                .Where(b => b.CreatedAt >= last30Days)
                .Select(b => b.UserId)
                .Distinct()
                .CountAsync();

            var activeUsers7 = await dbcontext.Bookings
                .Where(b => b.CreatedAt >= last7Days)
                .Select(b => b.UserId)
                .Distinct()
                .CountAsync();

            var activeUsersToday = await dbcontext.Bookings
                .Where(b => b.CreatedAt.Date == today)
                .Select(b => b.UserId)
                .Distinct()
                .CountAsync();

            var userBookingCounts = await dbcontext.Bookings
                .GroupBy(b => b.UserId)
                .Select(g => new { UserId = g.Key, Count = g.Count() })
                .ToListAsync();

            var returningCustomers = userBookingCounts.Count(u => u.Count > 1);
            var totalGuests = userBookingCounts.Count;

            var activityTrend = new List<UserActivityData>();
            for (int i = 0; i < 30; i++)
            {
                var date = today.AddDays(-i);
                var activeUsersOnDate = await dbcontext.Bookings
                    .Where(b => b.CreatedAt.Date == date)
                    .Select(b => b.UserId)
                    .Distinct()
                    .CountAsync();

                var newUsersOnDate = await dbcontext.Users
                    .CountAsync(u => u.CreatedAt.Date == date);

                var bookingsOnDate = await dbcontext.Bookings
                    .CountAsync(b => b.CreatedAt.Date == date);

                activityTrend.Add(new UserActivityData
                {
                    Date = date,
                    ActiveUsers = activeUsersOnDate,
                    NewUsers = newUsersOnDate,
                    BookingsMade = bookingsOnDate
                });
            }

            activityTrend.Reverse();

            var response = new UserEngagementStatsResponse
            {
                TotalRegisteredUsers = totalUsers,
                ActiveUsersLast30Days = activeUsers30,
                ActiveUsersLast7Days = activeUsers7,
                ActiveUsersToday = activeUsersToday,
                DailyActiveUserRate = totalUsers > 0 ? ((decimal)activeUsersToday / totalUsers * 100) : 0,
                WeeklyActiveUserRate = totalUsers > 0 ? ((decimal)activeUsers7 / totalUsers * 100) : 0,
                MonthlyActiveUserRate = totalUsers > 0 ? ((decimal)activeUsers30 / totalUsers * 100) : 0,
                AverageBookingsPerUser = totalGuests > 0 ?
                    (int)Math.Round((decimal)await dbcontext.Bookings.CountAsync() / totalGuests) : 0,
                ReturningCustomers = returningCustomers,
                ReturnCustomerRate = totalGuests > 0 ? ((decimal)returningCustomers / totalGuests * 100) : 0,
                UserActivityTrend = activityTrend
            };

            return Result.Success(response);
        }
        catch (Exception ex)
        {
            return Result.Failure<UserEngagementStatsResponse>(
                new Error("EngagementFailed", "Failed to retrieve user engagement stats", 500));
        }
    }

    public async Task<Result<PlatformBookingTrendsResponse>> GetPlatformBookingTrendsAsync(
        string userId,
        DateTime startDate,
        DateTime endDate)
    {
        try
        {
            var bookings = await dbcontext.Bookings
                .Where(b => b.CreatedAt >= startDate && b.CreatedAt <= endDate)
                .ToListAsync();

            var days = (endDate - startDate).Days;
            var avgPerDay = days > 0 ? (decimal)bookings.Count / days : 0;

            var dailyTrend = bookings
                .GroupBy(b => b.CreatedAt.Date)
                .Select(g => new DailyBookingData
                {
                    Date = g.Key,
                    BookingCount = g.Count(),
                    Revenue = g.Where(b => b.Status == BookingStatus.Completed).Sum(b => b.TotalPrice),
                    AverageBookingValue = g.Count() > 0 ?
                        g.Where(b => b.Status == BookingStatus.Completed).Sum(b => b.TotalPrice) / g.Count() : 0
                })
                .OrderBy(d => d.Date)
                .ToList();

            var monthlyTrend = bookings
                .GroupBy(b => new { b.CreatedAt.Year, b.CreatedAt.Month })
                .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
                .Select((g, index) => new MonthlyBookingData
                {
                    Month = $"{g.Key.Year}-{g.Key.Month:D2}",
                    BookingCount = g.Count(),
                    Revenue = g.Where(b => b.Status == BookingStatus.Completed).Sum(b => b.TotalPrice),
                    AverageBookingValue = g.Count() > 0 ?
                        g.Where(b => b.Status == BookingStatus.Completed).Sum(b => b.TotalPrice) / g.Count() : 0,
                    GrowthRate = 0 // Calculate based on previous month
                })
                .ToList();

            // Calculate growth rates
            for (int i = 1; i < monthlyTrend.Count; i++)
            {
                var current = monthlyTrend[i];
                var previous = monthlyTrend[i - 1];
                current.GrowthRate = previous.BookingCount > 0 ?
                    ((decimal)(current.BookingCount - previous.BookingCount) / previous.BookingCount * 100) : 0;
            }

            var statusBreakdown = new BookingStatusBreakdown
            {
                Pending = bookings.Count(b => b.Status == BookingStatus.Pending),
                Confirmed = bookings.Count(b => b.Status == BookingStatus.Confirmed),
                CheckedIn = bookings.Count(b => b.Status == BookingStatus.CheckedIn),
                Completed = bookings.Count(b => b.Status == BookingStatus.Completed),
                Cancelled = bookings.Count(b => b.Status == BookingStatus.Cancelled)
            };

            var response = new PlatformBookingTrendsResponse
            {
                StartDate = startDate,
                EndDate = endDate,
                TotalBookings = bookings.Count,
                AverageBookingsPerDay = avgPerDay,
                DailyTrend = dailyTrend,
                MonthlyTrend = monthlyTrend,
                Sources = new BookingSourceBreakdown(), // Populate if you track sources
                StatusBreakdown = statusBreakdown
            };

            return Result.Success(response);
        }
        catch (Exception ex)
        {
            return Result.Failure<PlatformBookingTrendsResponse>(
                new Error("TrendsFailed", "Failed to retrieve booking trends", 500));
        }
    }

    public async Task<Result<IEnumerable<TopPerformingUnitResponse>>> GetTopPerformingUnitsAsync(
        string userId,
        int topN = 10)
    {
        try
        {
            var units = await dbcontext.Units
                .Include(u => u.City)
                .Include(u => u.UnitType)
                .Include(u => u.Bookings)
                .Where(u => !u.IsDeleted)
                .ToListAsync();

            var performances = units.Select(u =>
            {
                var completedBookings = u.Bookings.Where(b => b.Status == BookingStatus.Completed).ToList();
                return new TopPerformingUnitResponse
                {
                    UnitId = u.Id,
                    UnitName = u.Name,
                    DepartmentId = u.CityId,
                    DepartmentName = u.City.Name,
                    UnitType = u.UnitType.Name,
                    TotalRevenue = completedBookings.Sum(b => b.TotalPrice),
                    TotalBookings = u.Bookings.Count,
                    AverageRating = u.AverageRating,
                    TotalReviews = u.TotalReviews,
                    OccupancyRate = 0, // Calculate if needed
                    AverageBookingValue = completedBookings.Any() ?
                        completedBookings.Average(b => b.TotalPrice) : 0,
                    IsVerified = u.IsVerified,
                    IsFeatured = u.IsFeatured
                };
            })
            .OrderByDescending(u => u.TotalRevenue)
            .Take(topN)
            .ToList();

            // Assign ranks
            for (int i = 0; i < performances.Count; i++)
            {
                performances[i].Rank = i + 1;
            }

            return Result.Success<IEnumerable<TopPerformingUnitResponse>>(performances);
        }
        catch (Exception ex)
        {
            return Result.Failure<IEnumerable<TopPerformingUnitResponse>>(
                new Error("TopUnitsFailed", "Failed to retrieve top performing units", 500));
        }
    }

    public async Task<Result<RegistrationRequestsOverviewResponse>> GetRegistrationRequestsOverviewAsync(
        string userId)
    {
        try
        {
            var today = DateTime.UtcNow.AddHours(3).Date;
            var thisMonth = new DateTime(today.Year, today.Month, 1);
            var thisWeek = today.AddDays(-(int)today.DayOfWeek);

            var allRequests = await dbcontext.Set<UnitRegistrationRequest>()
                .Include(r => r.Department)
                .ToListAsync();

            var processingTimes = allRequests
                .Where(r => r.ReviewedAt.HasValue)
                .Select(r => (r.ReviewedAt.Value - r.SubmittedAt).TotalHours)
                .ToList();

            var avgProcessingTime = processingTimes.Any() ? processingTimes.Average() : 0;

            var requestsByDept = allRequests
                .GroupBy(r => new { r.DepartmentId, r.Department.Name })
                .Select(g => new DepartmentRequestData
                {
                    DepartmentId = g.Key.DepartmentId,
                    DepartmentName = g.Key.Name,
                    TotalRequests = g.Count(),
                    PendingRequests = g.Count(r => r.Status == RegistrationRequestStatus.Pending),
                    ApprovedRequests = g.Count(r => r.Status == RegistrationRequestStatus.Approved),
                    RejectedRequests = g.Count(r => r.Status == RegistrationRequestStatus.Rejected)
                })
                .OrderByDescending(d => d.TotalRequests)
                .ToList();

            var recentRequests = allRequests
                .OrderByDescending(r => r.SubmittedAt)
                .Take(20)
                .Select(r => new RecentRegistrationRequest
                {
                    Id = r.Id,
                    UnitName = r.UnitName,
                    OwnerName = r.OwnerFullName,
                    DepartmentName = r.Department.Name,
                    Status = r.Status.ToString(),
                    SubmittedAt = r.SubmittedAt,
                    ReviewedAt = r.ReviewedAt,
                    DaysWaiting = r.ReviewedAt.HasValue ?
                        (int)(r.ReviewedAt.Value - r.SubmittedAt).TotalDays :
                        (int)(DateTime.UtcNow - r.SubmittedAt).TotalDays
                })
                .ToList();

            var response = new RegistrationRequestsOverviewResponse
            {
                TotalRequests = allRequests.Count,
                PendingRequests = allRequests.Count(r => r.Status == RegistrationRequestStatus.Pending),
                ApprovedRequests = allRequests.Count(r => r.Status == RegistrationRequestStatus.Approved),
                RejectedRequests = allRequests.Count(r => r.Status == RegistrationRequestStatus.Rejected),
                RequestsThisMonth = allRequests.Count(r => r.SubmittedAt >= thisMonth),
                RequestsThisWeek = allRequests.Count(r => r.SubmittedAt >= thisWeek),
                AverageProcessingTimeHours = (decimal)avgProcessingTime,
                RequestsByDepartment = requestsByDept,
                RecentRequests = recentRequests
            };

            return Result.Success(response);
        }
        catch (Exception ex)
        {
            return Result.Failure<RegistrationRequestsOverviewResponse>(
                new Error("RequestsOverviewFailed", "Failed to retrieve requests overview", 500));
        }
    }

    public async Task<Result<PlatformFinancialSummaryResponse>> GetPlatformFinancialSummaryAsync(
        string userId,
        DateTime? startDate = null,
        DateTime? endDate = null)
    {
        try
        {
            var start = startDate ?? DateTime.UtcNow.AddHours(3).AddMonths(-1).Date;
            var end = endDate ?? DateTime.UtcNow.AddHours(3).Date;

            var bookings = await dbcontext.Bookings
                .Include(b => b.Unit).ThenInclude(u => u.City)
                .Where(b => b.CreatedAt >= start && b.CreatedAt <= end)
                .ToListAsync();

            var totalRevenue = bookings.Where(b => b.Status == BookingStatus.Completed)
                .Sum(b => b.TotalPrice);
            var completedRevenue = totalRevenue;
            var pendingRevenue = bookings.Where(b => b.Status == BookingStatus.Pending ||
                b.Status == BookingStatus.Confirmed).Sum(b => b.TotalPrice);

            var totalRefunded = bookings.Where(b => b.Status == BookingStatus.Cancelled)
                .Sum(b => b.PaidAmount);
            var totalCancellations = bookings.Count(b => b.Status == BookingStatus.Cancelled);
            var cancellationRate = bookings.Count > 0 ?
                ((decimal)totalCancellations / bookings.Count * 100) : 0;

            var fullyPaid = bookings.Where(b => b.PaymentStatus == PaymentStatus.Paid)
                .Sum(b => b.TotalPrice);
            var partiallyPaid = bookings.Where(b => b.PaymentStatus == PaymentStatus.PartiallyPaid)
                .Sum(b => b.PaidAmount);
            var unpaid = bookings.Where(b => b.PaymentStatus == PaymentStatus.Pending)
                .Sum(b => b.TotalPrice);

            var avgBookingValue = bookings.Count > 0 ? totalRevenue / bookings.Count : 0;
            var totalUnits = await dbcontext.Units.CountAsync(u => !u.IsDeleted);
            var avgRevenuePerUnit = totalUnits > 0 ? totalRevenue / totalUnits : 0;
            var totalDepts = await dbcontext.Departments.CountAsync(d => !d.IsDeleted);
            var avgRevenuePerDept = totalDepts > 0 ? totalRevenue / totalDepts : 0;

            // Calculate growth
            var prevStart = start.AddMonths(-1);
            var prevEnd = start;
            var prevRevenue = await dbcontext.Bookings
                .Where(b => b.CreatedAt >= prevStart && b.CreatedAt < prevEnd &&
                    b.Status == BookingStatus.Completed)
                .SumAsync(b => b.TotalPrice);
            var revenueGrowth = prevRevenue > 0 ? ((totalRevenue - prevRevenue) / prevRevenue * 100) : 0;

            var prevBookingCount = await dbcontext.Bookings
                .CountAsync(b => b.CreatedAt >= prevStart && b.CreatedAt < prevEnd);
            var bookingGrowth = prevBookingCount > 0 ?
                ((decimal)(bookings.Count - prevBookingCount) / prevBookingCount * 100) : 0;

            // Top departments
            var topDepts = bookings
                .GroupBy(b => new { b.Unit.CityId, b.Unit.City.Name })
                .Select(g => new TopRevenueDepartment
                {
                    DepartmentId = g.Key.CityId,
                    DepartmentName = g.Key.Name,
                    Revenue = g.Where(b => b.Status == BookingStatus.Completed).Sum(b => b.TotalPrice)
                })
                .OrderByDescending(d => d.Revenue)
                .Take(10)
                .ToList();

            foreach (var dept in topDepts)
            {
                dept.Percentage = totalRevenue > 0 ? (dept.Revenue / totalRevenue * 100) : 0;
            }

            // Top units
            var topUnits = bookings
                .GroupBy(b => new resonseforthesuperadmin(b.UnitId, b.Unit.Name, b.Unit.City.Name))
                .Select(g => new TopRevenueUnit
                {
                    UnitId = g.Key.UnitId,
                    UnitName = g.Key.Name,
                    DepartmentName = g.Key.Name,
                    Revenue = g.Where(b => b.Status == BookingStatus.Completed).Sum(b => b.TotalPrice)
                })
                .OrderByDescending(u => u.Revenue)
                .Take(10)
                .ToList();

            foreach (var unit in topUnits)
            {
                unit.Percentage = totalRevenue > 0 ? (unit.Revenue / totalRevenue * 100) : 0;
            }

            var response = new PlatformFinancialSummaryResponse
            {
                StartDate = start,
                EndDate = end,
                TotalRevenue = totalRevenue,
                CompletedBookingsRevenue = completedRevenue,
                PendingRevenue = pendingRevenue,
                TotalRefunded = totalRefunded,
                TotalCancellations = totalCancellations,
                CancellationRate = cancellationRate,
                FullyPaidAmount = fullyPaid,
                PartiallyPaidAmount = partiallyPaid,
                UnpaidAmount = unpaid,
                AverageBookingValue = avgBookingValue,
                AverageRevenuePerUnit = avgRevenuePerUnit,
                AverageRevenuePerDepartment = avgRevenuePerDept,
                RevenueGrowth = revenueGrowth,
                BookingGrowth = bookingGrowth,
                TopDepartments = topDepts,
                TopUnits = topUnits
            };

            return Result.Success(response);
        }
        catch (Exception ex)
        {
            return Result.Failure<PlatformFinancialSummaryResponse>(
                new Error("FinancialSummaryFailed", "Failed to retrieve financial summary", 500));
        }
    }

    #endregion

    #region HELPER METHODS

    private async Task<decimal> CalculatePlatformOccupancyRateAsync(
        DateTime startDate,
        DateTime endDate)
    {
        var totalRoomNights = await dbcontext.SubUnits
            .Where(s => !s.IsDeleted)
            .CountAsync() * (endDate - startDate).Days;

        if (totalRoomNights == 0)
            return 0;

        var occupiedNights = await dbcontext.BookingRooms
            .Include(br => br.Booking)
            .Where(br => br.Booking.CheckInDate < endDate &&
                        br.Booking.CheckOutDate > startDate &&
                        br.Booking.Status != BookingStatus.Cancelled)
            .SumAsync(br => br.NumberOfNights);

        return (decimal)occupiedNights / totalRoomNights * 100;
    }

    #endregion


    #region PUBLIC ENDPOINTS

    public async Task<Result<IEnumerable<CityAdminResponse>>> GetAllCityAdminsAsync()
    {
        try
        {
            var cityAdmins = await dbcontext.Set<DepartmentAdmin>()
                .Include(da => da.User)
                .Include(da => da.City)
                .Where(da => da.IsActive && !da.City.IsDeleted)
                .OrderBy(da => da.City.Name)
                .ThenBy(da => da.IsPrimary ? 0 : 1) // Primary admins first
                .Select(da => new CityAdminResponse(
                    da.UserId,
                    da.User.FullName ?? "N/A",
                    da.User.Email ?? "N/A",
                    da.User.PhoneNumber,
                    da.CityId,
                    da.City.Name,
                    da.City.Country,
                    da.IsPrimary,
                    da.IsActive,
                    da.AssignedAt
                ))
                .ToListAsync();

            return Result.Success<IEnumerable<CityAdminResponse>>(cityAdmins);
        }
        catch (Exception ex)
        {
            return Result.Failure<IEnumerable<CityAdminResponse>>(
                new Error("CityAdminsFailed", "Failed to retrieve city admins", 500));
        }
    }

    #endregion



}

internal record resonseforthesuperadmin(int UnitId, string Name, string Item);

internal record resonseforsuper(int UnitId, string Name, string Item);
