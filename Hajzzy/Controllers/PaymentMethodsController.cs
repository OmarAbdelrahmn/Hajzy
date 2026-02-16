using Application.Contracts.other;
using Domain;
using Domain.Entities.others;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Hajzzy.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PaymentMethodsController(ApplicationDbcontext dbcontext) : ControllerBase
{
    private readonly ApplicationDbcontext dbcontext = dbcontext;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PaymentMethodDto>>> GetAll()
    {
        var policies = await dbcontext.PaymentMethods.ToListAsync();

        return Ok(policies.Select(p => new PaymentMethodDto
        {
            Id = p.Id,
            TitleA = p.TitleA,
            TitleE = p.TitleE,
            DescriptionA = p.DescriptionA,
            DescriptionE = p.DescriptionE
        }));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<PaymentMethodDto>> GetById(int id)
    {
        var policy = await dbcontext.PaymentMethods.FindAsync(id);

        if (policy == null)
            return NotFound();

        return Ok(new PaymentMethodDto
        {
            Id = policy.Id,
            TitleA = policy.TitleA,
            TitleE = policy.TitleE,
            DescriptionA = policy.DescriptionA,
            DescriptionE = policy.DescriptionE
        });
    }

    [HttpPost]
    public async Task<ActionResult<PaymentMethodDto>> Create(MethodCreateDto dto)
    {
        var policy = new PaymentMethodd
        {
            TitleA = dto.TitleA,
            TitleE = dto.TitleE,
            DescriptionA = dto.DescriptionA,
            DescriptionE = dto.DescriptionE
        };

        dbcontext.PaymentMethods.Add(policy);
        await dbcontext.SaveChangesAsync();

        var policyDto = new PaymentMethodDto
        {
            Id = policy.Id,
            TitleA = policy.TitleA,
            TitleE = policy.TitleE,
            DescriptionA = policy.DescriptionA,
            DescriptionE = policy.DescriptionE
        };

        return CreatedAtAction(nameof(GetById), new { id = policy.Id }, policyDto);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, updatePaymentMethodDto dto)
    {
        var policy = await dbcontext.PaymentMethods.FindAsync(id);

        if (policy == null)
            return NotFound();

        if (!string.IsNullOrEmpty(dto.TitleA))
            policy.TitleA = dto.TitleA;
        if (!string.IsNullOrEmpty(dto.TitleE))
            policy.TitleE = dto.TitleE;
        if (!string.IsNullOrEmpty(dto.DescriptionA))
            policy.DescriptionA = dto.DescriptionA;
        if (!string.IsNullOrEmpty(dto.DescriptionE))
            policy.DescriptionE = dto.DescriptionE;

        await dbcontext.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var policy = await dbcontext.PaymentMethods.FindAsync(id);

        if (policy == null)
            return NotFound();

        dbcontext.PaymentMethods.Remove(policy);
        await dbcontext.SaveChangesAsync();

        return NoContent();
    }
}
