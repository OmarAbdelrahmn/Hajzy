using Application.Abstraction;
using Application.Contracts.Review;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Service.DepartmentAdminService.Review
{
    public interface IDAReviewService
    {
        // Create & Update
        Task<Result<ReviewResponse>> CreateReviewAsync(CreateReviewRequest request);
        Task<Result<ReviewResponse>> UpdateReviewAsync(int reviewId, UpdateReviewRequest request, string userId);
        Task<Result> DeleteReviewAsync(int reviewId, string userId);

        // Read
        Task<Result<ReviewDetailsResponse>> GetReviewByIdAsync(int reviewId);
        Task<Result<PagedReviewResponse>> GetUnitReviewsAsync(int unitId, ReviewFilter filter);
        Task<Result<PagedReviewResponse>> GetUserReviewsAsync(string userId, int page = 1, int pageSize = 10);
        Task<Result<ReviewStatisticsResponse>> GetUnitReviewStatisticsAsync(int unitId);

        // Eligibility
        Task<Result<bool>> CanUserReviewAsync(string userId, int unitId);
        Task<Result<ReviewEligibilityResponse>> GetReviewEligibilityAsync(string userId, int unitId);

        // Owner Response
        Task<Result> AddOwnerResponseAsync(int reviewId, string response, string ownerId);

        // Admin/Moderation (optional - if you want manual moderation)
        Task<Result<PagedReviewResponse>> GetPendingReviewsAsync(int page = 1, int pageSize = 20);
        Task<Result> ApproveReviewAsync(int reviewId);
        Task<Result> RejectReviewAsync(int reviewId, string reason);

        // Helpful feature (optional)
        Task<Result> MarkReviewHelpfulAsync(int reviewId, string userId);
    }
}
