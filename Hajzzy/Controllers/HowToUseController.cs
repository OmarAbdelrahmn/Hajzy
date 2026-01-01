using Application.Contracts.other;
using Domain;
using Domain.Entities.others;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Hajzzy.Controllers;

[Route("api/[controller]")]
[ApiController]
public class HowToUseController(ApplicationDbcontext dbcontext) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<HowToUseDto>>> GetAll()
    {
        var items = await dbcontext.HowToUses.ToListAsync();

        return Ok(items.Select(h => new HowToUseDto
        {
            Id = h.Id,
            Url = h.Url
        }));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<HowToUseDto>> GetById(int id)
    {
        var item = await dbcontext.HowToUses.FindAsync(id);

        if (item == null)
            return NotFound();

        return Ok(new HowToUseDto
        {
            Id = item.Id,
            Url = item.Url
        });
    }

    [HttpPost]
    public async Task<ActionResult<HowToUseDto>> Create(CreateHowToUseDto dto)
    {
        var item = new HowToUse
        {
            Url = dto.Url,
        };

        dbcontext.HowToUses.Add(item);
        await dbcontext.SaveChangesAsync();

        var itemDto = new HowToUseDto
        {
            Id = item.Id,
            Url = item.Url
        };

        return CreatedAtAction(nameof(GetById), new { id = item.Id }, itemDto);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, UpdateHowToUseDto dto)
    {
        var item = await dbcontext.HowToUses.FindAsync(id);

        if (item == null)
            return NotFound();

        if (!string.IsNullOrEmpty(dto.Url))
            item.Url = dto.Url;


        await dbcontext.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var item = await dbcontext.HowToUses.FindAsync(id);

        if (item == null)
            return NotFound();

        dbcontext.HowToUses.Remove(item);
        await dbcontext.SaveChangesAsync();

        return NoContent();
    }
}
