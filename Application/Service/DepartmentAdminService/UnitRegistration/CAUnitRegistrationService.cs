using Application.Abstraction;
using Application.Abstraction.Consts;
using Application.Contracts.UnitRegisteration;
using Application.Helpers;
using Application.Service.Availability;

using Application.Service.DepartmentAdminService.CurrentDpartmentAdmin;
using Application.Service.S3Image;
using Domain;
using Domain.Consts;
using Domain.Entities;
using Hangfire;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using System.Text.Json;

namespace Application.Service.DepartmentAdminService.UnitRegistration;

public class CAUnitRegistrationService(
    ApplicationDbcontext context,
    UserManager<ApplicationUser> userManager,
    IS3ImageService s3Service,
    IEmailSender emailSender,
    IHttpContextAccessor httpContextAccessor,
    ILogger<CAUnitRegistrationService> logger,
    IConfiguration configuration,
    IAvailabilityService service,
    ICurrentDpartmentAdmin CurrentDepartmentAdmin
) : ICAUnitRegistrationService
{
    private readonly ApplicationDbcontext _context = context;
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly IS3ImageService _s3Service = s3Service;
    private readonly IEmailSender _emailSender = emailSender;
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
    private readonly ILogger<CAUnitRegistrationService> _logger = logger;
    private readonly IAvailabilityService _Service = service;
    private readonly ICurrentDpartmentAdmin _CurrentDepartmentAdmin = CurrentDepartmentAdmin;




    // ============= PUBLIC METHODS =============

    public async Task<Result<int>> SubmitRegistrationAsync(SubmitUnitRegistrationRequest request)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            // 1. Validate email isn't already in use
            var emailExists = await _userManager.FindByEmailAsync(request.OwnerEmail) != null;
            if (emailExists)
                return Result.Failure<int>(
                    new Error("EmailInUse", "This email is already registered", 400));

            var existingRequest = await _context.Set<UnitRegistrationRequest>()
                .AnyAsync(r => r.OwnerEmail == request.OwnerEmail &&
                              r.Status == RegistrationRequestStatus.Pending);

            if (existingRequest)
                return Result.Failure<int>(
                    new Error("PendingRequest",
                        "You already have a pending registration request", 400));

            // 2. Validate department and unit type exist
            var departmentExists = await _context.Departments
                .AnyAsync(d => d.Id == request.DepartmentId && !d.IsDeleted);

            if (!departmentExists)
                return Result.Failure<int>(
                    new Error("InvalidDepartment", "Invalid department selected", 400));

            var unitTypeExists = await _context.UnitTypes
                .AnyAsync(ut => ut.Id == request.UnitTypeId && ut.IsActive);

            if (!unitTypeExists)
                return Result.Failure<int>(
                    new Error("InvalidUnitType", "Invalid unit type selected", 400));

            // 3. Create registration request entity
            var registrationRequest = new UnitRegistrationRequest
            {
                OwnerFullName = request.OwnerFullName,
                OwnerEmail = request.OwnerEmail.ToLowerInvariant(),
                OwnerPhoneNumber = request.OwnerPhoneNumber,
                OwnerPassword = new PasswordHasher<ApplicationUser>()
                    .HashPassword(null!, request.OwnerPassword),

                UnitName = request.UnitName,
                Description = request.Description,
                Address = request.Address,
                DepartmentId = request.DepartmentId,
                UnitTypeId = request.UnitTypeId,
                Latitude = request.Latitude,
                Longitude = request.Longitude,
                BasePrice = request.BasePrice,
                MaxGuests = request.MaxGuests,
                Bedrooms = request.Bedrooms,
                Bathrooms = request.Bathrooms,
                Status = RegistrationRequestStatus.Pending,
                SubmittedAt = DateTime.UtcNow.AddHours(3)
            };

            await _context.Set<UnitRegistrationRequest>().AddAsync(registrationRequest);
            await _context.SaveChangesAsync();

            // 4. Upload images to S3
            var uploadResult = await _s3Service.UploadRegistrationImagesAsync(
                request.Images,
                registrationRequest.Id);

            if (!uploadResult.IsSuccess)
            {
                await transaction.RollbackAsync();
                return Result.Failure<int>(uploadResult.Error);
            }

            // 5. Store S3 keys as JSON
            registrationRequest.ImageS3Keys = JsonSerializer.Serialize(uploadResult.Value);
            registrationRequest.ImageCount = uploadResult.Value.Count;

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            // 6. Send confirmation email to requester
            await SendRequestConfirmationEmailAsync(registrationRequest);

            // 7. Notify Cityadmins of new request
            await NotifyCityAdminsOfNewRequestAsync(registrationRequest);

            _logger.LogInformation(
                "New unit registration request submitted. ID: {RequestId}, Email: {Email}",
                registrationRequest.Id, request.OwnerEmail);

            return Result.Success(registrationRequest.Id);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error submitting registration request");

            return Result.Failure<int>(
                new Error("SubmissionFailed",
                    "Failed to submit registration request. Please try again.", 500));
        }
    }

    public async Task<Result<bool>> IsEmailAvailableAsync(string email)
    {
        var emailInUse = await _userManager.FindByEmailAsync(email) != null ||
                        await _context.Set<UnitRegistrationRequest>()
                            .AnyAsync(r => r.OwnerEmail == email &&
                                          r.Status == RegistrationRequestStatus.Pending);

        return Result.Success(!emailInUse);
    }

    // ============= ADMIN METHODS =============

    public async Task<Result<IEnumerable<UnitRegistrationResponse>>> GetAllRequestsAsync(
        UnitRegistrationListFilter filter)
    {
       

        var query = _context.Set<UnitRegistrationRequest>()
            .Where(r => r.DepartmentId == _CurrentDepartmentAdmin.CityId)
            .Include(r => r.Department)
            .Include(r => r.UnitType)
            .Include(r => r.ReviewedByAdmin)
            .AsQueryable();

        if (filter.Status.HasValue)
            query = query.Where(r => r.Status == filter.Status.Value);

        if (filter.UnitTypeId.HasValue)
            query = query.Where(r => r.UnitTypeId == filter.UnitTypeId.Value);

        if (filter.SubmittedFrom.HasValue)
            query = query.Where(r => r.SubmittedAt >= filter.SubmittedFrom.Value);

        if (filter.SubmittedTo.HasValue)
            query = query.Where(r => r.SubmittedAt <= filter.SubmittedTo.Value);

        if (!string.IsNullOrWhiteSpace(filter.SearchKeyword))
        {
            var keyword = filter.SearchKeyword.ToLower();
            query = query.Where(r =>
                r.UnitName.ToLower().Contains(keyword) ||
                r.OwnerFullName.ToLower().Contains(keyword) ||
                r.OwnerEmail.ToLower().Contains(keyword));
        }

        var requests = await query
            .OrderByDescending(r => r.SubmittedAt)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        var responses = requests.Select(MapToResponse).ToList();

        return Result.Success<IEnumerable<UnitRegistrationResponse>>(responses);
    }

    public async Task<Result<UnitRegistrationResponse>> GetRequestByIdAsync(int requestId)
    {
        

        var request = await _context.Set<UnitRegistrationRequest>()
            //.Where(r => r.DepartmentId == _CurrentDepartmentAdmin.CityId)
            .Include(r => r.Department)
            .Include(r => r.UnitType)
            .Include(r => r.ReviewedByAdmin)
            .Include(r => r.CreatedUser)
            .Include(r => r.CreatedUnit)
            .FirstOrDefaultAsync(r => r.Id == requestId && r.DepartmentId == _CurrentDepartmentAdmin.CityId);

        if (request == null)
            return Result.Failure<UnitRegistrationResponse>(
                new Error("NotFound", "Registration request not found", 404));

        var response = MapToResponse(request);
        return Result.Success(response);
    }

    public async Task<Result<ApprovalResult>> ApproveRequestAsync(
        int requestId,
        string adminUserId)
    {

        
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var request = await _context.Set<UnitRegistrationRequest>()
                .Where(r => r.DepartmentId == _CurrentDepartmentAdmin.CityId)
                .Include(r => r.Department)
                .Include(r => r.UnitType)
                .FirstOrDefaultAsync(r => r.Id == requestId);

            if (request == null)
                return Result.Failure<ApprovalResult>(
                    new Error("NotFound", "Registration request not found", 404));

            if (request.Status != RegistrationRequestStatus.Pending)
                return Result.Failure<ApprovalResult>(
                    new Error("InvalidStatus",
                        $"Request is already {request.Status}", 400));

            var emailExists = await _userManager.FindByEmailAsync(request.OwnerEmail);
            if (emailExists != null)
                return Result.Failure<ApprovalResult>(
                    new Error("EmailTaken",
                        "Email is now in use. Request cannot be approved.", 400));

            // Create the user account
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
                return Result.Failure<ApprovalResult>(
                    new Error("UserCreationFailed",
                        $"Failed to create user: {errors}", 500));
            }

            var roleResult = await _userManager.AddToRoleAsync(newUser, DefaultRoles.HotelAdmin);
            if (!roleResult.Succeeded)
            {
                await transaction.RollbackAsync();
                return Result.Failure<ApprovalResult>(
                    new Error("RoleAssignmentFailed",
                        "Failed to assign HotelAdmin role", 500));
            }

            // Create the Unit
            var unit = new Domain.Entities.Unit
            {
                Name = request.UnitName,
                Description = request.Description,
                Address = request.Address,
                CityId = _CurrentDepartmentAdmin.CityId,
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

            var availabilityInit = await service.InitializeUnitDefaultAvailabilityAsync(unit.Id, 365);


            await _context.Units.AddAsync(unit);
            await _context.SaveChangesAsync();


            if (!availabilityInit.IsSuccess)
            {
                _logger.LogWarning(
                    "Failed to initialize availability for unit {UnitId}: {Error}",
                    unit.Id, availabilityInit.Error.Description);
            }


            // Assign user as Unit Admin
            var unitAdmin = new UniteAdmin
            {
                UserId = newUser.Id,
                UnitId = unit.Id,
                IsActive = true,
                AssignedAt = DateTime.UtcNow.AddHours(3)
            };

            await _context.Set<UniteAdmin>().AddAsync(unitAdmin);

            // Move images from temp to unit folder
            var imageKeys = JsonSerializer.Deserialize<List<string>>(request.ImageS3Keys)
                           ?? new List<string>();

            var moveResult = await _s3Service.MoveImagesToUnitAsync(imageKeys, unit.Id);
            if (!moveResult.IsSuccess)
            {
                _logger.LogWarning(
                    "Failed to move images for unit {UnitId}: {Error}",
                    unit.Id, moveResult.Error.Description);
            }

            // Update registration request
            request.Status = RegistrationRequestStatus.Approved;
            request.ReviewedAt = DateTime.UtcNow.AddHours(3);
            request.ReviewedByAdminId = adminUserId;
            request.CreatedUserId = newUser.Id;
            request.CreatedUnitId = unit.Id;

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            // Send welcome email with credentials
            var emailSent = await SendWelcomeEmailAsync(
                newUser.Email!,
                newUser.FullName!,
                unit.Id,
                unit.Name);

            _logger.LogInformation(
                "Registration request {RequestId} approved. User: {UserId}, Unit: {UnitId}",
                requestId, newUser.Id, unit.Id);

            return Result.Success(new ApprovalResult
            {
                CreatedUserId = newUser.Id,
                CreatedUserEmail = newUser.Email!,
                CreatedUnitId = unit.Id,
                CreatedUnitName = unit.Name,
                EmailSent = emailSent
            });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error approving registration request {RequestId}", requestId);

            return Result.Failure<ApprovalResult>(
                new Error("ApprovalFailed",
                    "Failed to approve registration request", 500));
        }
    }

    public async Task<Result> RejectRequestAsync(
        int requestId,
        string adminUserId,
        string rejectionReason)
    {
       
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var request = await _context.Set<UnitRegistrationRequest>()
                .FirstOrDefaultAsync(r => r.Id == requestId && r.DepartmentId == _CurrentDepartmentAdmin.CityId);

            if (request == null)
                return Result.Failure(
                    new Error("NotFound", "Registration request not found", 404));

            if (request.Status != RegistrationRequestStatus.Pending)
                return Result.Failure(
                    new Error("InvalidStatus",
                        $"Request is already {request.Status}", 400));

            request.Status = RegistrationRequestStatus.Rejected;
            request.ReviewedAt = DateTime.UtcNow.AddHours(3);
            request.ReviewedByAdminId = adminUserId;
            request.RejectionReason = rejectionReason;

            await _context.SaveChangesAsync();

            var imageKeys = JsonSerializer.Deserialize<List<string>>(request.ImageS3Keys)
                           ?? new List<string>();

            if (imageKeys.Any())
            {
                await _s3Service.DeleteImagesAsync(imageKeys);
            }

            await transaction.CommitAsync();

            await SendRejectionEmailAsync(
                request.OwnerEmail,
                request.OwnerFullName,
                request.UnitName,
                rejectionReason);

            _logger.LogInformation(
                "Registration request {RequestId} rejected by admin {AdminId}",
                requestId, adminUserId);

            return Result.Success();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error rejecting registration request {RequestId}", requestId);

            return Result.Failure(
                new Error("RejectionFailed", "Failed to reject request", 500));
        }
    }

    public async Task<Result> DeleteRequestAsync(int requestId)
    {
     

        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var request = await _context.Set<UnitRegistrationRequest>()
                .FirstOrDefaultAsync(r => r.Id == requestId && r.DepartmentId == _CurrentDepartmentAdmin.CityId);

            if (request == null)
                return Result.Failure(
                    new Error("NotFound", "Registration request not found", 404));

            if (request.Status == RegistrationRequestStatus.Approved)
                return Result.Failure(
                    new Error("CannotDelete",
                        "Cannot delete approved requests", 400));

            var imageKeys = JsonSerializer.Deserialize<List<string>>(request.ImageS3Keys)
                           ?? new List<string>();

            if (imageKeys.Any())
            {
                await _s3Service.DeleteImagesAsync(imageKeys);
            }

            _context.Set<UnitRegistrationRequest>().Remove(request);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            _logger.LogInformation("Registration request {RequestId} deleted", requestId);

            return Result.Success();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error deleting registration request {RequestId}", requestId);

            return Result.Failure(
                new Error("DeleteFailed", "Failed to delete request", 500));
        }
    }

    public async Task<Result<UnitRegistrationStatistics>> GetStatisticsAsync()
    {
       

        var requests = await _context.Set<UnitRegistrationRequest>()
            .Where(r => r.DepartmentId == _CurrentDepartmentAdmin.CityId)
            .Include(r => r.Department)
            .Include(r => r.UnitType)
            .ToListAsync();

        var now = DateTime.UtcNow;
        var weekAgo = now.AddDays(-7);
        var monthAgo = now.AddMonths(-1);

        var stats = new UnitRegistrationStatistics
        {
            TotalRequests = requests.Count,
            PendingRequests = requests.Count(r => r.Status == RegistrationRequestStatus.Pending),
            UnderReviewRequests = requests.Count(r => r.Status == RegistrationRequestStatus.UnderReview),
            ApprovedRequests = requests.Count(r => r.Status == RegistrationRequestStatus.Approved),
            RejectedRequests = requests.Count(r => r.Status == RegistrationRequestStatus.Rejected),
            CancelledRequests = requests.Count(r => r.Status == RegistrationRequestStatus.Cancelled),

            RequestsThisWeek = requests.Count(r => r.SubmittedAt >= weekAgo),
            RequestsThisMonth = requests.Count(r => r.SubmittedAt >= monthAgo),

            RequestsByDepartment = requests
                .GroupBy(r => r.Department.Name)
                .ToDictionary(g => g.Key, g => g.Count()),

            RequestsByUnitType = requests
                .GroupBy(r => r.UnitType.Name)
                .ToDictionary(g => g.Key, g => g.Count())
        };

        return Result.Success(stats);
    }

    public async Task<Result> ResendCredentialsEmailAsync(int requestId)
    {
       

        var request = await _context.Set<UnitRegistrationRequest>()
            .Where(r=> r.DepartmentId == _CurrentDepartmentAdmin.CityId)
            .Include(r => r.CreatedUser)
            .Include(r => r.CreatedUnit)
            .FirstOrDefaultAsync(r => r.Id == requestId);

        if (request == null)
            return Result.Failure(
                new Error("NotFound", "Request not found", 404));

        if (request.Status != RegistrationRequestStatus.Approved ||
            request.CreatedUser == null)
            return Result.Failure(
                new Error("NotApproved", "Request is not approved", 400));

        await SendWelcomeEmailAsync(
            request.CreatedUser.Email!,
            request.CreatedUser.FullName!,
            request.CreatedUnitId!.Value,
            request.CreatedUnit!.Name);

        return Result.Success();
    }

    // ============= EMAIL METHODS =============

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

    private async Task NotifyCityAdminsOfNewRequestAsync(UnitRegistrationRequest request)
    {
        try
        {
            // Get all City Admins
            var cityAdmins = await _context.DepartmentAdmins
                            .Where(ca => ca.CityId == request.DepartmentId && ca.IsActive)
                            .Select(ca => ca.User)
                            .ToListAsync();
                                    ;
            var origin = _httpContextAccessor.HttpContext?.Request.Headers.Origin.ToString();

            foreach (var admin in cityAdmins.Where(u => !u.IsDisable && u.EmailConfirmed))
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

    private async Task<bool> SendWelcomeEmailAsync(
        string email,
        string fullName,
        int unitId,
        string unitName)
    {
        try
        {
            var origin = _httpContextAccessor.HttpContext?.Request.Headers.Origin.ToString();

            var emailBody = EmailBodyBuilder.GenerateEmailBody("UnitRegistrationApproval",
                new Dictionary<string, string>
                {
                    { "{{name}}", fullName },
                    { "{{unit_name}}", unitName },
                    { "{{unit_id}}", unitId.ToString() },
                    { "{{login_url}}", $"{origin}/auth/login" },
                    { "{{dashboard_url}}", $"{origin}/hotel-admin/dashboard" },
                    { "{{unit_setup_url}}", $"{origin}/hotel-admin/units/{unitId}/setup" }
                });

            BackgroundJob.Enqueue(() =>
                _emailSender.SendEmailAsync(
                    email,
                    "Hujjzy: Your Unit Registration is Approved!",
                    emailBody));

            await Task.CompletedTask;
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send welcome email to {Email}", email);
            return false;
        }
    }

    private async Task SendRejectionEmailAsync(
        string email,
        string fullName,
        string unitName,
        string reason)
    {
        try
        {
            var origin = _httpContextAccessor.HttpContext?.Request.Headers.Origin.ToString();

            var emailBody = EmailBodyBuilder.GenerateEmailBody("UnitRegistrationRejection",
                new Dictionary<string, string>
                {
                    { "{{name}}", fullName },
                    { "{{unit_name}}", unitName },
                    { "{{rejection_reason}}", reason },
                    { "{{contact_url}}", $"{origin}/contact" },
                    { "{{resubmit_url}}", $"{origin}/register-unit" }
                });

            BackgroundJob.Enqueue(() =>
                _emailSender.SendEmailAsync(
                    email,
                    "Hujjzy: Unit Registration Update",
                    emailBody));

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send rejection email to {Email}", email);
        }
    }

    // ============= HELPER METHODS =============

    private UnitRegistrationResponse MapToResponse(UnitRegistrationRequest request)
    {
        var imageKeys = JsonSerializer.Deserialize<List<string>>(request.ImageS3Keys)
                       ?? new List<string>();

        return new UnitRegistrationResponse
        {
            Id = request.Id,
            Status = request.Status,
            StatusDisplay = request.Status.ToString(),

            OwnerFullName = request.OwnerFullName,
            OwnerEmail = request.OwnerEmail,
            OwnerPhoneNumber = request.OwnerPhoneNumber,

            UnitName = request.UnitName,
            Description = request.Description,
            Address = request.Address,
            DepartmentId = _CurrentDepartmentAdmin.CityId,
            DepartmentName = request.Department?.Name ?? "",
            UnitTypeId = request.UnitTypeId,
            UnitTypeName = request.UnitType?.Name ?? "",
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            BasePrice = request.BasePrice,
            MaxGuests = request.MaxGuests,
            Bedrooms = request.Bedrooms,
            Bathrooms = request.Bathrooms,

            ImageUrls = imageKeys.Select(key => _s3Service.GetCloudFrontUrl(key)).ToList(),
            ImageCount = request.ImageCount,

            SubmittedAt = request.SubmittedAt,
            ReviewedAt = request.ReviewedAt,
            ReviewedByAdminName = request.ReviewedByAdmin?.FullName,
            RejectionReason = request.RejectionReason,

            CreatedUserId = request.CreatedUserId,
            CreatedUnitId = request.CreatedUnitId
        };
    }

}