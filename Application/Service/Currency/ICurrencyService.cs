using Application.Abstraction;
using Application.Contracts.Currency;

namespace Application.Service.Currency;

public interface ICurrencyService
{
    // Platform admin operations

    Task<Result<IEnumerable<CurrencyResponse>>> GetAllAsync(bool activeOnly = false);
    Task<Result<CurrencyResponse>> GetByIdAsync(int currencyId);
    Task<Result<CurrencyResponse>> GetByCodeAsync(string code);
    Task<Result<CurrencyResponse>> GetDefaultAsync();
    Task<Result<CurrencyResponse>> CreateAsync(CreateCurrencyRequest request);
    Task<Result<CurrencyResponse>> UpdateAsync(int currencyId, UpdateCurrencyRequest request);
    Task<Result> DeactivateAsync(int currencyId);
    Task<Result> SetDefaultAsync(int currencyId);

    // Hotel-admin / unit operations

    Task<Result<CurrencyResponse>> GetUnitCurrencyAsync(int unitId);
    Task<Result> SetUnitCurrencyAsync(int unitId, int currencyId);

    // Seeding helper

    Task SeedDefaultCurrenciesAsync();
}