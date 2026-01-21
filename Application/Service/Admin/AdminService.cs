using Application.Abstraction;
using Application.Abstraction.Errors;
using Application.Contracts.Admin;
using Application.Contracts.Bookin;
using Application.Contracts.Unit;
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

    public async Task<Result<IEnumerable<UserResponse>>> GetAllUsers()
    {
        var users = await (from u in dbcontext.Users
               join ur in dbcontext.UserRoles
               on u.Id equals ur.UserId
               join r in dbcontext.Roles
               on ur.RoleId equals r.Id into roles
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
                   u.LastLoginAt
               })
                  .GroupBy(x => new { x.Id, x.FullName, x.Address, x.Email, x.IsDisable , x.EmailConfirmed , x.PhoneNumber , x.AvatarUrl , x.Bio , x.DateOfBirth , x.Nationality , x.CreatedAt , x.LastLoginAt})
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
                      c.Key.LastLoginAt
                      ))
                  .ToListAsync();

        if(users.Count() == 0)
            return Result.Failure<IEnumerable<UserResponse>>(UserErrors.UserNotFound);

            return Result.Success<IEnumerable<UserResponse>>(users);
        
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
            user.LastLoginAt);

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

}
