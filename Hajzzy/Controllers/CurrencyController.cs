using Application.Abstraction.Consts;
using Application.Contracts.Currency;
using Application.Service.Currency;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Hajzzy.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CurrencyController(ICurrencyService currencyService) : ControllerBase
{
    private readonly ICurrencyService _currencyService = currencyService;

    // =========================================================================
    // PUBLIC — no auth required
    // =========================================================================

    /// <summary>Returns all active currencies (for drop-downs in the UI).</summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll()
    {
        var result = await _currencyService.GetAllAsync(activeOnly: true);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    /// <summary>Returns the platform default currency.</summary>
    [HttpGet("default")]
    [AllowAnonymous]
    public async Task<IActionResult> GetDefault()
    {
        var result = await _currencyService.GetDefaultAsync();
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    /// <summary>Returns a single currency by its ISO code (e.g. SAR, USD).</summary>
    [HttpGet("code/{code}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetByCode(string code)
    {
        var result = await _currencyService.GetByCodeAsync(code);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    /// <summary>Returns a single currency by ID.</summary>
    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _currencyService.GetByIdAsync(id);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    // =========================================================================
    // PLATFORM ADMIN — SuperAdmin only
    // =========================================================================

    /// <summary>Returns ALL currencies including inactive ones.</summary>
    [HttpGet("admin/all")]
    [Authorize(Roles = DefaultRoles.SuperAdmin)]
    public async Task<IActionResult> GetAllAdmin()
    {
        var result = await _currencyService.GetAllAsync(activeOnly: false);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    /// <summary>Creates a new currency.</summary>
    [HttpPost]
    [Authorize(Roles = DefaultRoles.SuperAdmin)]
    public async Task<IActionResult> Create([FromBody] CreateCurrencyRequest request)
    {
        var result = await _currencyService.CreateAsync(request);
        if (!result.IsSuccess)
            return result.ToProblem();

        return CreatedAtAction(nameof(GetById), new { id = result.Value.Id }, result.Value);
    }

    /// <summary>Partially updates an existing currency.</summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = DefaultRoles.SuperAdmin)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateCurrencyRequest request)
    {
        var result = await _currencyService.UpdateAsync(id, request);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    /// <summary>
    /// Sets a currency as the platform default.
    /// Clears the flag from all other currencies automatically.
    /// </summary>
    [HttpPut("{id:int}/set-default")]
    [Authorize(Roles = DefaultRoles.SuperAdmin)]
    public async Task<IActionResult> SetDefault(int id)
    {
        var result = await _currencyService.SetDefaultAsync(id);
        return result.IsSuccess ? Ok(new { Message = "Default currency updated" }) : result.ToProblem();
    }

    /// <summary>
    /// Deactivates a currency.
    /// Fails if any active unit is still using it.
    /// </summary>
    [HttpPut("{id:int}/deactivate")]
    [Authorize(Roles = DefaultRoles.SuperAdmin)]
    public async Task<IActionResult> Deactivate(int id)
    {
        var result = await _currencyService.DeactivateAsync(id);
        return result.IsSuccess ? Ok(new { Message = "Currency deactivated" }) : result.ToProblem();
    }
}
