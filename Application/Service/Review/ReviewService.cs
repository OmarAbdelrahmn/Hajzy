//using Application.Abstraction;
//using Application.Service.Review;
//using Domain;
//using Hangfire;
using Domain;
using Microsoft.Extensions.Logging;
namespace Application.Service.Review;

public class ReviewService(
    ApplicationDbcontext context,
    ILogger<ReviewService> logger) : IReviewService
{
    //    private readonly ApplicationDbcontext _context = context;
    //    private readonly ILogger<ReviewService> _logger = logger;

    //    #region CREATE REVIEW WITH VALIDATION

    //    public async Task<Result<ReviewResponse>> CreateReviewAsync(CreateReviewRequest request)
    //    {
    //        try
    //        {
    //            // 1. Validate user has completed booking at this unit
    //            var eligibility = await ValidateReviewEligibilityAsync(
    //                request.UserId,
    //                request.UnitId);

    //            if (!eligibility.IsEligible)
    //                return Result.Failure<ReviewResponse>(
    //                    new Error("NotEligible", eligibility.Reason, 403));

    //            // 2. Check if user already reviewed this booking
    //            var existingReview = await _context.Set<Domain.Entities.Review>()
    //                .FirstOrDefaultAsync(r => r.BookingId == eligibility.BookingId &&
    //                                         !r.IsDeleted);

    //            if (existingReview != null)
    //                return Result.Failure<ReviewResponse>(
    //                    new Error("AlreadyReviewed",
    //                        "You have already reviewed this booking", 400));

    //            // 3. Validate ratings
    //            var ratingValidation = ValidateRatings(request);
    //            if (!ratingValidation.IsSuccess)
    //                return Result.Failure<ReviewResponse>(ratingValidation.Error);

    //            // 4. Get booking details for verification
    //            var booking = await _context.Bookings
    //                .Include(b => b.Unit)
    //                .FirstOrDefaultAsync(b => b.Id == eligibility.BookingId);

    //            if (booking == null)
    //                return Result.Failure<ReviewResponse>(
    //                    new Error("BookingNotFound", "Booking not found", 404));

    //            // 5. Ensure review is within allowed timeframe (e.g., 30 days after checkout)
    //            var daysSinceCheckout = (DateTime.UtcNow.Date - booking.CheckOutDate.Date).Days;
    //            if (daysSinceCheckout > 30)
    //                return Result.Failure<ReviewResponse>(
    //                    new Error("ReviewExpired",
    //                        "Review period has expired (30 days after checkout)", 400));

    //            // 6. Create review
    //            var review = new Domain.Entities.Review
    //            {
    //                UnitId = request.UnitId,
    //                BookingId = eligibility.BookingId,
    //                UserId = request.UserId,

    //                // Overall rating
    //                Rating = request.Rating,

    //                // Detailed ratings
    //                CleanlinessRating = request.CleanlinessRating,
    //                LocationRating = request.LocationRating,
    //                ServiceRating = request.ServiceRating,
    //                ValueRating = request.ValueRating,

    //                Comment = request.Comment?.Trim(),

    //                // Automatically verified since it's based on actual booking
    //                IsVerified = true,
    //                VerifiedPurchase = true,
    //                StayDuration = booking.NumberOfNights,
    //                CheckInDate = booking.CheckInDate,
    //                CheckOutDate = booking.CheckOutDate,

    //                CreatedAt = DateTime.UtcNow.AddHours(3),

    //                // Moderation
    //                ModerationStatus = ModerationStatus.Pending,
    //                IsPublished = false // Will be published after moderation
    //            };

    //            await _context.Set<Domain.Entities.Review>().AddAsync(review);

    //            // 7. Update unit statistics
    //            await UpdateUnitRatingStatisticsAsync(request.UnitId);

    //            await _context.SaveChangesAsync();

    //            // 8. Handle review images if provided
    //            if (request.ImageIds?.Any() == true)
    //            {
    //                await AttachReviewImagesAsync(review.Id, request.ImageIds);
    //            }

    //            // 9. Queue moderation
    //            BackgroundJob.Enqueue(() => ModerateReviewAsync(review.Id));

    //            _logger.LogInformation(
    //                "Review created for Unit {UnitId} by User {UserId}. Rating: {Rating}",
    //                request.UnitId, request.UserId, request.Rating);

    //            var response = await MapToResponseAsync(review);
    //            return Result.Success(response);
    //        }
    //        catch (Exception ex)
    //        {
    //            _logger.LogError(ex, "Error creating review");
    //            return Result.Failure<ReviewResponse>(
    //                new Error("ReviewFailed", "Failed to create review", 500));
    //        }
    //    }

    //    #endregion

    //    #region REVIEW ELIGIBILITY VALIDATION

    //    private async Task<ReviewEligibility> ValidateReviewEligibilityAsync(
    //        string userId,
    //        int unitId)
    //    {
    //        // Find completed bookings for this user at this unit
    //        var completedBooking = await _context.Bookings
    //            .Where(b => b.UserId == userId &&
    //                       b.UnitId == unitId &&
    //                       b.Status == BookingStatus.Completed &&
    //                       !b.IsDeleted)
    //            .OrderByDescending(b => b.CheckOutDate)
    //            .FirstOrDefaultAsync();

    //        if (completedBooking == null)
    //        {
    //            return new ReviewEligibility
    //            {
    //                IsEligible = false,
    //                Reason = "You must complete a stay at this property before reviewing"
    //            };
    //        }

    //        // Check if checkout was recent enough (within 30 days)
    //        var daysSinceCheckout = (DateTime.UtcNow.Date - completedBooking.CheckOutDate.Date).Days;
    //        if (daysSinceCheckout > 30)
    //        {
    //            return new ReviewEligibility
    //            {
    //                IsEligible = false,
    //                Reason = "Review period expired (must review within 30 days of checkout)",
    //                BookingId = completedBooking.Id
    //            };
    //        }

    //        // Check if already reviewed
    //        var existingReview = await _context.Set<Domain.Entities.Review>()
    //            .AnyAsync(r => r.BookingId == completedBooking.Id && !r.IsDeleted);

    //        if (existingReview)
    //        {
    //            return new ReviewEligibility
    //            {
    //                IsEligible = false,
    //                Reason = "You have already reviewed this booking",
    //                BookingId = completedBooking.Id
    //            };
    //        }

    //        return new ReviewEligibility
    //        {
    //            IsEligible = true,
    //            BookingId = completedBooking.Id,
    //            CheckOutDate = completedBooking.CheckOutDate,
    //            DaysRemaining = 30 - daysSinceCheckout
    //        };
    //    }

    //    public async Task<Result<bool>> CanUserReviewAsync(string userId, int unitId)
    //    {
    //        var eligibility = await ValidateReviewEligibilityAsync(userId, unitId);
    //        return Result.Success(eligibility.IsEligible);
    //    }

    //    public async Task<Result<ReviewEligibilityResponse>> GetReviewEligibilityAsync(
    //        string userId,
    //        int unitId)
    //    {
    //        var eligibility = await ValidateReviewEligibilityAsync(userId, unitId);

    //        var response = new ReviewEligibilityResponse
    //        {
    //            CanReview = eligibility.IsEligible,
    //            Reason = eligibility.Reason,
    //            BookingId = eligibility.BookingId,
    //            CheckOutDate = eligibility.CheckOutDate,
    //            DaysRemaining = eligibility.DaysRemaining
    //        };

    //        return Result.Success(response);
    //    }

    //    #endregion

    //    #region RATING VALIDATION

    //    private Result ValidateRatings(CreateReviewRequest request)
    //    {
    //        // Validate overall rating
    //        if (request.Rating < 1 || request.Rating > 5)
    //            return Result.Failure(
    //                new Error("InvalidRating", "Overall rating must be between 1 and 5", 400));

    //        // Validate detailed ratings
    //        if (request.CleanlinessRating < 1 || request.CleanlinessRating > 5)
    //            return Result.Failure(
    //                new Error("InvalidRating", "Cleanliness rating must be between 1 and 5", 400));

    //        if (request.LocationRating < 1 || request.LocationRating > 5)
    //            return Result.Failure(
    //                new Error("InvalidRating", "Location rating must be between 1 and 5", 400));

    //        if (request.ServiceRating < 1 || request.ServiceRating > 5)
    //            return Result.Failure(
    //                new Error("InvalidRating", "Service rating must be between 1 and 5", 400));

    //        if (request.ValueRating < 1 || request.ValueRating > 5)
    //            return Result.Failure(
    //                new Error("InvalidRating", "Value rating must be between 1 and 5", 400));

    //        // Validate average matches overall rating (with tolerance)
    //        var calculatedAverage = (request.CleanlinessRating + request.LocationRating +
    //                                request.ServiceRating + request.ValueRating) / 4.0;

    //        if (Math.Abs(calculatedAverage - request.Rating) > 0.5)
    //            return Result.Failure(
    //                new Error("RatingMismatch",
    //                    "Overall rating should match average of detailed ratings", 400));

    //        // Validate comment length if provided
    //        if (!string.IsNullOrEmpty(request.Comment))
    //        {
    //            var trimmed = request.Comment.Trim();
    //            if (trimmed.Length < 10)
    //                return Result.Failure(
    //                    new Error("CommentTooShort",
    //                        "Review comment must be at least 10 characters", 400));

    //            if (trimmed.Length > 2000)
    //                return Result.Failure(
    //                    new Error("CommentTooLong",
    //                        "Review comment must not exceed 2000 characters", 400));
    //        }

    //        return Result.Success();
    //    }

    //    #endregion

    //    #region UPDATE UNIT STATISTICS

    //    private async Task UpdateUnitRatingStatisticsAsync(int unitId)
    //    {
    //        var unit = await _context.Units
    //            .FirstOrDefaultAsync(u => u.Id == unitId);

    //        if (unit == null) return;

    //        // Calculate new averages from published reviews only
    //        var reviews = await _context.Set<Domain.Entities.Review>()
    //            .Where(r => r.UnitId == unitId &&
    //                       r.IsPublished &&
    //                       !r.IsDeleted)
    //            .ToListAsync();

    //        if (reviews.Any())
    //        {
    //            unit.AverageRating = reviews.Average(r => r.Rating);
    //            unit.TotalReviews = reviews.Count;

    //            // Store detailed averages
    //            unit.AverageCleanlinessRating = reviews.Average(r => r.CleanlinessRating);
    //            unit.AverageLocationRating = reviews.Average(r => r.LocationRating);
    //            unit.AverageServiceRating = reviews.Average(r => r.ServiceRating);
    //            unit.AverageValueRating = reviews.Average(r => r.ValueRating);

    //            unit.UpdatedAt = DateTime.UtcNow.AddHours(3);
    //        }

    //        await _context.SaveChangesAsync();
    //    }

    //    #endregion

    //    #region OWNER RESPONSE

    //    public async Task<Result> AddOwnerResponseAsync(
    //        int reviewId,
    //        string response,
    //        string ownerId)
    //    {
    //        try
    //        {
    //            var review = await _context.Set<Domain.Entities.Review>()
    //                .Include(r => r.Unit)
    //                    .ThenInclude(u => u.Admins)
    //                .FirstOrDefaultAsync(r => r.Id == reviewId && !r.IsDeleted);

    //            if (review == null)
    //                return Result.Failure(new Error("NotFound", "Review not found", 404));

    //            // Verify owner/admin permissions
    //            var isOwner = review.Unit.Admins.Any(a => a.UserId == ownerId && a.IsActive);
    //            if (!isOwner)
    //                return Result.Failure(
    //                    new Error("Unauthorized", "Only property owner can respond", 403));

    //            // Validate response
    //            if (string.IsNullOrWhiteSpace(response))
    //                return Result.Failure(
    //                    new Error("InvalidResponse", "Response cannot be empty", 400));

    //            if (response.Length > 1000)
    //                return Result.Failure(
    //                    new Error("ResponseTooLong",
    //                        "Response must not exceed 1000 characters", 400));

    //            review.OwnerResponse = response.Trim();
    //            review.OwnerResponseDate = DateTime.UtcNow.AddHours(3);
    //            review.RespondedBy = ownerId;

    //            await _context.SaveChangesAsync();

    //            _logger.LogInformation(
    //                "Owner response added to review {ReviewId} by {OwnerId}",
    //                reviewId, ownerId);

    //            return Result.Success();
    //        }
    //        catch (Exception ex)
    //        {
    //            _logger.LogError(ex, "Error adding owner response to review {ReviewId}", reviewId);
    //            return Result.Failure(new Error("ResponseFailed", "Failed to add response", 500));
    //        }
    //    }

    //    #endregion

    //    #region REVIEW MODERATION

    //    public async Task ModerateReviewAsync(int reviewId)
    //    {
    //        try
    //        {
    //            var review = await _context.Set<Domain.Entities.Review>()
    //                .FirstOrDefaultAsync(r => r.Id == reviewId);

    //            if (review == null) return;

    //            // Auto-moderation checks
    //            var moderationResult = await PerformAutoModerationAsync(review);

    //            review.ModerationStatus = moderationResult.Status;
    //            review.ModerationNotes = moderationResult.Notes;
    //            review.ModeratedAt = DateTime.UtcNow.AddHours(3);

    //            // Auto-approve if passes all checks
    //            if (moderationResult.Status == ModerationStatus.Approved)
    //            {
    //                review.IsPublished = true;
    //                review.PublishedAt = DateTime.UtcNow.AddHours(3);
    //            }

    //            await _context.SaveChangesAsync();

    //            _logger.LogInformation(
    //                "Review {ReviewId} moderated. Status: {Status}",
    //                reviewId, moderationResult.Status);
    //        }
    //        catch (Exception ex)
    //        {
    //            _logger.LogError(ex, "Error moderating review {ReviewId}", reviewId);
    //        }
    //    }

    //    private async Task<ModerationResult> PerformAutoModerationAsync(Domain.Entities.Review review)
    //    {
    //        var result = new ModerationResult
    //        {
    //            Status = ModerationStatus.Approved
    //        };

    //        // Check for spam patterns
    //        if (ContainsSpamPatterns(review.Comment))
    //        {
    //            result.Status = ModerationStatus.Flagged;
    //            result.Notes = "Potential spam detected";
    //            return result;
    //        }

    //        // Check for inappropriate content
    //        if (ContainsInappropriateContent(review.Comment))
    //        {
    //            result.Status = ModerationStatus.Rejected;
    //            result.Notes = "Inappropriate content detected";
    //            return result;
    //        }

    //        // Check for external links (not allowed)
    //        if (ContainsExternalLinks(review.Comment))
    //        {
    //            result.Status = ModerationStatus.Flagged;
    //            result.Notes = "Contains external links";
    //            return result;
    //        }

    //        // Check for personal information
    //        if (ContainsPersonalInfo(review.Comment))
    //        {
    //            result.Status = ModerationStatus.Flagged;
    //            result.Notes = "Contains personal information";
    //            return result;
    //        }

    //        return result;
    //    }

    //    private bool ContainsSpamPatterns(string text)
    //    {
    //        if (string.IsNullOrEmpty(text)) return false;

    //        var spamKeywords = new[] { "click here", "buy now", "limited offer", "act fast" };
    //        return spamKeywords.Any(keyword =>
    //            text.Contains(keyword, StringComparison.OrdinalIgnoreCase));
    //    }

    //    private bool ContainsInappropriateContent(string text)
    //    {
    //        if (string.IsNullOrEmpty(text)) return false;

    //        // Implement profanity filter
    //        var inappropriateWords = new[] { /* profanity list */ };
    //        return inappropriateWords.Any(word =>
    //            text.Contains(word, StringComparison.OrdinalIgnoreCase));
    //    }

    //    private bool ContainsExternalLinks(string text)
    //    {
    //        if (string.IsNullOrEmpty(text)) return false;

    //        return text.Contains("http://", StringComparison.OrdinalIgnoreCase) ||
    //               text.Contains("https://", StringComparison.OrdinalIgnoreCase) ||
    //               text.Contains("www.", StringComparison.OrdinalIgnoreCase);
    //    }

    //    private bool ContainsPersonalInfo(string text)
    //    {
    //        if (string.IsNullOrEmpty(text)) return false;

    //        // Basic email pattern
    //        var emailPattern = @"\b[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Z|a-z]{2,}\b";
    //        // Basic phone pattern
    //        var phonePattern = @"\b\d{3}[-.]?\d{3}[-.]?\d{4}\b";

    //        return System.Text.RegularExpressions.Regex.IsMatch(text, emailPattern) ||
    //               System.Text.RegularExpressions.Regex.IsMatch(text, phonePattern);
    //    }

    //    #endregion

    //    // ... Additional methods for Update, Delete, GetReviews, etc.
    //}

    //#region SUPPORTING CLASSES

    //public class ReviewEligibility
    //{
    //    public bool IsEligible { get; set; }
    //    public string Reason { get; set; } = string.Empty;
    //    public int? BookingId { get; set; }
    //    public DateTime? CheckOutDate { get; set; }
    //    public int? DaysRemaining { get; set; }
    //}

    //public class ModerationResult
    //{
    //    public ModerationStatus Status { get; set; }
    //    public string Notes { get; set; } = string.Empty;
    //}

    //public enum ModerationStatus
    //{
    //    Pending,
    //    Approved,
    //    Rejected,
    //    Flagged
    //}

    //public class CreateReviewRequest
    //{
    //    public int UnitId { get; set; }
    //    public string UserId { get; set; } = string.Empty;
    //    public int Rating { get; set; }
    //    public int CleanlinessRating { get; set; }
    //    public int LocationRating { get; set; }
    //    public int ServiceRating { get; set; }
    //    public int ValueRating { get; set; }
    //    public string? Comment { get; set; }
    //    public List<int>? ImageIds { get; set; }
    //}

    //public class ReviewEligibilityResponse
    //{
    //    public bool CanReview { get; set; }
    //    public string Reason { get; set; } = string.Empty;
    //    public int? BookingId { get; set; }
    //    public DateTime? CheckOutDate { get; set; }
    //    public int? DaysRemaining { get; set; }
    //}

    //#endregion}
}