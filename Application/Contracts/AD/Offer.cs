using Microsoft.AspNetCore.Http;

namespace Application.Contracts.Offer;

// ============= CREATE REQUEST =============
public record CreateOfferRequest(
    string? Title,
    string? Description,
    IFormFile Image,
    int? UnitId,
    bool IsFeatured,
    DateTime StartDate,
    DateTime EndDate,
    decimal? DiscountPercentage,
    decimal? DiscountAmount,
    string? Link,
    bool IsActive = true
);

// ============= UPDATE REQUEST =============
public record UpdateOfferRequest(
    string? Title,
    string? Description,
    IFormFile? Image,
    int? UnitId,
    DateTime? StartDate,
    bool? IsFeatured,
    DateTime? EndDate,
    decimal? DiscountPercentage,
    decimal? DiscountAmount,
    bool? IsActive,
    string? Link
);

// ============= RESPONSE =============
public record OfferResponse(
    int Id,
    string? Title,
    string? Description,
    string ImageUrl,
    int? UnitId,
    string? UnitName,
    string? UnitTypeName,
    DateTime StartDate,
    DateTime EndDate,
    bool IsFeatured,
    decimal? DiscountPercentage,
    decimal? DiscountAmount,
    bool IsActive,
    bool IsExpired,
    string UploadedByUserId,
    string? UploadedByName,
    DateTime CreatedAt,
    string? Link
);

// ============= LIST FILTER =============
public class OfferListFilter
{
    public bool? IsActive { get; set; }
    public bool? IsExpired { get; set; }
    public bool? IsFeatured { get; set; }
    public int? UnitId { get; set; }
    public DateTime? StartDateFrom { get; set; }
    public DateTime? StartDateTo { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}