using Application.Abstraction;
using Application.Abstraction.Consts;
using Application.Contracts.CityAdminContracts;
using Application.Helpers;
using Application.Notifications;
using Application.Service.Avilabilaties;
using Application.Service.Review;
using Application.Service.S3Image;
using Domain;
using Domain.Entities;
using Hangfire;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Application.Service.CityAdmin;

public class CityAdminService(
    ApplicationDbcontext context,
    UserManager<ApplicationUser> userManager,
    IEmailSender emailSender,
    ILogger<CityAdminService> logger,
    IHttpContextAccessor httpContextAccessor,
    IS3ImageService s3Service,
    INotinficationService emailNotificationService) : ICityAdminService
{
    private readonly ApplicationDbcontext _context = context;
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly IEmailSender _emailSender = emailSender;
    private readonly ILogger<CityAdminService> _logger = logger;
    private readonly IS3ImageService _s3Service = s3Service;
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
    private readonly INotinficationService _emailNotificationService = emailNotificationService;



    #region AVAILABILITY OVERVIEW

    public async Task<Result<CityAvailabilityOverviewResponse>> GetCityAvailabilityOverviewAsync(
        string userId,
        DateTime startDate,
        DateTime endDate)
    {
        try
        {
            var departmentId = await GetAdminDepartmentIdAsync(userId);
            if (!departmentId.IsSuccess)
                return Result.Failure<CityAvailabilityOverviewResponse>(departmentId.Error);

            var units = await _context.Units
                .Include(u => u.Rooms.Where(r => !r.IsDeleted))
                .Where(u => u.CityId == departmentId.Value && !u.IsDeleted)
                .ToListAsync();

            var totalSubUnits = units.Sum(u => u.Rooms.Count);
            var availableSubUnits = units.Sum(u => u.Rooms.Count(r => r.IsAvailable));

            // Get booked rooms in this period
            var unitIds = units.Select(u => u.Id).ToList();
            var bookedRooms = await _context.BookingRooms
                .Include(br => br.Booking)
                .Where(br => unitIds.Contains(br.Room.UnitId) &&
                    br.Booking.CheckInDate < endDate &&
                    br.Booking.CheckOutDate > startDate &&
                    br.Booking.Status != BookingStatus.Cancelled)
                .Select(br => br.RoomId)
                .Distinct()
                .CountAsync();

            var blockedSubUnits = totalSubUnits - availableSubUnits - bookedRooms;

            var response = new CityAvailabilityOverviewResponse
            {
                StartDate = startDate,
                EndDate = endDate,
                TotalSubUnits = totalSubUnits,
                AvailableSubUnits = availableSubUnits,
                BookedSubUnits = bookedRooms,
                BlockedSubUnits = Math.Max(0, blockedSubUnits),
                OverallOccupancyRate = totalSubUnits > 0 ? (decimal)bookedRooms / totalSubUnits * 100 : 0,
                UnitSummaries = units.Select(u => new UnitAvailabilitySummary
                {
                    UnitId = u.Id,
                    UnitName = u.Name,
                    TotalRooms = u.Rooms.Count,
                    AvailableRooms = u.Rooms.Count(r => r.IsAvailable),
                    OccupancyRate = 0 // Calculate specific occupancy per unit if needed
                }).ToList()
            };

            return Result.Success(response);
        }
        catch (Exception ex)
        {
            return Result.Failure<CityAvailabilityOverviewResponse>(
                new Error("GetAvailabilityFailed", "Failed to retrieve availability overview", 500));
        }
    }

    #endregion

    #region OFFERS & ADS MANAGEMENT

    public async Task<Result<PaginatedResponse<OfferResponse>>> GetCityOffersAsync(
       string userId,
       OfferFilter filter)
    {
        try
        {
            var departmentId = await GetAdminDepartmentIdAsync(userId);
            if (!departmentId.IsSuccess)
                return Result.Failure<PaginatedResponse<OfferResponse>>(departmentId.Error);

            var unitIds = await _context.Units
                .Where(u => u.CityId == departmentId.Value && !u.IsDeleted)
                .Select(u => u.Id)
                .ToListAsync();

            var query = _context.Offers
                .Include(o => o.Unit)
                .Where(o => o.UnitId.HasValue && unitIds.Contains(o.UnitId.Value))
                .AsQueryable();

            if (filter.UnitId.HasValue)
                query = query.Where(o => o.UnitId == filter.UnitId.Value);

            if (filter.IsActive.HasValue)
                query = query.Where(o => o.IsActive == filter.IsActive.Value);

            if (filter.IsFeatured.HasValue)
                query = query.Where(o => o.IsFeatured == filter.IsFeatured.Value);

            // ADD THIS: Get total count
            var totalCount = await query.CountAsync();

            var offers = await query
                .OrderByDescending(o => o.CreatedAt)
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(o => new OfferResponse
                {
                    Id = o.Id,
                    Title = o.Title,
                    Description = o.Description,
                    ImageUrl = o.ImageUrl,
                    UnitId = o.UnitId,
                    UnitName = o.Unit!.Name,
                    DiscountPercentage = o.DiscountPercentage,
                    DiscountAmount = o.DiscountAmount,
                    StartDate = o.StartDate,
                    EndDate = o.EndDate,
                    IsActive = o.IsActive,
                    IsFeatured = o.IsFeatured,
                    CreatedAt = o.CreatedAt
                })
                .ToListAsync();

            // CHANGE RETURN:
            var paginatedResult = CreatePaginatedResponse(offers, totalCount, filter.Page, filter.PageSize);
            return Result.Success(paginatedResult);
        }
        catch (Exception ex)
        {
            return Result.Failure<PaginatedResponse<OfferResponse>>(
                new Error("GetOffersFailed", "Failed to retrieve offers", 500));
        }
    }
    public async Task<Result<PaginatedResponse<AdResponse>>> GetCityAdsAsync(
       string userId,
       AdFilter filter)
    {
        try
        {
            var departmentId = await GetAdminDepartmentIdAsync(userId);
            if (!departmentId.IsSuccess)
                return Result.Failure<PaginatedResponse<AdResponse>>(departmentId.Error);

            var unitIds = await _context.Units
                .Where(u => u.CityId == departmentId.Value && !u.IsDeleted)
                .Select(u => u.Id)
                .ToListAsync();

            var query = _context.Ads
                .Include(a => a.Unit)
                .Where(a => a.UnitId.HasValue && unitIds.Contains(a.UnitId.Value))
                .AsQueryable();

            if (filter.UnitId.HasValue)
                query = query.Where(a => a.UnitId == filter.UnitId.Value);

            if (filter.IsActive.HasValue)
                query = query.Where(a => a.IsActive == filter.IsActive.Value);

            // ADD THIS: Get total count
            var totalCount = await query.CountAsync();

            var ads = await query
                .OrderByDescending(a => a.CreatedAt)
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(a => new AdResponse
                {
                    Id = a.Id,
                    Title = a.Title,
                    Description = a.Description,
                    ImageUrl = a.ImageUrl,
                    UnitId = a.UnitId,
                    UnitName = a.Unit!.Name,
                    StartDate = a.StartDate,
                    EndDate = a.EndDate,
                    IsActive = a.IsActive,
                    CreatedAt = a.CreatedAt
                })
                .ToListAsync();

            // CHANGE RETURN:
            var paginatedResult = CreatePaginatedResponse(ads, totalCount, filter.Page, filter.PageSize);
            return Result.Success(paginatedResult);
        }
        catch (Exception ex)
        {
            return Result.Failure<PaginatedResponse<AdResponse>>(
                new Error("GetAdsFailed", "Failed to retrieve ads", 500));
        }
    }

    public async Task<Result> ManageOfferAsync(string userId, int offerId, bool isApproved)
    {
        try
        {
            var offer = await _context.Offers
                .Include(o => o.Unit)
                .FirstOrDefaultAsync(o => o.Id == offerId);

            if (offer == null)
                return Result.Failure(new Error("NotFound", "Offer not found", 404));

            if (offer.UnitId.HasValue)
            {
                var hasAccess = await IsUnitInMyCityAsync(userId, offer.UnitId.Value);
                if (!hasAccess.Value)
                    return Result.Failure(
                        new Error("NoAccess", "You do not have access to this offer", 403));
            }

            offer.IsActive = isApproved;
            await _context.SaveChangesAsync();


            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(new Error("ManageOfferFailed", "Failed to manage offer", 500));
        }
    }

    public async Task<Result> ManageAdAsync(string userId, int adId, bool isApproved)
    {
        try
        {
            var ad = await _context.Ads
                .Include(a => a.Unit)
                .FirstOrDefaultAsync(a => a.Id == adId);

            if (ad == null)
                return Result.Failure(new Error("NotFound", "Ad not found", 404));

            if (ad.UnitId.HasValue)
            {
                var hasAccess = await IsUnitInMyCityAsync(userId, ad.UnitId.Value);
                if (!hasAccess.Value)
                    return Result.Failure(
                        new Error("NoAccess", "You do not have access to this ad", 403));
            }

            ad.IsActive = isApproved;
            await _context.SaveChangesAsync();


            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(new Error("ManageAdFailed", "Failed to manage ad", 500));
        }
    }

    #endregion

    #region COUPONS MANAGEMENT

    public async Task<Result<PaginatedResponse<CouponResponse>>> GetCityCouponsAsync(
     string userId,
     CouponFilter filter)
    {
        try
        {
            var departmentId = await GetAdminDepartmentIdAsync(userId);
            if (!departmentId.IsSuccess)
                return Result.Failure<PaginatedResponse<CouponResponse>>(departmentId.Error);

            var UnitsIds = await _context.Units
                .Where(u => u.CityId == departmentId.Value && !u.IsDeleted)
                .Select(u => u.Id)
                .ToListAsync();

            var query = _context.Coupons
                .Where(c => c.TargetCityId == departmentId.Value || (c.TargetUnitId.HasValue && UnitsIds.Contains(c.TargetUnitId.Value)))
                .AsQueryable();

            if (filter.IsActive.HasValue)
                query = query.Where(c => c.IsActive == filter.IsActive.Value);

            if (filter.Type.HasValue)
                query = query.Where(c => c.Type == filter.Type.Value);

            // ADD THIS: Get total count
            var totalCount = await query.CountAsync();

            var coupons = await query
                .OrderByDescending(c => c.CreatedAt)
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(c => new CouponResponse
                {
                    Id = c.Id,
                    Code = c.Code,
                    Description = c.Description,
                    Type = c.Type.ToString(),
                    DiscountAmount = c.DiscountAmount,
                    MinimumSpend = c.MinimumSpend,
                    MaximumDiscount = c.MaximumDiscount,
                    MaxUsageCount = c.MaxUsageCount,
                    CurrentUsageCount = c.CurrentUsageCount,
                    ValidFrom = c.ValidFrom,
                    ValidUntil = c.ValidUntil,
                    IsActive = c.IsActive
                })
                .ToListAsync();

            // CHANGE RETURN:
            var paginatedResult = CreatePaginatedResponse(coupons, totalCount, filter.Page, filter.PageSize);
            return Result.Success(paginatedResult);
        }
        catch (Exception ex)
        {
            return Result.Failure<PaginatedResponse<CouponResponse>>(
                new Error("GetCouponsFailed", "Failed to retrieve coupons", 500));
        }
    }

    public async Task<Result<CouponResponse>> CreateCityCouponAsync(
        string userId,
        CreateCouponRequest request)
    {
        try
        {
            var departmentId = await GetAdminDepartmentIdAsync(userId);
            if (!departmentId.IsSuccess)
                return Result.Failure<CouponResponse>(departmentId.Error);

            // Check if code already exists
            var exists = await _context.Coupons
                .AnyAsync(c => c.Code == request.Code && c.IsActive);

            if (exists)
                return Result.Failure<CouponResponse>(
                    new Error("AlreadyExists", "A coupon with this code already exists", 400));

            var coupon = new Coupon
            {
                Code = request.Code,
                Description = request.Description,
                Type = request.Type,
                DiscountAmount = request.DiscountAmount,
                MinimumSpend = request.MinimumSpend,
                MaximumDiscount = request.MaximumDiscount,
                MaxUsageCount = request.MaxUsageCount,
                MaxUsagePerUser = request.MaxUsagePerUser,
                ValidFrom = request.ValidFrom,
                ValidUntil = request.ValidUntil,
                TargetCityId = departmentId.Value,
                IsActive = true,
                CurrentUsageCount = 0,
                CreatedAt = DateTime.UtcNow.AddHours(3)
            };

            _context.Coupons.Add(coupon);
            await _context.SaveChangesAsync();

            var response = new CouponResponse
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
                ValidFrom = coupon.ValidFrom,
                ValidUntil = coupon.ValidUntil,
                IsActive = coupon.IsActive
            };



            return Result.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating coupon");
            return Result.Failure<CouponResponse>(
                new Error("CreateCouponFailed", "Failed to create coupon", 500));
        }
    }

    public async Task<Result> DeactivateCouponAsync(string userId, int couponId)
    {
        try
        {
            var departmentId = await GetAdminDepartmentIdAsync(userId);
            if (!departmentId.IsSuccess)
                return Result.Failure(departmentId.Error);

            var UnitsIds = await _context.Units
              .Where(u => u.CityId == departmentId.Value && !u.IsDeleted)
              .Select(u => u.Id)
              .ToListAsync();

            var coupon = await _context.Coupons
                .FirstOrDefaultAsync(c => c.Id == couponId && c.TargetCityId == departmentId.Value || (c.TargetUnitId.HasValue && UnitsIds.Contains(c.TargetUnitId.Value)));

            if (coupon == null)
                return Result.Failure(new Error("NotFound", "Coupon not found", 404));

            coupon.IsActive = false;
            await _context.SaveChangesAsync();

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(new Error("DeactivateFailed", "Failed to deactivate coupon", 500));
        }
    }

    #endregion

    #region REGISTRATION REQUESTS

    public async Task<Result<PaginatedResponse<UnitRegistrationRequestResponse>>> GetRegistrationRequestsAsync(
        string userId,
        RegistrationRequestFilter filter)
    {
        try
        {
            var departmentId = await GetAdminDepartmentIdAsync(userId);
            if (!departmentId.IsSuccess)
                return Result.Failure<PaginatedResponse<UnitRegistrationRequestResponse>>(departmentId.Error);

            var query = _context.Set<UnitRegistrationRequest>()
                .Include(r => r.UnitType)
                .Where(r => r.DepartmentId == departmentId.Value)
                .AsQueryable();

            // Apply filters
            if (filter.Status.HasValue)
                query = query.Where(r => r.Status == filter.Status.Value);

            if (filter.StartDate.HasValue)
                query = query.Where(r => r.SubmittedAt >= filter.StartDate.Value);

            if (filter.EndDate.HasValue)
                query = query.Where(r => r.SubmittedAt <= filter.EndDate.Value);

            // ADD THIS: Get total count
            var totalCount = await query.CountAsync();

            var requests = await query
                .OrderByDescending(r => r.SubmittedAt)
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(r => new UnitRegistrationRequestResponse
                {
                    Id = r.Id,
                    Status = r.Status.ToString(),
                    UnitName = r.UnitName,
                    OwnerFullName = r.OwnerFullName,
                    OwnerEmail = r.OwnerEmail,
                    OwnerPhoneNumber = r.OwnerPhoneNumber,
                    UnitTypeName = r.UnitType.Name,
                    Address = r.Address,
                    BasePrice = r.BasePrice,
                    ImageCount = r.ImageCount,
                    SubmittedAt = r.SubmittedAt,
                    ReviewedAt = r.ReviewedAt,
                    PrimaryImageUrl = r.ImageS3Keys, // ADD THIS (you'll need to parse from ImageS3Keys)
                })
                .ToListAsync();

            // CHANGE RETURN:
            var paginatedResult = CreatePaginatedResponse(requests, totalCount, filter.Page, filter.PageSize);
            return Result.Success(paginatedResult);
        }
        catch (Exception ex)
        {
            return Result.Failure<PaginatedResponse<UnitRegistrationRequestResponse>>(
                new Error("GetRequestsFailed", "Failed to retrieve registration requests", 500));
        }
    }

    public async Task<Result<UnitRegistrationRequestDetailsResponse>> GetRegistrationRequestDetailsAsync(
        string userId,
        int requestId)
    {
        try
        {
            var departmentId = await GetAdminDepartmentIdAsync(userId);
            if (!departmentId.IsSuccess)
                return Result.Failure<UnitRegistrationRequestDetailsResponse>(departmentId.Error);

            var request = await _context.Set<UnitRegistrationRequest>()
                .Include(r => r.Department)
                .Include(r => r.UnitType)
                .FirstOrDefaultAsync(r => r.Id == requestId && r.DepartmentId == departmentId.Value);

            if (request == null)
                return Result.Failure<UnitRegistrationRequestDetailsResponse>(
                    new Error("NotFound", "Registration request not found", 404));

            // Parse image keys from JSON
            var imageKeys = JsonSerializer.Deserialize<List<string>>(request.ImageS3Keys) ?? [];
            var imageUrls = imageKeys.Select(key => $"https://your-cdn.com/{key}").ToList();

            var response = new UnitRegistrationRequestDetailsResponse
            {
                Id = request.Id,
                Status = request.Status.ToString(),
                RejectionReason = request.RejectionReason,
                OwnerFullName = request.OwnerFullName,
                OwnerEmail = request.OwnerEmail,
                OwnerPhoneNumber = request.OwnerPhoneNumber,
                UnitName = request.UnitName,
                Description = request.Description,
                Address = request.Address,
                DepartmentId = request.DepartmentId,
                DepartmentName = request.Department.Name,
                UnitTypeId = request.UnitTypeId,
                UnitTypeName = request.UnitType.Name,
                Latitude = request.Latitude,
                Longitude = request.Longitude,
                BasePrice = request.BasePrice,
                MaxGuests = request.MaxGuests,
                Bedrooms = request.Bedrooms,
                Bathrooms = request.Bathrooms,
                ImageUrls = imageUrls,
                SubmittedAt = request.SubmittedAt,
                ReviewedAt = request.ReviewedAt,
                ReviewedByAdminId = request.ReviewedByAdminId
            };

            return Result.Success(response);
        }
        catch (Exception ex)
        {
            return Result.Failure<UnitRegistrationRequestDetailsResponse>(
                new Error("GetRequestFailed", "Failed to retrieve registration request details", 500));
        }
    }

    public async Task<Result> ApproveRegistrationRequestAsync(string userId, int requestId)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var departmentId = await GetAdminDepartmentIdAsync(userId);
            if (!departmentId.IsSuccess)
                return Result.Failure(departmentId.Error);

            var request = await _context.Set<UnitRegistrationRequest>()
                .Include(r => r.Department)
                .Include(r => r.UnitType)
                .FirstOrDefaultAsync(r => r.Id == requestId && r.DepartmentId == departmentId.Value);

            if (request == null)
                return Result.Failure(new Error("NotFound", "Registration request not found", 404));

            if (request.Status != RegistrationRequestStatus.Pending)
                return Result.Failure(
                    new Error("InvalidStatus", $"Request is already {request.Status}", 400));

            // Check if user already exists
            var existingUser = await _userManager.FindByEmailAsync(request.OwnerEmail);
            ApplicationUser user;
            bool userCreated = false;

            if (existingUser != null)
            {
                user = existingUser;
                var userRoles = await _userManager.GetRolesAsync(user);

                if (userRoles.Contains("User"))
                {
                    await _userManager.RemoveFromRoleAsync(user, "User");
                    await _userManager.AddToRoleAsync(user, "HotelAdmin");

                    _logger.LogInformation(
                        "Upgraded user {UserId} from User to HotelAdmin for registration request {RequestId}",
                        user.Id, requestId);
                }
                else if (!userRoles.Contains("HotelAdmin"))
                {
                    return Result.Failure(new Error("InvalidRole",
                        "User exists but doesn't have the correct role for this operation", 400));
                }
            }
            else
            {
                // Create new user account
                var newUser = new ApplicationUser
                {
                    UserName = request.OwnerEmail,
                    Email = request.OwnerEmail,
                    NormalizedEmail = request.OwnerEmail.ToUpperInvariant(),
                    NormalizedUserName = request.OwnerEmail.ToUpperInvariant(),
                    FullName = request.OwnerFullName,
                    PhoneNumber = request.OwnerPhoneNumber,
                    EmailConfirmed = true,
                    CreatedAt = DateTime.UtcNow.AddHours(3),
                    PasswordHash = request.OwnerPassword
                };

                var createResult = await _userManager.CreateAsync(newUser);
                if (!createResult.Succeeded)
                {
                    var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
                    return Result.Failure(
                        new Error("UserCreationFailed", $"Failed to create user: {errors}", 500));
                }

                var roleResult = await _userManager.AddToRoleAsync(newUser, "HotelAdmin");
                if (!roleResult.Succeeded)
                {
                    await transaction.RollbackAsync();
                    return Result.Failure(
                        new Error("RoleAssignmentFailed", "Failed to assign HotelAdmin role", 500));
                }

                user = newUser;
                userCreated = true;

                _logger.LogInformation(
                    "Created new user {UserId} for registration request {RequestId}",
                    user.Id, requestId);
            }

            // Create the Unit
            var unit = new Domain.Entities.Unit
            {
                Name = request.UnitName,
                Description = request.Description,
                Address = request.Address,
                CityId = request.DepartmentId,
                UnitTypeId = request.UnitTypeId,
                Latitude = request.Latitude,
                Longitude = request.Longitude,
                BasePrice = request.BasePrice,
                MaxGuests = request.MaxGuests,
                Bedrooms = request.Bedrooms,
                Bathrooms = request.Bathrooms,
                IsActive = false,
                IsVerified = false,
                CreatedAt = DateTime.UtcNow.AddHours(3)
            };

            await _context.Units.AddAsync(unit);
            await _context.SaveChangesAsync();

            // Assign user as Unit Admin
            var unitAdmin = new UniteAdmin
            {
                UserId = user.Id,
                UnitId = unit.Id,
                IsActive = true,
                AssignedAt = DateTime.UtcNow.AddHours(3)
            };

            await _context.Set<UniteAdmin>().AddAsync(unitAdmin);

            // Move images from temp to unit folder
            var imageKeys = JsonSerializer.Deserialize<List<string>>(request.ImageS3Keys) ?? [];

            if (imageKeys.Any())
            {
                var moveResult = await _s3Service.MoveImagesToUnitAsync(imageKeys, unit.Id);

                if (!moveResult.IsSuccess)
                {
                    _logger.LogWarning(
                        "Failed to move images for unit {UnitId}: {Error}",
                        unit.Id, moveResult.Error.Description);
                }
            }

            // Update registration request
            request.Status = RegistrationRequestStatus.Approved;
            request.ReviewedAt = DateTime.UtcNow.AddHours(3);
            request.ReviewedByAdminId = userId;
            request.CreatedUserId = user.Id;
            request.CreatedUnitId = unit.Id;

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            // Send welcome email (only if new user was created)
            if (userCreated)
            {
                try
                {

                    await SendRequestConfirmationEmailAsync(request);

                    // Notify admins
                    await NotifyAdminsOfNewRequestAsync(request);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send welcome email to {Email}", user.Email);
                }
            }

            _logger.LogInformation(
                "Registration request {RequestId} approved by city admin {UserId}. User: {CreatedUserId}, Unit: {UnitId}",
                requestId, userId, user.Id, unit.Id);

            return Result.Success();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error approving registration request {RequestId}", requestId);

            return Result.Failure(
                new Error("ApprovalFailed", "Failed to approve registration request", 500));
        }
    }

    private async Task SendRequestConfirmationEmailAsync(UnitRegistrationRequest request)
    {
        try
        {
            var origin = _httpContextAccessor.HttpContext?.Request.Headers.Origin.ToString();

            var emailBody = EmailBodyBuilder.GenerateEmailBody("UnitRegistrationConfirmation",
                new Dictionary<string, string>
                {
                    { "{{name}}", request.OwnerFullName },
                    { "{{unit_name}}", request.UnitName },
                    { "{{request_id}}", request.Id.ToString() },
                    { "{{submitted_date}}", request.SubmittedAt.ToString("MMMM dd, yyyy") },
                    { "{{dashboard_url}}", $"{origin}/registration/status/{request.Id}" }
                });

            BackgroundJob.Enqueue(() =>
                _emailSender.SendEmailAsync(
                    request.OwnerEmail,
                    "Hujjzy: Unit Registration Request Received",
                    emailBody));

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send confirmation email for request {RequestId}", request.Id);
        }
    }

    private async Task NotifyAdminsOfNewRequestAsync(UnitRegistrationRequest request)
    {
        try
        {
            // Get all Super Admins
            //var superAdmins = await _userManager.GetUsersInRoleAsync(DefaultRoles.CityAdmin);

            //var adminIds = await _context.DepartmentAdmins
            //    .Where(da => da.CityId == request.DepartmentId && da.IsActive)
            //    .Select(da => da.UserId)
            //    .ToListAsync();

            //superAdmins = [.. superAdmins.Where(sa => adminIds.Contains(sa.Id))];

            var superAdmins = await _userManager.GetUsersInRoleAsync(DefaultRoles.CityAdmin);

            var activeAdminIds = await _context.DepartmentAdmins
                .Where(da =>
                    da.CityId == request.DepartmentId &&
                    da.IsActive)
                .Select(da => da.UserId)
                .ToListAsync();

            superAdmins = superAdmins
                .IntersectBy(activeAdminIds, sa => sa.Id)
                .ToList();

            var origin = _httpContextAccessor.HttpContext?.Request.Headers.Origin.ToString();

            foreach (var admin in superAdmins.Where(u => !u.IsDisable && u.EmailConfirmed))
            {
                var emailBody = EmailBodyBuilder.GenerateEmailBody("AdminNewRegistrationNotification",
                    new Dictionary<string, string>
                    {
                        { "{{admin_name}}", admin.FullName ?? "Admin" },
                        { "{{owner_name}}", request.OwnerFullName },
                        { "{{owner_email}}", request.OwnerEmail },
                        { "{{unit_name}}", request.UnitName },
                        { "{{unit_type}}", request.UnitType?.Name ?? "N/A" },
                        { "{{department}}", request.Department?.Name ?? "N/A" },
                        { "{{submitted_date}}", request.SubmittedAt.ToString("MMMM dd, yyyy HH:mm") },
                        { "{{review_url}}", $"{origin}/admin/registrations/{request.Id}" }
                    });

                BackgroundJob.Enqueue(() =>
                    _emailSender.SendEmailAsync(
                        admin.Email!,
                        "Hujjzy: New Unit Registration Request",
                        emailBody));
            }

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to notify admins of new request {RequestId}", request.Id);
        }
    }

    private async Task SendWelcomeEmailAsync(string email, string fullName, int unitId, string unitName)
    {
        // Implement email sending logic similar to UnitRegistrationService
        // You can use the EmailBodyBuilder from the registration service
        _logger.LogInformation(
            "Sending welcome email to {Email} for unit {UnitId}",
            email, unitId);

        // TODO: Implement actual email sending using IEmailSender
        await Task.CompletedTask;
    }

    public async Task<Result> RejectRegistrationRequestAsync(
        string userId,
        int requestId,
        string rejectionReason)
    {
        try
        {
            var departmentId = await GetAdminDepartmentIdAsync(userId);
            if (!departmentId.IsSuccess)
                return Result.Failure(new Error(departmentId.Error.ToString(), departmentId.Error.ToString(), 400));

            var request = await _context.Set<UnitRegistrationRequest>()
                .FirstOrDefaultAsync(r => r.Id == requestId && r.DepartmentId == departmentId.Value);

            if (request == null)
                return Result.Failure(new Error("NotFound", "Registration request not found", 404));

            if (request.Status != RegistrationRequestStatus.Pending)
                return Result.Failure(
                    new Error("InvalidStatus", $"Cannot reject request with status {request.Status}", 400));

            request.Status = RegistrationRequestStatus.Rejected;
            request.RejectionReason = rejectionReason;
            request.ReviewedAt = DateTime.UtcNow.AddHours(3);
            request.ReviewedByAdminId = userId;

            await _context.SaveChangesAsync();

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(new Error("RejectFailed", "Failed to reject registration request", 500));
        }
    }

    #endregion

    #region CALENDAR & SCHEDULING

    public async Task<Result<IEnumerable<UpcomingCheckInResponse>>> GetCityUpcomingCheckInsAsync(
        string userId,
        int days = 1)
    {
        try
        {
            var departmentId = await GetAdminDepartmentIdAsync(userId);
            if (!departmentId.IsSuccess)
                return Result.Failure<IEnumerable<UpcomingCheckInResponse>>(departmentId.Error);

            var today = DateTime.UtcNow.AddHours(3).Date;
            var endDate = today.AddDays(days);

            var unitIds = await _context.Units
                .Where(u => u.CityId == departmentId.Value && !u.IsDeleted)
                .Select(u => u.Id)
                .ToListAsync();

            var checkIns = await _context.Bookings
                .Include(b => b.Unit)
                .Include(b => b.User)
                .Include(b => b.BookingRooms)
                    .ThenInclude(br => br.Room)
                .Where(b => unitIds.Contains(b.UnitId) &&
                    b.CheckInDate >= today &&
                    b.CheckInDate < endDate &&
                    b.Status == BookingStatus.Confirmed)
                .OrderBy(b => b.CheckInDate)
                .Select(b => new UpcomingCheckInResponse
                {
                    BookingId = b.Id,
                    BookingNumber = b.BookingNumber,
                    UnitName = b.Unit.Name,
                    GuestName = b.User.FullName ?? "N/A",
                    GuestEmail = b.User.Email ?? string.Empty,
                    GuestPhone = b.User.PhoneNumber ?? string.Empty,
                    CheckInDate = b.CheckInDate,
                    NumberOfGuests = b.NumberOfGuests,
                    Rooms = b.BookingRooms.Select(br => br.Room.RoomNumber).ToList(),
                    SpecialRequests = b.SpecialRequests
                })
                .ToListAsync();

            return Result.Success<IEnumerable<UpcomingCheckInResponse>>(checkIns);
        }
        catch (Exception ex)
        {
            return Result.Failure<IEnumerable<UpcomingCheckInResponse>>(
                new Error("GetCheckInsFailed", "Failed to retrieve upcoming check-ins", 500));
        }
    }

    public async Task<Result<IEnumerable<UpcomingCheckOutResponse>>> GetCityUpcomingCheckOutsAsync(
        string userId,
        int days = 1)
    {
        try
        {
            var departmentId = await GetAdminDepartmentIdAsync(userId);
            if (!departmentId.IsSuccess)
                return Result.Failure<IEnumerable<UpcomingCheckOutResponse>>(departmentId.Error);

            var today = DateTime.UtcNow.AddHours(3).Date;
            var endDate = today.AddDays(days);

            var unitIds = await _context.Units
                .Where(u => u.CityId == departmentId.Value && !u.IsDeleted)
                .Select(u => u.Id)
                .ToListAsync();

            var checkOuts = await _context.Bookings
                .Include(b => b.Unit)
                .Include(b => b.User)
                .Include(b => b.BookingRooms)
                    .ThenInclude(br => br.Room)
                .Where(b => unitIds.Contains(b.UnitId) &&
                    b.CheckOutDate >= today &&
                    b.CheckOutDate < endDate &&
                    b.Status == BookingStatus.CheckedIn)
                .OrderBy(b => b.CheckOutDate)
                .Select(b => new UpcomingCheckOutResponse
                {
                    BookingId = b.Id,
                    BookingNumber = b.BookingNumber,
                    UnitId = b.UnitId,
                    UnitName = b.Unit.Name,
                    GuestName = b.User.FullName ?? "N/A",
                    GuestEmail = b.User.Email ?? string.Empty,
                    GuestPhone = b.User.PhoneNumber ?? string.Empty,
                    CheckOutDate = b.CheckOutDate,
                    CheckOutTime = b.CheckOutDate.TimeOfDay,
                    RoomNumbers = b.BookingRooms.Select(br => br.Room.RoomNumber).ToList(),
                    TotalAmount = b.TotalPrice,
                    PaidAmount = b.PaidAmount,
                    BalanceDue = b.TotalPrice - b.PaidAmount
                })
                .ToListAsync();

            return Result.Success<IEnumerable<UpcomingCheckOutResponse>>(checkOuts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting upcoming check-outs for city admin {UserId}", userId);
            return Result.Failure<IEnumerable<UpcomingCheckOutResponse>>(
                new Error("GetCheckOutsFailed", "Failed to retrieve upcoming check-outs", 500));
        }
    }

    #endregion

    #region AMENITIES STATISTICS

    public async Task<Result<AmenitiesStatisticsResponse>> GetCityAmenitiesStatisticsAsync(string userId)
    {
        try
        {
            var departmentId = await GetAdminDepartmentIdAsync(userId);
            if (!departmentId.IsSuccess)
                return Result.Failure<AmenitiesStatisticsResponse>(departmentId.Error);

            var unitIds = await _context.Units
                .Where(u => u.CityId == departmentId.Value && !u.IsDeleted)
                .Select(u => u.Id)
                .ToListAsync();

            var totalUnits = unitIds.Count;

            var amenityUsage = await _context.UnitAmenities
                .Include(ua => ua.Amenity)
                .Where(ua => unitIds.Contains(ua.UnitId))
                .GroupBy(ua => new { ua.AmenityId, ua.Amenity.Name, ua.Amenity.Category })
                .Select(g => new AmenityUsage
                {
                    AmenityId = g.Key.AmenityId,
                    AmenityName = g.Key.Name,
                    Category = g.Key.Category,
                    UnitCount = g.Count(),
                    UsagePercentage = totalUnits > 0 ? (decimal)g.Count() / totalUnits * 100 : 0
                })
                .ToListAsync();

            var response = new AmenitiesStatisticsResponse
            {
                MostUsedAmenities = amenityUsage.OrderByDescending(a => a.UnitCount).Take(10).ToList(),
                LeastUsedAmenities = amenityUsage.OrderBy(a => a.UnitCount).Take(10).ToList(),
                TotalUniqueAmenities = amenityUsage.Count
            };

            return Result.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting amenities statistics for city admin {UserId}", userId);
            return Result.Failure<AmenitiesStatisticsResponse>(
                new Error("GetAmenitiesFailed", "Failed to retrieve amenities statistics", 500));
        }
    }

    #endregion

    #region UNIT ADMINS MANAGEMENT

    public async Task<Result<PaginatedResponse<UnitAdminResponse>>> GetCityUnitAdminsAsync(
      string userId,
      UnitAdminFilter filter)
    {
        try
        {
            var departmentId = await GetAdminDepartmentIdAsync(userId);
            if (!departmentId.IsSuccess)
                return Result.Failure<PaginatedResponse<UnitAdminResponse>>(departmentId.Error);

            var unitIds = await _context.Units
                .Where(u => u.CityId == departmentId.Value && !u.IsDeleted)
                .Select(u => u.Id)
                .ToListAsync();

            var query = _context.Set<UniteAdmin>()
                .Include(ua => ua.User)
                .Include(ua => ua.Unit)
                .Where(ua => unitIds.Contains(ua.UnitId))
                .AsQueryable();

            if (filter.UnitId.HasValue)
                query = query.Where(ua => ua.UnitId == filter.UnitId.Value);

            if (filter.IsActive.HasValue)
                query = query.Where(ua => ua.IsActive == filter.IsActive.Value);

            // ADD THIS: Get total count
            var totalCount = await query.CountAsync();

            var admins = await query
                .OrderByDescending(ua => ua.AssignedAt)
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(ua => new UnitAdminResponse
                {
                    Id = ua.Id,
                    UserId = ua.UserId,
                    UserName = ua.User.FullName ?? "N/A",
                    ProfileImageUrl = ua.User.AvatarUrl, // ADD THIS
                    Email = ua.User.Email ?? string.Empty,
                    UnitId = ua.UnitId,
                    UnitName = ua.Unit.Name,
                    IsActive = ua.IsActive,
                    AssignedAt = ua.AssignedAt
                })
                .ToListAsync();

            // CHANGE RETURN:
            var paginatedResult = CreatePaginatedResponse(admins, totalCount, filter.Page, filter.PageSize);
            return Result.Success(paginatedResult);
        }
        catch (Exception ex)
        {
            return Result.Failure<PaginatedResponse<UnitAdminResponse>>(
                new Error("GetAdminsFailed", "Failed to retrieve unit admins", 500));
        }
    }
    public async Task<Result> AssignUnitAdminAsync(string userId, AssignUnitAdminRequest request)
    {
        try
        {
            var hasAccess = await IsUnitInMyCityAsync(userId, request.UnitId);
            if (!hasAccess.Value)
                return Result.Failure(
                    new Error("NoAccess", "You do not have access to this unit", 403));

            // Check if user exists
            var user = await _userManager.FindByIdAsync(request.AdminUserId);
            if (user == null)
                return Result.Failure(new Error("NotFound", "User not found", 404));

            // Check if already assigned
            var existing = await _context.Set<UniteAdmin>()
                .FirstOrDefaultAsync(ua => ua.UnitId == request.UnitId &&
                    ua.UserId == request.AdminUserId);

            if (existing != null)
            {
                if (existing.IsActive)
                    return Result.Failure(new Error("AlreadyExists", "User is already assigned to this unit", 400));

                existing.IsActive = true;
                existing.AssignedAt = DateTime.UtcNow.AddHours(3);
            }
            else
            {
                var unitAdmin = new UniteAdmin
                {
                    UnitId = request.UnitId,
                    UserId = request.AdminUserId,
                    AssignedAt = DateTime.UtcNow.AddHours(3),
                    IsActive = true
                };

                _context.Set<UniteAdmin>().Add(unitAdmin);
            }

            // Add HotelAdmin role if not already
            if (!await _userManager.IsInRoleAsync(user, "HotelAdmin"))
                await _userManager.AddToRoleAsync(user, "HotelAdmin");

            await _context.SaveChangesAsync();


            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(new Error("AssignFailed", "Failed to assign unit admin", 500));
        }
    }

    public async Task<Result> RemoveUnitAdminAsync(string userId, int unitAdminId)
    {
        try
        {
            var unitAdmin = await _context.Set<UniteAdmin>()
                .Include(ua => ua.Unit)
                .FirstOrDefaultAsync(ua => ua.Id == unitAdminId);

            if (unitAdmin == null)
                return Result.Failure(new Error("NotFound", "Unit admin assignment not found", 404));

            var hasAccess = await IsUnitInMyCityAsync(userId, unitAdmin.UnitId);
            if (!hasAccess.Value)
                return Result.Failure(
                    new Error("NoAccess", "You do not have access to this unit", 403));

            _context.Set<UniteAdmin>().Remove(unitAdmin);
            await _context.SaveChangesAsync();


            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(new Error("RemoveFailed", "Failed to remove unit admin", 500));
        }
    }

    public async Task<Result> ToggleUnitAdminStatusAsync(string userId, int unitAdminId)
    {
        try
        {
            var unitAdmin = await _context.Set<UniteAdmin>()
                .Include(ua => ua.Unit)
                .FirstOrDefaultAsync(ua => ua.Id == unitAdminId);

            if (unitAdmin == null)
                return Result.Failure(new Error("NotFound", "Unit admin assignment not found", 404));

            var hasAccess = await IsUnitInMyCityAsync(userId, unitAdmin.UnitId);
            if (!hasAccess.Value)
                return Result.Failure(
                    new Error("NoAccess", "You do not have access to this unit", 403));

            unitAdmin.IsActive = !unitAdmin.IsActive;
            await _context.SaveChangesAsync();

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(new Error("ToggleFailed", "Failed to update unit admin status", 500));
        }
    }

    #endregion

    #region POLICIES MANAGEMENT

    public async Task<Result<IEnumerable<PolicyDetailResponse>>> GetCityPoliciesAsync(
        string userId,
        PolicyFilter filter)
    {
        try
        {
            var departmentId = await GetAdminDepartmentIdAsync(userId);
            if (!departmentId.IsSuccess)
                return Result.Failure<IEnumerable<PolicyDetailResponse>>(departmentId.Error);

            var unitIds = await _context.Units
                .Where(u => u.CityId == departmentId.Value && !u.IsDeleted)
                .Select(u => u.Id)
                .ToListAsync();

            var query = _context.GeneralPolicies
                .Include(p => p.Unit)
                .Where(p => p.UnitId.HasValue && unitIds.Contains(p.UnitId.Value))
                .AsQueryable();

            if (filter.UnitId.HasValue)
                query = query.Where(p => p.UnitId == filter.UnitId.Value);

            if (filter.PolicyType.HasValue)
                query = query.Where(p => p.PolicyType == filter.PolicyType.Value);

            if (filter.IsActive.HasValue)
                query = query.Where(p => p.IsActive == filter.IsActive.Value);

            var policies = await query
                .OrderBy(p => p.Unit.Name)
                .ThenBy(p => p.PolicyType)
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(p => new PolicyDetailResponse
                {
                    Id = p.Id,
                    UnitId = p.UnitId ?? 0,
                    UnitName = p.Unit!.Name,
                    Title = p.Title,
                    Description = p.Description,
                    PolicyType = p.PolicyType.ToString(),
                    IsActive = p.IsActive,
                })
                .ToListAsync();

            return Result.Success<IEnumerable<PolicyDetailResponse>>(policies);
        }
        catch (Exception ex)
        {
            return Result.Failure<IEnumerable<PolicyDetailResponse>>(
                new Error("GetPoliciesFailed", "Failed to retrieve policies", 500));
        }
    }

    public async Task<Result<CancellationPolicyUsageResponse>> GetCityCancellationPolicyUsageAsync(string userId)
    {
        try
        {
            var departmentId = await GetAdminDepartmentIdAsync(userId);
            if (!departmentId.IsSuccess)
                return Result.Failure<CancellationPolicyUsageResponse>(departmentId.Error);

            var units = await _context.Units
                .Include(u => u.CancellationPolicy)
                .Where(u => u.CityId == departmentId.Value && !u.IsDeleted)
                .ToListAsync();

            var totalUnits = units.Count;
            var policyUsage = units
                .Where(u => u.CancellationPolicyId.HasValue)
                .GroupBy(u => new { u.CancellationPolicyId, u.CancellationPolicy!.Name })
                .Select(g => new PolicyUsage
                {
                    PolicyId = g.Key.CancellationPolicyId!.Value,
                    PolicyName = g.Key.Name,
                    UnitCount = g.Count(),
                    UsagePercentage = totalUnits > 0 ? (decimal)g.Count() / totalUnits * 100 : 0
                })
                .ToList();

            var response = new CancellationPolicyUsageResponse
            {
                Policies = policyUsage,
                TotalUnits = totalUnits
            };

            return Result.Success(response);
        }
        catch (Exception ex)
        {
            return Result.Failure<CancellationPolicyUsageResponse>(
                new Error("GetPolicyUsageFailed", "Failed to retrieve cancellation policy usage", 500));
        }
    }

    #endregion

    #region COMPREHENSIVE REPORTS

    public async Task<Result<RevenueReportResponse>> GetCityRevenueReportAsync(
        string userId,
        RevenueReportFilter filter)
    {
        try
        {
            var departmentId = await GetAdminDepartmentIdAsync(userId);
            if (!departmentId.IsSuccess)
                return Result.Failure<RevenueReportResponse>(departmentId.Error);

            var unitIds = await _context.Units
                .Where(u => u.CityId == departmentId.Value && !u.IsDeleted)
                .Select(u => u.Id)
                .ToListAsync();

            var bookings = await _context.Bookings
                .Include(b => b.Unit)
                .Where(b => unitIds.Contains(b.UnitId) &&
                    b.Status == BookingStatus.Completed &&
                    b.CheckInDate >= filter.StartDate &&
                    b.CheckOutDate <= filter.EndDate)
                .ToListAsync();

            var totalRevenue = bookings.Sum(b => b.TotalPrice);
            var averageBookingValue = bookings.Any() ? bookings.Average(b => b.TotalPrice) : 0;

            var response = new RevenueReportResponse
            {
                StartDate = filter.StartDate,
                EndDate = filter.EndDate,
                TotalRevenue = totalRevenue,
                TotalBookings = bookings.Count,
                AverageBookingValue = averageBookingValue,
                RevenueByUnit = bookings
                    .GroupBy(b => new { b.UnitId, b.Unit.Name })
                    .Select(g => new UnitRevenueData
                    {
                        UnitId = g.Key.UnitId,
                        UnitName = g.Key.Name,
                        Revenue = g.Sum(b => b.TotalPrice),
                        BookingCount = g.Count()
                    })
                    .OrderByDescending(x => x.Revenue)
                    .ToList()
            };

            return Result.Success(response);
        }
        catch (Exception ex)
        {
            return Result.Failure<RevenueReportResponse>(
                new Error("GetRevenueFailed", "Failed to generate revenue report", 500));
        }
    }

    public async Task<Result<BookingAnalyticsResponse>> GetCityBookingAnalyticsAsync(
        string userId,
        AnalyticsFilter filter)
    {
        try
        {
            var departmentId = await GetAdminDepartmentIdAsync(userId);
            if (!departmentId.IsSuccess)
                return Result.Failure<BookingAnalyticsResponse>(departmentId.Error);

            var unitIds = await _context.Units
                .Where(u => u.CityId == departmentId.Value && !u.IsDeleted)
                .Select(u => u.Id)
                .ToListAsync();

            var bookings = await _context.Bookings
                .Where(b => unitIds.Contains(b.UnitId) &&
                    b.CreatedAt >= filter.StartDate &&
                    b.CreatedAt <= filter.EndDate)
                .ToListAsync();

            var response = new BookingAnalyticsResponse
            {
                TotalBookings = bookings.Count,
                ConfirmedBookings = bookings.Count(b => b.Status == BookingStatus.Confirmed),
                CompletedBookings = bookings.Count(b => b.Status == BookingStatus.Completed),
                CancelledBookings = bookings.Count(b => b.Status == BookingStatus.Cancelled),
                AverageLeadTime = bookings
                    .Where(b => b.Status != BookingStatus.Cancelled)
                    .Average(b => (b.CheckInDate - b.CreatedAt).TotalDays),
                AverageStayDuration = bookings.Average(b => b.NumberOfNights)
            };

            return Result.Success(response);
        }
        catch (Exception ex)
        {
            return Result.Failure<BookingAnalyticsResponse>(
                new Error("GetAnalyticsFailed", "Failed to generate booking analytics", 500));
        }
    }

    public async Task<Result<OccupancyReportResponse>> GetCityOccupancyReportAsync(
        string userId,
        DateTime startDate,
        DateTime endDate)
    {
        try
        {
            var departmentId = await GetAdminDepartmentIdAsync(userId);
            if (!departmentId.IsSuccess)
                return Result.Failure<OccupancyReportResponse>(departmentId.Error);

            var occupancyRate = await CalculateCityOccupancyRateAsync(departmentId.Value, startDate, endDate);

            var unitIds = await _context.Units
                .Where(u => u.CityId == departmentId.Value && !u.IsDeleted)
                .Select(u => u.Id)
                .ToListAsync();

            var totalRooms = await _context.SubUnits
                .Where(s => unitIds.Contains(s.UnitId) && !s.IsDeleted)
                .CountAsync();

            var bookedRoomNights = await _context.BookingRooms
                .Include(br => br.Booking)
                .Where(br => unitIds.Contains(br.Room.UnitId) &&
                    br.Booking.CheckInDate < endDate &&
                    br.Booking.CheckOutDate > startDate &&
                    br.Booking.Status != BookingStatus.Cancelled)
                .SumAsync(br => br.NumberOfNights);

            var response = new OccupancyReportResponse
            {
                StartDate = startDate,
                EndDate = endDate,
                OverallOccupancyRate = occupancyRate,
                TotalRooms = totalRooms,
                TotalRoomNights = totalRooms * (endDate - startDate).Days,
                BookedRoomNights = bookedRoomNights,
                AvailableRoomNights = (totalRooms * (endDate - startDate).Days) - bookedRoomNights
            };

            return Result.Success(response);
        }
        catch (Exception ex)
        {
            return Result.Failure<OccupancyReportResponse>(
                new Error("GetOccupancyFailed", "Failed to generate occupancy report", 500));
        }
    }

    public async Task<Result<FinancialReportResponse>> GetCityFinancialReportAsync(
        string userId,
        FinancialReportFilter filter)
    {
        try
        {
            var departmentId = await GetAdminDepartmentIdAsync(userId);
            if (!departmentId.IsSuccess)
                return Result.Failure<FinancialReportResponse>(departmentId.Error);

            var unitIds = await _context.Units
                .Where(u => u.CityId == departmentId.Value && !u.IsDeleted)
                .Select(u => u.Id)
                .ToListAsync();

            var bookings = await _context.Bookings
                .Where(b => unitIds.Contains(b.UnitId) &&
                    b.CreatedAt >= filter.StartDate &&
                    b.CreatedAt <= filter.EndDate)
                .ToListAsync();

            var totalRevenue = bookings.Where(b => b.Status == BookingStatus.Completed).Sum(b => b.TotalPrice);
            var pendingRevenue = bookings.Where(b => b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Pending).Sum(b => b.TotalPrice);
            var refundedAmount = bookings.Where(b => b.Status == BookingStatus.Cancelled).Sum(b => b.PaidAmount);

            var response = new FinancialReportResponse
            {
                Period = $"{filter.StartDate:yyyy-MM-dd} to {filter.EndDate:yyyy-MM-dd}",
                TotalRevenue = totalRevenue,
                PendingRevenue = pendingRevenue,
                RefundedAmount = refundedAmount,
                NetRevenue = totalRevenue - refundedAmount,
                TotalTransactions = bookings.Count,
                AverageTransactionValue = bookings.Any() ? bookings.Average(b => b.TotalPrice) : 0
            };

            return Result.Success(response);
        }
        catch (Exception ex)
        {
            return Result.Failure<FinancialReportResponse>(
                new Error("GetFinancialFailed", "Failed to generate financial report", 500));
        }
    }

    public async Task<Result<OccupancyStatisticsResponse>> GetCityOccupancyStatisticsAsync(
        string userId,
        OccupancyFilter filter)
    {
        try
        {
            var departmentId = await GetAdminDepartmentIdAsync(userId);
            if (!departmentId.IsSuccess)
                return Result.Failure<OccupancyStatisticsResponse>(departmentId.Error);

            var occupancyRate = await CalculateCityOccupancyRateAsync(
                departmentId.Value, filter.StartDate, filter.EndDate);

            var response = new OccupancyStatisticsResponse
            {
                Period = $"{filter.StartDate:yyyy-MM-dd} to {filter.EndDate:yyyy-MM-dd}",
                OccupancyRate = occupancyRate,
                // Add more detailed statistics here
            };

            return Result.Success(response);
        }
        catch (Exception ex)
        {
            return Result.Failure<OccupancyStatisticsResponse>(
                new Error("GetOccupancyStatsFailed", "Failed to generate occupancy statistics", 500));
        }
    }

    public async Task<Result<BookingTrendsResponse>> GetCityBookingTrendsAsync(
        string userId,
        TrendsFilter filter)
    {
        try
        {
            var departmentId = await GetAdminDepartmentIdAsync(userId);
            if (!departmentId.IsSuccess)
                return Result.Failure<BookingTrendsResponse>(departmentId.Error);

            var unitIds = await _context.Units
                .Where(u => u.CityId == departmentId.Value && !u.IsDeleted)
                .Select(u => u.Id)
                .ToListAsync();

            var bookings = await _context.Bookings
                .Where(b => unitIds.Contains(b.UnitId) &&
                    b.CreatedAt >= filter.StartDate &&
                    b.CreatedAt <= filter.EndDate)
                .ToListAsync();

            var monthlyTrends = bookings
                .GroupBy(b => new { b.CreatedAt.Year, b.CreatedAt.Month })
                .Select(g => new MonthlyTrend
                {
                    Month = $"{g.Key.Year}-{g.Key.Month:D2}",
                    BookingCount = g.Count(),
                    Revenue = g.Where(b => b.Status == BookingStatus.Completed).Sum(b => b.TotalPrice)
                })
                .OrderBy(x => x.Month)
                .ToList();

            var response = new BookingTrendsResponse
            {
                MonthlyTrends = monthlyTrends,
                TotalBookings = bookings.Count,
                TrendDirection = monthlyTrends.Count > 1 ?
                    (monthlyTrends.Last().BookingCount > monthlyTrends.First().BookingCount ? "Up" : "Down") : "Stable"
            };

            return Result.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting booking trends for city admin {UserId}", userId);
            return Result.Failure<BookingTrendsResponse>(
                new Error("GetTrendsFailed", "Failed to generate booking trends", 500));
        }
    }

    public async Task<Result<CustomerInsightsResponse>> GetCityCustomerInsightsAsync(
        string userId,
        InsightsFilter filter)
    {
        try
        {
            var departmentId = await GetAdminDepartmentIdAsync(userId);
            if (!departmentId.IsSuccess)
                return Result.Failure<CustomerInsightsResponse>(departmentId.Error);

            var unitIds = await _context.Units
                .Where(u => u.CityId == departmentId.Value && !u.IsDeleted)
                .Select(u => u.Id)
                .ToListAsync();

            var bookings = await _context.Bookings
                .Include(b => b.User)
                .Where(b => unitIds.Contains(b.UnitId) &&
                    b.CreatedAt >= filter.StartDate &&
                    b.CreatedAt <= filter.EndDate)
                .ToListAsync();

            var uniqueGuests = bookings.Select(b => b.UserId).Distinct().Count();
            var repeatGuests = bookings
                .GroupBy(b => b.UserId)
                .Count(g => g.Count() > 1);

            var response = new CustomerInsightsResponse
            {
                TotalUniqueGuests = uniqueGuests,
                ReturningGuests = repeatGuests,
                ReturnGuestRate = uniqueGuests > 0 ? (decimal)repeatGuests / uniqueGuests * 100 : 0,
                AverageBookingsPerGuest = uniqueGuests > 0 ? (decimal)bookings.Count / uniqueGuests : 0
            };

            return Result.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting customer insights for city admin {UserId}", userId);
            return Result.Failure<CustomerInsightsResponse>(
                new Error("GetInsightsFailed", "Failed to generate customer insights", 500));
        }
    }

    public async Task<Result<PerformanceComparisonResponse>> GetCityPerformanceComparisonAsync(
        string userId,
        ComparisonFilter filter)
    {
        try
        {
            var departmentId = await GetAdminDepartmentIdAsync(userId);
            if (!departmentId.IsSuccess)
                return Result.Failure<PerformanceComparisonResponse>(departmentId.Error);

            // Implementation for performance comparison
            var response = new PerformanceComparisonResponse
            {
                CurrentPeriodRevenue = 0,
                PreviousPeriodRevenue = 0,
                GrowthPercentage = 0
            };

            return Result.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting performance comparison for city admin {UserId}", userId);
            return Result.Failure<PerformanceComparisonResponse>(
                new Error("GetComparisonFailed", "Failed to generate performance comparison", 500));
        }
    }

    public async Task<Result<RevenueBreakdownResponse>> GetCityRevenueBreakdownAsync(
        string userId,
        RevenueBreakdownFilter filter)
    {
        try
        {
            var departmentId = await GetAdminDepartmentIdAsync(userId);
            if (!departmentId.IsSuccess)
                return Result.Failure<RevenueBreakdownResponse>(departmentId.Error);

            var unitIds = await _context.Units
                .Where(u => u.CityId == departmentId.Value && !u.IsDeleted)
                .Select(u => u.Id)
                .ToListAsync();

            var bookings = await _context.Bookings
                .Include(b => b.Unit)
                .Where(b => unitIds.Contains(b.UnitId) &&
                    b.Status == BookingStatus.Completed &&
                    b.CreatedAt >= filter.StartDate &&
                    b.CreatedAt <= filter.EndDate)
                .ToListAsync();

            var response = new RevenueBreakdownResponse
            {
                TotalRevenue = bookings.Sum(b => b.TotalPrice),
                AccommodationRevenue = bookings.Sum(b => b.TotalPrice),
                ServiceRevenue = 0,
                TaxRevenue = 0
            };

            return Result.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting revenue breakdown for city admin {UserId}", userId);
            return Result.Failure<RevenueBreakdownResponse>(
                new Error("GetBreakdownFailed", "Failed to generate revenue breakdown", 500));
        }
    }

    public async Task<Result<CancellationAnalyticsResponse>> GetCityCancellationAnalyticsAsync(
        string userId,
        CancellationFilter filter)
    {
        try
        {
            var departmentId = await GetAdminDepartmentIdAsync(userId);
            if (!departmentId.IsSuccess)
                return Result.Failure<CancellationAnalyticsResponse>(departmentId.Error);

            var unitIds = await _context.Units
                .Where(u => u.CityId == departmentId.Value && !u.IsDeleted)
                .Select(u => u.Id)
                .ToListAsync();

            var bookings = await _context.Bookings
                .Where(b => unitIds.Contains(b.UnitId) &&
                    b.CreatedAt >= filter.StartDate &&
                    b.CreatedAt <= filter.EndDate)
                .ToListAsync();

            var cancelledBookings = bookings.Where(b => b.Status == BookingStatus.Cancelled).ToList();
            var totalBookings = bookings.Count;

            var response = new CancellationAnalyticsResponse
            {
                TotalCancellations = cancelledBookings.Count,
                CancellationRate = totalBookings > 0 ? (decimal)cancelledBookings.Count / totalBookings * 100 : 0,
                RefundedAmount = cancelledBookings.Sum(b => b.PaidAmount),
                AverageCancellationLeadTime = (decimal)(cancelledBookings.Any() ?
                    cancelledBookings.Average(b => (b.CheckInDate - (b.CancelledAt ?? b.CreatedAt)).TotalDays) : 0)
            };

            return Result.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cancellation analytics for city admin {UserId}", userId);
            return Result.Failure<CancellationAnalyticsResponse>(
                new Error("GetCancellationFailed", "Failed to generate cancellation analytics", 500));
        }
    }


    #endregion

    #region REVIEWS MANAGEMENT

    public async Task<Result<PaginatedResponse<ReviewResponse>>> GetCityReviewsAsync(
      string userId,
      ReviewFilter filter)
    {
        try
        {
            var departmentId = await GetAdminDepartmentIdAsync(userId);
            if (!departmentId.IsSuccess)
                return Result.Failure<PaginatedResponse<ReviewResponse>>(departmentId.Error);

            var unitIds = await _context.Units
                .Where(u => u.CityId == departmentId.Value && !u.IsDeleted)
                .Select(u => u.Id)
                .ToListAsync();

            var query = _context.Reviews
                .Include(r => r.Unit)
                .Include(r => r.User)
                .Include(r => r.Booking)
                .Where(r => unitIds.Contains(r.UnitId))
                .AsQueryable();

            // Apply filters
            if (filter.MinRating.HasValue)
                query = query.Where(r => r.Rating >= filter.MinRating.Value);

            if (filter.UnitId.HasValue)
                query = query.Where(r => r.UnitId == filter.UnitId.Value);

            // ADD THIS: Get total count
            var totalCount = await query.CountAsync();

            var reviews = await query
                .OrderByDescending(r => r.CreatedAt)
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(r => new ReviewResponse
                {
                    Id = r.Id,
                    UnitId = r.UnitId,
                    UnitName = r.Unit.Name,
                    UserId = r.UserId,
                    GuestName = r.User.FullName ?? "N/A",
                    BookingId = r.BookingId,
                    Rating = r.Rating,
                    Comment = r.Comment,
                    CleanlinessRating = r.CleanlinessRating,
                    LocationRating = r.LocationRating,
                    ValueRating = r.ValueRating,
                    CreatedAt = r.CreatedAt
                })
                .ToListAsync();

            // CHANGE RETURN:
            var paginatedResult = CreatePaginatedResponse(reviews, totalCount, filter.Page, filter.PageSize);
            return Result.Success(paginatedResult);
        }
        catch (Exception ex)
        {
            return Result.Failure<PaginatedResponse<ReviewResponse>>(
                new Error("GetReviewsFailed", "Failed to retrieve reviews", 500));
        }
    }
    public async Task<Result> ToggleReviewVisibilityAsync(string userId, int reviewId)
    {
        try
        {
            var review = await _context.Set<Domain.Entities.Review>()
                .Include(r => r.Unit)
                .FirstOrDefaultAsync(r => r.Id == reviewId);

            if (review == null)
                return Result.Failure(new Error("NotFound", "Review not found", 404));

            // Verify the review belongs to a unit in this admin's city
            var hasAccess = await IsUnitInMyCityAsync(userId, review.UnitId);
            if (!hasAccess.Value)
                return Result.Failure(
                    new Error("NoAccess", "You do not have access to this review", 403));

            review.IsVisible = !review.IsVisible;
            review.UpdatedAt = DateTime.UtcNow.AddHours(3);

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Review {ReviewId} visibility toggled to {IsVisible} by city admin {UserId}",
                reviewId, review.IsVisible, userId);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling review visibility");
            return Result.Failure(new Error("ToggleFailed", "Failed to toggle review visibility", 500));
        }
    }

    public async Task<Result<PaginatedResponse<ReviewResponse>>> GetNonVisibleReviewsAsync(
      int page = 1,
      int pageSize = 10)
    {
        try
        {
            var query = _context.Set<Domain.Entities.Review>()
                .Include(r => r.Unit)
                .Include(r => r.User)
                .Include(r => r.Images)
                .Where(r => !r.IsVisible) // Only non-visible reviews
                .OrderByDescending(r => r.CreatedAt);

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var reviews = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                 .Select(r => new ReviewResponse
                 {
                     Id = r.Id,
                     UnitId = r.UnitId,
                     UnitName = r.Unit.Name,
                     UserId = r.UserId,
                     GuestName = r.User.FullName ?? "N/A",
                     BookingId = r.BookingId,
                     Rating = r.Rating,
                     Comment = r.Comment,
                     CleanlinessRating = r.CleanlinessRating,
                     LocationRating = r.LocationRating,
                     ValueRating = r.ValueRating,
                     CreatedAt = r.CreatedAt
                 })
                .AsNoTracking()
                .ToListAsync();

  

            var paginatedResult = CreatePaginatedResponse(
                reviews, totalCount, page, pageSize);

            return Result.Success(paginatedResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting non-visible reviews");
            return Result.Failure<PaginatedResponse<ReviewResponse>>(
                new Error("GetFailed", "Failed to retrieve non-visible reviews", 500));
        }
    }

    #endregion

    #region SUBUNITS MANAGEMENT

    public async Task<Result<PaginatedResponse<SubUnitComprehensiveDetail>>> GetCitySubUnitsAsync(
     string userId,
     int? unitId = null,
     int page = 1,
     int pageSize = 10)
    {
        try
        {
            var departmentId = await GetAdminDepartmentIdAsync(userId);
            if (!departmentId.IsSuccess)
                return Result.Failure<PaginatedResponse<SubUnitComprehensiveDetail>>(departmentId.Error);

            var query = _context.SubUnits
                .Include(s => s.Unit)
                    .ThenInclude(u => u.City)
                .Include(s => s.SubUnitImages.Where(i => !i.IsDeleted))
                .Include(s => s.SubUnitAmenities)
                    .ThenInclude(sa => sa.Amenity)
                .Where(s => s.Unit.CityId == departmentId.Value && !s.IsDeleted)
                .AsQueryable();

            if (unitId.HasValue)
                query = query.Where(s => s.UnitId == unitId.Value);

            // ADD THIS: Get total count before pagination
            var totalCount = await query.CountAsync();

            var subUnits = await query
                .OrderBy(s => s.Unit.Name)
                .ThenBy(s => s.RoomNumber)
                .Skip((page - 1) * pageSize)  // ADD pagination
                .Take(pageSize)                // ADD pagination
                .ToListAsync();

            var responses = subUnits.Select(s => new SubUnitComprehensiveDetail
            {
                Id = s.Id,
                UnitId = s.UnitId,
                UnitName = s.Unit.Name,
                RoomNumber = s.RoomNumber,
                Description = s.Description,
                PricePerNight = s.PricePerNight,
                MaxOccupancy = s.MaxOccupancy,
                Size = s.Size,
                IsAvailable = s.IsAvailable,
                Images = s.SubUnitImages.OrderBy(i => i.DisplayOrder).Select(i => new SubUnitImageResponse
                {
                    Id = i.Id,
                    ImageUrl = i.ImageUrl,
                    ThumbnailUrl = i.ThumbnailUrl,
                    IsPrimary = i.IsPrimary,
                    DisplayOrder = i.DisplayOrder,
                    Caption = i.Caption
                }).ToList(),
                Amenities = s.SubUnitAmenities.Select(sa => new AmenityResponse
                {
                    Id = sa.Amenity.Id,
                    Name = sa.Amenity.Name,
                    Category = sa.Amenity.Category,
                    IsAvailable = sa.IsAvailable
                }).ToList(),
            }).ToList();

            // CHANGE RETURN:
            var paginatedResult = CreatePaginatedResponse(responses, totalCount, page, pageSize);
            return Result.Success(paginatedResult);
        }
        catch (Exception ex)
        {
            return Result.Failure<PaginatedResponse<SubUnitComprehensiveDetail>>(
                new Error("GetSubUnitsFailed", "Failed to retrieve subunits", 500));
        }
    }

    public async Task<Result<SubUnitComprehensiveDetail>> GetSubUnitDetailsAsync(
        string userId,
        int subUnitId)
    {
        try
        {
            var subUnit = await _context.SubUnits
                .Include(s => s.Unit)
                    .ThenInclude(u => u.City)
                .Include(s => s.SubUnitImages.Where(i => !i.IsDeleted))
                .Include(s => s.SubUnitAmenities)
                    .ThenInclude(sa => sa.Amenity)
                .FirstOrDefaultAsync(s => s.Id == subUnitId && !s.IsDeleted);

            if (subUnit == null)
                return Result.Failure<SubUnitComprehensiveDetail>(
                    new Error("NotFound", "SubUnit not found", 404));

            // Verify access
            var hasAccess = await IsUnitInMyCityAsync(userId, subUnit.UnitId);
            if (!hasAccess.Value)
                return Result.Failure<SubUnitComprehensiveDetail>(
                    new Error("NoAccess", "You do not have access to this subunit", 403));

            var response = new SubUnitComprehensiveDetail
            {
                Id = subUnit.Id,
                UnitId = subUnit.UnitId,
                UnitName = subUnit.Unit.Name,
                RoomNumber = subUnit.RoomNumber,
                Description = subUnit.Description,
                PricePerNight = subUnit.PricePerNight,
                MaxOccupancy = subUnit.MaxOccupancy,
                Size = subUnit.Size,
                IsAvailable = subUnit.IsAvailable,
                Amenities = subUnit.SubUnitAmenities.Select(sa => new AmenityResponse
                {
                    Id = sa.Amenity.Id,
                    Name = sa.Amenity.Name,
                    Category = sa.Amenity.Category,
                    IsAvailable = sa.IsAvailable
                }).ToList()
            };

            return Result.Success(response);
        }
        catch (Exception ex)
        {
            return Result.Failure<SubUnitComprehensiveDetail>(
                new Error("GetSubUnitFailed", "Failed to retrieve subunit details", 500));
        }
    }

    #endregion

    #region DEPARTMENT IMAGE MANAGEMENT

    public async Task<Result<IEnumerable<DepartmentImageResponse>>> GetDepartmentImagesAsync(
        string userId,
        int departmentId)
    {
        try
        {
            var hasAccess = await IsCityAdminAsync(userId, departmentId);
            if (!hasAccess.Value)
                return Result.Failure<IEnumerable<DepartmentImageResponse>>(
                    new Error("NoAccess", "You do not have access to this department", 403));

            var images = await _context.Set<DepartmentImage>()
                .Where(i => i.DepartmentId == departmentId && !i.IsDeleted)
                .OrderBy(i => i.DisplayOrder)
                .Select(i => new DepartmentImageResponse
                {
                    Id = i.Id,
                    ImageUrl = i.ImageUrl,
                    ThumbnailUrl = i.ThumbnailUrl,
                    IsPrimary = i.IsPrimary,
                    DisplayOrder = i.DisplayOrder,
                    Caption = i.Caption,
                    ImageType = i.ImageType.ToString(),
                    UploadedAt = i.UploadedAt
                })
                .ToListAsync();

            return Result.Success<IEnumerable<DepartmentImageResponse>>(images);
        }
        catch (Exception ex)
        {
            return Result.Failure<IEnumerable<DepartmentImageResponse>>(
                new Error("GetImagesFailed", "Failed to get department images", 500));
        }
    }

    public async Task<Result> UpdateDepartmentImageOrderAsync(
        string userId,
        int departmentId,
        UpdateImageOrderRequest request)
    {
        try
        {
            var hasAccess = await IsCityAdminAsync(userId, departmentId);
            if (!hasAccess.Value)
                return Result.Failure(
                    new Error("NoAccess", "You do not have access to this department", 403));

            var images = await _context.Set<DepartmentImage>()
                .Where(i => i.DepartmentId == departmentId && !i.IsDeleted)
                .ToListAsync();

            // Reset primary if setting new primary
            if (request.PrimaryImageId.HasValue)
            {
                foreach (var img in images)
                    img.IsPrimary = false;

                var primaryImage = images.FirstOrDefault(i => i.Id == request.PrimaryImageId.Value);
                if (primaryImage != null)
                    primaryImage.IsPrimary = true;
            }

            // Update display order
            if (request.ImageOrders != null && request.ImageOrders.Any())
            {
                foreach (var order in request.ImageOrders)
                {
                    var image = images.FirstOrDefault(i => i.Id == order.ImageId);
                    if (image != null)
                        image.DisplayOrder = order.Order;
                }
            }

            await _context.SaveChangesAsync();

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(new Error("UpdateOrderFailed", "Failed to update image order", 500));
        }
    }

    public async Task<Result> DeleteDepartmentImageAsync(string userId, int imageId)
    {
        try
        {
            var image = await _context.Set<DepartmentImage>()
                .Include(i => i.Department)
                .FirstOrDefaultAsync(i => i.Id == imageId && !i.IsDeleted);

            if (image == null)
                return Result.Failure(new Error("NotFound", "Image not found", 404));

            var hasAccess = await IsCityAdminAsync(userId, image.DepartmentId);
            if (!hasAccess.Value)
                return Result.Failure(
                    new Error("NoAccess", "You do not have access to this image", 403));

            image.IsDeleted = true;
            image.DeletedAt = DateTime.UtcNow.AddHours(3);
            image.DeletedByUserId = userId;

            await _context.SaveChangesAsync();

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(new Error("DeleteImageFailed", "Failed to delete image", 500));
        }
    }


    public async Task<Result> UploadDepartmentImageAsync(
        string userId,
        UploadDepartmentImageRequest request)
    {
        try
        {
            var departmentI = await GetAdminDepartmentIdAsync(userId);
            if (!departmentI.IsSuccess)
                return Result.Failure(departmentI.Error);

            var departmentId = departmentI.Value;

            // Check access
            var hasAccess = await IsCityAdminAsync(userId, departmentId);
            if (!hasAccess.Value)
                return Result.Failure(
                    new Error("NoAccess", "You do not have access to this department", 403));

            // Upload image to S3
            var uploadResult = await _s3Service.UploadDepartmentImageAsync(
                request.ImageFile,
                departmentId,
                userId);

            if (!uploadResult.IsSuccess)
                return Result.Failure(uploadResult.Error);

            //// Parse image type
            //var imageType = Enum.TryParse<DepartmentImageType>(request.ImageType, out var type)
            //    ? type
            //    : DepartmentImageType.General;

            //// Create database record
            //var image = new DepartmentImage
            //{
            //    DepartmentId = departmentId,
            //    ImageUrl = uploadResult.Value.ImageUrl,
            //    S3Key = uploadResult.Value.S3Key,
            //    S3Bucket = "hujjzy-bucket",
            //    ThumbnailUrl = null, // No thumbnail
            //    MediumUrl = null,    // No medium size
            //    Caption = request.Caption,
            //    ImageType = imageType,
            //    IsPrimary = false,
            //    DisplayOrder = 0,
            //    UploadedByUserId = userId,
            //    UploadedAt = DateTime.UtcNow.AddHours(3),
            //    ProcessingStatus = ImageProcessingStatus.Completed,
            //    MimeType = "image/webp"
            //};

            //_context.Set<DepartmentImage>().Add(image);
            var department = await _context.Departments
                .FirstOrDefaultAsync(d => d.Id == departmentId);


            department?.ImageUrl = uploadResult.Value.ImageUrl;

            await _context.SaveChangesAsync();

            //// Return response
            //var response = new DepartmentImageResponse
            //{
            //    Id = image.Id,
            //    ImageUrl = image.ImageUrl,
            //    ThumbnailUrl = image.ThumbnailUrl,
            //    IsPrimary = image.IsPrimary,
            //    DisplayOrder = image.DisplayOrder,
            //    Caption = image.Caption,
            //    ImageType = image.ImageType.ToString(),
            //    UploadedAt = image.UploadedAt
            //};

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(
                new Error("UploadFailed", $"Failed to upload image: {ex.Message}", 500));
        }
    }


    #endregion

    #region UNIT IMAGES MANAGEMENT

    public async Task<Result<IEnumerable<ImageDetailResponse>>> GetUnitImagesAsync(
        string userId,
        int unitId)
    {
        try
        {
            var hasAccess = await IsUnitInMyCityAsync(userId, unitId);
            if (!hasAccess.Value)
                return Result.Failure<IEnumerable<ImageDetailResponse>>(
                    new Error("NoAccess", "You do not have access to this unit", 403));

            var images = await _context.Set<Domain.Entities.UnitImage>()
                .Where(i => i.UnitId == unitId && !i.IsDeleted)
                .OrderBy(i => i.DisplayOrder)
                .Select(i => new ImageDetailResponse
                {
                    Id = i.Id,
                    ImageUrl = i.ImageUrl,
                    ThumbnailUrl = i.ThumbnailUrl,
                    IsPrimary = i.IsPrimary,
                    DisplayOrder = i.DisplayOrder,
                    Caption = i.Caption,
                    AltText = i.AltText,
                    Width = i.Width,
                    Height = i.Height,
                    FileSizeBytes = i.FileSizeBytes,
                    UploadedAt = i.UploadedAt
                })
                .ToListAsync();

            return Result.Success<IEnumerable<ImageDetailResponse>>(images);
        }
        catch (Exception ex)
        {
            return Result.Failure<IEnumerable<ImageDetailResponse>>(
                new Error("GetImagesFailed", "Failed to get unit images", 500));
        }
    }

    public async Task<Result> DeleteUnitImageAsync(string userId, int imageId)
    {
        try
        {
            var image = await _context.Set<Domain.Entities.UnitImage>()
                .Include(i => i.Unit)
                .FirstOrDefaultAsync(i => i.Id == imageId && !i.IsDeleted);

            if (image == null)
                return Result.Failure(new Error("NotFound", "Image not found", 404));

            var hasAccess = await IsUnitInMyCityAsync(userId, image.UnitId);
            if (!hasAccess.Value)
                return Result.Failure(
                    new Error("NoAccess", "You do not have access to this image", 403));

            image.IsDeleted = true;
            image.DeletedAt = DateTime.UtcNow.AddHours(3);
            image.DeletedByUserId = userId;

            await _context.SaveChangesAsync();


            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(new Error("DeleteImageFailed", "Failed to delete image", 500));
        }
    }

    #endregion

    #region PAYMENT MANAGEMENT

    public async Task<Result<IEnumerable<PaymentHistoryResponse>>> GetCityPaymentHistoryAsync(
        string userId,
        PaymentFilter filter)
    {
        try
        {
            var departmentId = await GetAdminDepartmentIdAsync(userId);
            if (!departmentId.IsSuccess)
                return Result.Failure<IEnumerable<PaymentHistoryResponse>>(departmentId.Error);

            var unitIds = await _context.Units
                .Where(u => u.CityId == departmentId.Value && !u.IsDeleted)
                .Select(u => u.Id)
                .ToListAsync();

            var query = _context.Payments
                .Include(p => p.Booking)
                    .ThenInclude(b => b.Unit)
                .Include(p => p.Booking.User)
                .Where(p => unitIds.Contains(p.Booking.UnitId))
                .AsQueryable();

            // Apply filters
            if (filter.PaymentStatus.HasValue)
                query = query.Where(p => p.Status == filter.PaymentStatus.Value);

            if (filter.StartDate.HasValue)
                query = query.Where(p => p.PaymentDate >= filter.StartDate.Value);

            if (filter.EndDate.HasValue)
                query = query.Where(p => p.PaymentDate <= filter.EndDate.Value);

            var payments = await query
                .OrderByDescending(p => p.PaymentDate)
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(p => new PaymentHistoryResponse
                {
                    BookingId = p.BookingId,
                    BookingNumber = p.Booking.BookingNumber,
                    UnitName = p.Booking.Unit.Name,
                    GuestName = p.Booking.User.FullName ?? "N/A",
                    Amount = p.Amount,
                    PaymentMethod = p.PaymentMethod.ToString(),
                    Status = p.Status.ToString(),
                    TransactionId = p.TransactionId,
                    PaymentDate = p.PaymentDate,
                })
                .ToListAsync();

            return Result.Success<IEnumerable<PaymentHistoryResponse>>(payments);
        }
        catch (Exception ex)
        {
            return Result.Failure<IEnumerable<PaymentHistoryResponse>>(
                new Error("GetPaymentsFailed", "Failed to retrieve payment history", 500));
        }
    }

    public async Task<Result<IEnumerable<PendingPaymentResponse>>> GetCityPendingPaymentsAsync(string userId)
    {
        try
        {
            var departmentId = await GetAdminDepartmentIdAsync(userId);
            if (!departmentId.IsSuccess)
                return Result.Failure<IEnumerable<PendingPaymentResponse>>(departmentId.Error);

            var unitIds = await _context.Units
                .Where(u => u.CityId == departmentId.Value && !u.IsDeleted)
                .Select(u => u.Id)
                .ToListAsync();

            var pendingBookings = await _context.Bookings
                .Include(b => b.Unit)
                .Include(b => b.User)
                .Where(b => unitIds.Contains(b.UnitId) &&
                    (b.PaymentStatus == PaymentStatus.Pending ||
                     b.PaymentStatus == PaymentStatus.PartiallyPaid))
                .OrderBy(b => b.CheckInDate)
                .Select(b => new PendingPaymentResponse
                {
                    BookingId = b.Id,
                    BookingNumber = b.BookingNumber,
                    UnitName = b.Unit.Name,
                    GuestName = b.User.FullName ?? "N/A",
                    GuestEmail = b.User.Email ?? string.Empty,
                    TotalAmount = b.TotalPrice,
                    PaidAmount = b.PaidAmount,
                    PaymentStatus = b.PaymentStatus.ToString(),
                    CheckInDate = b.CheckInDate,
                    DaysUntilCheckIn = (int)(b.CheckInDate - DateTime.UtcNow.AddHours(3)).TotalDays
                })
                .ToListAsync();

            return Result.Success<IEnumerable<PendingPaymentResponse>>(pendingBookings);
        }
        catch (Exception ex)
        {
            return Result.Failure<IEnumerable<PendingPaymentResponse>>(
                new Error("GetPendingPaymentsFailed", "Failed to retrieve pending payments", 500));
        }
    }

    #endregion

    #region BOOKINGS MANAGEMENT

    public async Task<Result<PaginatedResponse<BookingComprehensiveResponse>>> GetCityBookingsAsync(
        string userId,
        Contracts.CityAdminContracts.BookingFilter filter)
    {
        try
        {
            var departmentId = await GetAdminDepartmentIdAsync(userId);
            if (!departmentId.IsSuccess)
                return Result.Failure<PaginatedResponse<BookingComprehensiveResponse>>(departmentId.Error);

            var unitIds = await _context.Units
                .Where(u => u.CityId == departmentId.Value && !u.IsDeleted)
                .Select(u => u.Id)
                .ToListAsync();

            var query = _context.Bookings
                .Include(b => b.Unit)
                .Include(b => b.User)
                .Include(b => b.BookingRooms)
                    .ThenInclude(br => br.Room)
                .Where(b => unitIds.Contains(b.UnitId))
                .AsQueryable();

            // Apply filters
            if (filter.Status.HasValue)
                query = query.Where(b => b.Status == filter.Status.Value);

            if (filter.PaymentStatus.HasValue)
                query = query.Where(b => b.PaymentStatus == filter.PaymentStatus.Value);

            if (filter.StartDate.HasValue)
                query = query.Where(b => b.CheckInDate >= filter.StartDate.Value);

            if (filter.EndDate.HasValue)
                query = query.Where(b => b.CheckOutDate <= filter.EndDate.Value);

            var totalCount = await query.CountAsync();

            var bookings = await query
                .OrderByDescending(b => b.CreatedAt)
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            var responses = bookings.Select(b => new BookingComprehensiveResponse
            {
                Id = b.Id,
                BookingNumber = b.BookingNumber,
                UnitId = b.UnitId,
                UnitName = b.Unit.Name,
                UserId = b.UserId,
                GuestName = b.User.FullName ?? "N/A",
                GuestEmail = b.User.Email ?? string.Empty,
                CheckInDate = b.CheckInDate,
                CheckOutDate = b.CheckOutDate,
                NumberOfGuests = b.NumberOfGuests,
                NumberOfNights = b.NumberOfNights,
                TotalPrice = b.TotalPrice,
                PaidAmount = b.PaidAmount,
                Status = b.Status.ToString(),
                PaymentStatus = b.PaymentStatus.ToString(),
                Rooms = b.BookingRooms.Select(br => new BookedRoomInfo
                {
                    RoomId = br.RoomId,
                    RoomNumber = br.Room.RoomNumber,
                    PricePerNight = br.PricePerNight
                }).ToList(),
                CreatedAt = b.CreatedAt
            }).ToList();

            var paginatedResult = CreatePaginatedResponse(responses, totalCount, filter.Page, filter.PageSize);
            return Result.Success(paginatedResult);
        }
        catch (Exception ex)
        {
            return Result.Failure<PaginatedResponse<BookingComprehensiveResponse>>(
                new Error("GetBookingsFailed", "Failed to retrieve bookings", 500));
        }
    }

    public async Task<Result<BookingDetailsResponse>> GetBookingDetailsAsync(
        string userId,
        int bookingId)
    {
        try
        {
            var booking = await _context.Bookings
                .Include(b => b.Unit)
                .Include(b => b.User)
                .Include(b => b.BookingRooms)
                    .ThenInclude(br => br.Room)
                .Include(b => b.Payments)
                .FirstOrDefaultAsync(b => b.Id == bookingId);

            if (booking == null)
                return Result.Failure<BookingDetailsResponse>(
                    new Error("NotFound", "Booking not found", 404));

            var hasAccess = await IsUnitInMyCityAsync(userId, booking.UnitId);
            if (!hasAccess.Value)
                return Result.Failure<BookingDetailsResponse>(
                    new Error("NoAccess", "You do not have access to this booking", 403));

            var response = new BookingDetailsResponse
            {
                Id = booking.Id,
                BookingNumber = booking.BookingNumber,
                UnitId = booking.UnitId,
                UnitName = booking.Unit.Name,
                UserId = booking.UserId,
                GuestName = booking.User.FullName ?? "N/A",
                GuestEmail = booking.User.Email ?? string.Empty,
                GuestPhone = booking.User.PhoneNumber ?? string.Empty,
                CheckInDate = booking.CheckInDate,
                CheckOutDate = booking.CheckOutDate,
                NumberOfGuests = booking.NumberOfGuests,
                NumberOfNights = booking.NumberOfNights,
                TotalPrice = booking.TotalPrice,
                PaidAmount = booking.PaidAmount,
                RemainingBalance = booking.TotalPrice - booking.PaidAmount,
                Status = booking.Status.ToString(),
                PaymentStatus = booking.PaymentStatus.ToString(),
                SpecialRequests = booking.SpecialRequests,
                CancellationReason = booking.CancellationReason,
                CancelledAt = booking.CancelledAt,
                Rooms = booking.BookingRooms.Select(br => new BookedRoomInfo
                {
                    RoomId = br.RoomId,
                    RoomNumber = br.Room.RoomNumber,
                    PricePerNight = br.PricePerNight
                }).ToList(),
                Payments = booking.Payments.Select(p => new Contracts.CityAdminContracts.PaymentInfo
                {
                    Id = p.Id,
                    TransactionId = p.TransactionId,
                    Amount = p.Amount,
                    PaymentMethod = p.PaymentMethod.ToString(),
                    Status = p.Status.ToString(),
                    PaymentDate = p.PaymentDate
                }).ToList(),
                CreatedAt = booking.CreatedAt,
                UpdatedAt = booking.UpdatedAt
            };

            return Result.Success(response);
        }
        catch (Exception ex)
        {
            return Result.Failure<BookingDetailsResponse>(
                new Error("GetBookingFailed", "Failed to retrieve booking details", 500));
        }
    }


    public async Task<Result> ConfirmBookingAsync(string userId, int bookingId)
    {
        try
        {
            var booking = await _context.Bookings
                .Include(b => b.Unit)
                .FirstOrDefaultAsync(b => b.Id == bookingId);

            if (booking == null)
                return Result.Failure(new Error("NotFound", "Booking not found", 404));

            var hasAccess = await IsUnitInMyCityAsync(userId, booking.UnitId);
            if (!hasAccess.Value)
                return Result.Failure(
                    new Error("NoAccess", "You do not have access to this booking", 403));

            if (booking.Status != BookingStatus.Pending)
                return Result.Failure(
                    new Error("InvalidStatus", $"Cannot confirm booking with status {booking.Status}", 400));

            booking.Status = BookingStatus.Confirmed;
            booking.UpdatedAt = DateTime.UtcNow.AddHours(3);

            await _context.SaveChangesAsync();

            BackgroundJob.Enqueue(() => _emailNotificationService.SendBookingStatusUpdateEmailAsync(bookingId, "Confirmed"));

            _logger.LogInformation(
                "Booking {BookingId} confirmed by city admin {UserId}",
                bookingId, userId);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error confirming booking {BookingId}", bookingId);
            return Result.Failure(new Error("ConfirmFailed", "Failed to confirm booking", 500));
        }
    }

    public async Task<Result> CheckInBookingAsync(string userId, int bookingId)
    {
        try
        {
            var booking = await _context.Bookings
                .Include(b => b.Unit)
                .FirstOrDefaultAsync(b => b.Id == bookingId);

            if (booking == null)
                return Result.Failure(new Error("NotFound", "Booking not found", 404));

            var hasAccess = await IsUnitInMyCityAsync(userId, booking.UnitId);
            if (!hasAccess.Value)
                return Result.Failure(
                    new Error("NoAccess", "You do not have access to this booking", 403));

            if (booking.Status != BookingStatus.Confirmed)
                return Result.Failure(
                    new Error("InvalidStatus", "Only confirmed bookings can be checked in", 400));

            if (DateTime.UtcNow.Date < booking.CheckInDate.Date)
                return Result.Failure(
                    new Error("TooEarly", "Check-in date has not arrived yet", 400));

            booking.Status = BookingStatus.CheckedIn;
            booking.UpdatedAt = DateTime.UtcNow.AddHours(3);

            await _context.SaveChangesAsync();

            BackgroundJob.Enqueue(() => _emailNotificationService.SendBookingStatusUpdateEmailAsync(bookingId, "CheckedIn"));

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking in booking {BookingId}", bookingId);
            return Result.Failure(new Error("CheckInFailed", "Failed to check in booking", 500));
        }
    }

    public async Task<Result> CheckOutBookingAsync(string userId, int bookingId)
    {
        try
        {
            var booking = await _context.Bookings
                .Include(b => b.Unit)
                .FirstOrDefaultAsync(b => b.Id == bookingId);

            if (booking == null)
                return Result.Failure(new Error("NotFound", "Booking not found", 404));

            var hasAccess = await IsUnitInMyCityAsync(userId, booking.UnitId);
            if (!hasAccess.Value)
                return Result.Failure(
                    new Error("NoAccess", "You do not have access to this booking", 403));

            if (booking.Status != BookingStatus.CheckedIn)
                return Result.Failure(
                    new Error("InvalidStatus", "Only checked-in bookings can be checked out", 400));

            booking.Status = BookingStatus.Completed;
            booking.UpdatedAt = DateTime.UtcNow.AddHours(3);

            await _context.SaveChangesAsync();

            BackgroundJob.Enqueue(() => _emailNotificationService.SendBookingCheckoutEmailAsync(bookingId));

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking out booking {BookingId}", bookingId);
            return Result.Failure(new Error("CheckOutFailed", "Failed to check out booking", 500));
        }
    }

    public async Task<Result> CancelBookingAsync(string userId, int bookingId, string cancellationReason)
    {
        try
        {
            var booking = await _context.Bookings
                .Include(b => b.Unit)
                    .ThenInclude(u => u.CancellationPolicy)
                .FirstOrDefaultAsync(b => b.Id == bookingId);

            if (booking == null)
                return Result.Failure(new Error("NotFound", "Booking not found", 404));

            var hasAccess = await IsUnitInMyCityAsync(userId, booking.UnitId);
            if (!hasAccess.Value)
                return Result.Failure(
                    new Error("NoAccess", "You do not have access to this booking", 403));

            if (booking.Status == BookingStatus.Completed || booking.Status == BookingStatus.Cancelled)
                return Result.Failure(
                    new Error("InvalidStatus", "Cannot cancel this booking", 400));

            // Calculate refund
            var refundAmount = CalculateRefundAmount(booking);

            booking.Status = BookingStatus.Cancelled;
            booking.CancellationReason = cancellationReason;
            booking.CancelledAt = DateTime.UtcNow.AddHours(3);
            booking.UpdatedAt = DateTime.UtcNow.AddHours(3);

            await _context.SaveChangesAsync();

            // Restore coupon usage if applied
            var bookingCoupon = await _context.Set<BookingCoupon>()
                .Include(bc => bc.Coupon)
                .FirstOrDefaultAsync(bc => bc.BookingId == bookingId);

            if (bookingCoupon != null)
            {
                var coupon = bookingCoupon.Coupon;
                if (coupon.CurrentUsageCount > 0)
                    coupon.CurrentUsageCount--;

                _context.Set<BookingCoupon>().Remove(bookingCoupon);
                await _context.SaveChangesAsync();
            }

            // Process refund if applicable
            if (refundAmount > 0)
            {
                BackgroundJob.Enqueue(() =>
                    RefundBookingAsync(userId, bookingId, refundAmount, "Booking cancelled"));
            }

            BackgroundJob.Enqueue(() => _emailNotificationService.SendBookingCancellationEmailAsync(bookingId, refundAmount));

            _logger.LogInformation(
                "Booking {BookingId} cancelled by city admin {UserId}. Refund: {Refund}",
                bookingId, userId, refundAmount);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling booking {BookingId}", bookingId);
            return Result.Failure(new Error("CancelFailed", "Failed to cancel booking", 500));
        }
    }

    public async Task<Result> ProcessBookingPaymentAsync(string userId, int bookingId, ProcessPaymentRequest request)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var booking = await _context.Bookings
                .Include(b => b.Unit)
                .FirstOrDefaultAsync(b => b.Id == bookingId);

            if (booking == null)
                return Result.Failure(new Error("NotFound", "Booking not found", 404));

            var hasAccess = await IsUnitInMyCityAsync(userId, booking.UnitId);
            if (!hasAccess.Value)
                return Result.Failure(
                    new Error("NoAccess", "You do not have access to this booking", 403));

            var payment = new Payment
            {
                BookingId = bookingId,
                TransactionId = request.TransactionId,
                Amount = request.Amount,
                PaymentMethod = (Domain.PaymentMethod)request.PaymentMethod,
                Status = PaymentStatus.Paid,
                PaymentDate = DateTime.UtcNow.AddHours(3),
                Notes = request.Notes
            };

            await _context.Set<Payment>().AddAsync(payment);

            booking.PaidAmount += request.Amount;

            if (booking.PaidAmount >= booking.TotalPrice)
            {
                booking.PaymentStatus = PaymentStatus.Paid;
                booking.Status = BookingStatus.Confirmed;
            }
            else
            {
                booking.PaymentStatus = PaymentStatus.PartiallyPaid;
            }

            booking.UpdatedAt = DateTime.UtcNow.AddHours(3);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            BackgroundJob.Enqueue(() => _emailNotificationService.SendPaymentConfirmationEmailAsync(bookingId, request.Amount));

            return Result.Success();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error processing payment for booking {BookingId}", bookingId);
            return Result.Failure(new Error("PaymentFailed", "Failed to process payment", 500));
        }
    }

    public async Task<Result> RefundBookingAsync(string userId, int bookingId, decimal refundAmount, string reason)
    {
        try
        {
            var booking = await _context.Bookings
                .Include(b => b.Unit)
                .FirstOrDefaultAsync(b => b.Id == bookingId);

            if (booking == null)
                return Result.Failure(new Error("NotFound", "Booking not found", 404));

            var hasAccess = await IsUnitInMyCityAsync(userId, booking.UnitId);
            if (!hasAccess.Value)
                return Result.Failure(
                    new Error("NoAccess", "You do not have access to this booking", 403));

            var refundPayment = new Payment
            {
                BookingId = bookingId,
                TransactionId = $"REFUND-{Guid.NewGuid()}",
                Amount = -refundAmount,
                PaymentMethod = PaymentMethod.BankTransfer,
                Status = PaymentStatus.Paid,
                PaymentDate = DateTime.UtcNow.AddHours(3),
                Notes = reason
            };

            await _context.Set<Payment>().AddAsync(refundPayment);

            booking.PaidAmount -= refundAmount;
            booking.PaymentStatus = PaymentStatus.Refunded;

            await _context.SaveChangesAsync();

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refunding booking {BookingId}", bookingId);
            return Result.Failure(new Error("RefundFailed", "Failed to refund booking", 500));
        }
    }

    private decimal CalculateRefundAmount(Domain.Entities.Booking booking)
    {
        if (booking.Unit?.CancellationPolicy == null)
            return 0;

        var policy = booking.Unit.CancellationPolicy;
        var daysUntilCheckIn = (booking.CheckInDate - DateTime.UtcNow).Days;

        if (daysUntilCheckIn >= policy.FullRefundDays)
            return booking.PaidAmount;

        if (daysUntilCheckIn >= policy.PartialRefundDays)
            return booking.PaidAmount * (policy.PartialRefundPercentage / 100);

        return 0;
    }

    #endregion

    #region UNITS MANAGEMENT

    public async Task<Result<PaginatedResponse<UnitComprehensiveResponse>>> GetCityUnitsAsync(string userId,
        UnitFilter filter)
    {
        try
        {
            var departmentId = await GetAdminDepartmentIdAsync(userId);
            if (!departmentId.IsSuccess)
                return Result.Failure<PaginatedResponse<UnitComprehensiveResponse>>(departmentId.Error);

            var query = _context.Units
                .Include(u => u.City)
                .Include(u => u.UnitType)
                .Include(u => u.Images.Where(i => !i.IsDeleted))
                .Include(u => u.UnitAmenities)
                    .ThenInclude(ua => ua.Amenity)
                .Include(u => u.Rooms.Where(r => !r.IsDeleted))
                .Where(u => u.CityId == departmentId.Value && !u.IsDeleted)
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrWhiteSpace(filter.Name))
                query = query.Where(u => u.Name.Contains(filter.Name));

            if (filter.IsActive.HasValue)
                query = query.Where(u => u.IsActive == filter.IsActive.Value);

            if (filter.IsVerified.HasValue)
                query = query.Where(u => u.IsVerified == filter.IsVerified.Value);

            var totalCount = await query.CountAsync();

            var units = await query.ToListAsync();

            // Get general policies
            var unitIds = units.Select(u => u.Id).ToList();
            var generalPolicies = await _context.GeneralPolicies
                .Include(p => p.CancellationPolicy)
                .Where(p => unitIds.Contains(p.UnitId.Value) && p.IsActive)
                .AsNoTracking()
                .ToListAsync();

            var responses = units.Select(u => MapToComprehensiveResponse(u,
                generalPolicies.Where(p => p.UnitId == u.Id).ToList())).ToList();

            var paginatedResult = CreatePaginatedResponse(responses, totalCount, filter.Page, filter.PageSize);
            return Result.Success(paginatedResult);
        }
        catch (Exception ex)
        {
            return Result.Failure<PaginatedResponse<UnitComprehensiveResponse>>(
                new Error("GetUnitsFailed", "Failed to retrieve units", 500));
        }
    }

    public async Task<Result<UnitComprehensiveResponse>> GetUnitDetailsAsync(
        string userId,
        int unitId)
    {
        try
        {
            var hasAccess = await IsUnitInMyCityAsync(userId, unitId);
            if (!hasAccess.Value)
                return Result.Failure<UnitComprehensiveResponse>(
                    new Error("NoAccess", "You do not have access to this unit", 403));

            var unit = await _context.Units
                .Include(u => u.City)
                .Include(u => u.UnitType)
                .Include(u => u.CancellationPolicy)
                .Include(u => u.Admins.Where(a => a.IsActive))
                    .ThenInclude(a => a.User)
                .Include(u => u.Images.Where(i => !i.IsDeleted))
                .Include(u => u.UnitAmenities)
                    .ThenInclude(ua => ua.Amenity)
                .Include(u => u.Rooms.Where(r => !r.IsDeleted))
                    .ThenInclude(r => r.SubUnitImages.Where(i => !i.IsDeleted))
                .Include(u => u.Rooms.Where(r => !r.IsDeleted))
                    .ThenInclude(r => r.SubUnitAmenities)
                    .ThenInclude(sa => sa.Amenity)
                .FirstOrDefaultAsync(u => u.Id == unitId && !u.IsDeleted);

            if (unit == null)
                return Result.Failure<UnitComprehensiveResponse>(
                    new Error("NotFound", "Unit not found", 404));

            var generalPolicies = await _context.GeneralPolicies
                .Include(p => p.CancellationPolicy)
                .Where(p => p.UnitId == unitId && p.IsActive)
                .AsNoTracking()
                .ToListAsync();

            var response = MapToComprehensiveResponse(unit, generalPolicies);
            return Result.Success(response);
        }
        catch (Exception ex)
        {
            return Result.Failure<UnitComprehensiveResponse>(
                new Error("GetUnitFailed", "Failed to retrieve unit details", 500));
        }
    }

    public async Task<Result> ToggleUnitVerificationAsync(
        string userId,
        int unitId)
    {
        try
        {
            var hasAccess = await IsUnitInMyCityAsync(userId, unitId);
            if (!hasAccess.Value)
                return Result.Failure(
                    new Error("NoAccess", "You do not have access to this unit", 403));

            var unit = await _context.Units
                .FirstOrDefaultAsync(u => u.Id == unitId && !u.IsDeleted);

            if (unit == null)
                return Result.Failure(new Error("NotFound", "Unit not found", 404));

            unit.IsVerified = !unit.IsVerified;
            unit.UpdatedAt = DateTime.UtcNow.AddHours(3);

            await _context.SaveChangesAsync();
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(new Error("ToggleFailed", "Failed to update unit verification", 500));
        }
    }

    public async Task<Result> ToggleUnitFeaturedAsync(
        string userId,
        int unitId)
    {
        try
        {
            var hasAccess = await IsUnitInMyCityAsync(userId, unitId);
            if (!hasAccess.Value)
                return Result.Failure(
                    new Error("NoAccess", "You do not have access to this unit", 403));

            var unit = await _context.Units
                .FirstOrDefaultAsync(u => u.Id == unitId && !u.IsDeleted);

            if (unit == null)
                return Result.Failure(new Error("NotFound", "Unit not found", 404));

            unit.IsFeatured = !unit.IsFeatured;
            unit.UpdatedAt = DateTime.UtcNow.AddHours(3);

            await _context.SaveChangesAsync();

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(new Error("ToggleFailed", "Failed to update unit featured status", 500));
        }
    }

    public async Task<Result> ToggleUnitStatusAsync(
        string userId,
        int unitId)
    {
        try
        {
            var hasAccess = await IsUnitInMyCityAsync(userId, unitId);
            if (!hasAccess.Value)
                return Result.Failure(
                    new Error("NoAccess", "You do not have access to this unit", 403));

            var unit = await _context.Units
                .FirstOrDefaultAsync(u => u.Id == unitId && !u.IsDeleted);

            if (unit == null)
                return Result.Failure(new Error("NotFound", "Unit not found", 404));

            unit.IsActive = !unit.IsActive;
            unit.UpdatedAt = DateTime.UtcNow.AddHours(3);

            await _context.SaveChangesAsync();

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(new Error("ToggleFailed", "Failed to update unit status", 500));
        }
    }

    #endregion

    #region MAPPING HELPER

    private UnitComprehensiveResponse MapToComprehensiveResponse(
       Domain.Entities.Unit unit,
        List<GeneralPolicy> policies)
    {
        return new UnitComprehensiveResponse
        {
            Id = unit.Id,
            Name = unit.Name,
            Description = unit.Description,
            Address = unit.Address,
            Latitude = unit.Latitude,
            Longitude = unit.Longitude,
            CityId = unit.CityId,
            CityName = unit.City.Name,
            UnitTypeId = unit.UnitTypeId,
            UnitTypeName = unit.UnitType.Name,
            BasePrice = unit.BasePrice,
            MaxGuests = unit.MaxGuests,
            Bedrooms = unit.Bedrooms,
            Bathrooms = unit.Bathrooms,
            IsActive = unit.IsActive,
            IsVerified = unit.IsVerified,
            IsFeatured = unit.IsFeatured,
            AverageRating = unit.AverageRating,
            TotalReviews = unit.TotalReviews,
            TotalRooms = unit.Rooms.Count(r => !r.IsDeleted),
            AvailableRooms = unit.Rooms.Count(r => !r.IsDeleted && r.IsAvailable),
            Images = unit.Images.OrderBy(i => i.DisplayOrder).Select(i => new ImageResponse
            {
                Id = i.Id,
                ImageUrl = i.ImageUrl,
                IsPrimary = i.IsPrimary,
                Caption = i.Caption
            }).ToList(),
            Amenities = unit.UnitAmenities.Select(ua => new AmenityResponse
            {
                Id = ua.Amenity.Id,
                Name = ua.Amenity.Name,
                Category = ua.Amenity.Category,
                IsAvailable = ua.IsAvailable
            }).ToList(),
            Policies = policies.Select(p => new PolicyResponse
            {
                Id = p.Id,
                Title = p.Title,
                Description = p.Description,
                PolicyType = p.PolicyType.ToString()
            }).ToList(),
            CreatedAt = unit.CreatedAt,
            UpdatedAt = unit.UpdatedAt
        };
    }

    #endregion

    #region DASHBOARD & OVERVIEW

    public async Task<Result<CityAdminDashboardResponse>> GetDashboardAsync(string userId)
    {
        try
        {
            var departmentId = await GetAdminDepartmentIdAsync(userId);
            if (!departmentId.IsSuccess)
                return Result.Failure<CityAdminDashboardResponse>(departmentId.Error);

            var deptId = departmentId.Value;
            var today = DateTime.UtcNow.AddHours(3).Date;

            // Get department info
            var department = await _context.Departments
                .Include(d => d.Units.Where(u => !u.IsDeleted))
                    .ThenInclude(u => u.Rooms.Where(r => !r.IsDeleted))
                .Include(d => d.Units.Where(u => !u.IsDeleted))
                    .ThenInclude(u => u.Bookings)
                .FirstOrDefaultAsync(d => d.Id == deptId && !d.IsDeleted);

            if (department == null)
                return Result.Failure<CityAdminDashboardResponse>(
                    new Error("NotFound", "Department not found", 404));

            var unitIds = department.Units.Select(u => u.Id).ToList();

            // Department summary
            var deptSummary = new DepartmentSummary
            {
                Id = department.Id,
                Name = department.Name,
                Country = department.Country,
                Description = department.Description,
                ImageUrl = department.ImageUrl,
                TotalUnits = department.Units.Count,
                ActiveUnits = department.Units.Count(u => u.IsActive),
                VerifiedUnits = department.Units.Count(u => u.IsVerified),
                TotalSubUnits = department.Units.Sum(u => u.Rooms.Count),
                AverageRating = department.AverageRating,
                TotalReviews = department.TotalBookings
            };

            // Dashboard stats
            var allBookings = department.Units.SelectMany(u => u.Bookings).ToList();

            var stats = new CityDashboardStats
            {
                TotalUnits = department.Units.Count,
                ActiveUnits = department.Units.Count(u => u.IsActive),
                VerifiedUnits = department.Units.Count(u => u.IsVerified),
                PendingRegistrations = await _context.Set<UnitRegistrationRequest>()
                    .CountAsync(r => r.DepartmentId == deptId && r.Status == RegistrationRequestStatus.Pending),
                TodayCheckIns = allBookings.Count(b => b.CheckInDate.Date == today &&
                    b.Status == BookingStatus.Confirmed),
                TodayCheckOuts = allBookings.Count(b => b.CheckOutDate.Date == today &&
                    b.Status == BookingStatus.CheckedIn),
                CurrentGuests = allBookings.Count(b => b.Status == BookingStatus.CheckedIn),
                TotalBookings = allBookings.Count,
                ActiveBookings = allBookings.Count(b => b.Status == BookingStatus.Confirmed ||
                    b.Status == BookingStatus.CheckedIn),
                TodayRevenue = allBookings
                    .Where(b => b.CreatedAt.Date == today && b.Status == BookingStatus.Completed)
                    .Sum(b => b.TotalPrice),
                WeekRevenue = allBookings
                    .Where(b => b.CreatedAt >= today.AddDays(-7) && b.Status == BookingStatus.Completed)
                    .Sum(b => b.TotalPrice),
                MonthRevenue = allBookings
                    .Where(b => b.CreatedAt.Month == today.Month && b.Status == BookingStatus.Completed)
                    .Sum(b => b.TotalPrice),
                OccupancyRate = await CalculateCityOccupancyRateAsync(deptId, today.AddDays(-30), today)
            };

            // Top performing units
            var topUnits = department.Units
                .OrderByDescending(u => u.AverageRating)
                .Take(5)
                .Select(u => new UnitPerformance
                {
                    UnitId = u.Id,
                    UnitName = u.Name,
                    Revenue = u.Bookings.Where(b => b.Status == BookingStatus.Completed)
                        .Sum(b => b.TotalPrice),
                    BookingCount = u.Bookings.Count,
                    Rating = u.AverageRating,
                    OccupancyRate = 0
                }).ToList();

            // Recent bookings
            var recentBookings = await _context.Bookings
                .Where(b => unitIds.Contains(b.UnitId))
                .Include(b => b.Unit)
                .Include(b => b.User)
                .OrderByDescending(b => b.CreatedAt)
                .Take(10)
                .Select(b => new RecentBooking
                {
                    Id = b.Id,
                    BookingNumber = b.BookingNumber,
                    UnitName = b.Unit.Name,
                    GuestName = b.User.FullName ?? "N/A",
                    CheckInDate = b.CheckInDate,
                    CheckOutDate = b.CheckOutDate,
                    Status = b.Status.ToString(),
                    TotalPrice = b.TotalPrice,
                    CreatedAt = b.CreatedAt
                }).ToListAsync();

            // Pending requests
            var pendingRequests = await _context.Set<UnitRegistrationRequest>()
                .Where(r => r.DepartmentId == deptId && r.Status == RegistrationRequestStatus.Pending)
                .OrderBy(r => r.SubmittedAt)
                .Take(5)
                .Select(r => new PendingRegistrationRequest
                {
                    Id = r.Id,
                    UnitName = r.UnitName,
                    OwnerName = r.OwnerFullName,
                    OwnerEmail = r.OwnerEmail,
                    SubmittedAt = r.SubmittedAt,
                    DaysWaiting = (int)(DateTime.UtcNow - r.SubmittedAt).TotalDays
                }).ToListAsync();

            // Revenue overview
            var monthlyRevenue = allBookings
                .Where(b => b.Status == BookingStatus.Completed)
                .GroupBy(b => new { b.CreatedAt.Year, b.CreatedAt.Month })
                .OrderByDescending(g => g.Key.Year)
                .ThenByDescending(g => g.Key.Month)
                .Take(12)
                .Select(g => new MonthlyRevenueData
                {
                    Month = $"{g.Key.Year}-{g.Key.Month:D2}",
                    Revenue = g.Sum(b => b.TotalPrice),
                    BookingCount = g.Count()
                }).ToList();

            var revenueOverview = new CityRevenueOverview
            {
                TodayRevenue = stats.TodayRevenue,
                WeekRevenue = stats.WeekRevenue,
                MonthRevenue = stats.MonthRevenue,
                YearRevenue = allBookings
                    .Where(b => b.CreatedAt.Year == today.Year && b.Status == BookingStatus.Completed)
                    .Sum(b => b.TotalPrice),
                PendingPayments = allBookings
                    .Where(b => b.PaymentStatus == PaymentStatus.Pending ||
                        b.PaymentStatus == PaymentStatus.PartiallyPaid)
                    .Sum(b => b.TotalPrice - b.PaidAmount),
                MonthlyTrend = monthlyRevenue
            };

            var dashboard = new CityAdminDashboardResponse
            {
                Department = deptSummary,
                Stats = stats,
                TopPerformingUnits = topUnits,
                RecentBookings = recentBookings,
                PendingRequests = pendingRequests,
                TodayCheckIns = [],
                TodayCheckOuts = [],
                RevenueOverview = revenueOverview
            };

            return Result.Success(dashboard);
        }
        catch (Exception ex)
        {
            return Result.Failure<CityAdminDashboardResponse>(
                new Error("DashboardFailed", "Failed to load dashboard", 500));
        }
    }

    public async Task<Result<CityAdminStatsResponse>> GetQuickStatsAsync(string userId)
    {
        try
        {
            var departmentId = await GetAdminDepartmentIdAsync(userId);
            if (!departmentId.IsSuccess)
                return Result.Failure<CityAdminStatsResponse>(departmentId.Error);

            var deptId = departmentId.Value;

            var department = await _context.Departments
                .Include(d => d.Units.Where(u => !u.IsDeleted))
                    .ThenInclude(u => u.Rooms.Where(r => !r.IsDeleted))
                .FirstOrDefaultAsync(d => d.Id == deptId && !d.IsDeleted);

            if (department == null)
                return Result.Failure<CityAdminStatsResponse>(
                    new Error("NotFound", "Department not found", 404));

            var unitIds = department.Units.Select(u => u.Id).ToList();

            var stats = new CityAdminStatsResponse
            {
                TotalUnits = department.Units.Count,
                ActiveUnits = department.Units.Count(u => u.IsActive),
                VerifiedUnits = department.Units.Count(u => u.IsVerified),
                FeaturedUnits = department.Units.Count(u => u.IsFeatured),
                TotalSubUnits = department.Units.Sum(u => u.Rooms.Count),
                AvailableSubUnits = department.Units.Sum(u => u.Rooms.Count(r => r.IsAvailable)),
                TotalBookings = await _context.Bookings.CountAsync(b => unitIds.Contains(b.UnitId)),
                ActiveBookings = await _context.Bookings
                    .CountAsync(b => unitIds.Contains(b.UnitId) &&
                        (b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.CheckedIn)),
                CompletedBookings = await _context.Bookings
                    .CountAsync(b => unitIds.Contains(b.UnitId) && b.Status == BookingStatus.Completed),
                TotalRevenue = await _context.Bookings
                    .Where(b => unitIds.Contains(b.UnitId) && b.Status == BookingStatus.Completed)
                    .SumAsync(b => b.TotalPrice),
                PendingRevenue = await _context.Bookings
                    .Where(b => unitIds.Contains(b.UnitId) &&
                        (b.Status == BookingStatus.Pending || b.Status == BookingStatus.Confirmed))
                    .SumAsync(b => b.TotalPrice),
                AverageRating = department.AverageRating ?? 0,
                TotalReviews = department.TotalBookings,
                OccupancyRate = await CalculateCityOccupancyRateAsync(deptId,
                    DateTime.UtcNow.AddDays(-30), DateTime.UtcNow),
                TotalUsers = await _context.Bookings
                    .Where(b => unitIds.Contains(b.UnitId))
                    .Select(b => b.UserId)
                    .Distinct()
                    .CountAsync()
            };

            return Result.Success(stats);
        }
        catch (Exception ex)
        {
            return Result.Failure<CityAdminStatsResponse>(
                new Error("StatsFailed", "Failed to load statistics", 500));
        }
    }

    #endregion

    #region DEPARTMENT MANAGEMENT


    public async Task<Result<DepartmentDetailsResponse>> GetDepartmentDetailsByIdAsync(
    string userId,
    int departmentId)
    {
        try
        {
            var hasAccess = await IsCityAdminAsync(userId, departmentId);
            if (!hasAccess.Value)
                return Result.Failure<DepartmentDetailsResponse>(
                    new Error("NoAccess", "You do not have access to this department", 403));

            var department = await _context.Departments
                .Include(d => d.DepartmentImages.Where(i => !i.IsDeleted))
                .Include(d => d.DepartmentAdmins.Where(a => a.IsActive))
                    .ThenInclude(a => a.User)
                .FirstOrDefaultAsync(d => d.Id == departmentId && !d.IsDeleted);

            if (department == null)
                return Result.Failure<DepartmentDetailsResponse>(
                    new Error("NotFound", "Department not found", 404));

            var response = new DepartmentDetailsResponse
            {
                Id = department.Id,
                Name = department.Name,
                Country = department.Country,
                Description = department.Description,
                ImageUrl = department.ImageUrl,
                Latitude = department.Latitude,
                Longitude = department.Longitude,
                IsActive = department.IsActive,
                TotalUnits = department.TotalUnits,
                TotalBookings = department.TotalBookings,
                AverageRating = department.AverageRating,
                Images = department.DepartmentImages.Select(i => new DepartmentImageResponse
                {
                    Id = i.Id,
                    ImageUrl = i.ImageUrl,
                    ThumbnailUrl = i.ThumbnailUrl,
                    IsPrimary = i.IsPrimary,
                    DisplayOrder = i.DisplayOrder,
                    Caption = i.Caption,
                    ImageType = i.ImageType.ToString(),
                    UploadedAt = i.UploadedAt
                }).ToList(),
                Admins = department.DepartmentAdmins.Select(a => new DepartmentAdminInfo
                {
                    Id = a.Id,
                    UserId = a.UserId,
                    UserName = a.User.FullName ?? "N/A",
                    Email = a.User.Email ?? string.Empty,
                    IsPrimary = a.IsPrimary,
                    IsActive = a.IsActive,
                    AssignedAt = a.AssignedAt
                }).ToList(),
                CreatedAt = department.CreatedAt
            };

            return Result.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting department {DepartmentId}", departmentId);
            return Result.Failure<DepartmentDetailsResponse>(
                new Error("GetDepartmentFailed", "Failed to retrieve department details", 500));
        }
    }

    public async Task<Result<DepartmentDetailsResponse>> GetMyDepartmentDetailsAsync(string userId)
    {
        try
        {
            var departmentId = await GetAdminDepartmentIdAsync(userId);
            if (!departmentId.IsSuccess)
                return Result.Failure<DepartmentDetailsResponse>(departmentId.Error);

            var department = await _context.Departments
                .Include(d => d.DepartmentImages.Where(i => !i.IsDeleted))
                .Include(d => d.DepartmentAdmins.Where(a => a.IsActive))
                    .ThenInclude(a => a.User)
                .FirstOrDefaultAsync(d => d.Id == departmentId.Value && !d.IsDeleted);

            if (department == null)
                return Result.Failure<DepartmentDetailsResponse>(
                    new Error("NotFound", "Department not found", 404));

            var response = new DepartmentDetailsResponse
            {
                Id = department.Id,
                Name = department.Name,
                Country = department.Country,
                Description = department.Description,
                ImageUrl = department.ImageUrl,
                Latitude = department.Latitude,
                Longitude = department.Longitude,
                IsActive = department.IsActive,
                TotalUnits = department.TotalUnits,
                TotalBookings = department.TotalBookings,
                AverageRating = department.AverageRating,
                Images = department.DepartmentImages.Select(i => new DepartmentImageResponse
                {
                    Id = i.Id,
                    ImageUrl = i.ImageUrl,
                    ThumbnailUrl = i.ThumbnailUrl,
                    IsPrimary = i.IsPrimary,
                    DisplayOrder = i.DisplayOrder,
                    Caption = i.Caption,
                    ImageType = i.ImageType.ToString(),
                    UploadedAt = i.UploadedAt
                }).ToList(),
                Admins = department.DepartmentAdmins.Select(a => new DepartmentAdminInfo
                {
                    Id = a.Id,
                    UserId = a.UserId,
                    UserName = a.User.FullName ?? "N/A",
                    Email = a.User.Email ?? string.Empty,
                    IsPrimary = a.IsPrimary,
                    IsActive = a.IsActive,
                    AssignedAt = a.AssignedAt
                }).ToList(),
                CreatedAt = department.CreatedAt
            };

            return Result.Success(response);
        }
        catch (Exception ex)
        {
            return Result.Failure<DepartmentDetailsResponse>(
                new Error("GetDepartmentFailed", "Failed to retrieve department details", 500));
        }
    }

    public async Task<Result<DepartmentResponse>> UpdateMyDepartmentAsync(
        string userId,
        int departmentId,
        UpdateDepartmentRequest request)
    {
        try
        {
            var hasAccess = await IsCityAdminAsync(userId, departmentId);
            if (!hasAccess.Value)
                return Result.Failure<DepartmentResponse>(
                    new Error("NoAccess", "You do not have access to this department", 403));

            var department = await _context.Departments
                .FirstOrDefaultAsync(d => d.Id == departmentId && !d.IsDeleted);

            if (department == null)
                return Result.Failure<DepartmentResponse>(
                    new Error("NotFound", "Department not found", 404));

            // Update fields
            if (request.Name != null) department.Name = request.Name;
            if (request.Country != null) department.Country = request.Country;
            if (request.Description != null) department.Description = request.Description;
            if (request.Latitude.HasValue) department.Latitude = request.Latitude.Value;
            if (request.Longitude.HasValue) department.Longitude = request.Longitude.Value;

            await _context.SaveChangesAsync();

            var response = new DepartmentResponse
            {
                Id = department.Id,
                Name = department.Name,
                Country = department.Country,
                Description = department.Description,
                IsActive = department.IsActive,
                UpdatedAt = DateTime.UtcNow.AddHours(3)
            };

            return Result.Success(response);
        }
        catch (Exception ex)
        {
            return Result.Failure<DepartmentResponse>(
                new Error("UpdateFailed", "Failed to update department", 500));
        }
    }

    #endregion

    #region HELPER METHODS

    public async Task<Result<bool>> IsCityAdminAsync(string userId, int departmentId)
    {
        var isAdmin = await _context.Set<DepartmentAdmin>()
            .AnyAsync(a => a.CityId == departmentId && a.UserId == userId && a.IsActive);

        return Result.Success(isAdmin);
    }

    public async Task<Result<bool>> IsUnitInMyCityAsync(string userId, int unitId)
    {
        var departmentId = await GetAdminDepartmentIdAsync(userId);
        if (!departmentId.IsSuccess)
            return Result.Success(false);

        var unit = await _context.Units
            .FirstOrDefaultAsync(u => u.Id == unitId && u.CityId == departmentId.Value && !u.IsDeleted);

        return Result.Success(unit != null);
    }

    public async Task<Result<int>> GetAdminDepartmentIdAsync(string userId)
    {
        var admin = await _context.Set<DepartmentAdmin>()
            .FirstOrDefaultAsync(a => a.UserId == userId && a.IsActive);

        if (admin == null)
            return Result.Failure<int>(
                new Error("NoAccess", "User is not a city administrator", 403));

        return Result.Success(admin.CityId);
    }

    private async Task<decimal> CalculateCityOccupancyRateAsync(
        int departmentId,
        DateTime startDate,
        DateTime endDate)
    {
        var unitIds = await _context.Units
            .Where(u => u.CityId == departmentId && !u.IsDeleted)
            .Select(u => u.Id)
            .ToListAsync();

        var totalRoomNights = await _context.SubUnits
            .Where(s => unitIds.Contains(s.UnitId) && !s.IsDeleted)
            .CountAsync() * (endDate - startDate).Days;

        if (totalRoomNights == 0)
            return 0;

        var occupiedNights = await _context.BookingRooms
            .Include(br => br.Booking)
            .Where(br => unitIds.Contains(br.Room.UnitId) &&
                        br.Booking.CheckInDate < endDate &&
                        br.Booking.CheckOutDate > startDate &&
                        br.Booking.Status != BookingStatus.Cancelled)
            .SumAsync(br => br.NumberOfNights);

        return (decimal)occupiedNights / totalRoomNights * 100;
    }

    #endregion

    #region NOTIFICATIONS MANAGEMENT

    public async Task<Result> SendCityWideNotificationAsync(
        string userId,
        SendNotificationRequest request)
    {
        try
        {
            var departmentId = await GetAdminDepartmentIdAsync(userId);
            if (!departmentId.IsSuccess)
                return Result.Failure(departmentId.Error);

            // Get all hotel admins in this city
            var unitIds = await _context.Units
                .Where(u => u.CityId == departmentId.Value && !u.IsDeleted)
                .Select(u => u.Id)
                .ToListAsync();

            var adminUserIds = await _context.Set<UniteAdmin>()
                .Where(ua => unitIds.Contains(ua.UnitId) && ua.IsActive)
                .Select(ua => ua.UserId)
                .Distinct()
                .ToListAsync();

            // Create notification
            var notification = new Notification
            {
                Title = request.Title,
                Message = request.Message,
                Type = request.Type,
                Priority = request.Priority,
                CreatedByUserId = userId,
                CreatedByRole = "CityAdmin",
                Target = NotificationTarget.HotelAdmins,
                TargetDepartmentId = departmentId.Value,
                IsSent = true,
                SentAt = DateTime.UtcNow.AddHours(3),
                CreatedAt = DateTime.UtcNow.AddHours(3),
                TotalRecipients = adminUserIds.Count
            };

            await _context.Set<Notification>().AddAsync(notification);
            await _context.SaveChangesAsync();

            // Create user notifications
            var userNotifications = adminUserIds.Select(adminId => new UserNotification
            {
                NotificationId = notification.Id,
                UserId = adminId,
                ReceivedAt = DateTime.UtcNow.AddHours(3)
            }).ToList();

            await _context.Set<UserNotification>().AddRangeAsync(userNotifications);
            await _context.SaveChangesAsync();

            notification.DeliveredCount = userNotifications.Count;
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "City-wide notification sent by {UserId} to {Count} admins in department {DepartmentId}",
                userId, adminUserIds.Count, departmentId.Value);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending city-wide notification");
            return Result.Failure(new Error("SendFailed", "Failed to send notification", 500));
        }
    }

    public async Task<Result<IEnumerable<AdminNotificationResponse>>> GetMyNotificationsAsync(
        string userId,
        NotificationFilter filter)
    {
        try
        {
            var query = _context.Set<UserNotification>()
                .Include(un => un.Notification)
                    .ThenInclude(n => n.TargetUnit)
                .Where(un => un.UserId == userId)
                .AsQueryable();

            if (filter.IsRead.HasValue)
                query = query.Where(un => un.IsRead == filter.IsRead.Value);

            if (filter.Type.HasValue)
                query = query.Where(un => un.Notification.Type == filter.Type.Value);

            var notifications = await query
                .OrderByDescending(un => un.ReceivedAt)
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(un => new AdminNotificationResponse
                {
                    Id = un.Id,
                    NotificationId = un.NotificationId,
                    Title = un.Notification.Title,
                    Message = un.Notification.Message,
                    Type = un.Notification.Type.ToString(),
                    Priority = un.Notification.Priority.ToString(),
                    IsRead = un.IsRead,
                    ReadAt = un.ReadAt,
                    ReceivedAt = un.ReceivedAt,
                    TargetUnitId = un.Notification.TargetUnitId,
                    TargetUnitName = un.Notification.TargetUnit != null ? un.Notification.TargetUnit.Name : null
                })
                .ToListAsync();

            return Result.Success<IEnumerable<AdminNotificationResponse>>(notifications);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting notifications for user {UserId}", userId);
            return Result.Failure<IEnumerable<AdminNotificationResponse>>(
                new Error("GetNotificationsFailed", "Failed to get notifications", 500));
        }
    }

    public async Task<Result> MarkNotificationAsReadAsync(string userId, int notificationId)
    {
        try
        {
            var userNotification = await _context.Set<UserNotification>()
                .FirstOrDefaultAsync(un => un.Id == notificationId && un.UserId == userId);

            if (userNotification == null)
                return Result.Failure(new Error("NotFound", "Notification not found", 404));

            if (!userNotification.IsRead)
            {
                userNotification.IsRead = true;
                userNotification.ReadAt = DateTime.UtcNow.AddHours(3);
                await _context.SaveChangesAsync();
            }

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking notification as read");
            return Result.Failure(new Error("MarkReadFailed", "Failed to mark notification as read", 500));
        }
    }

    #endregion

    #region USER/GUEST MANAGEMENT

    public async Task<Result<PaginatedResponse<CityUserResponse>>> GetCityUsersAsync(
        string userId,
        UserFilter filter)
    {
        try
        {
            var departmentId = await GetAdminDepartmentIdAsync(userId);
            if (!departmentId.IsSuccess)
                return Result.Failure<PaginatedResponse<CityUserResponse>>(departmentId.Error);

            var unitIds = await _context.Units
                .Where(u => u.CityId == departmentId.Value && !u.IsDeleted)
                .Select(u => u.Id)
                .ToListAsync();

            var query = _context.Bookings
                .Where(b => unitIds.Contains(b.UnitId))
                .GroupBy(b => b.UserId)
                .Select(g => new
                {
                    UserId = g.Key,
                    TotalBookings = g.Count(),
                    TotalSpent = g.Sum(b => b.TotalPrice),
                    LastBookingDate = g.Max(b => b.CreatedAt)
                })
                .AsQueryable();

            if (filter.StartDate.HasValue)
                query = query.Where(u => u.LastBookingDate >= filter.StartDate.Value);

            if (filter.EndDate.HasValue)
                query = query.Where(u => u.LastBookingDate <= filter.EndDate.Value);

            if (filter.MinBookings.HasValue)
                query = query.Where(u => u.TotalBookings >= filter.MinBookings.Value);

            var userStats = await query
                .OrderByDescending(u => u.TotalSpent)
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            var userIds = userStats.Select(u => u.UserId).ToList();
            var users = await _context.Users
                .Where(u => userIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id);

            var totalCount = await query.CountAsync();

            var responses = userStats.Select(stat => new CityUserResponse
            {
                UserId = stat.UserId,
                FullName = users.ContainsKey(stat.UserId) ? users[stat.UserId].FullName ?? "N/A" : "N/A",
                Email = users.ContainsKey(stat.UserId) ? users[stat.UserId].Email ?? "" : "",
                PhoneNumber = users.ContainsKey(stat.UserId) ? users[stat.UserId].PhoneNumber : null,
                ProfileImageUrl = users.ContainsKey(stat.UserId) ? users[stat.UserId].AvatarUrl : null, // ADD THIS
                TotalBookings = stat.TotalBookings,
                TotalSpent = stat.TotalSpent,
                LastBookingDate = stat.LastBookingDate,
                CreatedAt = users.ContainsKey(stat.UserId) ? users[stat.UserId].CreatedAt : DateTime.UtcNow
            }).ToList();

            var paginatedResult = CreatePaginatedResponse(responses, totalCount, filter.Page, filter.PageSize);
            return Result.Success(paginatedResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting city users");
            return Result.Failure<PaginatedResponse<CityUserResponse>>(
                new Error("GetUsersFailed", "Failed to retrieve users", 500));
        }
    }

    public async Task<Result<IEnumerable<BookingComprehensiveResponse>>> GetUserBookingHistoryAsync(
        string userId,
        string targetUserId)
    {
        try
        {
            var departmentId = await GetAdminDepartmentIdAsync(userId);
            if (!departmentId.IsSuccess)
                return Result.Failure<IEnumerable<BookingComprehensiveResponse>>(departmentId.Error);

            var unitIds = await _context.Units
                .Where(u => u.CityId == departmentId.Value && !u.IsDeleted)
                .Select(u => u.Id)
                .ToListAsync();

            var bookings = await _context.Bookings
                .Include(b => b.Unit)
                .Include(b => b.User)
                .Include(b => b.BookingRooms)
                    .ThenInclude(br => br.Room)
                .Where(b => b.UserId == targetUserId && unitIds.Contains(b.UnitId))
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();

            var responses = bookings.Select(b => new BookingComprehensiveResponse
            {
                Id = b.Id,
                BookingNumber = b.BookingNumber,
                UnitId = b.UnitId,
                UnitName = b.Unit.Name,
                UserId = b.UserId,
                GuestName = b.User.FullName ?? "N/A",
                GuestEmail = b.User.Email ?? string.Empty,
                CheckInDate = b.CheckInDate,
                CheckOutDate = b.CheckOutDate,
                NumberOfGuests = b.NumberOfGuests,
                NumberOfNights = b.NumberOfNights,
                TotalPrice = b.TotalPrice,
                PaidAmount = b.PaidAmount,
                Status = b.Status.ToString(),
                PaymentStatus = b.PaymentStatus.ToString(),
                Rooms = b.BookingRooms.Select(br => new BookedRoomInfo
                {
                    RoomId = br.RoomId,
                    RoomNumber = br.Room.RoomNumber,
                    PricePerNight = br.PricePerNight
                }).ToList(),
                CreatedAt = b.CreatedAt
            }).ToList();

            return Result.Success<IEnumerable<BookingComprehensiveResponse>>(responses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user booking history");
            return Result.Failure<IEnumerable<BookingComprehensiveResponse>>(
                new Error("GetHistoryFailed", "Failed to retrieve booking history", 500));
        }
    }

    #endregion

    #region LOYALTY PROGRAM

    public async Task<Result<LoyaltyProgramStatisticsResponse>> GetCityLoyaltyStatisticsAsync(string userId)
    {
        try
        {
            var departmentId = await GetAdminDepartmentIdAsync(userId);
            if (!departmentId.IsSuccess)
                return Result.Failure<LoyaltyProgramStatisticsResponse>(departmentId.Error);

            var unitIds = await _context.Units
                .Where(u => u.CityId == departmentId.Value && !u.IsDeleted)
                .Select(u => u.Id)
                .ToListAsync();

            var userIds = await _context.Bookings
                .Where(b => unitIds.Contains(b.UnitId))
                .Select(b => b.UserId)
                .Distinct()
                .ToListAsync();

            var loyaltyPrograms = await _context.Set<LoyaltyProgram>()
                .Where(lp => userIds.Contains(lp.UserId))
                .ToListAsync();

            var stats = new LoyaltyProgramStatisticsResponse
            {
                TotalMembers = loyaltyPrograms.Count,
                BronzeMembers = loyaltyPrograms.Count(lp => lp.Tier == LoyaltyTier.Bronze),
                SilverMembers = loyaltyPrograms.Count(lp => lp.Tier == LoyaltyTier.Silver),
                GoldMembers = loyaltyPrograms.Count(lp => lp.Tier == LoyaltyTier.Gold),
                PlatinumMembers = loyaltyPrograms.Count(lp => lp.Tier == LoyaltyTier.Platinum),
                TotalPointsIssued = loyaltyPrograms.Sum(lp => lp.LifetimePoints),
                TotalPointsRedeemed = loyaltyPrograms.Sum(lp => lp.LifetimePoints - lp.TotalPoints)
            };

            return Result.Success(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting loyalty statistics");
            return Result.Failure<LoyaltyProgramStatisticsResponse>(
                new Error("GetStatsFailed", "Failed to retrieve loyalty statistics", 500));
        }
    }

    #endregion

    #region BULK OPERATIONS

    public async Task<Result> BulkVerifyUnitsAsync(
        string userId,
        BulkUnitActionRequest request)
    {
        try
        {
            var hasAccess = await IsCityAdminAsync(userId, request.Value ? 1 : 0); // Need department ID
            var departmentId = await GetAdminDepartmentIdAsync(userId);
            if (!departmentId.IsSuccess)
                return Result.Failure(departmentId.Error);

            var units = await _context.Units
                .Where(u => request.UnitIds.Contains(u.Id) &&
                           u.CityId == departmentId.Value &&
                           !u.IsDeleted)
                .ToListAsync();

            if (units.Count != request.UnitIds.Count)
                return Result.Failure(
                    new Error("InvalidUnits", "Some units not found or not accessible", 400));

            foreach (var unit in units)
            {
                unit.IsVerified = request.Value;
                unit.UpdatedAt = DateTime.UtcNow.AddHours(3);
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Bulk verified {Count} units by city admin {UserId}",
                units.Count, userId);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error bulk verifying units");
            return Result.Failure(new Error("BulkVerifyFailed", "Failed to bulk verify units", 500));
        }
    }

    public async Task<Result> BulkFeatureUnitsAsync(
        string userId,
        BulkUnitActionRequest request)
    {
        try
        {
            var departmentId = await GetAdminDepartmentIdAsync(userId);
            if (!departmentId.IsSuccess)
                return Result.Failure(departmentId.Error);

            var units = await _context.Units
                .Where(u => request.UnitIds.Contains(u.Id) &&
                           u.CityId == departmentId.Value &&
                           !u.IsDeleted)
                .ToListAsync();

            if (units.Count != request.UnitIds.Count)
                return Result.Failure(
                    new Error("InvalidUnits", "Some units not found or not accessible", 400));

            foreach (var unit in units)
            {
                unit.IsFeatured = request.Value;
                unit.UpdatedAt = DateTime.UtcNow.AddHours(3);
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Bulk featured {Count} units by city admin {UserId}",
                units.Count, userId);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error bulk featuring units");
            return Result.Failure(new Error("BulkFeatureFailed", "Failed to bulk feature units", 500));
        }
    }

    public async Task<Result> BulkToggleUnitStatusAsync(
        string userId,
        BulkUnitActionRequest request)
    {
        try
        {
            var departmentId = await GetAdminDepartmentIdAsync(userId);
            if (!departmentId.IsSuccess)
                return Result.Failure(departmentId.Error);

            var units = await _context.Units
                .Where(u => request.UnitIds.Contains(u.Id) &&
                           u.CityId == departmentId.Value &&
                           !u.IsDeleted)
                .ToListAsync();

            if (units.Count != request.UnitIds.Count)
                return Result.Failure(
                    new Error("InvalidUnits", "Some units not found or not accessible", 400));

            foreach (var unit in units)
            {
                unit.IsActive = request.Value;
                unit.UpdatedAt = DateTime.UtcNow.AddHours(3);
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Bulk toggled status for {Count} units by city admin {UserId}",
                units.Count, userId);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error bulk toggling unit status");
            return Result.Failure(new Error("BulkToggleFailed", "Failed to bulk toggle unit status", 500));
        }
    }

    #endregion



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