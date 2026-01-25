using Application.Contracts.Review;
using Application.Extensions;
using Application.Service.Review;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Hajzzy.Controllers;

[Route("api/reviews")]
[ApiController]
public class ReviewController(IReviewService reviewService) : ControllerBase
{
    private readonly IReviewService _reviewService = reviewService;

    // ========== CREATE REVIEW ==========

    /// <summary>
    /// Create a new review for a unit (requires completed booking)
    /// </summary>
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateReview([FromBody] CreateReviewRequest request)
    {
        var userId = User.GetUserId();
        request = request with { UserId = userId };

        var result = await _reviewService.CreateReviewAsync(request);

        return result.IsSuccess
            ? CreatedAtAction(nameof(GetReviewById), new { reviewId = result.Value.Id }, result.Value)
            : result.ToProblem();
    }


    [HttpPost("all")]
    public async Task<IActionResult> GetAllReviews([FromBody] AllReviewsFilter filter)
    {
        var result = await _reviewService.GetAllReviewsAsync(filter);

        return result.IsSuccess
            ? Ok(result.Value)
            : result.ToProblem();
    }
    // ========== UPDATE REVIEW ==========

    /// <summary>
    /// Update an existing review (only by the review author)
    /// </summary>
    [HttpPut("{reviewId:int}")]
    [Authorize]
    public async Task<IActionResult> UpdateReview(int reviewId, [FromBody] UpdateReviewRequest request)
    {
        var userId = User.GetUserId();
        var result = await _reviewService.UpdateReviewAsync(reviewId, request, userId);

        return result.IsSuccess
            ? Ok(result.Value)
            : result.ToProblem();
    }

    // ========== DELETE REVIEW ==========

    /// <summary>
    /// Delete a review (soft delete)
    /// </summary>
    [HttpDelete("{reviewId:int}")]
    [Authorize]
    public async Task<IActionResult> DeleteReview(int reviewId)
    {
        var userId = User.GetUserId();
        var result = await _reviewService.DeleteReviewAsync(reviewId, userId);

        return result.IsSuccess
            ? Ok(new { message = "Review deleted successfully" })
            : result.ToProblem();
    }

    // ========== GET REVIEW BY ID ==========

    /// <summary>
    /// Get a specific review by ID
    /// </summary>
    [HttpGet("{reviewId:int}")]
    public async Task<IActionResult> GetReviewById(int reviewId)
    {
        var result = await _reviewService.GetReviewByIdAsync(reviewId);

        return result.IsSuccess
            ? Ok(result.Value)
            : result.ToProblem();
    }

    // ========== GET UNIT REVIEWS ==========

    /// <summary>
    /// Get all reviews for a specific unit with filtering and pagination
    /// </summary>
    [HttpPost("unit/{unitId:int}")]
    public async Task<IActionResult> GetUnitReviews(
        int unitId,
        [FromBody] ReviewFilter filter)
    {
        var result = await _reviewService.GetUnitReviewsAsync(unitId, filter);

        return result.IsSuccess
            ? Ok(result.Value)
            : result.ToProblem();
    }

    // ========== GET UNIT REVIEW STATISTICS ==========

    /// <summary>
    /// Get review statistics for a unit
    /// </summary>
    [HttpGet("unit/{unitId:int}/statistics")]
    [Authorize(Roles = "CityAdmin,SuperAdmin,HotelAdmin,User")]
    public async Task<IActionResult> GetUnitReviewStatistics(int unitId)
    {
        var result = await _reviewService.GetUnitReviewStatisticsAsync(unitId);

        return result.IsSuccess
            ? Ok(result.Value)
            : result.ToProblem();
    }

    // ========== CHECK REVIEW ELIGIBILITY ==========

