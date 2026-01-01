using Application.Contracts.other;
using Domain;
using Domain.Entities.others;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Hajzzy.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TermsAndConditionsController(ApplicationDbcontext context) : ControllerBase
{
    private readonly ApplicationDbcontext _context = context;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TermsAndConditionsDto>>> GetTermsAndConditions([FromQuery] bool? isActive = null)
    {
        var query = _context.TermsAndConditions.AsQueryable();

        if (isActive.HasValue)
            query = query.Where(t => t.IsActive == isActive.Value);

        var terms = await query.OrderBy(t => t.DisplayOrder).ToListAsync();

        return Ok(terms.Select(t => new TermsAndConditionsDto
        {
            Id = t.Id,
            TitleA = t.TitleA,
            TitleE = t.TitleE,
            DescriptionA = t.DescriptionA,
            DescriptionE = t.DescriptionE,
            IsActive = t.IsActive,
            DisplayOrder = t.DisplayOrder
        }));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TermsAndConditionsDto>> GetTermsAndCondition(int id)
    {
        var terms = await _context.TermsAndConditions.FindAsync(id);

        if (terms == null)
            return NotFound();

        return Ok(new TermsAndConditionsDto
        {
            Id = terms.Id,
            TitleA = terms.TitleA,
            TitleE = terms.TitleE,
            DescriptionA = terms.DescriptionA,
            DescriptionE = terms.DescriptionE,
            IsActive = terms.IsActive,
            DisplayOrder = terms.DisplayOrder
        });
    }

    [HttpPost]
    public async Task<ActionResult<TermsAndConditionsDto>> CreateTermsAndConditions(CreateTermsAndConditionsDto dto)
    {
        var terms = new TermsAndConditions
        {
            TitleA = dto.TitleA,
            TitleE = dto.TitleE,
            DescriptionA = dto.DescriptionA,
            DescriptionE = dto.DescriptionE,
            IsActive = dto.IsActive,
            DisplayOrder = dto.DisplayOrder,
            CreatedAt = DateTime.UtcNow
        };

        _context.TermsAndConditions.Add(terms);
        await _context.SaveChangesAsync();

        var termsDto = new TermsAndConditionsDto
        {
            Id = terms.Id,
            TitleA = terms.TitleA,
            TitleE = terms.TitleE,
            DescriptionA = terms.DescriptionA,
            DescriptionE = terms.DescriptionE,
            IsActive = terms.IsActive,
            DisplayOrder = terms.DisplayOrder
        };

        return CreatedAtAction(nameof(GetTermsAndCondition), new { id = terms.Id }, termsDto);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateTermsAndConditions(int id, UpdateTermsAndConditionsDto dto)
    {
        var terms = await _context.TermsAndConditions.FindAsync(id);

        if (terms == null)
            return NotFound();

        if (!string.IsNullOrEmpty(dto.TitleA))
            terms.TitleA = dto.TitleA;
        if (!string.IsNullOrEmpty(dto.TitleE))
            terms.TitleE = dto.TitleE;
        if (dto.DescriptionA != null)
            terms.DescriptionA = dto.DescriptionA;
        if (dto.DescriptionE != null)
            terms.DescriptionE = dto.DescriptionE;
        if (dto.IsActive.HasValue)
            terms.IsActive = dto.IsActive.Value;
        if (dto.DisplayOrder.HasValue)
            terms.DisplayOrder = dto.DisplayOrder.Value;

        terms.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTermsAndConditions(int id)
    {
        var terms = await _context.TermsAndConditions.FindAsync(id);

        if (terms == null)
            return NotFound();

        _context.TermsAndConditions.Remove(terms);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}