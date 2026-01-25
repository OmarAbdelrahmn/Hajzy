using Amazon.S3;
using Amazon.S3.Transfer;
using Application.Abstraction;
using Application.Abstraction.Consts;
using Application.Abstraction.Errors;
using Application.Contracts.UnitRegisteration;
using Application.Helpers;
using Application.Service.Avilabilaties;
using Application.Service.ImageProcessingJob;
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
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using System.Text.Json;

namespace Application.Service.UnitRegistration;

public class UnitRegistrationService(
    ApplicationDbcontext context,
    UserManager<ApplicationUser> userManager,
    IS3ImageService s3Service,
    IEmailSender emailSender,
    IHttpContextAccessor httpContextAccessor,
    ILogger<UnitRegistrationService> logger,
    IConfiguration configuration ,
    IAvailabilityService service ,
    IAmazonS3 amazonS3) : IUnitRegistrationService
{
    private readonly ApplicationDbcontext _context = context;
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly IS3ImageService _s3Service = s3Service;
    private readonly IEmailSender _emailSender = emailSender;
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
    private readonly ILogger<UnitRegistrationService> _logger = logger;
    private readonly IConfiguration configuration = configuration;
    private readonly IAvailabilityService service = service;
    private readonly IAmazonS3 amazonS3 = amazonS3;


    // ============= PUBLIC METHODS =============
    public async Task<Result<ImageProcessingStatusDto>> GetProcessingStatusAsync(int requestId)
    {
        var request = await _context.Set<UnitRegistrationRequest>()
            .AsNoTracking()
            .Where(r => r.Id == requestId)
            .Select(r => new ImageProcessingStatusDto
            {
                RequestId = r.Id,
                Status = r.ImageProcessingStatus,
                ProcessedAt = r.ImagesProcessedAt,
                Error = r.ImageProcessingError,
                TotalImages = r.ImageCount
            })
            .FirstOrDefaultAsync();

        if (request == null)
            return Result.Failure<ImageProcessingStatusDto>(
                new Error("NotFound", "Request not found", 404));

        return Result.Success(request);
    }

    // DTO
    public class ImageProcessingStatusDto
    {
        public int RequestId { get; set; }
        public ImageProcessingStatus Status { get; set; }
        public DateTime? ProcessedAt { get; set; }
        public string? Error { get; set; }
        public int TotalImages { get; set; }
        public string StatusDisplay => Status.ToString();
    }
    public async Task<Result<int>> SubmitRegistrationAsync(SubmitUnitRegistrationRequest request)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            // 1. Check for existing pending request
            var existingRequest = await _context.Set<UnitRegistrationRequest>()
                .AnyAsync(r => r.OwnerEmail == request.OwnerEmail &&
                              r.Status == RegistrationRequestStatus.Pending);

            if (existingRequest)
                return Result.Failure<int>(
                    new Error("PendingRequest",
                        "You already have a pending registration request", 400));

            // 2. Validate department and unit type
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

            // 3. Create registration request entity (NO IMAGES YET)
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
                SubmittedAt = DateTime.UtcNow.AddHours(3),

                // ⚡ Images will be added in separate endpoint
                ImageS3Keys = "[]",
                ImageCount = 0,
                ImageProcessingStatus = ImageProcessingStatus.Pending
            };

            await _context.Set<UnitRegistrationRequest>().AddAsync(registrationRequest);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            // Send confirmation email
            await SendRequestConfirmationEmailAsync(registrationRequest);

            // Notify admins
            await NotifyAdminsOfNewRequestAsync(registrationRequest);

            return Result.Success(registrationRequest.Id);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();

            return Result.Failure<int>(
                new Error("SubmissionFailed",
                    "Failed to submit registration request. Please try again.", 500));
        }
    }

    public async Task<Result<ImageUploadResult>> UploadRegistrationImagesAsync(
        int requestId,
        List<IFormFile> images)
    {
        try
        {
            // 1. Validate request exists and is pending
            var request = await _context.Set<UnitRegistrationRequest>()
                .FirstOrDefaultAsync(r => r.Id == requestId);

            if (request == null)
                return Result.Failure<ImageUploadResult>(
                    new Error("NotFound", "Registration request not found", 404));

            if (request.Status != RegistrationRequestStatus.Pending)
                return Result.Failure<ImageUploadResult>(
                    new Error("InvalidStatus", "Can only upload images for pending requests", 400));

            // 2. Validate images
            var validationResult = ValidateImages(images);
            if (!validationResult.IsSuccess)
                return Result.Failure<ImageUploadResult>(validationResult.Error);

            _logger.LogInformation(
                "Starting image upload and processing for request {RequestId}, {Count} images",
                requestId, images.Count);

            // 3. Update status to Processing
            request.ImageProcessingStatus = ImageProcessingStatus.Processing;
            await _context.SaveChangesAsync();

            // 4. Process images synchronously (convert to WebP THEN upload)
            var uploadedKeys = new List<string>();
            var failedImages = new List<string>();
            var transferUtility = new TransferUtility(amazonS3);

            for (int i = 0; i < images.Count; i++)
            {
                var image = images[i];

                try
                {

                    // Convert to WebP in memory FIRST
                    using var inputStream = image.OpenReadStream();
                    using var webpStream = new MemoryStream();

                    await ConvertToWebpAsync(inputStream, webpStream);
                    webpStream.Position = 0;

                    // NOW upload the WebP to S3
                    var s3Key = $"registrations/temp/request-{requestId}/{Guid.NewGuid()}.webp";

                    var uploadRequest = new TransferUtilityUploadRequest
                    {
                        InputStream = webpStream,
                        Key = s3Key,
                        BucketName = "hujjzy-bucket",
                        ContentType = "image/webp",
                        CannedACL = S3CannedACL.Private,
                        Metadata =
                    {
                        ["original-filename"] = image.FileName,
                        ["uploaded-at"] = DateTime.UtcNow.ToString("o"),
                        ["request-id"] = requestId.ToString(),
                        ["index"] = i.ToString()
                    }
                    };

                    await transferUtility.UploadAsync(uploadRequest);
                    uploadedKeys.Add(s3Key);

                    _logger.LogDebug(
                        "Successfully uploaded image {Index}/{Total}: {Key}",
                        i + 1, images.Count, s3Key);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "Failed to process image {Index}/{Total}: {FileName}",
                        i + 1, images.Count, image.FileName);

                    failedImages.Add(image.FileName);
                }
            }

            // 5. Update database with results
            request.ImageS3Keys = JsonSerializer.Serialize(uploadedKeys);
            request.ImageCount = uploadedKeys.Count;
            request.ImagesProcessedAt = DateTime.UtcNow.AddHours(3);

            if (failedImages.Count == 0)
            {
                request.ImageProcessingStatus = ImageProcessingStatus.Completed;
                request.ImageProcessingError = null;
            }
            else if (uploadedKeys.Any())
            {
                request.ImageProcessingStatus = ImageProcessingStatus.Completed;
                request.ImageProcessingError = $"{failedImages.Count} images failed: {string.Join(", ", failedImages)}";
            }
            else
            {
                request.ImageProcessingStatus = ImageProcessingStatus.Failed;
                request.ImageProcessingError = "All images failed to upload";
            }

            await _context.SaveChangesAsync();


            return Result.Success(new ImageUploadResult
            {
                RequestId = requestId,
                TotalImages = images.Count,
                SuccessfulUploads = uploadedKeys.Count,
                FailedUploads = failedImages.Count,
                FailedFileNames = failedImages,
                UploadedKeys = uploadedKeys
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Critical error uploading images for request {RequestId}",
                requestId);

            // Update status to failed
            try
            {
                var request = await _context.Set<UnitRegistrationRequest>()
                    .FirstOrDefaultAsync(r => r.Id == requestId);

                if (request != null)
                {
                    request.ImageProcessingStatus = ImageProcessingStatus.Failed;
                    request.ImageProcessingError = ex.Message;
                    await _context.SaveChangesAsync();
                }
            }
            catch { /* Ignore secondary errors */ }

            return Result.Failure<ImageUploadResult>(
                new Error("UploadFailed",
                    $"Failed to upload images: {ex.Message}", 500));
        }
    }

    private Result ValidateImages(List<IFormFile> images)
    {
        if (images.Count < 1 || images.Count > 15)
            return Result.Failure(
                new Error("InvalidImageCount", "Between 1 and 15 images required", 400));

        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
        var maxFileSize = 10 * 1024 * 1024; // 10MB

        foreach (var image in images)
        {
            var extension = Path.GetExtension(image.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
                return Result.Failure(
                    new Error("InvalidFormat", $"Invalid image format: {extension}", 400));

            if (image.Length > maxFileSize)
                return Result.Failure(
                    new Error("FileTooLarge", "Image size must be less than 10MB", 400));

            if (image.Length == 0)
                return Result.Failure(
                    new Error("EmptyFile", "Empty image file detected", 400));
        }

        return Result.Success();
    }

    public class ImageUploadResult
    {
        public int RequestId { get; set; }
        public int TotalImages { get; set; }
        public int SuccessfulUploads { get; set; }
        public int FailedUploads { get; set; }
        public List<string> FailedFileNames { get; set; } = new();
        public List<string> UploadedKeys { get; set; } = new();
    }


    private async Task ConvertToWebpAsync(Stream input, Stream output)
    {
        // Run CPU-intensive work on thread pool
        await Task.Run(() =>
        {
            using var image = Image.Load(input);

            var encoder = new WebpEncoder
            {
                Quality = 75,
                Method = WebpEncodingMethod.Fastest,
                SkipMetadata = true
            };

            image.Save(output, encoder);
        });
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
            .Include(r => r.Department)
            .Include(r => r.UnitType)
            .Include(r => r.ReviewedByAdmin)
            .AsQueryable();

        if (filter.Status.HasValue)
            query = query.Where(r => r.Status == filter.Status.Value);

        if (filter.DepartmentId.HasValue)
            query = query.Where(r => r.DepartmentId == filter.DepartmentId.Value);

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
    public async Task<Result<IEnumerable<UnitRegistrationResponse>>> GetAllRequestsAsync(
        string UserID
        )
    {

        var DepartmentIDs = await _context.DepartmentAdmins
            .Where(da => da.UserId == UserID && da.IsActive)
            .Select(da => da.CityId)
            .ToListAsync();

        if (DepartmentIDs.Count == 0)
        {
            return Result.Failure<IEnumerable<UnitRegistrationResponse>>(
                new Error("NoDepartments", "User is not an admin of any department", 403));
        }




        var query = _context.Set<UnitRegistrationRequest>()
            .Where(r => DepartmentIDs.Contains(r.DepartmentId))
            .Include(r => r.Department)
            .Include(r => r.UnitType)
            .Include(r => r.ReviewedByAdmin)
            .AsQueryable();


        var requests = await query
            .ToListAsync();

        var responses = requests.Select(MapToResponse).ToList();

        return Result.Success<IEnumerable<UnitRegistrationResponse>>(responses);
    }

    public async Task<Result<UnitRegistrationResponse>> GetRequestByIdAsync(int requestId)
    {
        var request = await _context.Set<UnitRegistrationRequest>()
            .Include(r => r.Department)
            .Include(r => r.UnitType)
            .Include(r => r.ReviewedByAdmin)
            .Include(r => r.CreatedUser)
            .Include(r => r.CreatedUnit)
            .FirstOrDefaultAsync(r => r.Id == requestId);

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

            // Check if user already exists
            var existingUser = await _userManager.FindByEmailAsync(request.OwnerEmail);
            ApplicationUser user;
            bool userCreated = false;

            if (existingUser != null)
            {
                // User already exists, use the existing user
                user = existingUser;

                // Check if user has User role and upgrade to HotelAdmin
                var userRoles = await _userManager.GetRolesAsync(user);
                if (userRoles.Contains(DefaultRoles.User))
                {
                    await _userManager.RemoveFromRoleAsync(user, DefaultRoles.User);
                    await _userManager.AddToRoleAsync(user, DefaultRoles.HotelAdmin);

                    //_logger.LogInformation(
                    //    "Upgraded user {UserId} from User to HotelAdmin for registration request {RequestId}",
                    //    user.Id, requestId);
                }
                else
                {

                    return Result.Failure<ApprovalResult>(new Error("the user has admin role", "this user dont' have the correct role for this operation", 400));
                    //_logger.LogInformation(
                    //    "Using existing user {UserId} for registration request {RequestId}",
                    //    user.Id, requestId);
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

            var availabilityInit = await service.InitializeUnitAvailabilityAsync(unit.Id, 365);

            //if (!availabilityInit.IsSuccess)
            //{
            //    _logger.LogWarning(
            //        "Failed to initialize availability for unit {UnitId}: {Error}",
            //        unit.Id, availabilityInit.Error.Description);
            //}

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
            var imageKeys = JsonSerializer.Deserialize<List<string>>(request.ImageS3Keys)
                           ?? new List<string>();

            var moveResult = await _s3Service.MoveImagesToUnitAsync(imageKeys, unit.Id);

            //if (moveResult.IsSuccess)
            //{


            //    var permanentKeys = moveResult.Value;

            //    foreach (var (permanentKey, index) in permanentKeys.Select((k, i) => (k, i)))
            //    {
            //        var unitImage = new Domain.Entities.UnitImage
            //        {
            //            UnitId = unit.Id,

            //            // ORIGINAL
            //            ImageUrl = _s3Service.GetCloudFrontUrl(permanentKey),
            //            S3Key = permanentKey,
            //            S3Bucket = "huzjjy-bucket",

            //            // THUMBNAIL
            //            ThumbnailUrl = _s3Service.GetCloudFrontUrl(GetThumbnailKey(permanentKey)),
            //            ThumbnailS3Key = GetThumbnailKey(permanentKey),

            //            // MEDIUM
            //            MediumUrl = _s3Service.GetCloudFrontUrl(GetMediumKey(permanentKey)),
            //            MediumS3Key = GetMediumKey(permanentKey),

            //            DisplayOrder = index,
            //            IsPrimary = index == 0,
            //            UploadedByUserId = user.Id,
            //            UploadedAt = DateTime.UtcNow.AddHours(3),
            //            ProcessingStatus = ImageProcessingStatus.Completed
            //        };

            //        await _context.Set<Domain.Entities.UnitImage>().AddAsync(unitImage);
            //        await _context.SaveChangesAsync();
            //    }
            //}
            //else
            //{ 

            //}

            // Update registration request
            request.Status = RegistrationRequestStatus.Approved;
            request.ReviewedAt = DateTime.UtcNow.AddHours(3);
            request.ReviewedByAdminId = adminUserId;
            request.CreatedUserId = user.Id;
            request.CreatedUnitId = unit.Id;

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            // Send welcome email with credentials (only if new user was created)
            var emailSent = false;
            if (userCreated)
            {
                emailSent = await SendWelcomeEmailAsync(
                    user.Email!,
                    user.FullName!,
                    unit.Id,
                    unit.Name);
            }

            return Result.Success(new ApprovalResult
            {
                CreatedUserId = user.Id,
                CreatedUserEmail = user.Email!,
                CreatedUnitId = unit.Id,
                CreatedUnitName = unit.Name,
                EmailSent = emailSent
            });
        }
        catch (Exception ex)
        {


            return Result.Failure<ApprovalResult>(
                new Error("ApprovalFailed",
                    $"Failed to approve registration request : {ex}", 500));
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
                .FirstOrDefaultAsync(r => r.Id == requestId);

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
                .FirstOrDefaultAsync(r => r.Id == requestId);

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
            DepartmentId = request.DepartmentId,
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


    public async Task<Result<IEnumerable<DAUnitRegistrationResponse>>> DepartmentAdmin_GetAllRequestsAsync
        (DAUnitRegistrationListFilter filter, string UserId, CancellationToken ct)
    {
        var user = await _userManager.FindByIdAsync(UserId);

        if (user == null)
        {
            return Result.Failure<IEnumerable<DAUnitRegistrationResponse>>(UserErrors.UserNotFound);
        }
        var roles = await _userManager.GetRolesAsync(user);
        if (!roles.Contains(DefaultRoles.CityAdmin))
        {
            return Result.Failure<IEnumerable<DAUnitRegistrationResponse>>(UserErrors.Unauthorized);
        }
        var city = await _context.Set<DepartmentAdmin>()
                .Where(x => x.UserId == UserId && x.IsActive)
                .FirstOrDefaultAsync(ct);
        if (city is null)
        {
            return Result.Failure<IEnumerable<DAUnitRegistrationResponse>>(UserErrors.Unauthorized);
        }

        var query = _context.Set<UnitRegistrationRequest>()
            .Where(x => x.DepartmentId == city.CityId)
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
            .ToListAsync(ct);

        var responses = requests.Select(CAMapToResponse).ToList();

        return Result.Success<IEnumerable<DAUnitRegistrationResponse>>(responses);
    }


    public async Task<Result<DAUnitRegistrationResponse>> DepartmentAdmin_GetRequestByIdAsync(int requestId, string userId, CancellationToken ct)
    {
        var user = await _userManager.FindByIdAsync(userId);

        if (user == null)
        {
            return Result.Failure<DAUnitRegistrationResponse>(UserErrors.UserNotFound);
        }
        var roles = await _userManager.GetRolesAsync(user);
        if (!roles.Contains(DefaultRoles.CityAdmin))
        {
            return Result.Failure<DAUnitRegistrationResponse>(UserErrors.Unauthorized);
        }
        var city = await _context.Set<DepartmentAdmin>()
               .Where(x => x.UserId == userId && x.IsActive)
               .FirstOrDefaultAsync(ct);

        if (city is null)
        {
            return Result.Failure<DAUnitRegistrationResponse>(UserErrors.Unauthorized);
        }

        var request = await _context.Set<UnitRegistrationRequest>()
           .Include(r => r.Department)
           .Include(r => r.UnitType)
           .Include(r => r.ReviewedByAdmin)
           .Include(r => r.CreatedUser)
           .Include(r => r.CreatedUnit)
           .FirstOrDefaultAsync(r => r.Id == requestId && r.DepartmentId == city.CityId, ct);

        if (request == null)
            return Result.Failure<DAUnitRegistrationResponse>(
                new Error("NotFound", "Registration request not found", 404));

        var response = CAMapToResponse(request);
        return Result.Success(response);
    }

    public async Task<Result<DAapprovalResult>> DepartmentAdmin_ApproveRequestAsync(int requestId, string UserId, CancellationToken ct)
    {
        var user = await _userManager.FindByIdAsync(UserId);

        if (user == null)
        {
            return Result.Failure<DAapprovalResult>(UserErrors.UserNotFound);
        }
        var roles = await _userManager.GetRolesAsync(user);
        if (!roles.Contains(DefaultRoles.CityAdmin))
        {
            return Result.Failure<DAapprovalResult>(UserErrors.Unauthorized);
        }
        var city = await _context.Set<DepartmentAdmin>()
                .Where(x => x.UserId == UserId && x.IsActive)
                .FirstOrDefaultAsync(ct);
        if (city is null)
        {
            return Result.Failure<DAapprovalResult>(UserErrors.Unauthorized);
        }

        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var request = await _context.Set<UnitRegistrationRequest>()
                .Where(r => r.Id == requestId && r.DepartmentId == city.CityId)
                .Include(r => r.Department)
                .Include(r => r.UnitType)
                .FirstOrDefaultAsync(ct);

            if (request == null)
                return Result.Failure<DAapprovalResult>(
                    new Error("NotFound", "Registration request not found", 404));

            if (request.Status != RegistrationRequestStatus.Pending)
                return Result.Failure<DAapprovalResult>(
                    new Error("InvalidStatus",
                        $"Request is already {request.Status}", 400));

            var emailExists = await _userManager.FindByEmailAsync(request.OwnerEmail);
            if (emailExists != null)
                return Result.Failure<DAapprovalResult>(
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
                return Result.Failure<DAapprovalResult>(
                    new Error("UserCreationFailed",
                        $"Failed to create user: {errors}", 500));
            }

            var roleResult = await _userManager.AddToRoleAsync(newUser, DefaultRoles.HotelAdmin);
            if (!roleResult.Succeeded)
            {
                await transaction.RollbackAsync();
                return Result.Failure<DAapprovalResult>(
                    new Error("RoleAssignmentFailed",
                        "Failed to assign HotelAdmin role", 500));
            }

            // Create the Unit
            var unit = new Domain.Entities.Unit
            {
                Name = request.UnitName,
                Description = request.Description,
                Address = request.Address,
                CityId = city.CityId,
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

            var availabilityInit = await service.InitializeUnitAvailabilityAsync(unit.Id, 365);


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
            request.ReviewedByAdminId = UserId;
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

            return Result.Success(new DAapprovalResult
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

            return Result.Failure<DAapprovalResult>(
                new Error("ApprovalFailed",
                    "Failed to approve registration request", 500));
        }
    }

    public async Task<Result> DepartmentAdmin_RejectRequestAsync(int requestId, string UserId, string rejectionReason, CancellationToken ct)
    {
        var user = await _userManager.FindByIdAsync(UserId);

        if (user == null)
        {
            Result.Failure(
                    (UserErrors.UserNotFound));
        }
        var roles = await _userManager.GetRolesAsync(user);
        if (!roles.Contains(DefaultRoles.CityAdmin))
        {
            return Result.Failure(
                    (UserErrors.Unauthorized));
        }
        var city = await _context.Set<DepartmentAdmin>()
                .Where(x => x.UserId == UserId && x.IsActive)
                .FirstOrDefaultAsync(ct);
        if (city is null)
        {
            return Result.Failure(UserErrors.Unauthorized);
        }


        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var request = await _context.Set<UnitRegistrationRequest>()
                .FirstOrDefaultAsync(r => r.Id == requestId && r.DepartmentId == city.CityId, ct);

            if (request == null)
                return Result.Failure(
                    new Error("NotFound", "Registration request not found", 404));

            if (request.Status != RegistrationRequestStatus.Pending)
                return Result.Failure(
                    new Error("InvalidStatus",
                        $"Request is already {request.Status}", 400));

            request.Status = RegistrationRequestStatus.Rejected;
            request.ReviewedAt = DateTime.UtcNow.AddHours(3);
            request.ReviewedByAdminId = UserId;
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
                requestId, UserId);

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

    public async Task<Result> DepartmentAdmin_DeleteRequestAsync(int requestId, string userId, CancellationToken ct)
    {
        var user = await _userManager.FindByIdAsync(userId);

        if (user == null)
        {
            return Result.Failure(UserErrors.UserNotFound);
        }
        var roles = await _userManager.GetRolesAsync(user);
        if (!roles.Contains(DefaultRoles.CityAdmin))
        {
            return Result.Failure(UserErrors.Unauthorized);
        }
        var city = await _context.Set<DepartmentAdmin>()
                .Where(x => x.UserId == userId && x.IsActive)
                .FirstOrDefaultAsync(ct);
        if (city is null)
        {
            return Result.Failure(UserErrors.Unauthorized);
        }

        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var request = await _context.Set<UnitRegistrationRequest>()
                .FirstOrDefaultAsync(r => r.Id == requestId && r.DepartmentId == city.CityId, ct);

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

    public async Task<Result<DAUnitRegistrationStatistics>> DepartmentAdmin_GetStatisticsAsync(string userId, CancellationToken ct)
    {
        var user = await _userManager.FindByIdAsync(userId);

        if (user == null)
        {
            return Result.Failure<DAUnitRegistrationStatistics>(UserErrors.UserNotFound);
        }
        var roles = await _userManager.GetRolesAsync(user);
        if (!roles.Contains(DefaultRoles.CityAdmin))
        {
            return Result.Failure<DAUnitRegistrationStatistics>(UserErrors.Unauthorized);
        }
        var city = await _context.Set<DepartmentAdmin>()
                .Where(x => x.UserId == userId && x.IsActive)
                .FirstOrDefaultAsync(ct);
        if (city is null)
        {
            return Result.Failure<DAUnitRegistrationStatistics>(UserErrors.UserNotFound);
        }

        var requests = await _context.Set<UnitRegistrationRequest>()
           .Where(r => r.DepartmentId == city.CityId)
           .Include(r => r.Department)
           .Include(r => r.UnitType)
           .ToListAsync();

        var now = DateTime.UtcNow;
        var weekAgo = now.AddDays(-7);
        var monthAgo = now.AddMonths(-1);

        var stats = new DAUnitRegistrationStatistics
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



    private DAUnitRegistrationResponse CAMapToResponse(UnitRegistrationRequest request)
    {
        var imageKeys = JsonSerializer.Deserialize<List<string>>(request.ImageS3Keys)
                       ?? new List<string>();

        return new DAUnitRegistrationResponse
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