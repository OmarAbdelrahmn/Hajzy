using System.ComponentModel.DataAnnotations;

namespace Domain.Entities.others;

public class FAQ
{
    public int Id { get; set; }

    [Required]
    [MaxLength(500)]
    public string QuestionEnglish { get; set; } = string.Empty;

    [MaxLength(500)]
    public string QuestionArabic { get; set; } = "A";

    [Required]
    public string AnswerEnglish { get; set; } = string.Empty;

    public string AnswerArabic { get; set; } = string.Empty;

    [MaxLength(100)]
    public string CategoryA { get; set; } = string.Empty;
    public string CategoryE { get; set; } = string.Empty;

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int DisplayOrder { get; set; }
}
