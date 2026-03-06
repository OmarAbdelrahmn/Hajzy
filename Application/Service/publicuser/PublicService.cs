// Application/Service/Public/PublicService.cs
using Application.Abstraction;
using Application.Contracts.Aminety;
using Application.Contracts.other;
using Application.Contracts.publicuser;
using Application.Contracts.SubUnit;
using Application.Contracts.Unit;
using Application.Service.SubUnit;   // for OptionValueResponse
using Domain;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Service.publicuser;

public class PublicService(ApplicationDbcontext context) : IPublicServise
{
    private readonly ApplicationDbcontext _context = context;


    public async Task<Result<IEnumerable<Contracts.hoteladmincont.PackageResponse>>> GetUnitPackagesAsync(int unitId)
    {
        try
        {
            var unitExists = await _context.Units
                .AnyAsync(u => u.Id == unitId && !u.IsDeleted && u.IsActive && u.IsVerified);

            if (!unitExists)
                return Result.Failure<IEnumerable<Contracts.hoteladmincont.PackageResponse>>(
                    new Error("NotFound", "Unit not found or not available", 404));

            var packages = await _context.Set<Package>()
                .Where(p => p.UnitId == unitId && p.IsActive)
                .Select(p => new Contracts.hoteladmincont.PackageResponse
                {
                    Id = p.Id,
                    UnitId = p.UnitId,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    Features = System.Text.Json.JsonSerializer.Deserialize<List<string>>(p.FeaturesJson) ?? new(),
                    IsActive = p.IsActive,
                    CreatedAt = p.CreatedAt
                }).AsNoTracking()
                .ToListAsync(); 
                

            return Result.Success<IEnumerable<Contracts.hoteladmincont.PackageResponse>>(packages);
        }
        catch
        {
            return Result.Failure<IEnumerable<Contracts.hoteladmincont.PackageResponse>>(
                new Error("GetFailed", "Failed to retrieve unit packages", 500));
        }
    }
    public async Task<Result<List<PublicImageInfo>>> GetUnitImagesAsync(int unitId)
    {
        try
        {
            var exists = await _context.Units
                .AnyAsync(u => u.Id == unitId && !u.IsDeleted && u.IsActive && u.IsVerified);

            if (!exists)
                return Result.Failure<List<PublicImageInfo>>(
                    new Error("NotFound", "Unit not found or not available", 404));

            var images = await _context.Units
                .Where(u => u.Id == unitId)
                .SelectMany(u => u.Images.Where(i => !i.IsDeleted))
                .OrderBy(i => i.DisplayOrder)
                .Select(i => new PublicImageInfo
                {
                    ImageUrl = i.ImageUrl,
                    ThumbnailUrl = i.ThumbnailUrl,
                    MediumUrl = i.MediumUrl,
                    IsPrimary = i.IsPrimary,
                    DisplayOrder = i.DisplayOrder,
                    Caption = i.Caption
                })
                .AsNoTracking()
                .ToListAsync();

            return Result.Success(images);
        }
        catch
        {
            return Result.Failure<List<PublicImageInfo>>(
                new Error("GetFailed", "Failed to retrieve unit images", 500));
        }
    }

    public async Task<Result<List<PublicImageInfo>>> GetSubUnitImagesAsync(int subUnitId)
    {
        try
        {
            var exists = await _context.SubUnits
                .AnyAsync(s => s.Id == subUnitId && !s.IsDeleted && s.IsAvailable
                            && !s.Unit.IsDeleted && s.Unit.IsActive && s.Unit.IsVerified);

            if (!exists)
                return Result.Failure<List<PublicImageInfo>>(
                    new Error("NotFound", "SubUnit not found or not available", 404));

            var images = await _context.SubUnits
                .Where(s => s.Id == subUnitId)
                .SelectMany(s => s.SubUnitImages.Where(i => !i.IsDeleted))
                .OrderBy(i => i.DisplayOrder)
                .Select(i => new PublicImageInfo
                {
                    ImageUrl = i.ImageUrl,
                    ThumbnailUrl = i.ThumbnailUrl,
                    MediumUrl = i.MediumUrl,
                    IsPrimary = i.IsPrimary,
                    DisplayOrder = i.DisplayOrder,
                    Caption = i.Caption
                })
                .AsNoTracking()
                .ToListAsync();

            return Result.Success(images);
        }
        catch
        {
            return Result.Failure<List<PublicImageInfo>>(
                new Error("GetFailed", "Failed to retrieve subunit images", 500));
        }
    }

    #region UNITS

