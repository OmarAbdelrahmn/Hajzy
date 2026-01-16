// Application/Service/Public/PublicService.cs
using Application.Abstraction;
using Application.Contracts.publicuser;
using Application.Service.publicuser;
using Domain;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Application.Service.publicuser;

public class PublicService(
    ApplicationDbcontext context) : IPublicServise
{
    private readonly ApplicationDbcontext _context = context;
    #region UNITS

    public async Task<Result<IEnumerable<PublicUnitResponse>>> GetAllUnitsAsync(PublicUnitFilter filter)
    {
        try
        {
            var query = _context.Units
                .Include(u => u.City)
                .Include(u => u.UnitType)
                .Include(u => u.Images.Where(i => !i.IsDeleted))
                .Where(u => !u.IsDeleted && u.IsActive && u.IsVerified)
                .AsQueryable();

            // Apply filters
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

            // Amenity filter
            if (filter.Amenities?.Any() == true)
            {
                foreach (var amenity in filter.Amenities)
                {
                    query = query.Where(u => u.UnitAmenities
                        .Any(ua => ua.Amenity.Name.ToString().Contains(amenity) && ua.IsAvailable));
                }
            }

            // Availability filter
            if (filter.CheckIn.HasValue && filter.CheckOut.HasValue)
            {
                var checkIn = filter.CheckIn.Value;
                var checkOut = filter.CheckOut.Value;

                query = query.Where(u => u.Rooms.Any(r =>
                    !r.IsDeleted &&
                    r.IsAvailable &&
                    !r.BookingRooms.Any(br =>
                        br.Booking.CheckInDate < checkOut &&
                        br.Booking.CheckOutDate > checkIn &&
                        br.Booking.Status != BookingStatus.Cancelled)));
            }

            // Sorting
            query = ApplySorting(query, filter.SortBy, filter.SortDirection);

            // Pagination
            var skip = (filter.Page - 1) * filter.PageSize;
            var units = await query
                .Skip(skip)
                .Take(filter.PageSize)
                .AsNoTracking()
                .ToListAsync();

            var responses = units.Select(MapToPublicResponse).ToList();

            return Result.Success<IEnumerable<PublicUnitResponse>>(responses);
        }
        catch (Exception ex)
        {
            return Result.Failure<IEnumerable<PublicUnitResponse>>(
                new Error("GetFailed", "Failed to retrieve units", 500));
        }
    }

    public async Task<Result<PublicUnitDetailsResponse>> GetUnitDetailsAsync(int unitId)
    {
        try
        {

            var unit = await _context.Units
                .Include(u => u.City)
                .Include(u => u.UnitType)
                .Include(u => u.Images.Where(i => !i.IsDeleted))
                .Include(u => u.UnitAmenities)
                    .ThenInclude(ua => ua.Amenity)
                .Include(u => u.Rooms.Where(r => !r.IsDeleted && r.IsAvailable))
                    .ThenInclude(r => r.SubUnitImages.Where(i => !i.IsDeleted))
                .Include(u => u.CancellationPolicy)
                .Include(u => u.Reviews.OrderByDescending(r => r.CreatedAt).Take(10))
                    .ThenInclude(r => r.Images)
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == unitId && !u.IsDeleted && u.IsActive && u.IsVerified);

            if (unit == null)
                return Result.Failure<PublicUnitDetailsResponse>(
                    new Error("NotFound", "Unit not found or not available", 404));

            // Get general policies
            var policies = await _context.GeneralPolicies
                .Where(p => p.UnitId == unitId && p.IsActive)
                .AsNoTracking()
                .ToListAsync();

            var response = MapToPublicDetailsResponse(unit, policies);

            // Cache for 5 minutes

            return Result.Success(response);
        }
        catch (Exception ex)
        {
            return Result.Failure<PublicUnitDetailsResponse>(
                new Error("GetFailed", "Failed to retrieve unit details", 500));
        }
    }

    public async Task<Result<IEnumerable<PublicUnitResponse>>> SearchUnitsAsync(PublicSearchRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Keyword))
                return Result.Success<IEnumerable<PublicUnitResponse>>(new List<PublicUnitResponse>());

            var keyword = request.Keyword.ToLower();

            var query = _context.Units
                .Include(u => u.City)
                .Include(u => u.UnitType)
                .Include(u => u.Images.Where(i => !i.IsDeleted))
                .Where(u => !u.IsDeleted && u.IsActive && u.IsVerified &&
                           (u.Name.ToLower().Contains(keyword) ||
                            u.Description.ToLower().Contains(keyword) ||
                            u.Address.ToLower().Contains(keyword) ||
                            u.City.Name.ToLower().Contains(keyword)))
                .AsQueryable();

            if (request.CityId.HasValue)
                query = query.Where(u => u.CityId == request.CityId.Value);

            // Availability filter
            if (request.CheckIn.HasValue && request.CheckOut.HasValue)
            {
                var checkIn = request.CheckIn.Value;
                var checkOut = request.CheckOut.Value;

                query = query.Where(u => u.Rooms.Any(r =>
                    !r.IsDeleted &&
                    r.IsAvailable &&
                    !r.BookingRooms.Any(br =>
                        br.Booking.CheckInDate < checkOut &&
                        br.Booking.CheckOutDate > checkIn &&
                        br.Booking.Status != BookingStatus.Cancelled)));
            }

            var skip = (request.Page - 1) * request.PageSize;
            var units = await query
                .Skip(skip)
                .Take(request.PageSize)
                .AsNoTracking()
                .ToListAsync();

            var responses = units.Select(MapToPublicResponse).ToList();

            return Result.Success<IEnumerable<PublicUnitResponse>>(responses);
        }
        catch (Exception ex)
        {
            return Result.Failure<IEnumerable<PublicUnitResponse>>(
                new Error("SearchFailed", "Failed to search units", 500));
        }
    }

    public async Task<Result<List<PublicUnitResponse>>> GetFeaturedUnitsAsync()
    {
        try
        {
            // Get top rated units
            var topRated = await _context.Units
                //.Where(u => u.IsFeatured)
                .Include(u => u.City)
                .Include(u => u.UnitType)
                .Include(u => u.Images.Where(i => !i.IsDeleted))
                .Where(u => !u.IsDeleted && u.IsActive && u.IsVerified && u.IsFeatured)
                .OrderByDescending(u => u.AverageRating)
                .ThenByDescending(u => u.TotalReviews)
                .AsNoTracking()
                .ToListAsync();

            var response = new FeaturedUnitsResponse
            {
                Units = topRated.Select(MapToPublicResponse).ToList(),
                Criteria = "Top Rated"
            };

            // Cache for 30 minutes

            return Result.Success(response.Units);
        }
        catch (Exception ex)
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
                .Include(s => s.SubUnitImages.Where(i => !i.IsDeleted))
                .Include(s => s.SubUnitAmenities)
                    .ThenInclude(sa => sa.Amenity)
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == subUnitId && !s.IsDeleted && s.IsAvailable);

            if (subUnit == null || !subUnit.Unit.IsActive || !subUnit.Unit.IsVerified || subUnit.Unit.IsDeleted)
                return Result.Failure<PublicSubUnitDetailsResponse>(
                    new Error("NotFound", "SubUnit not found or not available", 404));

            var response = MapToPublicSubUnitDetails(subUnit);

            return Result.Success(response);
        }
        catch (Exception ex)
        {
            return Result.Failure<PublicSubUnitDetailsResponse>(
                new Error("GetFailed", "Failed to retrieve subunit details", 500));
        }
    }

    public async Task<Result<IEnumerable<PublicSubUnitSummary>>> GetAvailableSubUnitsAsync(
        int unitId,
        DateTime checkIn,
        DateTime checkOut)
    {
        try
        {
            var unit = await _context.Units
                .FirstOrDefaultAsync(u => u.Id == unitId && !u.IsDeleted && u.IsActive && u.IsVerified);

            if (unit == null)
                return Result.Failure<IEnumerable<PublicSubUnitSummary>>(
                    new Error("NotFound", "Unit not found", 404));

            var subUnits = await _context.SubUnits
                .Include(s => s.SubUnitImages.Where(i => !i.IsDeleted && i.IsPrimary))
                .Where(s => s.UnitId == unitId && !s.IsDeleted && s.IsAvailable)
                .AsNoTracking()
                .ToListAsync();

            // Filter out booked rooms
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
                    s.Id,
                    s.RoomNumber,
                    s.Type.ToString(),
                    s.PricePerNight,
                    s.MaxOccupancy,
                    s.IsAvailable,
                    s.SubUnitImages.FirstOrDefault(i => i.IsPrimary)?.ImageUrl
                ))
                .ToList();

            return Result.Success<IEnumerable<PublicSubUnitSummary>>(available);
        }
        catch (Exception ex)
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
                //.Where(d => !d.IsDeleted && d.IsActive && d.TotalUnits > 0)
                .OrderBy(d => d.Name)
                .AsNoTracking()
                .ToListAsync();

            var responses = cities.Select(c => new PublicCityResponse
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
            }).ToList();

            return Result.Success<IEnumerable<PublicCityResponse>>(responses);
        }
        catch (Exception ex)
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

            var response = new PublicCityResponse
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
            };

            return Result.Success(response);
        }
        catch (Exception ex)
        {
            return Result.Failure<PublicCityResponse>(
                new Error("GetFailed", "Failed to retrieve city details", 500));
        }
    }

    public async Task<Result<IEnumerable<PublicUnitResponse>>> GetUnitsByCityAsync(
        int cityId,
        PublicUnitFilter? filter = null)
    {
        filter ??= new PublicUnitFilter();
        filter = filter with { CityId = cityId };

        return await GetAllUnitsAsync(filter);
    }

    #endregion

    #region AVAILABILITY

    public async Task<Result<AvailabilityCheckResponse>> CheckAvailabilityAsync(CheckAvailabilityRequest request)
    {
        try
        {
            var unit = await _context.Units
                .FirstOrDefaultAsync(u => u.Id == request.UnitId && !u.IsDeleted && u.IsActive);

            if (unit == null)
                return Result.Failure<AvailabilityCheckResponse>(
                    new Error("NotFound", "Unit not found", 404));

            var availableResult = await GetAvailableSubUnitsAsync(
                request.UnitId,
                request.CheckIn,
                request.CheckOut);

            if (!availableResult.IsSuccess)
                return Result.Failure<AvailabilityCheckResponse>(availableResult.Error);

            var availableSubUnits = availableResult.Value.ToList();
            var isAvailable = availableSubUnits.Any();

            // Calculate estimated price
            var nights = (request.CheckOut - request.CheckIn).Days;
            var estimatedPrice = availableSubUnits
                .Take(request.NumberOfGuests) // Simplistic - take one room per guest group
                .Sum(s => s.PricePerNight * nights);

            var response = new AvailabilityCheckResponse
            {
                IsAvailable = isAvailable,
                AvailableRooms = availableSubUnits.Count,
                EstimatedPrice = estimatedPrice,
                AvailableSubUnits = availableSubUnits
            };

            return Result.Success(response);
        }
        catch (Exception ex)
        {
            return Result.Failure<AvailabilityCheckResponse>(
                new Error("CheckFailed", "Failed to check availability", 500));
        }
    }

    #endregion

    #region FAVORITES

    //public async Task<Result<FavoritesResponse>> GetFavoritesAsync(string sessionId)
    //{
    //    try
    //    {
    //        // For anonymous users, favorites would be stored in localStorage
    //        // This endpoint would be for authenticated users only in a real scenario
    //        // For now, return empty list as this is just a placeholder

    //        return Result.Success(new FavoritesResponse
    //        {
    //            Units = new List<PublicUnitResponse>(),
    //            TotalCount = 0
    //        });
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogError(ex, "Error getting favorites");
    //        return Result.Failure<FavoritesResponse>(
    //            new Error("GetFailed", "Failed to retrieve favorites", 500));
    //    }
    //}

    #endregion

    #region NEARBY UNITS

    public async Task<Result<IEnumerable<PublicUnitResponse>>> GetNearbyUnitsAsync(
        decimal latitude,
        decimal longitude,
        int radiusKm = 50)
    {
        try
        {
            var units = await _context.Units
                .Include(u => u.City)
                .Include(u => u.UnitType)
                .Include(u => u.Images.Where(i => !i.IsDeleted && i.IsPrimary))
                .Where(u => !u.IsDeleted && u.IsActive && u.IsVerified)
                .AsNoTracking()
                .ToListAsync();

            var nearby = units.Where(u =>
            {
                var distance = CalculateDistance(latitude, longitude, u.Latitude, u.Longitude);
                return distance <= radiusKm;
            })
            .OrderBy(u => CalculateDistance(latitude, longitude, u.Latitude, u.Longitude))
            .Take(20)
            .Select(MapToPublicResponse)
            .ToList();

            return Result.Success<IEnumerable<PublicUnitResponse>>(nearby);
        }
        catch (Exception ex)
        {
            return Result.Failure<IEnumerable<PublicUnitResponse>>(
                new Error("GetFailed", "Failed to retrieve nearby units", 500));
        }
    }

    #endregion

    #region HELPER METHODS

    private static PublicUnitResponse MapToPublicResponse(Domain.Entities.Unit unit)
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
            IsFeatured = unit.AverageRating >= 4.5m && unit.TotalReviews >= 10
        };
    }

    private static PublicUnitDetailsResponse MapToPublicDetailsResponse(Domain.Entities.Unit unit, List<GeneralPolicy> policies)
    {
        return new PublicUnitDetailsResponse
        {
            Id = unit.Id,
            Name = unit.Name,
            Description = unit.Description,
            Address = unit.Address,
            Latitude = unit.Latitude,
            Longitude = unit.Longitude,
            City = new PublicCityInfo(
                unit.City!.Id,
                unit.City.Name,
                unit.City.Country,
                unit.City.Description,
                unit.City.Latitude,
                unit.City.Longitude
            ),
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
                    ua.Amenity.Category.ToString()
                )).ToList() ?? new(),
            SubUnits = unit.Rooms?.Where(r => !r.IsDeleted && r.IsAvailable)
                .Select(r => new PublicSubUnitSummary(
                    r.Id,
                    r.RoomNumber,
                    r.Type.ToString(),
                    r.PricePerNight,
                    r.MaxOccupancy,
                    r.IsAvailable,
                    r.SubUnitImages.FirstOrDefault(i => i.IsPrimary)?.ImageUrl
                )).ToList() ?? new(),
            RecentReviews = unit.Reviews?
                .OrderByDescending(r => r.CreatedAt)
                .Take(10)
                .Select(r => new PublicReviewSummary
                {
                    Rating = r.Rating,
                    Comment = r.Comment,
                    CreatedAt = r.CreatedAt,
                    ReviewerName = "Guest", // Never expose real names
                    ImageUrls = r.Images.Where(i => !i.IsDeleted)
                        .Select(i => i.ImageUrl).ToList()
                }).ToList() ?? new(),
            CancellationPolicy = unit.CancellationPolicy != null
                ? new PublicCancellationPolicy(
                    unit.CancellationPolicy.Name,
                    unit.CancellationPolicy.Description,
                    unit.CancellationPolicy.FullRefundDays,
                    unit.CancellationPolicy.PartialRefundDays,
                    unit.CancellationPolicy.PartialRefundPercentage
                )
                : null,
            Policies = policies.Select(p => new PublicPolicyInfo(
                p.Title,
                p.Description,
                p.PolicyType.ToString(),
                p.IsMandatory
            )).ToList()
        };
    }

    private static PublicSubUnitDetailsResponse MapToPublicSubUnitDetails(Domain.Entities.SubUnit subUnit)
    {
        return new PublicSubUnitDetailsResponse
        {
            Id = subUnit.Id,
            UnitId = subUnit.UnitId,
            UnitName = subUnit.Unit?.Name ?? "",
            RoomNumber = subUnit.RoomNumber,
            Type = subUnit.Type.ToString(),
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
                    sa.Amenity.Category.ToString()
                )).ToList() ?? new()
        };
    }

    private static IQueryable<Domain.Entities.Unit> ApplySorting(
        IQueryable<Domain.Entities.Unit> query,
        string? sortBy,
        string? sortDirection)
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
        const double R = 6371; // Earth radius in km
        var dLat = ToRadians((double)(lat2 - lat1));
        var dLon = ToRadians((double)(lon2 - lon1));

        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRadians((double)lat1)) * Math.Cos(ToRadians((double)lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return R * c;
    }

    private static double ToRadians(double degrees) => degrees * Math.PI / 180;

    #endregion


    public async Task<Result<IEnumerable<PublicOfferResponse>>> GetActiveOffersAsync()
    {
        try
        {
            var now = DateTime.UtcNow;

            var offers = await _context.Set<Offer>()
                .Include(o => o.Unit)
                .Where(o => !o.IsDeleted &&
                           o.IsActive &&
                           o.StartDate <= now &&
                           o.EndDate >= now)
                .OrderByDescending(o => o.CreatedAt)
                .AsNoTracking()
                .ToListAsync();

            var responses = offers.Select(MapToPublicOfferResponse).ToList();
            return Result.Success<IEnumerable<PublicOfferResponse>>(responses);
        }
        catch (Exception ex)
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
                .Where(a => !a.IsDeleted &&
                           a.IsActive &&
                           a.StartDate <= now &&
                           a.EndDate >= now)
                .OrderByDescending(a => a.CreatedAt)
                .AsNoTracking()
                .ToListAsync();

            var responses = ads.Select(MapToPublicAdResponse).ToList();
            return Result.Success<IEnumerable<PublicAdResponse>>(responses);
        }
        catch (Exception ex)
        {
            return Result.Failure<IEnumerable<PublicAdResponse>>(
                new Error("GetFailed", "Failed to retrieve active ads", 500));
        }
    }


    private static PublicAdResponse MapToPublicAdResponse(Ad ad)
    {
        return new PublicAdResponse
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
    }

    private static PublicOfferResponse MapToPublicOfferResponse(Offer offer)
    {
        return new PublicOfferResponse
        {
            Id = offer.Id,
            Title = offer.Title,
            Description = offer.Description,
            ImageUrl = offer.ImageUrl,
            UnitId = offer.UnitId,
            UnitName = offer.Unit?.Name,
            StartDate = offer.StartDate,
            EndDate = offer.EndDate,
            DiscountPercentage = offer.DiscountPercentage,
            DiscountAmount = offer.DiscountAmount,
            CreatedAt = offer.CreatedAt
        };
    }
}