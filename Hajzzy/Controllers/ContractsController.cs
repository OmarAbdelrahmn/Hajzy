using Application.Contracts.other;
using Domain;
using Domain.Entities.others;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Hajzzy.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ContractsController(ApplicationDbcontext dbcontext) : ControllerBase
{
    private readonly ApplicationDbcontext dbcontext = dbcontext;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ContractDto>>> GetContracts([FromQuery] bool? isActive = null)
    {
        var query = dbcontext.Contracts.AsQueryable();

        if (isActive.HasValue)
            query = query.Where(c => c.IsActive == isActive.Value);

        var contracts = await query.ToListAsync();

        return Ok(contracts.Select(c => new ContractDto
        {
            Id = c.Id,
            Title = c.Title,
            ContentEnglish = c.ContentEnglish,
            ContentArabic = c.ContentArabic,
            Url = c.Url,
            Type = c.Type,
            IsActive = c.IsActive
        }));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ContractDto>> GetContract(int id)
    {
        var contract = await dbcontext.Contracts.FindAsync(id);

        if (contract == null)
            return NotFound();

        return Ok(new ContractDto
        {
            Id = contract.Id,
            Title = contract.Title,
            ContentEnglish = contract.ContentEnglish,
            ContentArabic = contract.ContentArabic,
            Url = contract.Url,
            Type = contract.Type,
            IsActive = contract.IsActive
        });
    }

    [HttpPost]
    public async Task<ActionResult<ContractDto>> CreateContract(CreateContractDto dto)
    {
        var contract = new Contract
        {
            Title = dto.Title,
            ContentEnglish = dto.ContentEnglish,
            ContentArabic = dto.ContentArabic,
            Url = dto.Url,
            Type = dto.Type,
            IsActive = dto.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        dbcontext.Contracts.Add(contract);
        await dbcontext.SaveChangesAsync();

        var contractDto = new ContractDto
        {
            Id = contract.Id,
            Title = contract.Title,
            ContentEnglish = contract.ContentEnglish,
            ContentArabic = contract.ContentArabic,
            Url = contract.Url,
            Type = contract.Type,
            IsActive = contract.IsActive
        };

        return CreatedAtAction(nameof(GetContract), new { id = contract.Id }, contractDto);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateContract(int id, UpdateContractDto dto)
    {
        var contract = await dbcontext.Contracts.FindAsync(id);

        if (contract == null)
            return NotFound();

        if (!string.IsNullOrEmpty(dto.Title))
            contract.Title = dto.Title;
        if (dto.ContentEnglish != null)
            contract.ContentEnglish = dto.ContentEnglish;
        if (dto.ContentArabic != null)
            contract.ContentArabic = dto.ContentArabic;
        if (dto.Url != null)
            contract.Url = dto.Url;
        if (dto.Type.HasValue)
            contract.Type = dto.Type.Value;
        if (dto.IsActive.HasValue)
            contract.IsActive = dto.IsActive.Value;

        contract.UpdatedAt = DateTime.UtcNow;

        await dbcontext.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteContract(int id)
    {
        var contract = await dbcontext.Contracts.FindAsync(id);

        if (contract == null)
            return NotFound();

        dbcontext.Contracts.Remove(contract);
        await dbcontext.SaveChangesAsync();

        return NoContent();
    }
}