    /// <summary>
    /// Check if current user can review a specific unit
    /// </summary>
    [HttpGet("unit/{unitId:int}/eligibility")]
    [Authorize]
    public async Task<IActionResult> CheckReviewEligibility(int unitId)
    {
        var userId = User.GetUserId();
        var result = await _reviewService.GetReviewEligibilityAsync(userId, unitId);

        return result.IsSuccess
            ? Ok(result.Value)
            : result.ToProblem();
    }

    /// <summary>
    /// Simple check if user can review (returns boolean)
    /// </summary>
    [HttpGet("unit/{unitId:int}/can-review")]
    [Authorize]
    public async Task<IActionResult> CanUserReview(int unitId)
    {
        var userId = User.GetUserId();
        var result = await _reviewService.CanUserReviewAsync(userId, unitId);

        return result.IsSuccess
            ? Ok(new { canReview = result.Value })
            : result.ToProblem();
    }

    // ========== OWNER RESPONSE ==========

    /// <summary>
    /// Add owner/admin response to a review
    /// </summary>
    [HttpPost("{reviewId:int}/owner-response")]
    [Authorize(Roles = "HotelAdmin,SuperAdmin,CityAdmin")]
    public async Task<IActionResult> AddOwnerResponse(
        int reviewId,
        [FromBody] AddOwnerResponseRequest request)
    {
        var ownerId = User.GetUserId();
        var result = await _reviewService.AddOwnerResponseAsync(reviewId, request.Response, ownerId);

        return result.IsSuccess
            ? Ok(new { message = "Response added successfully" })
            : result.ToProblem();
    }

    // ========== GET MY REVIEWS ==========

    /// <summary>
    /// Get all reviews written by the current user
    /// </summary>
    [HttpGet("my-reviews")]
    [Authorize]
    public async Task<IActionResult> GetMyReviews([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var userId = User.GetUserId();
        var result = await _reviewService.GetUserReviewsAsync(userId, page, pageSize);

        return result.IsSuccess
            ? Ok(result.Value)
            : result.ToProblem();
    }

    // ========== HELPFUL/UNHELPFUL (OPTIONAL) ==========

    /// <summary>
    /// Mark a review as helpful
    /// </summary>
    [HttpPost("{reviewId:int}/helpful")]
    [Authorize]
    public async Task<IActionResult> MarkReviewHelpful(int reviewId)
    {
        var userId = User.GetUserId();
        var result = await _reviewService.MarkReviewHelpfulAsync(reviewId, userId);

        return result.IsSuccess
            ? Ok(new { message = "Review marked as helpful" })
            : result.ToProblem();
    }

    // ========== ADMIN: MODERATE REVIEWS ==========

    /// <summary>
    /// Get pending reviews for moderation (Admin only)
    /// </summary>
    [HttpGet("admin/pending")]
    [Authorize(Roles = "CityAdmin,SuperAdmin")]
    public async Task<IActionResult> GetPendingReviews(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await _reviewService.GetPendingReviewsAsync(page, pageSize);

        return result.IsSuccess
            ? Ok(result.Value)
            : result.ToProblem();
    }

    /// <summary>
    /// Approve a review (Admin only)
    /// </summary>
    [HttpPost("admin/{reviewId:int}/approve")]
    [Authorize(Roles = "CityAdmin,SuperAdmin")]
    public async Task<IActionResult> ApproveReview(int reviewId)
    {
        var result = await _reviewService.ApproveReviewAsync(reviewId);

        return result.IsSuccess
            ? Ok(new { message = "Review approved" })
            : result.ToProblem();
    }

    /// <summary>
    /// Reject a review (Admin only)
    /// </summary>
    [HttpPost("admin/{reviewId:int}/reject")]
    [Authorize(Roles = "CityAdmin,SuperAdmin")]
    public async Task<IActionResult> RejectReview(int reviewId, [FromBody] string reason)
    {
        var result = await _reviewService.RejectReviewAsync(reviewId, reason);

        return result.IsSuccess
            ? Ok(new { message = "Review rejected" })
            : result.ToProblem();
    }
}