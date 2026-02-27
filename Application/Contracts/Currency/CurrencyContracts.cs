namespace Application.Contracts.Currency;

// ── Response ──────────────────────────────────────────────────────────────────

public record CurrencyResponse(
    int Id,
    string Code,
    string NameEnglish,
    string NameArabic,
    string Symbol,
    int DisplayOrder,
    bool IsActive,
    bool IsDefault
);

// ── Requests ──────────────────────────────────────────────────────────────────

public record CreateCurrencyRequest(
    string Code,
    string NameEnglish,
    string NameArabic,
    string Symbol,
    int DisplayOrder = 0,
    bool IsActive = true,
    bool IsDefault = false
);

public record UpdateCurrencyRequest(
    string? Code = null,
    string? NameEnglish = null,
    string? NameArabic = null,
    string? Symbol = null,
    int? DisplayOrder = null,
    bool? IsActive = null,
    bool? IsDefault = null
);

// Used by hotel admin to change the currency of their unit
public record SetUnitCurrencyRequest(int CurrencyId);