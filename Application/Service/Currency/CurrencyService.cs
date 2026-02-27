using Application.Abstraction;
using Application.Contracts.Currency;
using Domain;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Service.Currency;

public class CurrencyService(
    ApplicationDbcontext context,
    ILogger<CurrencyService> logger) : ICurrencyService
{
    private readonly ApplicationDbcontext _context = context;
    private readonly ILogger<CurrencyService> _logger = logger;

    // =========================================================================
    // READ
    // =========================================================================

    public async Task<Result<IEnumerable<CurrencyResponse>>> GetAllAsync(bool activeOnly = false)
    {
        try
        {
            var query = _context.Currencies.AsQueryable();
            if (activeOnly) query = query.Where(c => c.IsActive);

            var list = await query
                .OrderBy(c => c.DisplayOrder)
                .ThenBy(c => c.Code)
                .AsNoTracking()
                .ToListAsync();

            return Result.Success(list.Select(Map));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching all currencies");
            return Result.Failure<IEnumerable<CurrencyResponse>>(
                new Error("GetCurrenciesFailed", "Failed to retrieve currencies", 500));
        }
    }

    public async Task<Result<CurrencyResponse>> GetByIdAsync(int currencyId)
    {
        try
        {
            var currency = await _context.Currencies
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == currencyId);

            return currency is null
                ? Result.Failure<CurrencyResponse>(new Error("NotFound", "Currency not found", 404))
                : Result.Success(Map(currency));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching currency {CurrencyId}", currencyId);
            return Result.Failure<CurrencyResponse>(
                new Error("GetCurrencyFailed", "Failed to retrieve currency", 500));
        }
    }

    public async Task<Result<CurrencyResponse>> GetByCodeAsync(string code)
    {
        try
        {
            var currency = await _context.Currencies
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Code.ToUpper() == code.ToUpper().Trim());

            return currency is null
                ? Result.Failure<CurrencyResponse>(
                    new Error("NotFound", $"Currency with code '{code}' not found", 404))
                : Result.Success(Map(currency));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching currency by code {Code}", code);
            return Result.Failure<CurrencyResponse>(
                new Error("GetCurrencyFailed", "Failed to retrieve currency", 500));
        }
    }

    public async Task<Result<CurrencyResponse>> GetDefaultAsync()
    {
        try
        {
            var currency = await _context.Currencies
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.IsDefault && c.IsActive);

            // Fall back to SAR when no default is explicitly set
            currency ??= await _context.Currencies
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Code == "SAR" && c.IsActive);

            return currency is null
                ? Result.Failure<CurrencyResponse>(
                    new Error("NotFound", "No default currency configured", 404))
                : Result.Success(Map(currency));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching default currency");
            return Result.Failure<CurrencyResponse>(
                new Error("GetCurrencyFailed", "Failed to retrieve default currency", 500));
        }
    }

    // =========================================================================
    // WRITE
    // =========================================================================

    public async Task<Result<CurrencyResponse>> CreateAsync(CreateCurrencyRequest request)
    {
        try
        {
            var code = request.Code.ToUpper().Trim();

            var duplicate = await _context.Currencies
                .AnyAsync(c => c.Code.ToUpper() == code);

            if (duplicate)
                return Result.Failure<CurrencyResponse>(
                    new Error("DuplicateCode", $"Currency with code '{code}' already exists", 409));

            // Unset any existing default when creating a new one flagged as default
            if (request.IsDefault)
                await ClearDefaultFlagAsync();

            var currency = new Domain.Entities.Currency
            {
                Code = code,
                NameEnglish = request.NameEnglish.Trim(),
                NameArabic = request.NameArabic.Trim(),
                Symbol = request.Symbol.Trim(),
                DisplayOrder = request.DisplayOrder,
                IsActive = request.IsActive,
                IsDefault = request.IsDefault,
                CreatedAt = DateTime.UtcNow.AddHours(3)
            };

            _context.Currencies.Add(currency);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Currency {Code} created (Id={Id})", currency.Code, currency.Id);
            return Result.Success(Map(currency));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating currency");
            return Result.Failure<CurrencyResponse>(
                new Error("CreateCurrencyFailed", "Failed to create currency", 500));
        }
    }

    public async Task<Result<CurrencyResponse>> UpdateAsync(
        int currencyId, UpdateCurrencyRequest request)
    {
        try
        {
            var currency = await _context.Currencies
                .FirstOrDefaultAsync(c => c.Id == currencyId);

            if (currency is null)
                return Result.Failure<CurrencyResponse>(
                    new Error("NotFound", "Currency not found", 404));

            if (request.Code is not null)
            {
                var code = request.Code.ToUpper().Trim();
                var duplicate = await _context.Currencies
                    .AnyAsync(c => c.Code.ToUpper() == code && c.Id != currencyId);
                if (duplicate)
                    return Result.Failure<CurrencyResponse>(
                        new Error("DuplicateCode", $"Code '{code}' is already in use", 409));
                currency.Code = code;
            }

            if (request.NameEnglish is not null) currency.NameEnglish = request.NameEnglish.Trim();
            if (request.NameArabic is not null) currency.NameArabic = request.NameArabic.Trim();
            if (request.Symbol is not null) currency.Symbol = request.Symbol.Trim();
            if (request.DisplayOrder.HasValue) currency.DisplayOrder = request.DisplayOrder.Value;
            if (request.IsActive.HasValue) currency.IsActive = request.IsActive.Value;

            if (request.IsDefault.HasValue && request.IsDefault.Value && !currency.IsDefault)
            {
                await ClearDefaultFlagAsync();
                currency.IsDefault = true;
            }
            else if (request.IsDefault.HasValue)
            {
                currency.IsDefault = request.IsDefault.Value;
            }

            currency.UpdatedAt = DateTime.UtcNow.AddHours(3);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Currency {Id} ({Code}) updated", currency.Id, currency.Code);
            return Result.Success(Map(currency));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating currency {CurrencyId}", currencyId);
            return Result.Failure<CurrencyResponse>(
                new Error("UpdateCurrencyFailed", "Failed to update currency", 500));
        }
    }

    public async Task<Result> DeactivateAsync(int currencyId)
    {
        try
        {
            var currency = await _context.Currencies
                .FirstOrDefaultAsync(c => c.Id == currencyId);

            if (currency is null)
                return Result.Failure(new Error("NotFound", "Currency not found", 404));

            var inUse = await _context.Units
                .AnyAsync(u => u.CurrencyId == currencyId && !u.IsDeleted);

            if (inUse)
                return Result.Failure(new Error("InUse",
                    "Cannot deactivate: one or more active units are using this currency", 400));

            currency.IsActive = false;
            currency.IsDefault = false; // A deactivated currency cannot remain the default
            currency.UpdatedAt = DateTime.UtcNow.AddHours(3);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Currency {Id} deactivated", currencyId);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deactivating currency {CurrencyId}", currencyId);
            return Result.Failure(new Error("DeactivateFailed", "Failed to deactivate currency", 500));
        }
    }

    public async Task<Result> SetDefaultAsync(int currencyId)
    {
        try
        {
            var currency = await _context.Currencies
                .FirstOrDefaultAsync(c => c.Id == currencyId);

            if (currency is null)
                return Result.Failure(new Error("NotFound", "Currency not found", 404));

            if (!currency.IsActive)
                return Result.Failure(new Error("Inactive",
                    "Cannot set an inactive currency as the default", 400));

            await ClearDefaultFlagAsync();
            currency.IsDefault = true;
            currency.UpdatedAt = DateTime.UtcNow.AddHours(3);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Currency {Id} ({Code}) set as default", currency.Id, currency.Code);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting default currency {CurrencyId}", currencyId);
            return Result.Failure(new Error("SetDefaultFailed", "Failed to set default currency", 500));
        }
    }

    // =========================================================================
    // UNIT CURRENCY
    // =========================================================================

    public async Task<Result<CurrencyResponse>> GetUnitCurrencyAsync(int unitId)
    {
        try
        {
            var currency = await _context.Units
                .Where(u => u.Id == unitId && !u.IsDeleted)
                .Select(u => u.Currency)
                .FirstOrDefaultAsync();

            if (currency is not null)
                return Result.Success(Map(currency));

            // Unit has no explicit currency — return the platform default
            return await GetDefaultAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching currency for unit {UnitId}", unitId);
            return Result.Failure<CurrencyResponse>(
                new Error("GetUnitCurrencyFailed", "Failed to retrieve unit currency", 500));
        }
    }

    public async Task<Result> SetUnitCurrencyAsync(int unitId, int currencyId)
    {
        try
        {
            var currency = await _context.Currencies
                .FirstOrDefaultAsync(c => c.Id == currencyId && c.IsActive);

            if (currency is null)
                return Result.Failure(new Error("NotFound",
                    "Currency not found or is inactive", 404));

            var affected = await _context.Units
                .Where(u => u.Id == unitId && !u.IsDeleted)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(u => u.CurrencyId, currencyId)
                    .SetProperty(u => u.UpdatedAt, DateTime.UtcNow.AddHours(3)));

            if (affected == 0)
                return Result.Failure(new Error("NotFound", "Unit not found", 404));

            _logger.LogInformation("Unit {UnitId} currency set to {Code}", unitId, currency.Code);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting currency for unit {UnitId}", unitId);
            return Result.Failure(new Error("SetUnitCurrencyFailed",
                "Failed to update unit currency", 500));
        }
    }

    // =========================================================================
    // SEED
    // =========================================================================

    public async Task SeedDefaultCurrenciesAsync()
    {
        try
        {
            var seeds = new[]
            {
                new { Code = "SAR", NameEnglish = "Saudi Riyal",    NameArabic = "ريال سعودي",      Symbol = "﷼",  Order = 1, IsDefault = true  },
                new { Code = "USD", NameEnglish = "US Dollar",      NameArabic = "دولار أمريكي",     Symbol = "$",  Order = 2, IsDefault = false },
                new { Code = "AED", NameEnglish = "UAE Dirham",     NameArabic = "درهم إماراتي",     Symbol = "د.إ", Order = 3, IsDefault = false },
                new { Code = "YER", NameEnglish = "Yemeni Rial",    NameArabic = "ريال يمني",        Symbol = "﷼",  Order = 4, IsDefault = false },
                new { Code = "EGP", NameEnglish = "Egyptian Pound", NameArabic = "جنيه مصري",        Symbol = "ج.م", Order = 5, IsDefault = false },
                new { Code = "EUR", NameEnglish = "Euro",           NameArabic = "يورو",              Symbol = "€",  Order = 6, IsDefault = false },
                new { Code = "GBP", NameEnglish = "British Pound",  NameArabic = "جنيه إسترليني",    Symbol = "£",  Order = 7, IsDefault = false },
            };

            foreach (var s in seeds)
            {
                var exists = await _context.Currencies
                    .AnyAsync(c => c.Code == s.Code);

                if (!exists)
                {
                    _context.Currencies.Add(new Domain.Entities.Currency
                    {
                        Code = s.Code,
                        NameEnglish = s.NameEnglish,
                        NameArabic = s.NameArabic,
                        Symbol = s.Symbol,
                        DisplayOrder = s.Order,
                        IsActive = true,
                        IsDefault = s.IsDefault,
                        CreatedAt = DateTime.UtcNow.AddHours(3)
                    });
                }
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("Currency seed completed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error seeding default currencies");
        }
    }

    // =========================================================================
    // PRIVATE HELPERS
    // =========================================================================

    private async Task ClearDefaultFlagAsync()
    {
        await _context.Currencies
            .Where(c => c.IsDefault)
            .ExecuteUpdateAsync(s => s.SetProperty(c => c.IsDefault, false));
    }

    private static CurrencyResponse Map(Domain.Entities.Currency c) => new(
        c.Id,
        c.Code,
        c.NameEnglish,
        c.NameArabic,
        c.Symbol,
        c.DisplayOrder,
        c.IsActive,
        c.IsDefault
    );
}