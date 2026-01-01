using Application.Contracts.other;
using Domain;
using Domain.Entities.others;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Hajzzy.Controllers;

[Route("api/[controller]")]
[ApiController]
public class FAQsController(ApplicationDbcontext context) : ControllerBase
{
    private readonly ApplicationDbcontext _context = context;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<FAQDto>>> GetFAQs(
        [FromQuery] bool? isActive = null,
        [FromQuery] string category = null)
    {
        var query = _context.FAQs.AsQueryable();

        if (isActive.HasValue)
            query = query.Where(f => f.IsActive == isActive.Value);

        if (!string.IsNullOrEmpty(category))
            query = query.Where(f => f.CategoryA == category);

        var faqs = await query.OrderBy(f => f.DisplayOrder).ToListAsync();

        return Ok(faqs.Select(f => new FAQDto
        {
            Id = f.Id,
            QuestionEnglish = f.QuestionEnglish,
            QuestionArabic = f.QuestionArabic,
            AnswerEnglish = f.AnswerEnglish,
            AnswerArabic = f.AnswerArabic,
            CategoryA = f.CategoryA,
            CategoryE = f.CategoryE,
            IsActive = f.IsActive,
            DisplayOrder = f.DisplayOrder
        }));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<FAQDto>> GetFAQ(int id)
    {
        var faq = await _context.FAQs.FindAsync(id);

        if (faq == null)
            return NotFound();

        return Ok(new FAQDto
        {
            Id = faq.Id,
            QuestionEnglish = faq.QuestionEnglish,
            QuestionArabic = faq.QuestionArabic,
            AnswerEnglish = faq.AnswerEnglish,
            AnswerArabic = faq.AnswerArabic,
            CategoryA = faq.CategoryA,
            CategoryE = faq.CategoryE,
            IsActive = faq.IsActive,
            DisplayOrder = faq.DisplayOrder
        });
    }

    [HttpGet("categories")]
    public async Task<ActionResult<IEnumerable<string>>> GetCategories()
    {
        var categories = await _context.FAQs
            .Where(f => f.IsActive && !string.IsNullOrEmpty(f.CategoryA))
            .Select(f => f.CategoryA)
            .Distinct()
            .ToListAsync();

        return Ok(categories);
    }

    [HttpPost]
    public async Task<ActionResult<FAQDto>> CreateFAQ(CreateFAQDto dto)
    {
        var faq = new FAQ
        {
            QuestionEnglish = dto.QuestionEnglish,
            QuestionArabic = dto.QuestionArabic,
            AnswerEnglish = dto.AnswerEnglish,
            AnswerArabic = dto.AnswerArabic,
            CategoryA = dto.CategoryA,
            CategoryE = dto.CategoryE,
            IsActive = dto.IsActive,
            CreatedAt = DateTime.UtcNow,
            DisplayOrder = dto.DisplayOrder
        };

        _context.FAQs.Add(faq);
        await _context.SaveChangesAsync();

        var faqDto = new FAQDto
        {
            Id = faq.Id,
            QuestionEnglish = faq.QuestionEnglish,
            QuestionArabic = faq.QuestionArabic,
            AnswerEnglish = faq.AnswerEnglish,
            AnswerArabic = faq.AnswerArabic,
            CategoryA = faq.CategoryA,
            CategoryE = faq.CategoryE,
            IsActive = faq.IsActive,
            DisplayOrder = faq.DisplayOrder
        };

        return CreatedAtAction(nameof(GetFAQ), new { id = faq.Id }, faqDto);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateFAQ(int id, UpdateFAQDto dto)
    {
        var faq = await _context.FAQs.FindAsync(id);

        if (faq == null)
            return NotFound();

        if (!string.IsNullOrEmpty(dto.QuestionEnglish))
            faq.QuestionEnglish = dto.QuestionEnglish;
        if (dto.QuestionArabic != null)
            faq.QuestionArabic = dto.QuestionArabic;
        if (!string.IsNullOrEmpty(dto.AnswerEnglish))
            faq.AnswerEnglish = dto.AnswerEnglish;
        if (dto.AnswerArabic != null)
            faq.AnswerArabic = dto.AnswerArabic;
        if (dto.CategoryA != null)
            faq.CategoryA = dto.CategoryA;
        if (dto.CategoryE != null)
            faq.CategoryE = dto.CategoryE;
        if (dto.IsActive.HasValue)
            faq.IsActive = dto.IsActive.Value;
        if (dto.DisplayOrder.HasValue)
            faq.DisplayOrder = dto.DisplayOrder.Value;

        faq.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteFAQ(int id)
    {
        var faq = await _context.FAQs.FindAsync(id);

        if (faq == null)
            return NotFound();

        _context.FAQs.Remove(faq);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}

