using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Contracts.AD;

internal class ad
{
}


// ============= CREATE REQUEST =============
public record CreateAdRequest(
    string? Title,
    string? Description,
    IFormFile Image,
    int? UnitId,
    DateTime StartDate,
    DateTime EndDate,
    bool IsActive = true
);

// ============= UPDATE REQUEST =============
public record UpdateAdRequest(
    string? Title,
    string? Description,
    IFormFile? Image,
    int? UnitId,
    DateTime? StartDate,
    DateTime? EndDate,
    bool? IsActive
);
public class PaginatedResponse<T>
{
    public IEnumerable<T> Items { get; set; } = [];
    public int TotalPages { get; set; }
    public int CurrentPage { get; set; }
    public int? NextPage { get; set; }
    public int? PrevPage { get; set; }
    public int TotalCount { get; set; }
}
// ============= RESPONSE =============
public record AdResponse(
    int Id,
    string? Title,
    string? Description,
    string ImageUrl,
    int? UnitId,
    string? UnitName,
    DateTime StartDate,
    DateTime EndDate,
    bool IsActive,
    bool IsExpired,
    string UploadedByUserId,
    string? UploadedByName,
    DateTime CreatedAt
);

// ============= LIST FILTER =============
public class AdListFilter
{
    public bool? IsActive { get; set; }
    public bool? IsExpired { get; set; }
    public int? UnitId { get; set; }
    public DateTime? StartDateFrom { get; set; }
    public DateTime? StartDateTo { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}