    public async Task<Result<PaginatedResponse<PublicUnitResponse>>> GetAllUnitsAsync(
        PublicUnitFilter filter)
    {
        try
        {
            var query = _context.Units
                .Include(c => c.Currency)
                .Include(u => u.CustomPolicies.Where(p => p.IsActive))
                .Include(u => u.City)
                .Include(u => u.UnitType)
                .Include(u => u.Images.Where(i => !i.IsDeleted))
                // ── NEW: unit-level option values ──────────────────────────
                .Include(u => u.OptionValues)
                    .ThenInclude(ov => ov.UnitTypeOption)
                // ──────────────────────────────────────────────────────────
                .Where(u => !u.IsDeleted && u.IsActive && u.IsVerified)
                .AsQueryable();

            if (filter.CityId.HasValue)
                query = query.Where(u => u.CityId == filter.CityId.Value);
            if (!string.IsNullOrWhiteSpace(filter.CityName))
                query = query.Where(u => u.City.Name.Contains(filter.CityName));
            if (!string.IsNullOrWhiteSpace(filter.Country))
                query = query.Where(u => u.City.Country.Contains(filter.Country));
            if (filter.UnitTypeId.HasValue)
                query = query.Where(u => u.UnitTypeId == filter.UnitTypeId.Value);
            if (filter.MinPrice.HasValue)
                query = query.Where(u => u.BasePrice >= filter.MinPrice.Value);
            if (filter.MaxPrice.HasValue)
                query = query.Where(u => u.BasePrice <= filter.MaxPrice.Value);
            if (filter.MinRating.HasValue)
                query = query.Where(u => u.AverageRating >= filter.MinRating.Value);
            if (filter.MinGuests.HasValue)
                query = query.Where(u => u.MaxGuests >= filter.MinGuests.Value);

            if (filter.Amenities?.Any() == true)
            {
                foreach (var amenity in filter.Amenities)
                    query = query.Where(u => u.UnitAmenities
                        .Any(ua => ua.Amenity.Name.ToString().Contains(amenity) && ua.IsAvailable));
            }

            if (filter.CheckIn.HasValue && filter.CheckOut.HasValue)
            {
                var checkIn = filter.CheckIn.Value;
                var checkOut = filter.CheckOut.Value;
                query = query.Where(u => u.Rooms.Any(r =>
                    !r.IsDeleted && r.IsAvailable &&
                    !r.BookingRooms.Any(br =>
                        br.Booking.CheckInDate < checkOut &&
                        br.Booking.CheckOutDate > checkIn &&
                        br.Booking.Status != BookingStatus.Cancelled)));
            }

            query = ApplySorting(query, filter.SortBy, filter.SortDirection);

            var totalCount = await query.CountAsync();
            var skip = (filter.Page - 1) * filter.PageSize;
            var units = await query.Skip(skip).Take(filter.PageSize).AsNoTracking().ToListAsync();

            var defaultCurrencyCode = await GetDefaultCurrencyCodeAsync();
            var responses = units.Select(u => MapToPublicResponse(u, defaultCurrencyCode)).ToList();
            return Result.Success(
                CreatePaginatedResponse(responses, totalCount, filter.Page, filter.PageSize));
        }
        catch
        {
            return Result.Failure<PaginatedResponse<PublicUnitResponse>>(
                new Error("GetFailed", "Failed to retrieve units", 500));
        }
    }

