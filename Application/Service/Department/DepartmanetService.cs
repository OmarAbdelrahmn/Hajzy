using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Application.Abstraction;
using Application.Abstraction.Consts;
using Application.Contracts.Department;
using Domain;
using Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Service.Department;

public class DepartmanetService(
    ApplicationDbcontext dbContext,
    UserManager<ApplicationUser> manager,
    IAmazonS3 s3Client,
    IConfiguration configuration,
    ILogger<DepartmanetService> logger) : IDepartmanetService
{
    private readonly ApplicationDbcontext dbcontext = dbContext;
    private readonly UserManager<ApplicationUser> manager = manager;
    private readonly IAmazonS3 _s3Client = s3Client;
    private readonly IConfiguration _configuration = configuration;
    private readonly ILogger<DepartmanetService> _logger = logger;
    private readonly string _bucketName = configuration["AWS:S3:BucketName"]
            ?? throw new InvalidOperationException("S3 bucket name not configured");
    private readonly string _cloudFrontDomain = configuration["AWS:S3:Domain"] ?? "";

    #region CRUD Operations

    public async Task<Result<DepartmentResponse>> GetByIdAsync(int departmentId)
    {
        var department = await dbcontext.Departments
            .Include(d => d.DepartmentAdmins)
                .ThenInclude(da => da.User)
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == departmentId);

        if (department == null)
            return Result.Failure<DepartmentResponse>(
                new Error("NotFound", "Department not found", 404));

        var response = MapToResponse(department);
        return Result.Success(response);
    }

    public async Task<Result<IEnumerable<DepartmentResponse>>> GetAllAsync(bool includeInactive = false)
    {
        var query = dbcontext.Departments
            .Include(d => d.DepartmentAdmins)
                .ThenInclude(da => da.User)
            .Where(d => !d.IsDeleted)
            .AsNoTracking();

        if (!includeInactive)
            query = query.Where(d => d.IsActive);

        var departments = await query.ToListAsync();
        var responses = departments.Select(MapToResponse).ToList();

        return Result.Success<IEnumerable<DepartmentResponse>>(responses);
    }

    public async Task<Result<DepartmentResponse>> CreateAsync(CreateDepartmentRequest request)
    {
        var exists = await dbcontext.Departments
            .AnyAsync(d => d.Name == request.Name &&
                          d.Country == request.Country &&
                          !d.IsDeleted);

        if (exists)
            return Result.Failure<DepartmentResponse>(
                new Error("DuplicateDepartment",
                    "A department with this name already exists in this country", 400));

        var department = new Domain.Entities.Department
        {
            Name = request.Name,
            Country = request.Country,
            Description = request.Description,
            ImageUrl = request.ImageUrl,
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            IsActive = request.IsActive,
            CreatedAt = DateTime.UtcNow.AddHours(3)
        };

        await dbcontext.Departments.AddAsync(department);
        await dbcontext.SaveChangesAsync();

        var response = MapToResponse(department);
        return Result.Success(response);
    }

    public async Task<Result<DepartmentResponse>> UpdateAsync(
        int departmentId,
        UpdateDepartmentRequest request)
    {
        var department = await dbcontext.Departments
            .Include(d => d.DepartmentAdmins)
                .ThenInclude(da => da.User)
            .FirstOrDefaultAsync(d => d.Id == departmentId && !d.IsDeleted);

        if (department == null)
            return Result.Failure<DepartmentResponse>(
                new Error("NotFound", "Department not found", 404));

        // Check for duplicate name if name is being changed
        if (request.Name != null && request.Name != department.Name)
        {
            var exists = await dbcontext.Departments
                .AnyAsync(d => d.Name == request.Name &&
                              d.Country == (request.Country ?? department.Country) &&
                              d.Id != departmentId &&
                              !d.IsDeleted);

            if (exists)
                return Result.Failure<DepartmentResponse>(
                    new Error("DuplicateDepartment",
                        "A department with this name already exists", 400));
        }

        // Update properties
        if (request.Name != null) department.Name = request.Name;
        if (request.Country != null) department.Country = request.Country;
        if (request.Description != null) department.Description = request.Description;
        if (request.ImageUrl != null) department.ImageUrl = request.ImageUrl;
        if (request.Latitude.HasValue) department.Latitude = request.Latitude.Value;
        if (request.Longitude.HasValue) department.Longitude = request.Longitude.Value;
        if (request.IsActive.HasValue) department.IsActive = request.IsActive.Value;

        await dbcontext.SaveChangesAsync();

        var response = MapToResponse(department);
        return Result.Success(response);
    }

    public async Task<Result> DeleteAsync(int departmentId, bool softDelete = true)
    {
        var department = await dbcontext.Departments
            .Include(d => d.Units)
            .Include(d => d.DepartmentAdmins)
            .FirstOrDefaultAsync(d => d.Id == departmentId);

        if (department == null)
            return Result.Failure(
                new Error("NotFound", "Department not found", 404));

        // Check if department has active units
        if (department.Units.Any(u => !u.IsDeleted))
            return Result.Failure(
                new Error("HasActiveUnits",
                    "Cannot delete department with active units", 400));

        if (softDelete)
        {
            department.IsDeleted = true;
            department.DeletedAt = DateTime.UtcNow.AddHours(3);
            department.IsActive = false;

            // Deactivate all admins
            foreach (var admin in department.DepartmentAdmins)
            {
                admin.IsActive = false;
            }
        }
        else
        {
            // Delete department image from S3 if exists
            if (!string.IsNullOrEmpty(department.ImageUrl))
            {
                var s3Key = ExtractS3KeyFromUrl(department.ImageUrl);
                if (!string.IsNullOrEmpty(s3Key))
                {
                    _ = DeleteImageFromS3Async(s3Key); // Fire and forget
                }
            }

            dbcontext.Departments.Remove(department);
        }

        await dbcontext.SaveChangesAsync();
        return Result.Success();
    }

    public async Task<Result> RestoreAsync(int departmentId)
    {
        var department = await dbcontext.Departments
            .FirstOrDefaultAsync(d => d.Id == departmentId && d.IsDeleted);

        if (department == null)
            return Result.Failure(
                new Error("NotFound", "Deleted department not found", 404));

        department.IsDeleted = false;
        department.DeletedAt = null;
        department.IsActive = true;

        await dbcontext.SaveChangesAsync();
        return Result.Success();
    }

    #endregion

    #region Image Management

    public async Task<Result<string>> UploadDepartmentImageAsync(
        int departmentId,
        IFormFile image,
        string userId)
    {
        using var transaction = await dbcontext.Database.BeginTransactionAsync();

        try
        {
            // Verify department exists
            var department = await dbcontext.Departments
                .FirstOrDefaultAsync(d => d.Id == departmentId && !d.IsDeleted);

            if (department == null)
                return Result.Failure<string>(
                    new Error("NotFound", "Department not found", 404));

            // Validate image
            var validationResult = ValidateImage(image);
            if (!validationResult.IsSuccess)
                return Result.Failure<string>(validationResult.Error);

            // Delete old image if exists
            if (!string.IsNullOrEmpty(department.ImageUrl))
            {
                var oldS3Key = ExtractS3KeyFromUrl(department.ImageUrl);
                if (!string.IsNullOrEmpty(oldS3Key))
                {
                    _ = DeleteImageFromS3Async(oldS3Key); // Fire and forget
                }
            }

            // Upload new image to S3
            var fileExtension = Path.GetExtension(image.FileName).ToLowerInvariant();
            var s3Key = $"departments/{departmentId}/image/{Guid.NewGuid()}{fileExtension}";

            using var stream = image.OpenReadStream();
            var transferUtility = new TransferUtility(_s3Client);

            var uploadRequest = new TransferUtilityUploadRequest
            {
                InputStream = stream,
                Key = s3Key,
                BucketName = _bucketName,
                ContentType = image.ContentType,
                CannedACL = S3CannedACL.PublicRead,
                Metadata =
                {
                    ["original-filename"] = image.FileName,
                    ["uploaded-at"] = DateTime.UtcNow.ToString("o"),
                    ["uploaded-by"] = userId,
                    ["department-id"] = departmentId.ToString()
                }
            };

            await transferUtility.UploadAsync(uploadRequest);

            // Generate CloudFront URL
            var imageUrl = GetCloudFrontImageUrl(s3Key);

            // Update department with new image URL
            department.ImageUrl = imageUrl;
            await dbcontext.SaveChangesAsync();
            await transaction.CommitAsync();

            // Generate thumbnails asynchronously
            _ = GenerateThumbnailAsync(image, s3Key);

            _logger.LogInformation(
                "Image uploaded for department {DepartmentId}. S3 Key: {S3Key}",
                departmentId, s3Key);

            return Result.Success(imageUrl);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error uploading image for department {DepartmentId}", departmentId);
            return Result.Failure<string>(
                new Error("UploadFailed", $"Failed to upload department image: {ex.Message}", 500));
        }
    }

    public async Task<Result> DeleteDepartmentImageAsync(int departmentId)
    {
        using var transaction = await dbcontext.Database.BeginTransactionAsync();

        try
        {
            var department = await dbcontext.Departments
                .FirstOrDefaultAsync(d => d.Id == departmentId && !d.IsDeleted);

            if (department == null)
                return Result.Failure(
                    new Error("NotFound", "Department not found", 404));

            if (string.IsNullOrEmpty(department.ImageUrl))
                return Result.Failure(
                    new Error("NoImage", "Department has no image", 400));

            // Extract S3 key from URL
            var s3Key = ExtractS3KeyFromUrl(department.ImageUrl);
            if (string.IsNullOrEmpty(s3Key))
                return Result.Failure(
                    new Error("InvalidImageUrl", "Could not extract S3 key from image URL", 400));

            // Delete from S3
            var deleteResult = await DeleteImageFromS3Async(s3Key);
            if (!deleteResult.IsSuccess)
            {
                _logger.LogWarning("Failed to delete image from S3: {Error}", deleteResult.Error.Description);
            }

            // Update department
            department.ImageUrl = null;
            await dbcontext.SaveChangesAsync();
            await transaction.CommitAsync();

            _logger.LogInformation(
                "Image deleted for department {DepartmentId}",
                departmentId);

            return Result.Success();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error deleting image for department {DepartmentId}", departmentId);
            return Result.Failure(
                new Error("DeleteFailed", $"Failed to delete department image: {ex.Message}", 500));
        }
    }

    public async Task<Result<string>> GetPresignedImageUrlAsync(string s3Key, int expirationMinutes = 60)
    {
        try
        {
            var request = new GetPreSignedUrlRequest
            {
                BucketName = _bucketName,
                Key = s3Key,
                Expires = DateTime.UtcNow.AddMinutes(expirationMinutes)
            };

            var url = await _s3Client.GetPreSignedURLAsync(request);
            return Result.Success(url);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating presigned URL for S3 key: {S3Key}", s3Key);
            return Result.Failure<string>(
                new Error("UrlFailed", $"Failed to generate presigned URL: {ex.Message}", 500));
        }
    }

    public string GetCloudFrontImageUrl(string s3Key)
    {
        if (string.IsNullOrEmpty(s3Key))
            return string.Empty;

        if (string.IsNullOrEmpty(_cloudFrontDomain))
            return $"https://{_bucketName}.s3.amazonaws.com/{s3Key}";

        return $"https://{_cloudFrontDomain}/{s3Key}";
    }

    #endregion

    #region Admin Management

    public async Task<Result> AttachAdminAsync(int departmentId, string userId, bool setAsPrimary = false)
    {
        using var transaction = await dbcontext.Database.BeginTransactionAsync();
        try
        {
            var department = await dbcontext.Departments
                .Include(d => d.DepartmentAdmins)
                .FirstOrDefaultAsync(d => d.Id == departmentId && !d.IsDeleted);

            if (department == null)
                return Result.Failure(
                    new Error("NotFound", "Department not found", 404));

            var user = await manager.FindByIdAsync(userId);
            if (user == null)
                return Result.Failure(
                    new Error("UserNotFound", "User not found", 404));

            var roles = await manager.GetRolesAsync(user);
            if (!roles.Contains(DefaultRoles.CityAdmin))
                return Result.Failure(
                    new Error("InvalidRole",
                        "User must have CityAdmin role to be assigned to a department", 400));

            // Check if user is already an active admin in THIS department
            var existingInDepartment = department.DepartmentAdmins
                .FirstOrDefault(da => da.UserId == userId);

            if (existingInDepartment != null && existingInDepartment.IsActive)
                return Result.Failure(
                    new Error("AlreadyAssigned",
                        "User is already an active admin of this department", 400));

            // Check if user is already an active admin in ANY OTHER department
            var existingInOtherDept = await dbcontext.DepartmentAdmins
                .FirstOrDefaultAsync(da => da.UserId == userId &&
                                          da.CityId != departmentId &&
                                          da.IsActive);

            if (existingInOtherDept != null)
                return Result.Failure(
                    new Error("AlreadyAssignedElsewhere",
                        "User is already an active admin of another department", 400));

            var activeAdmins = department.DepartmentAdmins.Where(da => da.IsActive).ToList();
            var hasPrimaryAdmin = activeAdmins.Any(da => da.IsPrimary);

            // If this is the first admin or setAsPrimary is true, make them primary
            bool shouldBePrimary = !hasPrimaryAdmin || setAsPrimary;

            // If setting as primary and there's already a primary, demote the current primary
            if (shouldBePrimary && hasPrimaryAdmin)
            {
                var currentPrimary = activeAdmins.First(da => da.IsPrimary);
                currentPrimary.IsPrimary = false;
            }

            // Create or reactivate admin assignment
            if (existingInDepartment != null)
            {
                existingInDepartment.IsActive = true;
                existingInDepartment.IsPrimary = shouldBePrimary;
                existingInDepartment.AssignedAt = DateTime.UtcNow.AddHours(3);
            }
            else
            {
                var newAdmin = new DepartmentAdmin
                {
                    UserId = userId,
                    CityId = departmentId,
                    IsActive = true,
                    IsPrimary = shouldBePrimary,
                    AssignedAt = DateTime.UtcNow.AddHours(3)
                };
                await dbcontext.DepartmentAdmins.AddAsync(newAdmin);
            }

            await dbcontext.SaveChangesAsync();
            await transaction.CommitAsync();

            return Result.Success();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return Result.Failure(
                new Error("AttachError", $"Failed to attach admin: {ex.Message}", 500));
        }
    }

    public async Task<Result> SetPrimaryAdminAsync(int departmentId, string userId)
    {
        using var transaction = await dbcontext.Database.BeginTransactionAsync();
        try
        {
            var department = await dbcontext.Departments
                .Include(d => d.DepartmentAdmins)
                .FirstOrDefaultAsync(d => d.Id == departmentId && !d.IsDeleted);

            if (department == null)
                return Result.Failure(
                    new Error("NotFound", "Department not found", 404));

            var targetAdmin = department.DepartmentAdmins
                .FirstOrDefault(da => da.UserId == userId && da.IsActive);

            if (targetAdmin == null)
                return Result.Failure(
                    new Error("AdminNotFound", "Admin not found in this department", 404));

            if (targetAdmin.IsPrimary)
                return Result.Failure(
                    new Error("AlreadyPrimary", "This admin is already the primary admin", 400));

            // Demote current primary admin
            var currentPrimary = department.DepartmentAdmins
                .FirstOrDefault(da => da.IsPrimary && da.IsActive);

            if (currentPrimary != null)
            {
                currentPrimary.IsPrimary = false;
            }

            // Promote new primary admin
            targetAdmin.IsPrimary = true;

            await dbcontext.SaveChangesAsync();
            await transaction.CommitAsync();

            return Result.Success();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return Result.Failure(
                new Error("SetPrimaryError", $"Failed to set primary admin: {ex.Message}", 500));
        }
    }

    public async Task<Result> RemoveAdminAsync(int departmentId, string userId)
    {
        using var transaction = await dbcontext.Database.BeginTransactionAsync();
        try
        {
            var department = await dbcontext.Departments
                .Include(d => d.DepartmentAdmins)
                .FirstOrDefaultAsync(d => d.Id == departmentId && !d.IsDeleted);

            if (department == null)
                return Result.Failure(
                    new Error("NotFound", "Department not found", 404));

            var admin = department.DepartmentAdmins
                .FirstOrDefault(da => da.UserId == userId);

            if (admin == null)
                return Result.Failure(
                    new Error("NotFound", "Admin assignment not found", 404));

            bool wasPrimary = admin.IsPrimary;

            dbcontext.DepartmentAdmins.Remove(admin);
            await dbcontext.SaveChangesAsync();

            // If removed admin was primary, promote another active admin
            if (wasPrimary)
            {
                var nextAdmin = department.DepartmentAdmins
                    .Where(da => da.UserId != userId && da.IsActive)
                    .OrderBy(da => da.AssignedAt)
                    .FirstOrDefault();

                if (nextAdmin != null)
                {
                    nextAdmin.IsPrimary = true;
                    await dbcontext.SaveChangesAsync();
                }
            }

            await transaction.CommitAsync();
            return Result.Success();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return Result.Failure(
                new Error("RemoveError", $"Failed to remove admin: {ex.Message}", 500));
        }
    }

    public async Task<Result> DeactivateAdminAsync(int departmentId, string userId)
    {
        using var transaction = await dbcontext.Database.BeginTransactionAsync();
        try
        {
            var department = await dbcontext.Departments
                .Include(d => d.DepartmentAdmins)
                .FirstOrDefaultAsync(d => d.Id == departmentId && !d.IsDeleted);

            if (department == null)
                return Result.Failure(
                    new Error("NotFound", "Department not found", 404));

            var admin = department.DepartmentAdmins
                .FirstOrDefault(da => da.UserId == userId && da.IsActive);

            if (admin == null)
                return Result.Failure(
                    new Error("NotFound", "Active admin not found", 404));

            bool wasPrimary = admin.IsPrimary;
            admin.IsActive = false;

            // If deactivated admin was primary, promote another active admin
            if (wasPrimary)
            {
                admin.IsPrimary = false;
                var nextAdmin = department.DepartmentAdmins
                    .Where(da => da.UserId != userId && da.IsActive)
                    .OrderBy(da => da.AssignedAt)
                    .FirstOrDefault();

                if (nextAdmin != null)
                {
                    nextAdmin.IsPrimary = true;
                }
            }

            await dbcontext.SaveChangesAsync();
            await transaction.CommitAsync();

            return Result.Success();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return Result.Failure(
                new Error("DeactivateError", $"Failed to deactivate admin: {ex.Message}", 500));
        }
    }

    public async Task<Result> ActivateAdminAsync(int departmentId, string userId)
    {
        var admin = await dbcontext.DepartmentAdmins
            .FirstOrDefaultAsync(da => da.CityId == departmentId &&
                                      da.UserId == userId &&
                                      !da.IsActive);

        if (admin == null)
            return Result.Failure(
                new Error("NotFound", "Inactive admin not found", 404));

        admin.IsActive = true;
        admin.AssignedAt = DateTime.UtcNow.AddHours(3);

        // If no primary admin exists, make this one primary
        var hasPrimaryAdmin = await dbcontext.DepartmentAdmins
            .AnyAsync(da => da.CityId == departmentId &&
                           da.IsActive &&
                           da.IsPrimary);

        if (!hasPrimaryAdmin)
        {
            admin.IsPrimary = true;
        }

        await dbcontext.SaveChangesAsync();
        return Result.Success();
    }

    public async Task<Result<DepartmentAdminsResponse>> GetDepartmentAdminsAsync(int departmentId)
    {
        var department = await dbcontext.Departments
            .Include(d => d.DepartmentAdmins)
                .ThenInclude(da => da.User)
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == departmentId && !d.IsDeleted);

        if (department == null)
            return Result.Failure<DepartmentAdminsResponse>(
                new Error("NotFound", "Department not found", 404));

        var admins = department.DepartmentAdmins
            .Select(da => new DepartmentAdminInfo(
                UserId: da.UserId,
                FullName: da.User?.FullName ?? "N/A",
                Email: da.User?.Email ?? "N/A",
                PhoneNumber: da.User?.PhoneNumber,
                IsActive: da.IsActive,
                IsPrimary: da.IsPrimary,
                AssignedAt: da.AssignedAt
            ))
            .ToList();

        var response = new DepartmentAdminsResponse(
            DepartmentId: department.Id,
            DepartmentName: department.Name,
            TotalAdmins: admins.Count,
            ActiveAdmins: admins.Count(a => a.IsActive),
            Admins: admins
        );

        return Result.Success(response);
    }

    public async Task<Result<IEnumerable<DepartmentWithAdminsResponse>>> GetDepartmentsWithAdminsAsync()
    {
        var departments = await dbcontext.Departments
            .Include(d => d.DepartmentAdmins)
                .ThenInclude(da => da.User)
            .Where(d => !d.IsDeleted)
            .AsNoTracking()
            .ToListAsync();

        var responses = departments.Select(d =>
        {
            var admins = d.DepartmentAdmins
                .Select(da => new AdminSummary(
                    UserId: da.UserId,
                    Name: da.User?.FullName ?? "N/A",
                    Email: da.User?.Email ?? "N/A",
                    IsActive: da.IsActive,
                    IsPrimary: da.IsPrimary,
                    AssignedAt: da.AssignedAt
                ))
                .ToList();

            return new DepartmentWithAdminsResponse(
                Id: d.Id,
                Name: d.Name,
                Country: d.Country,
                IsActive: d.IsActive,
                TotalAdmins: admins.Count,
                ActiveAdmins: admins.Count(a => a.IsActive),
                Admins: admins
            );
        }).ToList();

        return Result.Success<IEnumerable<DepartmentWithAdminsResponse>>(responses);
    }

    #endregion

    #region Statistics & Details

    public async Task<Result<DepartmentDetailsResponse>> GetDepartmentDetailsAsync(int departmentId)
    {
        var department = await dbcontext.Departments
            .Include(d => d.DepartmentAdmins.Where(da => da.IsActive))
                .ThenInclude(da => da.User)
            .Include(d => d.Units.Where(u => !u.IsDeleted))
                .ThenInclude(u => u.UnitType)
            .Include(d => d.Units)
                .ThenInclude(u => u.Bookings)
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == departmentId && !d.IsDeleted);

        if (department == null)
            return Result.Failure<DepartmentDetailsResponse>(
                new Error("NotFound", "Department not found", 404));

        var admins = department.DepartmentAdmins
            .Select(da => new DepartmentAdminInfo(
                UserId: da.UserId,
                FullName: da.User?.FullName ?? "N/A",
                Email: da.User?.Email ?? "N/A",
                PhoneNumber: da.User?.PhoneNumber,
                IsActive: da.IsActive,
                IsPrimary: da.IsPrimary,
                AssignedAt: da.AssignedAt
            ))
            .ToList();

        var primaryAdmin = admins.FirstOrDefault(a => a.IsPrimary);

        var units = department.Units.Select(u => new UnitSummary(
            Id: u.Id,
            Name: u.Name,
            UnitType: u.UnitType?.Name ?? "N/A",
            IsActive: u.IsActive,
            IsVerified: u.IsVerified,
            AverageRating: u.AverageRating,
            TotalReviews: u.TotalReviews
        )).ToList();

        var allBookings = department.Units.SelectMany(u => u.Bookings).ToList();
        var totalRevenue = allBookings
            .Where(b => b.Status == BookingStatus.Completed)
            .Sum(b => b.TotalPrice);

        var metrics = new DepartmentMetrics(
            ActiveUnits: department.Units.Count(u => u.IsActive),
            InactiveUnits: department.Units.Count(u => !u.IsActive),
            VerifiedUnits: department.Units.Count(u => u.IsVerified),
            UnverifiedUnits: department.Units.Count(u => !u.IsVerified),
            PendingBookings: allBookings.Count(b => b.Status == BookingStatus.Pending),
            ConfirmedBookings: allBookings.Count(b => b.Status == BookingStatus.Confirmed),
            CompletedBookings: allBookings.Count(b => b.Status == BookingStatus.Completed),
            TotalRevenue: totalRevenue,
            AverageBookingValue: allBookings.Count > 0 ? totalRevenue / allBookings.Count : 0
        );

        var response = new DepartmentDetailsResponse(
            Id: department.Id,
            Name: department.Name,
            Country: department.Country,
            Description: department.Description,
            ImageUrl: department.ImageUrl,
            Latitude: department.Latitude,
            Longitude: department.Longitude,
            IsActive: department.IsActive,
            TotalUnits: department.TotalUnits,
            TotalBookings: department.TotalBookings,
            AverageRating: department.AverageRating,
            CreatedAt: department.CreatedAt,
            Admins: admins,
            PrimaryAdmin: primaryAdmin,
            Units: units,
            Metrics: metrics
        );

        return Result.Success(response);
    }

    public async Task<Result<DepartmentStatisticsResponse>> GetDepartmentStatisticsAsync(int departmentId)
    {
        var department = await dbcontext.Departments
            .Include(d => d.Units.Where(u => !u.IsDeleted))
                .ThenInclude(u => u.UnitType)
            .Include(d => d.Units)
                .ThenInclude(u => u.Bookings)
            .Include(d => d.Units)
                .ThenInclude(u => u.Reviews)
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == departmentId && !d.IsDeleted);

        if (department == null)
            return Result.Failure<DepartmentStatisticsResponse>(
                new Error("NotFound", "Department not found", 404));

        var allBookings = department.Units.SelectMany(u => u.Bookings).ToList();
        var allReviews = department.Units.SelectMany(u => u.Reviews).ToList();

        var unitsByType = department.Units
            .GroupBy(u => u.UnitType?.Name ?? "Unknown")
            .ToDictionary(g => g.Key, g => g.Count());

        var bookingsByStatus = allBookings
            .GroupBy(b => b.Status.ToString())
            .ToDictionary(g => g.Key, g => g.Count());

        var monthlyRevenue = allBookings
            .Where(b => b.Status == BookingStatus.Completed)
            .GroupBy(b => new { b.CreatedAt.Year, b.CreatedAt.Month })
            .OrderByDescending(g => g.Key.Year)
            .ThenByDescending(g => g.Key.Month)
            .Take(12)
            .ToDictionary(
                g => $"{g.Key.Year}-{g.Key.Month:D2}",
                g => g.Sum(b => b.TotalPrice)
            );

        var totalRevenue = allBookings
            .Where(b => b.Status == BookingStatus.Completed)
            .Sum(b => b.TotalPrice);

        var response = new DepartmentStatisticsResponse(
            DepartmentId: department.Id,
            DepartmentName: department.Name,
            TotalUnits: department.Units.Count,
            TotalBookings: allBookings.Count,
            AverageRating: department.AverageRating,
            TotalReviews: allReviews.Count,
            TotalRevenue: totalRevenue,
            ActiveUnits: department.Units.Count(u => u.IsActive),
            InactiveUnits: department.Units.Count(u => !u.IsActive),
            UnitsByType: unitsByType,
            BookingsByStatus: bookingsByStatus,
            MonthlyRevenue: monthlyRevenue
        );

        return Result.Success(response);
    }

    public async Task<Result<IEnumerable<DepartmentStatisticsSummary>>> GetAllDepartmentStatisticsAsync()
    {
        var departments = await dbcontext.Departments
            .Include(d => d.DepartmentAdmins.Where(da => da.IsActive))
                .ThenInclude(da => da.User)
            .Where(d => !d.IsDeleted)
            .AsNoTracking()
            .ToListAsync();

        var summaries = departments.Select(d =>
        {
            var primaryAdmin = d.DepartmentAdmins.FirstOrDefault(da => da.IsPrimary);
            return new DepartmentStatisticsSummary(
                Id: d.Id,
                Name: d.Name,
                Country: d.Country,
                TotalUnits: d.TotalUnits,
                TotalBookings: d.TotalBookings,
                AverageRating: d.AverageRating,
                HasActiveAdmin: d.DepartmentAdmins.Any(),
                AdminName: primaryAdmin?.User?.FullName
            );
        }).ToList();

        return Result.Success<IEnumerable<DepartmentStatisticsSummary>>(summaries);
    }

    #endregion

    #region Search & Filter

    public async Task<Result<IEnumerable<DepartmentResponse>>> FilterAsync(DepartmentFilter filter)
    {
        var query = dbcontext.Departments
            .Include(d => d.DepartmentAdmins)
                .ThenInclude(da => da.User)
            .AsQueryable();

        // Apply filters
        if (!string.IsNullOrWhiteSpace(filter.Name))
            query = query.Where(d => d.Name.Contains(filter.Name));

        if (!string.IsNullOrWhiteSpace(filter.Country))
            query = query.Where(d => d.Country.Contains(filter.Country));

        if (filter.IsActive.HasValue)
            query = query.Where(d => d.IsActive == filter.IsActive.Value);

        if (filter.IsDeleted.HasValue)
            query = query.Where(d => d.IsDeleted == filter.IsDeleted.Value);
        else
            query = query.Where(d => !d.IsDeleted);

        if (filter.HasAdmin.HasValue)
        {
            if (filter.HasAdmin.Value)
                query = query.Where(d => d.DepartmentAdmins.Any(da => da.IsActive));
            else
                query = query.Where(d => !d.DepartmentAdmins.Any(da => da.IsActive));
        }

        if (filter.MinUnits.HasValue)
            query = query.Where(d => d.TotalUnits >= filter.MinUnits.Value);

        if (filter.MaxUnits.HasValue)
            query = query.Where(d => d.TotalUnits <= filter.MaxUnits.Value);

        if (filter.MinRating.HasValue)
            query = query.Where(d => d.AverageRating >= filter.MinRating.Value);

        if (filter.MaxRating.HasValue)
            query = query.Where(d => d.AverageRating <= filter.MaxRating.Value);

        // Apply sorting
        if (!string.IsNullOrWhiteSpace(filter.SortBy))
        {
            bool descending = filter.SortDirection?.ToUpper() == "DESC";

            query = filter.SortBy switch
            {
                "Name" => descending ? query.OrderByDescending(d => d.Name)
                                    : query.OrderBy(d => d.Name),
                "Country" => descending ? query.OrderByDescending(d => d.Country)
                                       : query.OrderBy(d => d.Country),
                "TotalUnits" => descending ? query.OrderByDescending(d => d.TotalUnits)
                                          : query.OrderBy(d => d.TotalUnits),
                "TotalBookings" => descending ? query.OrderByDescending(d => d.TotalBookings)
                                             : query.OrderBy(d => d.TotalBookings),
                "AverageRating" => descending ? query.OrderByDescending(d => d.AverageRating)
                                             : query.OrderBy(d => d.AverageRating),
                "CreatedAt" => descending ? query.OrderByDescending(d => d.CreatedAt)
                                         : query.OrderBy(d => d.CreatedAt),
                _ => query.OrderBy(d => d.Name)
            };
        }
        else
        {
            query = query.OrderBy(d => d.Name);
        }

        // Pagination
        int skip = (filter.Page - 1) * filter.PageSize;
        var totalCount = await query.CountAsync();

        var departments = await query
            .Skip(skip)
            .Take(filter.PageSize)
            .ToListAsync();

        var responses = departments.Select(MapToResponse).ToList();

        return Result.Success<IEnumerable<DepartmentResponse>>(responses);
    }

    public async Task<Result<IEnumerable<DepartmentResponse>>> SearchAsync(string keyword)
    {
        keyword = keyword.ToLower();

        var departments = await dbcontext.Departments
            .Include(d => d.DepartmentAdmins)
                .ThenInclude(da => da.User)
            .Where(d => !d.IsDeleted &&
                       (d.Name.ToLower().Contains(keyword) ||
                        d.Country.ToLower().Contains(keyword) ||
                        (d.Description != null && d.Description.ToLower().Contains(keyword))))
            .AsNoTracking()
            .ToListAsync();

        var responses = departments.Select(MapToResponse).ToList();
        return Result.Success<IEnumerable<DepartmentResponse>>(responses);
    }

    public async Task<Result<IEnumerable<DepartmentResponse>>> GetByCountryAsync(string country)
    {
        var departments = await dbcontext.Departments
            .Include(d => d.DepartmentAdmins)
                .ThenInclude(da => da.User)
            .Where(d => d.Country == country && !d.IsDeleted)
            .AsNoTracking()
            .ToListAsync();

        var responses = departments.Select(MapToResponse).ToList();
        return Result.Success<IEnumerable<DepartmentResponse>>(responses);
    }

    #endregion

    #region Validation

    public async Task<Result<bool>> HasActiveAdminAsync(int departmentId)
    {
        var hasAdmin = await dbcontext.DepartmentAdmins
            .AnyAsync(da => da.CityId == departmentId && da.IsActive);

        return Result.Success(hasAdmin);
    }

    public async Task<Result<bool>> CanAssignAdminAsync(string userId)
    {
        var user = await manager.FindByIdAsync(userId);
        if (user == null)
            return Result.Failure<bool>(
                new Error("UserNotFound", "User not found", 404));

        // Check role
        var roles = await manager.GetRolesAsync(user);
        if (!roles.Contains(DefaultRoles.CityAdmin))
            return Result.Success(false);

        // Check if already assigned to any department
        var isAssigned = await dbcontext.DepartmentAdmins
            .AnyAsync(da => da.UserId == userId && da.IsActive);

        return Result.Success(!isAssigned);
    }

    #endregion

    #region Private Helper Methods

    private DepartmentResponse MapToResponse(Domain.Entities.Department department)
    {
        var activeAdmins = department.DepartmentAdmins?.ToList()
                          ?? new List<DepartmentAdmin>();

        return new DepartmentResponse(
            Id: department.Id,
            Name: department.Name,
            Country: department.Country,
            Description: department.Description,
            ImageUrl: department.ImageUrl,
            Latitude: department.Latitude,
            Longitude: department.Longitude,
            IsActive: department.IsActive,
            TotalUnits: department.TotalUnits,
            TotalBookings: department.TotalBookings,
            AverageRating: department.AverageRating,
            IsDeleted: department.IsDeleted,
            CreatedAt: department.CreatedAt,
            DeletedAt: department.DeletedAt,
            AdminCount: activeAdmins.Count,
            HasActiveAdmin: activeAdmins.Any()
        );
    }

    private Result ValidateImage(IFormFile image)
    {
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
        var maxFileSize = 10 * 1024 * 1024; // 10MB

        var extension = Path.GetExtension(image.FileName).ToLowerInvariant();

        if (!allowedExtensions.Contains(extension))
            return Result.Failure(
                new Error("InvalidFormat", $"Invalid image format: {extension}. Allowed: jpg, jpeg, png, webp", 400));

        if (image.Length > maxFileSize)
            return Result.Failure(
                new Error("FileTooLarge", "Image size must be less than 10MB", 400));

        if (image.Length == 0)
            return Result.Failure(
                new Error("EmptyFile", "Empty image file detected", 400));

        return Result.Success();
    }

    private async Task GenerateThumbnailAsync(IFormFile originalImage, string originalS3Key)
    {
        try
        {
            using var image = await Image.LoadAsync(originalImage.OpenReadStream());

            var resized = image.Clone(x => x.Resize(new ResizeOptions
            {
                Size = new Size(400, 400),
                Mode = ResizeMode.Max
            }));

            using var outputStream = new MemoryStream();
            await resized.SaveAsJpegAsync(outputStream);
            outputStream.Position = 0;

            var thumbnailKey = GetThumbnailKey(originalS3Key);

            var uploadRequest = new TransferUtilityUploadRequest
            {
                InputStream = outputStream,
                Key = thumbnailKey,
                BucketName = _bucketName,
                ContentType = "image/jpeg",
                CannedACL = S3CannedACL.PublicRead
            };

            var transferUtility = new TransferUtility(_s3Client);
            await transferUtility.UploadAsync(uploadRequest);

            _logger.LogInformation("Thumbnail generated for {S3Key}", originalS3Key);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to generate thumbnail for {S3Key}", originalS3Key);
        }
    }

    private async Task<Result> DeleteImageFromS3Async(string s3Key)
    {
        try
        {
            // Delete main image
            var deleteRequest = new DeleteObjectRequest
            {
                BucketName = _bucketName,
                Key = s3Key
            };

            await _s3Client.DeleteObjectAsync(deleteRequest);

            // Delete thumbnail
            var thumbnailKey = GetThumbnailKey(s3Key);
            var thumbDeleteRequest = new DeleteObjectRequest
            {
                BucketName = _bucketName,
                Key = thumbnailKey
            };

            try
            {
                await _s3Client.DeleteObjectAsync(thumbDeleteRequest);
            }
            catch { /* Ignore thumbnail delete errors */ }

            _logger.LogInformation("Image deleted from S3: {S3Key}", s3Key);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting image from S3: {S3Key}", s3Key);
            return Result.Failure(
                new Error("S3DeleteFailed", $"Failed to delete image from S3: {ex.Message}", 500));
        }
    }

    private string GetThumbnailKey(string originalKey)
    {
        var directory = Path.GetDirectoryName(originalKey)?.Replace("\\", "/");
        var filename = Path.GetFileNameWithoutExtension(originalKey);
        var extension = Path.GetExtension(originalKey);

        return $"{directory}/{filename}_thumb{extension}";
    }

    private string ExtractS3KeyFromUrl(string imageUrl)
    {
        if (string.IsNullOrEmpty(imageUrl))
            return string.Empty;

        try
        {
            // Handle CloudFront URLs
            if (!string.IsNullOrEmpty(_cloudFrontDomain) && imageUrl.Contains(_cloudFrontDomain))
            {
                var uri = new Uri(imageUrl);
                return uri.AbsolutePath.TrimStart('/');
            }

            // Handle S3 direct URLs
            if (imageUrl.Contains(".s3."))
            {
                var uri = new Uri(imageUrl);
                return uri.AbsolutePath.TrimStart('/');
            }

            // If it's already just the key
            if (imageUrl.StartsWith("departments/"))
            {
                return imageUrl;
            }

            return string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to extract S3 key from URL: {ImageUrl}", imageUrl);
            return string.Empty;
        }
    }

    #endregion
}

public record DepartmentFilter
{
    // Search filters
    public string? Name { get; init; }
    public string? Country { get; init; }

    // Status filters
    public bool? IsActive { get; init; }
    public bool? IsDeleted { get; init; }
    public bool? HasAdmin { get; init; }

    // Numeric range filters
    public int? MinUnits { get; init; }
    public int? MaxUnits { get; init; }
    public decimal? MinRating { get; init; }
    public decimal? MaxRating { get; init; }

    // Date filters (optional)
    public DateTime? CreatedFrom { get; init; }
    public DateTime? CreatedTo { get; init; }

    // Sorting
    public string? SortBy { get; init; } = "Name";
    public string? SortDirection { get; init; } = "ASC";

    // Pagination
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}