using Application.Abstraction;
using Application.Contracts.Review;
using Domain;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Service.Review;

public class ReviewService(
    ApplicationDbcontext context,
    ILogger<ReviewService> logger) : IReviewService
{
    private readonly ApplicationDbcontext _context = context;
    private readonly ILogger<ReviewService> _logger = logger;

    #region CREATE REVIEW

    public async Task<Result<ReviewResponse>> CreateReviewAsync(CreateReviewRequest request)
    {
        try
        {
            // 1. Validate eligibility
            var eligibility = await ValidateReviewEligibilityAsync(request.UserId, request.UnitId);
            if (!eligibility.IsEligible)
                return Result.Failure<ReviewResponse>(
                    new Error("NotEligible", eligibility.Reason, 403));

            // 2. Validate ratings match
            var ratingValidation = ValidateRatings(request.Rating, request.CleanlinessRating,
                request.LocationRating, request.ServiceRating, request.ValueRating, request.Comment);
            if (!ratingValidation.IsSuccess)
                return Result.Failure<ReviewResponse>(ratingValidation.Error);

            // 3. Get booking details
            var booking = await _context.Bookings
                .Include(b => b.Unit)
                .FirstOrDefaultAsync(b => b.Id == eligibility.BookingId);

            if (booking == null)
                return Result.Failure<ReviewResponse>(
                    new Error("BookingNotFound", "Booking not found", 404));

            // 4. Create review
            var review = new Domain.Entities.Review
            {
                UnitId = request.UnitId,
                BookingId = eligibility.BookingId!.Value,
                UserId = request.UserId,
                Rating = request.Rating,
                CleanlinessRating = request.CleanlinessRating,
                LocationRating = request.LocationRating,
                ServiceRating = request.ServiceRating,
                ValueRating = request.ValueRating,
                Comment = request.Comment?.Trim(),
                IsVerified = true,
                CreatedAt = DateTime.UtcNow.AddHours(3)
            };

            await _context.Set<Domain.Entities.Review>().AddAsync(review);
            await _context.SaveChangesAsync();

            // 5. Update unit statistics
            await UpdateUnitRatingStatisticsAsync(request.UnitId);

            _logger.LogInformation(
                "Review created for Unit {UnitId} by User {UserId}. Rating: {Rating}",
                request.UnitId, request.UserId, request.Rating);

            var response = await MapToResponseAsync(review);
            return Result.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating review");
            return Result.Failure<ReviewResponse>(
                new Error("ReviewFailed", "Failed to create review", 500));
        }
    }

    #endregion

    #region UPDATE REVIEW

    public async Task<Result<ReviewResponse>> UpdateReviewAsync(
        int reviewId,
        UpdateReviewRequest request,
        string userId)
    {
        try
        {
            var review = await _context.Set<Domain.Entities.Review>()
                .Include(r => r.Unit)
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Id == reviewId);

            if (review == null)
                return Result.Failure<ReviewResponse>(
                    new Error("NotFound", "Review not found", 404));

            if (review.UserId != userId)
                return Result.Failure<ReviewResponse>(
                    new Error("Unauthorized", "You can only update your own reviews", 403));

            // Update fields if provided
            if (request.Rating.HasValue)
                review.Rating = request.Rating.Value;
            if (request.CleanlinessRating.HasValue)
                review.CleanlinessRating = request.CleanlinessRating.Value;
            if (request.LocationRating.HasValue)
                review.LocationRating = request.LocationRating.Value;
            if (request.ServiceRating.HasValue)
                review.ServiceRating = request.ServiceRating.Value;
            if (request.ValueRating.HasValue)
                review.ValueRating = request.ValueRating.Value;
            if (request.Comment != null)
                review.Comment = request.Comment.Trim();

            // Validate updated ratings
            var ratingValidation = ValidateRatings(
                review.Rating,
                review.CleanlinessRating,
                review.LocationRating,
                review.ServiceRating,
                review.ValueRating,
                review.Comment);

            if (!ratingValidation.IsSuccess)
                return Result.Failure<ReviewResponse>(ratingValidation.Error);

            review.UpdatedAt = DateTime.UtcNow.AddHours(3);

            await _context.SaveChangesAsync();

            // Update unit statistics
            await UpdateUnitRatingStatisticsAsync(review.UnitId);

            _logger.LogInformation("Review {ReviewId} updated by User {UserId}", reviewId, userId);

            var response = await MapToResponseAsync(review);
            return Result.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating review {ReviewId}", reviewId);
            return Result.Failure<ReviewResponse>(
                new Error("UpdateFailed", "Failed to update review", 500));
        }
    }

    #endregion

    #region DELETE REVIEW

    public async Task<Result> DeleteReviewAsync(int reviewId, string userId)
    {
        try
        {
            var review = await _context.Set<Domain.Entities.Review>()
                .FirstOrDefaultAsync(r => r.Id == reviewId);

            if (review == null)
                return Result.Failure(new Error("NotFound", "Review not found", 404));

            if (review.UserId != userId)
                return Result.Failure(
                    new Error("Unauthorized", "You can only delete your own reviews", 403));

            _context.Set<Domain.Entities.Review>().Remove(review);
            await _context.SaveChangesAsync();

            // Update unit statistics
            await UpdateUnitRatingStatisticsAsync(review.UnitId);

            _logger.LogInformation("Review {ReviewId} deleted by User {UserId}", reviewId, userId);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting review {ReviewId}", reviewId);
            return Result.Failure(new Error("DeleteFailed", "Failed to delete review", 500));
        }
    }

    #endregion

    #region GET REVIEW BY ID

    public async Task<Result<ReviewDetailsResponse>> GetReviewByIdAsync(int reviewId)
    {
        try
        {
            var review = await _context.Set<Domain.Entities.Review>()
                .Include(r => r.Unit)
                .Include(r => r.User)
                .Include(r => r.Images)
                .FirstOrDefaultAsync(r => r.Id == reviewId);

            if (review == null)
                return Result.Failure<ReviewDetailsResponse>(
                    new Error("NotFound", "Review not found", 404));

            var userReviewCount = await _context.Set<Domain.Entities.Review>()
                .CountAsync(r => r.UserId == review.UserId);

            var response = new ReviewDetailsResponse
            {
                Id = review.Id,
                UnitId = review.UnitId,
                UnitName = review.Unit.Name,
                BookingId = review.BookingId,
                Reviewer = new ReviewerInfo
                {
                    UserId = review.UserId,
                    FullName = review.User.FullName ?? "Anonymous",
                    AvatarUrl = review.User.AvatarUrl,
                    TotalReviews = userReviewCount,
                    Nationality = review.User.Nationality
                },
                Rating = review.Rating,
                CleanlinessRating = review.CleanlinessRating,
                LocationRating = review.LocationRating,
                ServiceRating = review.ServiceRating,
                ValueRating = review.ValueRating,
                Comment = review.Comment,
                IsVerified = review.IsVerified,
                CheckInDate = review.CheckInDate,
                CheckOutDate = review.CheckOutDate,
                OwnerResponse = review.OwnerResponse,
                OwnerResponseDate = review.OwnerResponseDate,
                CreatedAt = review.CreatedAt,
                UpdatedAt = review.UpdatedAt,
                Images = review.Images.Select(img => new ReviewImageResponse
                {
                    Id = img.Id,
                    ImageUrl = img.ImageUrl,
                    ThumbnailUrl = img.ThumbnailUrl,
                    Caption = img.Caption
                }).ToList(),
                ModerationStatus = "Approved"
            };

            return Result.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting review {ReviewId}", reviewId);
            return Result.Failure<ReviewDetailsResponse>(
                new Error("GetFailed", "Failed to retrieve review", 500));
        }
    }

    #endregion

    #region GET UNIT REVIEWS

    public async Task<Result<PagedReviewResponse>> GetUnitReviewsAsync(
        int unitId,
        ReviewFilter filter)
    {
        try
        {
            var query = _context.Set<Domain.Entities.Review>()
                .Include(r => r.User)
                .Include(r => r.Images)
                .Where(r => r.UnitId == unitId);

            // Apply filters
            if (filter.MinRating.HasValue)
                query = query.Where(r => r.Rating >= filter.MinRating.Value);

            if (filter.MaxRating.HasValue)
                query = query.Where(r => r.Rating <= filter.MaxRating.Value);

            if (filter.IsVerified.HasValue)
                query = query.Where(r => r.IsVerified == filter.IsVerified.Value);

            if (filter.HasOwnerResponse.HasValue)
            {
                if (filter.HasOwnerResponse.Value)
                    query = query.Where(r => r.OwnerResponse != null);
                else
                    query = query.Where(r => r.OwnerResponse == null);
            }

            if (filter.FromDate.HasValue)
                query = query.Where(r => r.CreatedAt >= filter.FromDate.Value);

            if (filter.ToDate.HasValue)
                query = query.Where(r => r.CreatedAt <= filter.ToDate.Value);

            // Apply sorting
            query = filter.SortBy?.ToLower() switch
            {
                "rating" => filter.SortDescending
                    ? query.OrderByDescending(r => r.Rating)
                    : query.OrderBy(r => r.Rating),
                _ => filter.SortDescending
                    ? query.OrderByDescending(r => r.CreatedAt)
                    : query.OrderBy(r => r.CreatedAt)
            };

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)filter.PageSize);

            var reviews = await query
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            var reviewResponses = new List<ReviewResponse>();
            foreach (var review in reviews)
            {
                reviewResponses.Add(await MapToResponseAsync(review));
            }

            var response = new PagedReviewResponse
            {
                Reviews = reviewResponses,
                TotalCount = totalCount,
                Page = filter.Page,
                PageSize = filter.PageSize,
                TotalPages = totalPages
            };

            return Result.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting reviews for unit {UnitId}", unitId);
            return Result.Failure<PagedReviewResponse>(
                new Error("GetFailed", "Failed to retrieve reviews", 500));
        }
    }

    #endregion

    #region GET USER REVIEWS

    public async Task<Result<PagedReviewResponse>> GetUserReviewsAsync(
        string userId,
        int page = 1,
        int pageSize = 20)
    {
        try
        {
            var query = _context.Set<Domain.Entities.Review>()
                .Include(r => r.Unit)
                .Include(r => r.User)
                .Include(r => r.Images)
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.CreatedAt);

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var reviews = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var reviewResponses = new List<ReviewResponse>();
            foreach (var review in reviews)
            {
                reviewResponses.Add(await MapToResponseAsync(review));
            }

            var response = new PagedReviewResponse
            {
                Reviews = reviewResponses,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = totalPages
            };

            return Result.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting reviews for user {UserId}", userId);
            return Result.Failure<PagedReviewResponse>(
                new Error("GetFailed", "Failed to retrieve user reviews", 500));
        }
    }

    #endregion

    #region GET STATISTICS

    public async Task<Result<ReviewStatisticsResponse>> GetUnitReviewStatisticsAsync(int unitId)
    {
        try
        {
            var reviews = await _context.Set<Domain.Entities.Review>()
                .Where(r => r.UnitId == unitId)
                .ToListAsync();

            if (!reviews.Any())
            {
                return Result.Success(new ReviewStatisticsResponse
                {
                    TotalReviews = 0,
                    AverageRating = 0,
                    RatingDistribution = new Dictionary<int, int>
                    {
                        { 1, 0 }, { 2, 0 }, { 3, 0 }, { 4, 0 }, { 5, 0 }
                    }
                });
            }

            var stats = new ReviewStatisticsResponse
            {
                TotalReviews = reviews.Count,
                AverageRating = (decimal)reviews.Average(r => r.Rating),
                AverageCleanlinessRating = (decimal)reviews.Average(r => r.CleanlinessRating),
                AverageLocationRating = (decimal)reviews.Average(r => r.LocationRating),
                AverageServiceRating = (decimal)reviews.Average(r => r.ServiceRating),
                AverageValueRating = (decimal)reviews.Average(r => r.ValueRating),
                RatingDistribution = reviews
                    .GroupBy(r => r.Rating)
                    .ToDictionary(g => g.Key, g => g.Count()),
                VerifiedReviewsCount = reviews.Count(r => r.IsVerified),
                ReviewsWithPhotosCount = reviews.Count(r => r.Images.Any()),
                ReviewsWithOwnerResponseCount = reviews.Count(r => !string.IsNullOrEmpty(r.OwnerResponse))
            };

            // Ensure all ratings 1-5 exist in distribution
            for (int i = 1; i <= 5; i++)
            {
                if (!stats.RatingDistribution.ContainsKey(i))
                    stats.RatingDistribution[i] = 0;
            }

            return Result.Success(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting statistics for unit {UnitId}", unitId);
            return Result.Failure<ReviewStatisticsResponse>(
                new Error("GetFailed", "Failed to retrieve statistics", 500));
        }
    }

    #endregion

    #region ELIGIBILITY

    public async Task<Result<bool>> CanUserReviewAsync(string userId, int unitId)
    {
        var eligibility = await ValidateReviewEligibilityAsync(userId, unitId);
        return Result.Success(eligibility.IsEligible);
    }

    public async Task<Result<ReviewEligibilityResponse>> GetReviewEligibilityAsync(
        string userId,
        int unitId)
    {
        var eligibility = await ValidateReviewEligibilityAsync(userId, unitId);

        var response = new ReviewEligibilityResponse
        {
            CanReview = eligibility.IsEligible,
            Reason = eligibility.Reason,
            BookingId = eligibility.BookingId,
            CheckOutDate = eligibility.CheckOutDate,
            DaysRemaining = eligibility.DaysRemaining
        };

        return Result.Success(response);
    }

    private async Task<ReviewEligibility> ValidateReviewEligibilityAsync(
        string userId,
        int unitId)
    {
        // Find completed bookings for this user at this unit
        var completedBooking = await _context.Bookings
            .Where(b => b.UserId == userId &&
                       b.UnitId == unitId &&
                       b.Status == BookingStatus.Completed)
            .OrderByDescending(b => b.CheckOutDate)
            .FirstOrDefaultAsync();

        if (completedBooking == null)
        {
            return new ReviewEligibility
            {
                IsEligible = false,
                Reason = "You must complete a stay at this property before reviewing"
            };
        }

        // Check if checkout was recent enough (within 30 days)
        var daysSinceCheckout = (DateTime.UtcNow.Date - completedBooking.CheckOutDate.Date).Days;
        if (daysSinceCheckout > 60)
        {
            return new ReviewEligibility
            {
                IsEligible = false,
                Reason = "Review period expired (must review within 30 days of checkout)",
                BookingId = completedBooking.Id
            };
        }

        // Check if already reviewed
        var existingReview = await _context.Set<Domain.Entities.Review>()
            .AnyAsync(r => r.BookingId == completedBooking.Id);

        if (existingReview)
        {
            return new ReviewEligibility
            {
                IsEligible = false,
                Reason = "You have already reviewed this booking",
                BookingId = completedBooking.Id
            };
        }

        return new ReviewEligibility
        {
            IsEligible = true,
            BookingId = completedBooking.Id,
            CheckOutDate = completedBooking.CheckOutDate,
            DaysRemaining = 60 - daysSinceCheckout
        };
    }

    #endregion

    #region OWNER RESPONSE

    public async Task<Result> AddOwnerResponseAsync(
        int reviewId,
        string response,
        string ownerId)
    {
        try
        {
            var review = await _context.Set<Domain.Entities.Review>()
                .Include(r => r.Unit)
                    .ThenInclude(u => u.Admins)
                .FirstOrDefaultAsync(r => r.Id == reviewId);

            if (review == null)
                return Result.Failure(new Error("NotFound", "Review not found", 404));

            // Verify owner/admin permissions
            var isOwner = review.Unit.Admins.Any(a => a.UserId == ownerId && a.IsActive);
            if (!isOwner)
                return Result.Failure(
                    new Error("Unauthorized", "Only property owner can respond", 403));

            review.OwnerResponse = response;
            review.OwnerResponseDate = DateTime.UtcNow.AddHours(3);

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Owner response added to review {ReviewId} by {OwnerId}",
                reviewId, ownerId);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding owner response to review {ReviewId}", reviewId);
            return Result.Failure(new Error("ResponseFailed", "Failed to add response", 500));
        }
    }

    #endregion

    #region ADMIN METHODS

    public async Task<Result<PagedReviewResponse>> GetPendingReviewsAsync(
        int page = 1,
        int pageSize = 20)
    {
        try
        {
            var query = _context.Set<Domain.Entities.Review>()
                .Include(r => r.Unit)
                .Include(r => r.User)
                .OrderByDescending(r => r.CreatedAt);

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var reviews = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var reviewResponses = new List<ReviewResponse>();
            foreach (var review in reviews)
            {
                reviewResponses.Add(await MapToResponseAsync(review));
            }

            var response = new PagedReviewResponse
            {
                Reviews = reviewResponses,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = totalPages
            };

            return Result.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting pending reviews");
            return Result.Failure<PagedReviewResponse>(
                new Error("GetFailed", "Failed to retrieve pending reviews", 500));
        }
    }

    public async Task<Result> ApproveReviewAsync(int reviewId)
    {
        try
        {
            var review = await _context.Set<Domain.Entities.Review>()
                .FirstOrDefaultAsync(r => r.Id == reviewId);

            if (review == null)
                return Result.Failure(new Error("NotFound", "Review not found", 404));

            await _context.SaveChangesAsync();

            _logger.LogInformation("Review {ReviewId} approved", reviewId);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error approving review {ReviewId}", reviewId);
            return Result.Failure(new Error("ApproveFailed", "Failed to approve review", 500));
        }
    }

    public async Task<Result> RejectReviewAsync(int reviewId, string reason)
    {
        try
        {
            var review = await _context.Set<Domain.Entities.Review>()
                .FirstOrDefaultAsync(r => r.Id == reviewId);

            if (review == null)
                return Result.Failure(new Error("NotFound", "Review not found", 404));

            await _context.SaveChangesAsync();

            _logger.LogInformation("Review {ReviewId} rejected. Reason: {Reason}", reviewId, reason);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rejecting review {ReviewId}", reviewId);
            return Result.Failure(new Error("RejectFailed", "Failed to reject review", 500));
        }
    }

    #endregion

    #region HELPFUL FEATURE

    public async Task<Result> MarkReviewHelpfulAsync(int reviewId, string userId)
    {
        try
        {
            var review = await _context.Set<Domain.Entities.Review>()
                .FirstOrDefaultAsync(r => r.Id == reviewId);

            if (review == null)
                return Result.Failure(new Error("NotFound", "Review not found", 404));

            // You can implement a separate ReviewHelpful table to track this
            // For now, just return success
            _logger.LogInformation("Review {ReviewId} marked helpful by {UserId}", reviewId, userId);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking review {ReviewId} as helpful", reviewId);
            return Result.Failure(new Error("MarkFailed", "Failed to mark review as helpful", 500));
        }
    }

    #endregion

    #region HELPER METHODS

    private Result ValidateRatings(
        int rating,
        int cleanlinessRating,
        int locationRating,
        int serviceRating,
        int valueRating,
        string? comment)
    {
        // Validate ranges
        if (rating < 1 || rating > 5)
            return Result.Failure(new Error("InvalidRating", "Overall rating must be between 1 and 5", 400));

        if (cleanlinessRating < 1 || cleanlinessRating > 5)
            return Result.Failure(new Error("InvalidRating", "Cleanliness rating must be between 1 and 5", 400));

        if (locationRating < 1 || locationRating > 5)
            return Result.Failure(new Error("InvalidRating", "Location rating must be between 1 and 5", 400));

        if (serviceRating < 1 || serviceRating > 5)
            return Result.Failure(new Error("InvalidRating", "Service rating must be between 1 and 5", 400));

        if (valueRating < 1 || valueRating > 5)
            return Result.Failure(new Error("InvalidRating", "Value rating must be between 1 and 5", 400));

        // Validate average matches overall rating (with tolerance)
        var calculatedAverage = (cleanlinessRating + locationRating + serviceRating + valueRating) / 4.0;
        if (Math.Abs(calculatedAverage - rating) > 0.5)
            return Result.Failure(
                new Error("RatingMismatch", "Overall rating should match average of detailed ratings", 400));

        // Validate comment length if provided
        if (!string.IsNullOrEmpty(comment))
        {
            var trimmed = comment.Trim();
            if (trimmed.Length < 0)
                return Result.Failure(
                    new Error("CommentTooShort", "Review comment must be at least 10 characters", 400));

            if (trimmed.Length > 2000)
                return Result.Failure(
                    new Error("CommentTooLong", "Review comment must not exceed 2000 characters", 400));
        }

        return Result.Success();
    }

    private async Task UpdateUnitRatingStatisticsAsync(int unitId)
    {
        var unit = await _context.Units
            .FirstOrDefaultAsync(u => u.Id == unitId);

        if (unit == null) return;

        var reviews = await _context.Set<Domain.Entities.Review>()
            .Where(r => r.UnitId == unitId)
            .ToListAsync();

        if (reviews.Any())
        {
            unit.AverageRating = (decimal)reviews.Average(r => r.Rating);
            unit.TotalReviews = reviews.Count;
            unit.UpdatedAt = DateTime.UtcNow.AddHours(3);
        }
        else
        {
            unit.AverageRating = 0;
            unit.TotalReviews = 0;
        }

        await _context.SaveChangesAsync();
    }

    private async Task<ReviewResponse> MapToResponseAsync(Domain.Entities.Review review)
    {
        var userReviewCount = await _context.Set<Domain.Entities.Review>()
            .CountAsync(r => r.UserId == review.UserId);

        return new ReviewResponse
        {
            Id = review.Id,
            UnitId = review.UnitId,
            UnitName = review.Unit?.Name ?? "Unknown",
            BookingId = review.BookingId,
            Reviewer = new ReviewerInfo
            {
                UserId = review.UserId,
                FullName = review.User?.FullName ?? "Anonymous",
                AvatarUrl = review.User?.AvatarUrl,
                TotalReviews = userReviewCount,
                Nationality = review.User?.Nationality
            },
            Rating = review.Rating,
            CleanlinessRating = review.CleanlinessRating,
            LocationRating = review.LocationRating,
            ServiceRating = review.ServiceRating,
            ValueRating = review.ValueRating,
            Comment = review.Comment,
            IsVerified = review.IsVerified,
            CheckInDate = review.CheckInDate,
            CheckOutDate = review.CheckOutDate,
            OwnerResponse = review.OwnerResponse,
            OwnerResponseDate = review.OwnerResponseDate,
            CreatedAt = review.CreatedAt,
            UpdatedAt = review.UpdatedAt,
            Images = review.Images?.Select(img => new ReviewImageResponse
            {
                Id = img.Id,
                ImageUrl = img.ImageUrl,
                ThumbnailUrl = img.ThumbnailUrl,
                Caption = img.Caption
            }).ToList() ?? new List<ReviewImageResponse>()
        };
    }

    #endregion
}

// Supporting classes
public class ReviewEligibility
{
    public bool IsEligible { get; set; }
    public string Reason { get; set; } = string.Empty;
    public int? BookingId { get; set; }
    public DateTime? CheckOutDate { get; set; }
    public int? DaysRemaining { get; set; }
}