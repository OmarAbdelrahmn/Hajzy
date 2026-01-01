using Application.Contracts.other;
using Domain;
using Domain.Entities.others;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Hajzzy.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PrivacyPoliciesController(ApplicationDbcontext context) : ControllerBase
{
    private readonly ApplicationDbcontext _context = context;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PrivacyPolicyDto>>> GetPrivacyPolicies([FromQuery] bool? isActive = null)
    {
        var query = _context.PrivacyPolicies.AsQueryable();

        if (isActive.HasValue)
            query = query.Where(p => p.IsActive == isActive.Value);

        var policies = await query.ToListAsync();

        return Ok(policies.Select(p => new PrivacyPolicyDto
        {
            Id = p.Id,
            TitleA = p.TitleA,
            TitleE = p.TitleE,
            DescreptionA = p.DescreptionA,
            DescreptionE = p.DescreptionE,
            Version = p.Version,
            IsActive = p.IsActive
        }));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<PrivacyPolicyDto>> GetPrivacyPolicy(int id)
    {
        var p = await _context.PrivacyPolicies.FindAsync(id);

        if (p == null)
            return NotFound();

        return Ok(new PrivacyPolicyDto
        {
            Id = p.Id,
            TitleA = p.TitleA,
            TitleE = p.TitleE,
            DescreptionA = p.DescreptionA,
            DescreptionE = p.DescreptionE,
            Version = p.Version,
            IsActive = p.IsActive
        });
    }

    [HttpPost]
    public async Task<ActionResult<PrivacyPolicyDto>> CreatePrivacyPolicy(CreatePrivacyPolicyDto dto)
    {
        var policy = new PrivacyPolicy
        {
            TitleA = dto.TitleA,
            TitleE = dto.TitleE,
            DescreptionA = dto.DescreptionA,
            DescreptionE = dto.DescreptionE,
            Version = dto.Version,
            IsActive = dto.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        _context.PrivacyPolicies.Add(policy);
        await _context.SaveChangesAsync();

        var policyDto = new PrivacyPolicyDto
        {
            Id = policy.Id,
            TitleA = policy.TitleA,
            TitleE = policy.TitleE,
            DescreptionA = policy.DescreptionA,
            DescreptionE = policy.DescreptionE,
            Version = policy.Version,
            IsActive = policy.IsActive
        };

        return CreatedAtAction(nameof(GetPrivacyPolicy), new { id = policy.Id }, policyDto);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdatePrivacyPolicy(int id, UpdatePrivacyPolicyDto dto)
    {
        var policy = await _context.PrivacyPolicies.FindAsync(id);

        if (policy == null)
            return NotFound();

        if (!string.IsNullOrEmpty(dto.TitleA))
            policy.TitleA = dto.TitleA;
        if (!string.IsNullOrEmpty(dto.TitleE))
            policy.TitleE = dto.TitleE;
        if (dto.DescreptionA != null)
            policy.DescreptionA = dto.DescreptionA;
        if (dto.DescreptionE != null)
            policy.DescreptionE = dto.DescreptionE;
        if (!string.IsNullOrEmpty(dto.Version))
            policy.Version = dto.Version;
        if (dto.IsActive.HasValue)
            policy.IsActive = dto.IsActive.Value;

        policy.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePrivacyPolicy(int id)
    {
        var policy = await _context.PrivacyPolicies.FindAsync(id);

        if (policy == null)
            return NotFound();

        _context.PrivacyPolicies.Remove(policy);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}