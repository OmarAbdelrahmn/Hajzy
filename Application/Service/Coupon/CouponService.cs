using Application.Abstraction;
using Application.Contracts;
using Domain;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Application.Service.PromoCode;

public class CouponService(
    ApplicationDbcontext context,
    ILogger<CouponService> logger) : ICouponService
{
    private readonly ApplicationDbcontext _context = context;
    private readonly ILogger<CouponService> _logger = logger;

    #region CRUD OPERATIONS

    public async Task<Result<CouponResponse>> CreateCouponAsync(CreateCouponRequest request, string userId)
    {
        try
        {
            // Validate coupon code is unique
            var existingCoupon = await _context.Set<Coupon>()
                .FirstOrDefaultAsync(c => c.Code.ToUpper() == request.Code.ToUpper());

            if (existingCoupon != null)
                return Result.Failure<CouponResponse>(
                    new Error("DuplicateCode", "A coupon with this code already exists", 400));

            // Validate dates
            if (request.ValidFrom >= request.ValidUntil)
                return Result.Failure<CouponResponse>(
                    new Error("InvalidDates", "Valid until date must be after valid from date", 400));

            // Validate discount amount
            if (request.DiscountAmount <= 0)
                return Result.Failure<CouponResponse>(
                    new Error("InvalidDiscount", "Discount amount must be greater than zero", 400));

            if (request.Type == CouponType.Percentage && request.DiscountAmount > 100)
                return Result.Failure<CouponResponse>(
                    new Error("InvalidPercentage", "Percentage discount cannot exceed 100%", 400));

            // Validate targeting
            if (request.TargetUnitId.HasValue)
            {
                var unitExists = await _context.Units
                    .AnyAsync(u => u.Id == request.TargetUnitId.Value && !u.IsDeleted);
                if (!unitExists)
                    return Result.Failure<CouponResponse>(
                        new Error("InvalidUnit", "Target unit not found", 404));
            }

            if (request.TargetCityId.HasValue)
            {
                var cityExists = await _context.Set<Domain.Entities.Department>()
                    .AnyAsync(d => d.Id == request.TargetCityId.Value && !d.IsDeleted);
                if (!cityExists)
                    return Result.Failure<CouponResponse>(
                        new Error("InvalidCity", "Target city not found", 404));
            }

            if (request.TargetUnitTypeId.HasValue)
            {
                var typeExists = await _context.Set<Domain.Entities.UnitType>()
                    .AnyAsync(t => t.Id == request.TargetUnitTypeId.Value);
                if (!typeExists)
                    return Result.Failure<CouponResponse>(
                        new Error("InvalidUnitType", "Target unit type not found", 404));
            }

            // Create coupon
            var coupon = new Coupon
            {
                Code = request.Code.ToUpper(),
                Description = request.Description,
                Type = request.Type,
                DiscountAmount = request.DiscountAmount,
                MinimumSpend = request.MinimumSpend,
                MaximumDiscount = request.MaximumDiscount,
                MaxUsageCount = request.MaxUsageCount,
                MaxUsagePerUser = request.MaxUsagePerUser,
                ValidFrom = request.ValidFrom,
                ValidUntil = request.ValidUntil,
                TargetUnitId = request.TargetUnitId,
                TargetCityId = request.TargetCityId,
                TargetUnitTypeId = request.TargetUnitTypeId,
                IsActive = request.IsActive,
                CreatedBy = userId,
                CreatedAt = DateTime.UtcNow.AddHours(3)
            };

            await _context.Set<Coupon>().AddAsync(coupon);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Coupon {CouponCode} created by user {UserId}",
                coupon.Code, userId);

            var response = await MapToResponseAsync(coupon);
            return Result.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating coupon");
            return Result.Failure<CouponResponse>(
                new Error("CreateFailed", "Failed to create coupon", 500));
        }
    }

    public async Task<Result<CouponResponse>> UpdateCouponAsync(
        int couponId,
        UpdateCouponRequest request,
        string userId)
    {
        try
        {
            var coupon = await _context.Set<Coupon>()
                .FirstOrDefaultAsync(c => c.Id == couponId);

            if (coupon == null)
                return Result.Failure<CouponResponse>(
                    new Error("NotFound", "Coupon not found", 404));

            // Update code if provided and different
            if (!string.IsNullOrEmpty(request.Code) && request.Code.ToUpper() != coupon.Code)
            {
                var existingCoupon = await _context.Set<Coupon>()
                    .FirstOrDefaultAsync(c => c.Code.ToUpper() == request.Code.ToUpper() && c.Id != couponId);

                if (existingCoupon != null)
                    return Result.Failure<CouponResponse>(
                        new Error("DuplicateCode", "A coupon with this code already exists", 400));

                coupon.Code = request.Code.ToUpper();
            }

            // Update fields if provided
            if (request.Description != null)
                coupon.Description = request.Description;

            if (request.Type.HasValue)
                coupon.Type = request.Type.Value;

            if (request.DiscountAmount.HasValue)
            {
                if (request.DiscountAmount.Value <= 0)
                    return Result.Failure<CouponResponse>(
                        new Error("InvalidDiscount", "Discount amount must be greater than zero", 400));

                if (request.Type == CouponType.Percentage && request.DiscountAmount.Value > 100)
                    return Result.Failure<CouponResponse>(
                        new Error("InvalidPercentage", "Percentage discount cannot exceed 100%", 400));

                coupon.DiscountAmount = request.DiscountAmount.Value;
            }

            if (request.MinimumSpend.HasValue)
                coupon.MinimumSpend = request.MinimumSpend.Value;

            if (request.MaximumDiscount.HasValue)
                coupon.MaximumDiscount = request.MaximumDiscount.Value;

            if (request.MaxUsageCount.HasValue)
                coupon.MaxUsageCount = request.MaxUsageCount.Value;

            if (request.MaxUsagePerUser.HasValue)
                coupon.MaxUsagePerUser = request.MaxUsagePerUser.Value;

            if (request.ValidFrom.HasValue)
                coupon.ValidFrom = request.ValidFrom.Value;

            if (request.ValidUntil.HasValue)
            {
                if (coupon.ValidFrom >= request.ValidUntil.Value)
                    return Result.Failure<CouponResponse>(
                        new Error("InvalidDates", "Valid until date must be after valid from date", 400));

                coupon.ValidUntil = request.ValidUntil.Value;
            }

            if (request.TargetUnitId.HasValue)
                coupon.TargetUnitId = request.TargetUnitId.Value;

            if (request.TargetCityId.HasValue)
                coupon.TargetCityId = request.TargetCityId.Value;

            if (request.TargetUnitTypeId.HasValue)
                coupon.TargetUnitTypeId = request.TargetUnitTypeId.Value;

            if (request.IsActive.HasValue)
                coupon.IsActive = request.IsActive.Value;

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Coupon {CouponId} updated by user {UserId}",
                couponId, userId);

            var response = await MapToResponseAsync(coupon);
            return Result.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating coupon {CouponId}", couponId);
            return Result.Failure<CouponResponse>(
                new Error("UpdateFailed", "Failed to update coupon", 500));
        }
    }

    public async Task<Result> DeleteCouponAsync(int couponId)
    {
        try
        {
            var coupon = await _context.Set<Coupon>()
                .FirstOrDefaultAsync(c => c.Id == couponId);

            if (coupon == null)
                return Result.Failure(new Error("NotFound", "Coupon not found", 404));

            // Check if coupon has been used
            var hasUsage = await _context.Set<BookingCoupon>()
                .AnyAsync(bc => bc.CouponId == couponId);

            if (hasUsage)
            {
                // Soft delete - just deactivate
                coupon.IsActive = false;
                _logger.LogInformation("Coupon {CouponId} deactivated (has usage history)", couponId);
            }
            else
            {
                // Hard delete if never used
                _context.Set<Coupon>().Remove(coupon);
                _logger.LogInformation("Coupon {CouponId} deleted (no usage history)", couponId);
            }

            await _context.SaveChangesAsync();
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting coupon {CouponId}", couponId);
            return Result.Failure(new Error("DeleteFailed", "Failed to delete coupon", 500));
        }
    }

    public async Task<Result<CouponDetailsResponse>> GetCouponByIdAsync(int couponId)
    {
        try
        {
            var coupon = await _context.Set<Coupon>()
                .Include(c => c.TargetUnit)
                .Include(c => c.TargetCity)
                .Include(c => c.TargetUnitType)
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == couponId);

            if (coupon == null)
                return Result.Failure<CouponDetailsResponse>(
                    new Error("NotFound", "Coupon not found", 404));

            var response = await MapToDetailsResponseAsync(coupon);
            return Result.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting coupon {CouponId}", couponId);
            return Result.Failure<CouponDetailsResponse>(
                new Error("GetFailed", "Failed to retrieve coupon", 500));
        }
    }

    public async Task<Result<CouponDetailsResponse>> GetCouponByCodeAsync(string code)
    {
        try
        {
            var coupon = await _context.Set<Coupon>()
                .Include(c => c.TargetUnit)
                .Include(c => c.TargetCity)
                .Include(c => c.TargetUnitType)
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Code.ToUpper() == code.ToUpper());

            if (coupon == null)
                return Result.Failure<CouponDetailsResponse>(
                    new Error("NotFound", "Coupon not found", 404));

            var response = await MapToDetailsResponseAsync(coupon);
            return Result.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting coupon by code {Code}", code);
            return Result.Failure<CouponDetailsResponse>(
                new Error("GetFailed", "Failed to retrieve coupon", 500));
        }
    }

    public async Task<Result<PagedCouponResponse>> GetCouponsAsync(CouponFilter filter)
    {
        try
        {
            var query = _context.Set<Coupon>()
                .Include(c => c.TargetUnit)
                .Include(c => c.TargetCity)
                .Include(c => c.TargetUnitType)
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(filter.Code))
                query = query.Where(c => c.Code.Contains(filter.Code.ToUpper()));

            if (filter.Type.HasValue)
                query = query.Where(c => c.Type == filter.Type.Value);

            if (filter.IsActive.HasValue)
                query = query.Where(c => c.IsActive == filter.IsActive.Value);

            if (filter.IsExpired.HasValue)
            {
                var now = DateTime.UtcNow;
                if (filter.IsExpired.Value)
                    query = query.Where(c => c.ValidUntil < now);
                else
                    query = query.Where(c => c.ValidUntil >= now);
            }

            if (filter.TargetUnitId.HasValue)
                query = query.Where(c => c.TargetUnitId == filter.TargetUnitId.Value);

            if (filter.TargetCityId.HasValue)
                query = query.Where(c => c.TargetCityId == filter.TargetCityId.Value);

            if (filter.TargetUnitTypeId.HasValue)
                query = query.Where(c => c.TargetUnitTypeId == filter.TargetUnitTypeId.Value);

            if (filter.ValidFrom.HasValue)
                query = query.Where(c => c.ValidFrom >= filter.ValidFrom.Value);

            if (filter.ValidUntil.HasValue)
                query = query.Where(c => c.ValidUntil <= filter.ValidUntil.Value);

            // Apply sorting
            query = filter.SortBy?.ToLower() switch
            {
                "code" => filter.SortDescending
                    ? query.OrderByDescending(c => c.Code)
                    : query.OrderBy(c => c.Code),
                "usage" => filter.SortDescending
                    ? query.OrderByDescending(c => c.CurrentUsageCount)
                    : query.OrderBy(c => c.CurrentUsageCount),
                "discount" => filter.SortDescending
                    ? query.OrderByDescending(c => c.DiscountAmount)
                    : query.OrderBy(c => c.DiscountAmount),
                "expiry" => filter.SortDescending
                    ? query.OrderByDescending(c => c.ValidUntil)
                    : query.OrderBy(c => c.ValidUntil),
                _ => filter.SortDescending
                    ? query.OrderByDescending(c => c.CreatedAt)
                    : query.OrderBy(c => c.CreatedAt)
            };

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)filter.PageSize);

            var coupons = await query
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .AsNoTracking()
                .ToListAsync();

            var couponResponses = new List<CouponResponse>();
            foreach (var coupon in coupons)
            {
                couponResponses.Add(await MapToResponseAsync(coupon));
            }

            var response = new PagedCouponResponse
            {
                Coupons = couponResponses,
                TotalCount = totalCount,
                Page = filter.Page,
                PageSize = filter.PageSize,
                TotalPages = totalPages
            };

            return Result.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting coupons");
            return Result.Failure<PagedCouponResponse>(
                new Error("GetFailed", "Failed to retrieve coupons", 500));
        }
    }

    #endregion

    #region VALIDATION & APPLICATION

    public async Task<Result<CouponValidationResponse>> ValidateCouponAsync(ValidateCouponRequest request)
    {
        try
        {
            var coupon = await _context.Set<Coupon>()
                .Include(c => c.TargetUnit)
                .Include(c => c.TargetCity)
                .Include(c => c.TargetUnitType)
                .FirstOrDefaultAsync(c => c.Code.ToUpper() == request.CouponCode.ToUpper());

            if (coupon == null)
            {
                return Result.Success(new CouponValidationResponse
                {
                    IsValid = false,
                    ErrorMessage = "Coupon code not found"
                });
            }

            // Check if active
            if (!coupon.IsActive)
            {
                return Result.Success(new CouponValidationResponse
                {
                    IsValid = false,
                    ErrorMessage = "This coupon is no longer active"
                });
            }

            // Check expiry
            var now = DateTime.UtcNow;
            if (now < coupon.ValidFrom)
            {
                return Result.Success(new CouponValidationResponse
                {
                    IsValid = false,
                    ErrorMessage = $"This coupon is not valid until {coupon.ValidFrom:MMM dd, yyyy}"
                });
            }

            if (now > coupon.ValidUntil)
            {
                return Result.Success(new CouponValidationResponse
                {
                    IsValid = false,
                    ErrorMessage = "This coupon has expired"
                });
            }

            // Check usage limits
            if (coupon.MaxUsageCount.HasValue && coupon.CurrentUsageCount >= coupon.MaxUsageCount.Value)
            {
                return Result.Success(new CouponValidationResponse
                {
                    IsValid = false,
                    ErrorMessage = "This coupon has reached its usage limit"
                });
            }

            // Check per-user usage limit
            if (coupon.MaxUsagePerUser.HasValue)
            {
                var userUsageCount = await _context.Set<BookingCoupon>()
                    .Include(bc => bc.Booking)
                    .CountAsync(bc => bc.CouponId == coupon.Id &&
                                     bc.Booking.UserId == request.UserId);

                if (userUsageCount >= coupon.MaxUsagePerUser.Value)
                {
                    return Result.Success(new CouponValidationResponse
                    {
                        IsValid = false,
                        ErrorMessage = "You have already used this coupon the maximum number of times"
                    });
                }
            }

            // Check minimum spend
            if (coupon.MinimumSpend.HasValue && request.BookingAmount < coupon.MinimumSpend.Value)
            {
                return Result.Success(new CouponValidationResponse
                {
                    IsValid = false,
                    ErrorMessage = $"Minimum booking amount of {coupon.MinimumSpend.Value:C} required"
                });
            }

            // Check targeting
            if (coupon.TargetUnitId.HasValue && coupon.TargetUnitId.Value != request.UnitId)
            {
                return Result.Success(new CouponValidationResponse
                {
                    IsValid = false,
                    ErrorMessage = "This coupon is not valid for the selected property"
                });
            }

            if (coupon.TargetCityId.HasValue && coupon.TargetCityId.Value != request.CityId)
            {
                return Result.Success(new CouponValidationResponse
                {
                    IsValid = false,
                    ErrorMessage = "This coupon is not valid for properties in this city"
                });
            }

            if (coupon.TargetUnitTypeId.HasValue && coupon.TargetUnitTypeId.Value != request.UnitTypeId)
            {
                return Result.Success(new CouponValidationResponse
                {
                    IsValid = false,
                    ErrorMessage = "This coupon is not valid for this property type"
                });
            }

            // Calculate discount
            var discount = CalculateDiscount(coupon, request.BookingAmount);
            var finalPrice = request.BookingAmount - discount;

            var couponResponse = await MapToResponseAsync(coupon);

            return Result.Success(new CouponValidationResponse
            {
                IsValid = true,
                DiscountAmount = discount,
                FinalPrice = finalPrice,
                Coupon = couponResponse
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating coupon {CouponCode}", request.CouponCode);
            return Result.Failure<CouponValidationResponse>(
                new Error("ValidationFailed", "Failed to validate coupon", 500));
        }
    }

    public async Task<Result<ApplyCouponResponse>> CalculateCouponDiscountAsync(ApplyCouponRequest request)
    {
        var validation = await ValidateCouponAsync(new ValidateCouponRequest(
            request.CouponCode,
            request.BookingAmount,
            request.UnitId,
            request.CityId,
            request.UnitTypeId,
            request.UserId,
            null,
            null
        ));

        if (!validation.IsSuccess)
            return Result.Failure<ApplyCouponResponse>(validation.Error);

        if (!validation.Value.IsValid)
            return Result.Failure<ApplyCouponResponse>(
                new Error("InvalidCoupon", validation.Value.ErrorMessage ?? "Coupon is not valid", 400));

        var response = new ApplyCouponResponse
        {
            CouponId = validation.Value.Coupon!.Id,
            CouponCode = validation.Value.Coupon.Code,
            OriginalPrice = request.BookingAmount,
            DiscountAmount = validation.Value.DiscountAmount,
            FinalPrice = validation.Value.FinalPrice,
            DiscountType = validation.Value.Coupon.Type
        };

        return Result.Success(response);
    }

    public async Task<Result<ApplyCouponResponse>> ApplyCouponToBookingAsync(
        int bookingId,
        string couponCode,
        decimal originalPrice,
        string userId)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            // Get booking
            var booking = await _context.Bookings
                .Include(b => b.Unit)
                .FirstOrDefaultAsync(b => b.Id == bookingId);

            if (booking == null)
                return Result.Failure<ApplyCouponResponse>(
                    new Error("NotFound", "Booking not found", 404));

            // Check if booking already has a coupon
            var existingCoupon = await _context.Set<BookingCoupon>()
                .FirstOrDefaultAsync(bc => bc.BookingId == bookingId);

            if (existingCoupon != null)
                return Result.Failure<ApplyCouponResponse>(
                    new Error("CouponExists", "This booking already has a coupon applied", 400));

            // Validate coupon
            var validation = await ValidateCouponAsync(new ValidateCouponRequest(
                couponCode,
                originalPrice,
                booking.UnitId,
                booking.Unit.CityId,
                booking.Unit.UnitTypeId,
                userId,
                booking.CheckInDate,
                booking.CheckOutDate
            ));

            if (!validation.IsSuccess || !validation.Value.IsValid)
                return Result.Failure<ApplyCouponResponse>(
                    new Error("InvalidCoupon",
                        validation.Value.ErrorMessage ?? "Coupon is not valid", 400));

            var coupon = await _context.Set<Coupon>()
                .FirstOrDefaultAsync(c => c.Code.ToUpper() == couponCode.ToUpper());

            if (coupon == null)
                return Result.Failure<ApplyCouponResponse>(
                    new Error("NotFound", "Coupon not found", 404));

            // Apply coupon to booking
            var bookingCoupon = new BookingCoupon
            {
                BookingId = bookingId,
                CouponId = coupon.Id,
                DiscountApplied = validation.Value.DiscountAmount,
                AppliedAt = DateTime.UtcNow.AddHours(3)
            };

            await _context.Set<BookingCoupon>().AddAsync(bookingCoupon);

            // Update booking price
            booking.TotalPrice = validation.Value.FinalPrice;

            // Update coupon usage
            coupon.CurrentUsageCount++;

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            _logger.LogInformation(
                "Coupon {CouponCode} applied to booking {BookingId}. Discount: {Discount}",
                couponCode, bookingId, validation.Value.DiscountAmount);

            var response = new ApplyCouponResponse
            {
                CouponId = coupon.Id,
                CouponCode = coupon.Code,
                OriginalPrice = originalPrice,
                DiscountAmount = validation.Value.DiscountAmount,
                FinalPrice = validation.Value.FinalPrice,
                DiscountType = coupon.Type.ToString()
            };

            return Result.Success(response);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error applying coupon to booking {BookingId}", bookingId);
            return Result.Failure<ApplyCouponResponse>(
                new Error("ApplyFailed", "Failed to apply coupon to booking", 500));
        }
    }

    public async Task<Result> RemoveCouponFromBookingAsync(int bookingId)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var bookingCoupon = await _context.Set<BookingCoupon>()
                .Include(bc => bc.Coupon)
                .Include(bc => bc.Booking)
                .FirstOrDefaultAsync(bc => bc.BookingId == bookingId);

            if (bookingCoupon == null)
                return Result.Failure(
                    new Error("NotFound", "No coupon applied to this booking", 404));

            var booking = bookingCoupon.Booking;
            var coupon = bookingCoupon.Coupon;

            // Restore original price
            booking.TotalPrice += bookingCoupon.DiscountApplied;

            // Decrease usage count
            if (coupon.CurrentUsageCount > 0)
                coupon.CurrentUsageCount--;

            // Remove booking coupon
            _context.Set<BookingCoupon>().Remove(bookingCoupon);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            _logger.LogInformation(
                "Coupon removed from booking {BookingId}",
                bookingId);

            return Result.Success();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error removing coupon from booking {BookingId}", bookingId);
            return Result.Failure(
                new Error("RemoveFailed", "Failed to remove coupon from booking", 500));
        }
    }

    #endregion

    #region USER OPERATIONS

    public async Task<Result<List<CouponResponse>>> GetAvailableCouponsForUserAsync(
        string userId,
        int? unitId = null,
        int? cityId = null)
    {
        try
        {
            var now = DateTime.UtcNow;
            var query = _context.Set<Coupon>()
                .Include(c => c.TargetUnit)
                .Include(c => c.TargetCity)
                .Include(c => c.TargetUnitType)
                .Where(c => c.IsActive &&
                           c.ValidFrom <= now &&
                           c.ValidUntil >= now &&
                           (!c.MaxUsageCount.HasValue || c.CurrentUsageCount < c.MaxUsageCount.Value))
                .AsQueryable();

            // Filter by targeting
            if (unitId.HasValue)
                query = query.Where(c => !c.TargetUnitId.HasValue || c.TargetUnitId == unitId.Value);

            if (cityId.HasValue)
                query = query.Where(c => !c.TargetCityId.HasValue || c.TargetCityId == cityId.Value);

            var coupons = await query
                .OrderByDescending(c => c.DiscountAmount)
                .AsNoTracking()
                .ToListAsync();

            // Filter by per-user usage limit
            var validCoupons = new List<Coupon>();
            foreach (var coupon in coupons)
            {
                if (coupon.MaxUsagePerUser.HasValue)
                {
                    var userUsageCount = await _context.Set<BookingCoupon>()
                        .Include(bc => bc.Booking)
                        .CountAsync(bc => bc.CouponId == coupon.Id &&
                                         bc.Booking.UserId == userId);

                    if (userUsageCount >= coupon.MaxUsagePerUser.Value)
                        continue;
                }

                validCoupons.Add(coupon);
            }

            var responses = new List<CouponResponse>();
            foreach (var coupon in validCoupons)
            {
                responses.Add(await MapToResponseAsync(coupon));
            }

            return Result.Success(responses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting available coupons for user {UserId}", userId);
            return Result.Failure<List<CouponResponse>>(
                new Error("GetFailed", "Failed to retrieve available coupons", 500));
        }
    }

    public async Task<Result<List<CouponUsageInfo>>> GetUserCouponHistoryAsync(string userId)
    {
        try
        {
            var usages = await _context.Set<BookingCoupon>()
                .Include(bc => bc.Coupon)
                .Include(bc => bc.Booking)
                    .ThenInclude(b => b.User)
                .Where(bc => bc.Booking.UserId == userId)
                .OrderByDescending(bc => bc.AppliedAt)
                .AsNoTracking()
                .ToListAsync();

            var responses = usages.Select(bc => new CouponUsageInfo
            {
                BookingId = bc.BookingId,
                BookingNumber = bc.Booking.BookingNumber,
                UserName = bc.Booking.User?.FullName ?? "Unknown",
                DiscountApplied = bc.DiscountApplied,
                AppliedAt = bc.AppliedAt
            }).ToList();

            return Result.Success(responses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting coupon history for user {UserId}", userId);
            return Result.Failure<List<CouponUsageInfo>>(
                new Error("GetFailed", "Failed to retrieve coupon history", 500));
        }
    }

    public async Task<Result<int>> GetUserCouponUsageCountAsync(string userId, string couponCode)
    {
        try
        {
            var count = await _context.Set<BookingCoupon>()
                .Include(bc => bc.Coupon)
                .Include(bc => bc.Booking)
                .CountAsync(bc => bc.Coupon.Code.ToUpper() == couponCode.ToUpper() &&
                                 bc.Booking.UserId == userId);

            return Result.Success(count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error getting usage count for coupon {CouponCode} and user {UserId}",
                couponCode, userId);
            return Result.Failure<int>(
                new Error("GetFailed", "Failed to retrieve usage count", 500));
        }
    }

    #endregion

    #region ADMIN/STATISTICS

    public async Task<Result<CouponStatistics>> GetCouponStatisticsAsync(int couponId)
    {
        try
        {
            var coupon = await _context.Set<Coupon>()
                .FirstOrDefaultAsync(c => c.Id == couponId);

            if (coupon == null)
                return Result.Failure<CouponStatistics>(
                    new Error("NotFound", "Coupon not found", 404));

            var usages = await _context.Set<BookingCoupon>()
                .Include(bc => bc.Booking)
                .Where(bc => bc.CouponId == couponId)
                .AsNoTracking()
                .ToListAsync();

            var statistics = new CouponStatistics
            {
                TotalUsageCount = usages.Count,
                TotalDiscountGiven = usages.Sum(bc => bc.DiscountApplied),
                AverageDiscountPerBooking = usages.Any()
                    ? usages.Average(bc => bc.DiscountApplied)
                    : 0,
                UniqueUsersCount = usages.Select(bc => bc.Booking.UserId).Distinct().Count(),
                ConversionRate = 0, // TODO: Calculate based on views vs usage
                UsageByMonth = usages
                    .GroupBy(bc => new { bc.AppliedAt.Year, bc.AppliedAt.Month })
                    .ToDictionary(
                        g => $"{g.Key.Year}-{g.Key.Month:D2}",
                        g => g.Count())
            };

            return Result.Success(statistics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting statistics for coupon {CouponId}", couponId);
            return Result.Failure<CouponStatistics>(
                new Error("GetFailed", "Failed to retrieve coupon statistics", 500));
        }
    }

    public async Task<Result<CouponReportResponse>> GetCouponReportAsync(CouponReportRequest request)
    {
        try
        {
            var query = _context.Set<Coupon>()
                .Include(c => c.BookingCoupons)
                    .ThenInclude(bc => bc.Booking)
                .AsQueryable();

            // Apply filters
            if (request.StartDate.HasValue)
                query = query.Where(c => c.CreatedAt >= request.StartDate.Value);

            if (request.EndDate.HasValue)
                query = query.Where(c => c.CreatedAt <= request.EndDate.Value);

            if (request.UnitId.HasValue)
                query = query.Where(c => c.TargetUnitId == request.UnitId.Value ||
                                        !c.TargetUnitId.HasValue);

            if (request.CityId.HasValue)
                query = query.Where(c => c.TargetCityId == request.CityId.Value ||
                                        !c.TargetCityId.HasValue);

            if (request.Type.HasValue)
                query = query.Where(c => c.Type == request.Type.Value);

            var coupons = await query.AsNoTracking().ToListAsync();

            var now = DateTime.UtcNow;
            var allUsages = coupons.SelectMany(c => c.BookingCoupons).ToList();

            var report = new CouponReportResponse
            {
                TotalCoupons = coupons.Count,
                ActiveCoupons = coupons.Count(c => c.IsActive && c.ValidUntil >= now),
                ExpiredCoupons = coupons.Count(c => c.ValidUntil < now),
                TotalUsageCount = allUsages.Count,
                TotalDiscountGiven = allUsages.Sum(bc => bc.DiscountApplied),
                TotalRevenueGenerated = allUsages
                    .Sum(bc => bc.Booking?.TotalPrice ?? 0),
                AverageDiscountPerCoupon = coupons.Any()
                    ? allUsages.Sum(bc => bc.DiscountApplied) / coupons.Count
                    : 0,
                TopPerformingCoupons = coupons
                    .OrderByDescending(c => c.CurrentUsageCount)
                    .Take(10)
                    .Select(c => new TopCouponInfo
                    {
                        Code = c.Code,
                        UsageCount = c.CurrentUsageCount,
                        TotalDiscount = c.BookingCoupons.Sum(bc => bc.DiscountApplied)
                    })
                    .ToList(),
                DiscountByMonth = allUsages
                    .GroupBy(bc => new { bc.AppliedAt.Year, bc.AppliedAt.Month })
                    .ToDictionary(
                        g => $"{g.Key.Year}-{g.Key.Month:D2}",
                        g => g.Sum(bc => bc.DiscountApplied))
            };

            return Result.Success(report);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating coupon report");
            return Result.Failure<CouponReportResponse>(
                new Error("ReportFailed", "Failed to generate coupon report", 500));
        }
    }

    public async Task<Result> ToggleCouponStatusAsync(int couponId, bool isActive)
    {
        try
        {
            var coupon = await _context.Set<Coupon>()
                .FirstOrDefaultAsync(c => c.Id == couponId);

            if (coupon == null)
                return Result.Failure(new Error("NotFound", "Coupon not found", 404));

            coupon.IsActive = isActive;
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Coupon {CouponId} status toggled to {Status}",
                couponId, isActive ? "Active" : "Inactive");

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling coupon status {CouponId}", couponId);
            return Result.Failure(
                new Error("ToggleFailed", "Failed to toggle coupon status", 500));
        }
    }

    public async Task<Result<List<CouponResponse>>> GetExpiringCouponsAsync(int days = 7)
    {
        try
        {
            var expiryDate = DateTime.UtcNow.AddDays(days);
            var coupons = await _context.Set<Coupon>()
                .Include(c => c.TargetUnit)
                .Include(c => c.TargetCity)
                .Include(c => c.TargetUnitType)
                .Where(c => c.IsActive &&
                           c.ValidUntil >= DateTime.UtcNow &&
                           c.ValidUntil <= expiryDate)
                .OrderBy(c => c.ValidUntil)
                .AsNoTracking()
                .ToListAsync();

            var responses = new List<CouponResponse>();
            foreach (var coupon in coupons)
            {
                responses.Add(await MapToResponseAsync(coupon));
            }

            return Result.Success(responses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting expiring coupons");
            return Result.Failure<List<CouponResponse>>(
                new Error("GetFailed", "Failed to retrieve expiring coupons", 500));
        }
    }

    public async Task<Result<List<CouponResponse>>> GetTopPerformingCouponsAsync(int count = 10)
    {
        try
        {
            var coupons = await _context.Set<Coupon>()
                .Include(c => c.TargetUnit)
                .Include(c => c.TargetCity)
                .Include(c => c.TargetUnitType)
                .OrderByDescending(c => c.CurrentUsageCount)
                .Take(count)
                .AsNoTracking()
                .ToListAsync();

            var responses = new List<CouponResponse>();
            foreach (var coupon in coupons)
            {
                responses.Add(await MapToResponseAsync(coupon));
            }

            return Result.Success(responses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting top performing coupons");
            return Result.Failure<List<CouponResponse>>(
                new Error("GetFailed", "Failed to retrieve top performing coupons", 500));
        }
    }

    #endregion

    #region HELPER METHODS

    private decimal CalculateDiscount(Coupon coupon, decimal bookingAmount)
    {
        decimal discount = 0;

        if (coupon.Type == CouponType.Percentage)
        {
            discount = bookingAmount * (coupon.DiscountAmount / 100);

            // Apply maximum discount cap if set
            if (coupon.MaximumDiscount.HasValue && discount > coupon.MaximumDiscount.Value)
                discount = coupon.MaximumDiscount.Value;
        }
        else if (coupon.Type == CouponType.FixedAmount)
        {
            discount = coupon.DiscountAmount;

            // Don't let discount exceed booking amount
            if (discount > bookingAmount)
                discount = bookingAmount;
        }

        return Math.Round(discount, 2);
    }

    private async Task<CouponResponse> MapToResponseAsync(Coupon coupon)
    {
        // Load related entities if not already loaded
        if (coupon.TargetUnit == null && coupon.TargetUnitId.HasValue)
        {
            coupon.TargetUnit = await _context.Units
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == coupon.TargetUnitId.Value);
        }

        if (coupon.TargetCity == null && coupon.TargetCityId.HasValue)
        {
            coupon.TargetCity = await _context.Set<Domain.Entities.Department>()
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.Id == coupon.TargetCityId.Value);
        }

        if (coupon.TargetUnitType == null && coupon.TargetUnitTypeId.HasValue)
        {
            coupon.TargetUnitType = await _context.Set<Domain.Entities.UnitType>()
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == coupon.TargetUnitTypeId.Value);
        }

        // Calculate total discount given
        var totalDiscount = await _context.Set<BookingCoupon>()
            .Where(bc => bc.CouponId == coupon.Id)
            .SumAsync(bc => bc.DiscountApplied);

        return new CouponResponse
        {
            Id = coupon.Id,
            Code = coupon.Code,
            Description = coupon.Description,
            Type = coupon.Type.ToString(),
            DiscountAmount = coupon.DiscountAmount,
            MinimumSpend = coupon.MinimumSpend,
            MaximumDiscount = coupon.MaximumDiscount,
            MaxUsageCount = coupon.MaxUsageCount,
            CurrentUsageCount = coupon.CurrentUsageCount,
            MaxUsagePerUser = coupon.MaxUsagePerUser,
            ValidFrom = coupon.ValidFrom,
            ValidUntil = coupon.ValidUntil,
            IsActive = coupon.IsActive,
            TargetUnitId = coupon.TargetUnitId,
            TargetUnitName = coupon.TargetUnit?.Name,
            TargetCityId = coupon.TargetCityId,
            TargetCityName = coupon.TargetCity?.Name,
            TargetUnitTypeId = coupon.TargetUnitTypeId,
            TargetUnitTypeName = coupon.TargetUnitType?.Name,
            TotalDiscountGiven = totalDiscount,
            CreatedBy = coupon.CreatedBy,
            CreatedAt = coupon.CreatedAt,
            IsExpired = DateTime.UtcNow > coupon.ValidUntil,
            IsValid = coupon.IsActive &&
                     DateTime.UtcNow >= coupon.ValidFrom &&
                     DateTime.UtcNow <= coupon.ValidUntil &&
                     (!coupon.MaxUsageCount.HasValue ||
                      coupon.CurrentUsageCount < coupon.MaxUsageCount.Value)
        };
    }

    private async Task<CouponDetailsResponse> MapToDetailsResponseAsync(Coupon coupon)
    {
        var baseResponse = await MapToResponseAsync(coupon);

        // Get recent usages
        var recentUsages = await _context.Set<BookingCoupon>()
            .Include(bc => bc.Booking)
                .ThenInclude(b => b.User)
            .Where(bc => bc.CouponId == coupon.Id)
            .OrderByDescending(bc => bc.AppliedAt)
            .Take(10)
            .AsNoTracking()
            .ToListAsync();

        var usageInfos = recentUsages.Select(bc => new CouponUsageInfo
        {
            BookingId = bc.BookingId,
            BookingNumber = bc.Booking.BookingNumber,
            UserName = bc.Booking.User?.FullName ?? "Unknown",
            DiscountApplied = bc.DiscountApplied,
            AppliedAt = bc.AppliedAt
        }).ToList();

        // Get statistics
        var statisticsResult = await GetCouponStatisticsAsync(coupon.Id);
        var statistics = statisticsResult.IsSuccess
            ? statisticsResult.Value
            : new CouponStatistics();

        return new CouponDetailsResponse
        {
            Id = baseResponse.Id,
            Code = baseResponse.Code,
            Description = baseResponse.Description,
            Type = baseResponse.Type,
            DiscountAmount = baseResponse.DiscountAmount,
            MinimumSpend = baseResponse.MinimumSpend,
            MaximumDiscount = baseResponse.MaximumDiscount,
            MaxUsageCount = baseResponse.MaxUsageCount,
            CurrentUsageCount = baseResponse.CurrentUsageCount,
            MaxUsagePerUser = baseResponse.MaxUsagePerUser,
            ValidFrom = baseResponse.ValidFrom,
            ValidUntil = baseResponse.ValidUntil,
            IsActive = baseResponse.IsActive,
            TargetUnitId = baseResponse.TargetUnitId,
            TargetUnitName = baseResponse.TargetUnitName,
            TargetCityId = baseResponse.TargetCityId,
            TargetCityName = baseResponse.TargetCityName,
            TargetUnitTypeId = baseResponse.TargetUnitTypeId,
            TargetUnitTypeName = baseResponse.TargetUnitTypeName,
            TotalDiscountGiven = baseResponse.TotalDiscountGiven,
            CreatedBy = baseResponse.CreatedBy,
            CreatedAt = baseResponse.CreatedAt,
            UpdatedAt = baseResponse.UpdatedAt,
            IsExpired = baseResponse.IsExpired,
            IsValid = baseResponse.IsValid,
            RecentUsages = usageInfos,
            Statistics = statistics
        };
    }

    #endregion
}