    public async Task<Result<PublicUnitDetailsResponses>> GetUnitDetailsAsync(int unitId)
    {
        try
        {
            var unit = await _context.Units
                .Include(c => c.Currency)
                .Include(u => u.CustomPolicies.Where(p => p.IsActive))
                .Include(c => c.Rooms)
                .Include(u => u.City)
                .Include(u => u.UnitType)
                .Include(u => u.Images.Where(i => !i.IsDeleted))
                .Include(u => u.UnitAmenities)
                    .ThenInclude(ua => ua.Amenity)
                // ── NEW: unit-level option values ──────────────────────────
                .Include(u => u.OptionValues)
                    .ThenInclude(ov => ov.UnitTypeOption)
                // ──────────────────────────────────────────────────────────
                .Include(u => u.Rooms.Where(r => !r.IsDeleted && r.IsAvailable))
                    .ThenInclude(r => r.SubUnitImages.Where(i => !i.IsDeleted))
                // ── NEW: subunit-level option values ───────────────────────
                .Include(u => u.Rooms.Where(r => !r.IsDeleted && r.IsAvailable))
                    .ThenInclude(r => r.OptionValues)
                        .ThenInclude(ov => ov.SubUnitTypeOption)
                // ──────────────────────────────────────────────────────────
                .Include(u => u.Reviews
                    .Where(r => r.IsVisible)
                    .OrderByDescending(r => r.CreatedAt).Take(10))
                    .ThenInclude(r => r.Images)
                .AsNoTracking()
                .FirstOrDefaultAsync(u =>
                    u.Id == unitId && !u.IsDeleted && u.IsActive && u.IsVerified);

            if (unit == null)
                return Result.Failure<PublicUnitDetailsResponses>(
                    new Error("NotFound", "Unit not found or not available", 404));


            var policies = await _context.GeneralPolicies
                .Where(p => p.UnitId == unitId && p.IsActive)
                .AsNoTracking()
                .ToListAsync();
            var defaultCurrencyCode = await GetDefaultCurrencyCodeAsync();


            var subUnits = unit.Rooms.Where(r => !r.IsDeleted && r.IsAvailable).ToList();

            var response = new PublicUnitDetailsResponses
            {
                Id = unit.Id,
                Name = unit.Name,
                IsStandaloneUnit = !subUnits.Any(),
                Description = unit.Description,
                Address = unit.Address,
                Latitude = unit.Latitude,
                Longitude = unit.Longitude,
                City = new PublicCityInfo(
                    unit.City!.Id, unit.City.Name, unit.City.Country,
                    unit.City.Description, unit.City.Latitude, unit.City.Longitude),
                UnitTypeName = unit.UnitType?.Name ?? "",
                BasePrice = unit.BasePrice,
                MaxGuests = unit.MaxGuests,
                Bedrooms = unit.Bedrooms,
                Bathrooms = unit.Bathrooms,
                AverageRating = unit.AverageRating,
                TotalReviews = unit.TotalReviews,
                Images = unit.Images?.Where(i => !i.IsDeleted)
                    .OrderBy(i => i.DisplayOrder)
                    .Select(i => new PublicImageInfo
                    {
                        ImageUrl = i.ImageUrl,
                        ThumbnailUrl = i.ThumbnailUrl,
                        MediumUrl = i.MediumUrl,
                        IsPrimary = i.IsPrimary,
                        DisplayOrder = i.DisplayOrder,
                        Caption = i.Caption
                    }).ToList() ?? new(),
                Amenities = unit.UnitAmenities?.Where(ua => ua.IsAvailable)
                    .Select(ua => new PublicAmenityInfo(
                        ua.Amenity.Name.ToString(),
                        ua.Amenity.Description,
                        ua.Amenity.Category.ToString(), ua.Amenity.Icon)).ToList() ?? new(),
                SubUnits = subUnits.Select(r => new PublicSubUnitSummary(
                    r.Id, r.RoomNumber, r.SubUnitTypeId, r.PricePerNight, r.MaxOccupancy,
                    r.IsAvailable,
                    r.SubUnitImages.FirstOrDefault(i => i.IsPrimary)?.ImageUrl,
                    // ── NEW ────────────────────────────────────────────────
                    MapSubUnitOptionValues(r.OptionValues)
                // ──────────────────────────────────────────────────────
                )).ToList(),
                RecentReviews = unit.Reviews?
                    .OrderByDescending(r => r.CreatedAt).Take(10)
                    .Select(r => new PublicReviewSummary
                    {
                        Rating = r.Rating,
                        Comment = r.Comment,
                        CreatedAt = r.CreatedAt,
                        ReviewerName = "Guest",
                        ImageUrls = r.Images.Where(i => !i.IsDeleted)
                            .Select(i => i.ImageUrl).ToList()
                    }).ToList() ?? new(),
                Policies = policies.Select(p => new PublicPolicyInfo(
                    p.Title, p.Description, p.PolicyType.ToString(), p.IsMandatory)).ToList(),
                Currency = unit.Currency?.Code ?? defaultCurrencyCode,
                CustomPolicies = unit.CustomPolicies
                    .OrderBy(p => p.DisplayOrder)
                    .Select(p => new PublicCustomPolicyInfo
                    {
                        Title = p.Title,
                        Description = p.Description,
                        Category = p.Category
                    }).ToList(),
                // ── NEW ──────────────────────────────────────────────────────
                OptionValues = MapUnitOptionValues(unit.OptionValues),
                IsStandAlone = unit.UnitType?.IsStandalone
            };

            return Result.Success(response);
        }
        catch
        {
            return Result.Failure<PublicUnitDetailsResponses>(
                new Error("GetFailed", "Failed to retrieve unit details", 500));
        }
    }
    private async Task<string?> GetDefaultCurrencyCodeAsync()
    {
        return await _context.Currencies
            .AsNoTracking()
            .Where(c => c.IsDefault && c.IsActive)
            .Select(c => c.Code)
            .FirstOrDefaultAsync();
    }
    public async Task<Result<IEnumerable<PublicUnitResponse>>> SearchUnitsAsync(
        PublicSearchRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Keyword))
                return Result.Success<IEnumerable<PublicUnitResponse>>(new List<PublicUnitResponse>());

            var keyword = request.Keyword.ToLower();

            var query = _context.Units
                .Include(c => c.Currency)
                .Include(u => u.CustomPolicies.Where(p => p.IsActive))
                .Include(u => u.City)
                .Include(u => u.UnitType)
                .Include(u => u.Images.Where(i => !i.IsDeleted))
                // ── NEW: unit-level option values ──────────────────────────
                .Include(u => u.OptionValues)
                    .ThenInclude(ov => ov.UnitTypeOption)
                // ──────────────────────────────────────────────────────────
                .Where(u => !u.IsDeleted && u.IsActive && u.IsVerified &&
                            (u.Name.ToLower().Contains(keyword) ||
                             u.Description.ToLower().Contains(keyword) ||
                             u.Address.ToLower().Contains(keyword) ||
                             u.City.Name.ToLower().Contains(keyword)))
                .AsQueryable();

            if (request.CityId.HasValue)
                query = query.Where(u => u.CityId == request.CityId.Value);

            if (request.CheckIn.HasValue && request.CheckOut.HasValue)
            {
                var checkIn = request.CheckIn.Value;
                var checkOut = request.CheckOut.Value;
                query = query.Where(u => u.Rooms.Any(r =>
                    !r.IsDeleted && r.IsAvailable &&
                    !r.BookingRooms.Any(br =>
                        br.Booking.CheckInDate < checkOut &&
                        br.Booking.CheckOutDate > checkIn &&
                        br.Booking.Status != BookingStatus.Cancelled)));
            }

            var skip = (request.Page - 1) * request.PageSize;
            var units = await query.Skip(skip).Take(request.PageSize).AsNoTracking().ToListAsync();
            var defaultCurrencyCode = await GetDefaultCurrencyCodeAsync();

            return Result.Success<IEnumerable<PublicUnitResponse>>(
                units.Select(u => MapToPublicResponse(u, defaultCurrencyCode)).ToList());
        }
        catch
        {
            return Result.Failure<IEnumerable<PublicUnitResponse>>(
                new Error("SearchFailed", "Failed to search units", 500));
        }
    }

    public async Task<Result<List<PublicUnitResponse>>> GetFeaturedUnitsAsync()
    {
        try
        {
            var topRated = await _context.Units
                .Include(c => c.Currency)
                .Include(u => u.CustomPolicies.Where(p => p.IsActive))
                .Include(u => u.City)
                .Include(u => u.UnitType)
                .Include(u => u.Images.Where(i => !i.IsDeleted))
                // ── NEW: unit-level option values ──────────────────────────
                .Include(u => u.OptionValues)
                    .ThenInclude(ov => ov.UnitTypeOption)
                // ──────────────────────────────────────────────────────────
                .Where(u => !u.IsDeleted && u.IsActive && u.IsVerified && u.IsFeatured)
                .OrderByDescending(u => u.AverageRating)
                .ThenByDescending(u => u.TotalReviews)
                .AsNoTracking()
                .ToListAsync();

            var defaultCurrencyCode = await GetDefaultCurrencyCodeAsync();
            return Result.Success(topRated.Select(u => MapToPublicResponse(u, defaultCurrencyCode)).ToList());
        }
        catch
        {
            return Result.Failure<List<PublicUnitResponse>>(
                new Error("GetFailed", "Failed to retrieve featured units", 500));
        }
    }

    #endregion

    #region SUBUNITS

    public async Task<Result<PublicSubUnitDetailsResponse>> GetSubUnitDetailsAsync(int subUnitId)
    {
        try
        {
            var subUnit = await _context.SubUnits
                .Include(s => s.Unit)
                .ThenInclude(u => u.Currency)
                .Include(s => s.SubUnitImages.Where(i => !i.IsDeleted))
                .Include(s => s.SubUnitAmenities)
                    .ThenInclude(sa => sa.Amenity)
                // ── NEW: subunit-level option values ───────────────────────
                .Include(s => s.OptionValues)
                    .ThenInclude(ov => ov.SubUnitTypeOption)
                // ──────────────────────────────────────────────────────────
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == subUnitId && !s.IsDeleted && s.IsAvailable);

            if (subUnit == null ||
                !subUnit.Unit.IsActive || !subUnit.Unit.IsVerified || subUnit.Unit.IsDeleted)
                return Result.Failure<PublicSubUnitDetailsResponse>(
                    new Error("NotFound", "SubUnit not found or not available", 404));

            var defaultCurrencyCode = await GetDefaultCurrencyCodeAsync();
            return Result.Success(MapToPublicSubUnitDetails(subUnit, defaultCurrencyCode));
        }
        catch
        {
            return Result.Failure<PublicSubUnitDetailsResponse>(
                new Error("GetFailed", "Failed to retrieve subunit details", 500));
        }
    }

    public async Task<Result<IEnumerable<PublicSubUnitSummary>>> GetAvailableSubUnitsAsync(
        int unitId, DateTime checkIn, DateTime checkOut)
    {
        try
        {
            var unit = await _context.Units
                .FirstOrDefaultAsync(u => u.Id == unitId &&
                    !u.IsDeleted && u.IsActive && u.IsVerified);

            if (unit == null)
                return Result.Failure<IEnumerable<PublicSubUnitSummary>>(
                    new Error("NotFound", "Unit not found", 404));

            var subUnits = await _context.SubUnits
                .Include(s => s.SubUnitImages.Where(i => !i.IsDeleted && i.IsPrimary))
                // ── NEW: subunit-level option values ───────────────────────
                .Include(s => s.OptionValues)
                    .ThenInclude(ov => ov.SubUnitTypeOption)
                // ──────────────────────────────────────────────────────────
                .Where(s => s.UnitId == unitId && !s.IsDeleted && s.IsAvailable)
                .AsNoTracking()
                .ToListAsync();

            var bookedRoomIds = await _context.BookingRooms
                .Include(br => br.Booking)
                .Where(br => br.Room.UnitId == unitId &&
                             br.Booking.CheckInDate < checkOut &&
                             br.Booking.CheckOutDate > checkIn &&
                             br.Booking.Status != BookingStatus.Cancelled)
                .Select(br => br.RoomId)
                .ToListAsync();

            var available = subUnits
                .Where(s => !bookedRoomIds.Contains(s.Id))
                .Select(s => new PublicSubUnitSummary(
                    s.Id, s.RoomNumber, s.SubUnitTypeId, s.PricePerNight, s.MaxOccupancy,
                    s.IsAvailable,
                    s.SubUnitImages.FirstOrDefault(i => i.IsPrimary)?.ImageUrl,
                    // ── NEW ────────────────────────────────────────────────
                    MapSubUnitOptionValues(s.OptionValues)
                // ──────────────────────────────────────────────────────
                ))
                .ToList();

            return Result.Success<IEnumerable<PublicSubUnitSummary>>(available);
        }
        catch
        {
            return Result.Failure<IEnumerable<PublicSubUnitSummary>>(
                new Error("GetFailed", "Failed to retrieve available rooms", 500));
        }
    }

    #endregion

    #region CITIES

    public async Task<Result<IEnumerable<PublicCityResponse>>> GetAllCitiesAsync()
    {
        try
        {
            var cities = await _context.Departments
                .OrderBy(d => d.Name)
                .AsNoTracking()
                .ToListAsync();

            return Result.Success<IEnumerable<PublicCityResponse>>(
                cities.Select(c => new PublicCityResponse
                {
                    Id = c.Id,
                    Name = c.Name,
                    Country = c.Country,
                    Description = c.Description,
                    ImageUrl = c.ImageUrl,
                    Latitude = c.Latitude,
                    Longitude = c.Longitude,
                    TotalUnits = c.TotalUnits,
                    AverageRating = c.AverageRating
                }).ToList());
        }
        catch
        {
            return Result.Failure<IEnumerable<PublicCityResponse>>(
                new Error("GetFailed", "Failed to retrieve cities", 500));
        }
    }

    public async Task<Result<PublicCityResponse>> GetCityDetailsAsync(int cityId)
    {
        try
        {
            var city = await _context.Departments
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.Id == cityId && !d.IsDeleted && d.IsActive);

            if (city == null)
                return Result.Failure<PublicCityResponse>(
                    new Error("NotFound", "City not found", 404));

            return Result.Success(new PublicCityResponse
            {
                Id = city.Id,
                Name = city.Name,
                Country = city.Country,
                Description = city.Description,
                ImageUrl = city.ImageUrl,
                Latitude = city.Latitude,
                Longitude = city.Longitude,
                TotalUnits = city.TotalUnits,
                AverageRating = city.AverageRating
            });
        }
        catch
        {
            return Result.Failure<PublicCityResponse>(
                new Error("GetFailed", "Failed to retrieve city details", 500));
        }
    }

    public async Task<Result<PaginatedResponse<PublicUnitResponse>>> GetUnitsByCityAsync(
        int cityId, PublicUnitFilter? filter = null)
    {
        filter ??= new PublicUnitFilter();
        filter = filter with { CityId = cityId };
        return await GetAllUnitsAsync(filter);
    }

    #endregion

    #region AVAILABILITY

    public async Task<Result<AvailabilityCheckResponse>> CheckAvailabilityAsync(
        CheckAvailabilityRequest request)
    {
        try
        {
            var unit = await _context.Units
                .FirstOrDefaultAsync(u => u.Id == request.UnitId &&
                    !u.IsDeleted && u.IsActive);

            if (unit == null)
                return Result.Failure<AvailabilityCheckResponse>(
                    new Error("NotFound", "Unit not found", 404));

            var availableResult = await GetAvailableSubUnitsAsync(
                request.UnitId, request.CheckIn, request.CheckOut);

            if (!availableResult.IsSuccess)
                return Result.Failure<AvailabilityCheckResponse>(availableResult.Error);

            var availableSubUnits = availableResult.Value.ToList();
            var nights = (request.CheckOut - request.CheckIn).Days;
            var estimatedPrice = availableSubUnits
                .Take(request.NumberOfGuests)
                .Sum(s => s.PricePerNight * nights);

            return Result.Success(new AvailabilityCheckResponse
            {
                IsAvailable = availableSubUnits.Any(),
                AvailableRooms = availableSubUnits.Count,
                EstimatedPrice = estimatedPrice,
                AvailableSubUnits = availableSubUnits
            });
        }
        catch
        {
            return Result.Failure<AvailabilityCheckResponse>(
                new Error("CheckFailed", "Failed to check availability", 500));
        }
    }

    #endregion

    #region NEARBY UNITS

    public async Task<Result<IEnumerable<PublicUnitResponse>>> GetNearbyUnitsAsync(
        decimal latitude, decimal longitude, int radiusKm = 50)
    {
        try
        {
            var units = await _context.Units
                .Include(c=>c.Currency)
                .Include(u => u.CustomPolicies.Where(p => p.IsActive))
                .Include(u => u.City)
                .Include(u => u.UnitType)
                .Include(u => u.Images.Where(i => !i.IsDeleted && i.IsPrimary))
                // ── NEW: unit-level option values ──────────────────────────
                .Include(u => u.OptionValues)
                    .ThenInclude(ov => ov.UnitTypeOption)
                // ──────────────────────────────────────────────────────────
                .Where(u => !u.IsDeleted && u.IsActive && u.IsVerified)
                .AsNoTracking()
                .ToListAsync();

            var defaultCurrencyCode = await GetDefaultCurrencyCodeAsync();
            var nearby = units
                .Where(u => CalculateDistance(latitude, longitude, u.Latitude, u.Longitude) <= radiusKm)
                .OrderBy(u => CalculateDistance(latitude, longitude, u.Latitude, u.Longitude))
                .Take(20)
                .Select(u => MapToPublicResponse(u, defaultCurrencyCode))
                .ToList();

            return Result.Success<IEnumerable<PublicUnitResponse>>(nearby);
        }
        catch
        {
            return Result.Failure<IEnumerable<PublicUnitResponse>>(
                new Error("GetFailed", "Failed to retrieve nearby units", 500));
        }
    }

    #endregion

    #region OTHER PUBLIC ENDPOINTS

    public async Task<Result<IEnumerable<PublicOfferResponse>>> GetActiveOffersAsync()
    {
        try
        {
            var now = DateTime.UtcNow;
            var offers = await _context.Set<Offer>()
                .Include(o => o.Unit)
                .Where(o => !o.IsDeleted && o.IsActive &&
                            o.StartDate <= now && o.EndDate >= now)
                .OrderByDescending(o => o.CreatedAt)
                .AsNoTracking()
                .ToListAsync();

            return Result.Success<IEnumerable<PublicOfferResponse>>(
                offers.Select(MapToPublicOfferResponse).ToList());
        }
        catch
        {
            return Result.Failure<IEnumerable<PublicOfferResponse>>(
                new Error("GetFailed", "Failed to retrieve active offers", 500));
        }
    }

    public async Task<Result<IEnumerable<PublicAdResponse>>> GetActiveAdsAsync()
    {
        try
        {
            var now = DateTime.UtcNow;
            var ads = await _context.Set<Ad>()
                .Include(a => a.Unit)
                .Where(a => !a.IsDeleted && a.IsActive &&
                            a.StartDate <= now && a.EndDate >= now)
                .OrderByDescending(a => a.CreatedAt)
                .AsNoTracking()
                .ToListAsync();

            return Result.Success<IEnumerable<PublicAdResponse>>(
                ads.Select(MapToPublicAdResponse).ToList());
        }
        catch
        {
            return Result.Failure<IEnumerable<PublicAdResponse>>(
                new Error("GetFailed", "Failed to retrieve active ads", 500));
        }
    }

    public async Task<Result<IEnumerable<PaymentMethodDto>>> GetPaymentMethodesAsync()
    {
        var methods = await _context.PaymentMethods.ToListAsync();
        return Result.Success<IEnumerable<PaymentMethodDto>>(
            methods.Select(p => new PaymentMethodDto
            {
                Id = p.Id,
                TitleA = p.TitleA,
                TitleE = p.TitleE,
                DescriptionA = p.DescriptionA,
                DescriptionE = p.DescriptionE
            }));
    }

    public async Task<Result<IEnumerable<UnitTypeResponse>>> GetUnitTypesAsync()
    {
        var unitTypes = await _context.UnitTypes.AsNoTracking().ToListAsync();
        var responses = new List<UnitTypeResponse>();

        foreach (var unitType in unitTypes)
        {
            var totalUnits = await _context.Units
                .CountAsync(u => u.UnitTypeId == unitType.Id && !u.IsDeleted);
            responses.Add(MapToUnitTypeResponse(unitType, totalUnits));
        }

        return Result.Success<IEnumerable<UnitTypeResponse>>(responses);
    }

    public async Task<Result<IEnumerable<AmenityResponse>>> GetAminitiesAsync()
    {
        var amenities = await _context.Set<Domain.Entities.Amenity>()
            .AsNoTracking().ToListAsync();

        return Result.Success<IEnumerable<AmenityResponse>>(
            amenities.Select(a => new AmenityResponse(
                a.Id, a.Name, a.Description, a.Category,a.Icon)).ToList());
    }

    #endregion

    #region HELPER METHODS

    // ── NEW: option value mappers ──────────────────────────────────────────────

    /// <summary>Groups UnitOptionValue rows into one OptionValueResponse per option.</summary>
    private static List<OptionValueResponse> MapUnitOptionValues(
        IEnumerable<UnitOptionValue>? optionValues)
    {
        if (optionValues == null) return [];

        return optionValues
            .GroupBy(ov => new
            {
                ov.UnitTypeOptionId,
                ov.UnitTypeOption?.Name,
                InputType = ov.UnitTypeOption?.InputType.ToString() ?? string.Empty
            })
            .Select(g => new OptionValueResponse
            {
                OptionId = g.Key.UnitTypeOptionId,
                OptionName = g.Key.Name ?? string.Empty,
                InputType = g.Key.InputType,
                Values = g.Select(ov => ov.Value).ToList()
            })
            .ToList();
    }

    /// <summary>Groups SubUnitOptionValue rows into one OptionValueResponse per option.</summary>
    private static List<OptionValueResponse> MapSubUnitOptionValues(
        IEnumerable<SubUnitOptionValue>? optionValues)
    {
        if (optionValues == null) return [];

        return optionValues
            .GroupBy(ov => new
            {
                ov.SubUnitTypeOptionId,
                ov.SubUnitTypeOption?.Name,
                InputType = ov.SubUnitTypeOption?.InputType.ToString() ?? string.Empty
            })
            .Select(g => new OptionValueResponse
            {
                OptionId = g.Key.SubUnitTypeOptionId,
                OptionName = g.Key.Name ?? string.Empty,
                InputType = g.Key.InputType,
                Values = g.Select(ov => ov.Value).ToList()
            })
            .ToList();
    }

    // ─────────────────────────────────────────────────────────────────────────

    private PublicUnitResponse MapToPublicResponse(Domain.Entities.Unit unit, string? defaultCurrencyCode = null)
    {

        return new PublicUnitResponse
        {
            Id = unit.Id,
            Name = unit.Name,
            Description = unit.Description,
            Address = unit.Address,
            Latitude = unit.Latitude,
            Longitude = unit.Longitude,
            CityName = unit.City?.Name ?? "",
            Country = unit.City?.Country ?? "",
            UnitTypeName = unit.UnitType?.Name ?? "",
            BasePrice = unit.BasePrice,
            MaxGuests = unit.MaxGuests,
            Bedrooms = unit.Bedrooms,
            Bathrooms = unit.Bathrooms,
            AverageRating = unit.AverageRating,
            TotalReviews = unit.TotalReviews,
            PrimaryImageUrl = unit.Images?.FirstOrDefault()?.ImageUrl,
            IsAvailable = unit.IsActive && unit.IsVerified,
            IsFeatured = unit.AverageRating >= 4.5m && unit.TotalReviews >= 10,
            Currency = unit.Currency?.Code ?? defaultCurrencyCode,
            CustomPolicies = unit.CustomPolicies
                .OrderBy(p => p.DisplayOrder)
                .Select(p => new PublicCustomPolicyInfo
                {
                    Title = p.Title,
                    Description = p.Description,
                    Category = p.Category
                }).ToList(),
            // ── NEW ──────────────────────────────────────────────────────
            OptionValues = MapUnitOptionValues(unit.OptionValues),
            IsStandAlone = unit.UnitType?.IsStandalone
        };
    }

    private static PublicSubUnitDetailsResponse MapToPublicSubUnitDetails(
        Domain.Entities.SubUnit subUnit, string? defaultCurrencyCode = null)
    {
        return new PublicSubUnitDetailsResponse
        {
            Id = subUnit.Id,
            UnitId = subUnit.UnitId,
            UnitName = subUnit.Unit?.Name ?? "",
            RoomNumber = subUnit.RoomNumber,
            TypeId = subUnit.SubUnitTypeId,
            PricePerNight = subUnit.PricePerNight,
            MaxOccupancy = subUnit.MaxOccupancy,
            Bedrooms = subUnit.Bedrooms,
            Bathrooms = subUnit.Bathrooms,
            Size = subUnit.Size,
            Description = subUnit.Description,
            IsAvailable = subUnit.IsAvailable,
            Images = subUnit.SubUnitImages?.Where(i => !i.IsDeleted)
                .OrderBy(i => i.DisplayOrder)
                .Select(i => new PublicImageInfo
                {
                    ImageUrl = i.ImageUrl,
                    ThumbnailUrl = i.ThumbnailUrl,
                    MediumUrl = i.MediumUrl,
                    IsPrimary = i.IsPrimary,
                    DisplayOrder = i.DisplayOrder,
                    Caption = i.Caption
                }).ToList() ?? new(),
            Amenities = subUnit.SubUnitAmenities?.Where(sa => sa.IsAvailable)
                .Select(sa => new PublicAmenityInfo(
                    sa.Amenity.Name.ToString(),
                    sa.Amenity.Description,
                    sa.Amenity.Category.ToString(),
                    sa.Amenity.Icon)).ToList() ?? new(),
            // ── NEW ──────────────────────────────────────────────────────
            OptionValues = MapSubUnitOptionValues(subUnit.OptionValues),

            Currency = subUnit.Unit?.Currency?.Code ?? defaultCurrencyCode
        };
    }

    private static PublicAdResponse MapToPublicAdResponse(Ad ad) =>
        new()
        {
            Id = ad.Id,
            Title = ad.Title,
            Description = ad.Description,
            ImageUrl = ad.ImageUrl,
            UnitId = ad.UnitId,
            UnitName = ad.Unit?.Name,
            StartDate = ad.StartDate,
            EndDate = ad.EndDate,
            CreatedAt = ad.CreatedAt
        };

    private static PublicOfferResponse MapToPublicOfferResponse(Offer offer) =>
        new()
        {
            Id = offer.Id,
            Title = offer.Title,
            Description = offer.Description,
            ImageUrl = offer.ImageUrl,
            IsFeatured = offer.IsFeatured,
            UnitId = offer.UnitId,
            UnitName = offer.Unit?.Name,
            StartDate = offer.StartDate,
            EndDate = offer.EndDate,
            DiscountPercentage = offer.DiscountPercentage,
            DiscountAmount = offer.DiscountAmount,
            CreatedAt = offer.CreatedAt
        };

    private static UnitTypeResponse MapToUnitTypeResponse(Domain.Entities.UnitType unitType,
        int totalUnits) =>
        new(unitType.Id, unitType.Name, unitType.Description,
            unitType.IsActive, totalUnits, unitType.IsStandalone);

    private static IQueryable<Domain.Entities.Unit> ApplySorting(
        IQueryable<Domain.Entities.Unit> query, string? sortBy, string? sortDirection)
    {
        var descending = sortDirection?.ToUpper() == "DESC";
        return sortBy switch
        {
            "Name" => descending ? query.OrderByDescending(u => u.Name) : query.OrderBy(u => u.Name),
            "Price" => descending ? query.OrderByDescending(u => u.BasePrice) : query.OrderBy(u => u.BasePrice),
            "Rating" => descending ? query.OrderByDescending(u => u.AverageRating) : query.OrderBy(u => u.AverageRating),
            _ => query.OrderBy(u => u.Name)
        };
    }

    private static double CalculateDistance(decimal lat1, decimal lon1, decimal lat2, decimal lon2)
    {
        const double R = 6371;
        var dLat = ToRadians((double)(lat2 - lat1));
        var dLon = ToRadians((double)(lon2 - lon1));
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRadians((double)lat1)) * Math.Cos(ToRadians((double)lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        return R * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
    }

    private static double ToRadians(double degrees) => degrees * Math.PI / 180;

    private PaginatedResponse<T> CreatePaginatedResponse<T>(
        IEnumerable<T> items, int totalCount, int page, int pageSize)
    {
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        return new PaginatedResponse<T>
        {
            Items = items,
            TotalCount = totalCount,
            TotalPages = totalPages,
            CurrentPage = page,
            NextPage = page < totalPages ? page + 1 : null,
            PrevPage = page > 1 ? page - 1 : null
        };
    }

    public class PaginatedResponse<T>
    {
        public IEnumerable<T> Items { get; set; } = [];
        public int TotalPages { get; set; }
        public int CurrentPage { get; set; }
        public int? NextPage { get; set; }
        public int? PrevPage { get; set; }
        public int TotalCount { get; set; }
    }

    #endregion
}