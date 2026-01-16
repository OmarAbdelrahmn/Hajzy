using Microsoft.AspNetCore.Http;
using System;

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
    bool? IsActive
);

// ============= RESPONSE =============
public record OfferResponse(
    int Id,
    string? Title,
    string? Description,
    string ImageUrl,
    int? UnitId,
    string? UnitName,
    DateTime StartDate,
    DateTime EndDate,
    bool IsFeatured,
    decimal? DiscountPercentage,
    decimal? DiscountAmount,
    bool IsActive,
    bool IsExpired,
    string UploadedByUserId,
    string? UploadedByName,
    DateTime CreatedAt
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