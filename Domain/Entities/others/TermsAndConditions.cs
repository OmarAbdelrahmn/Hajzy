using System.ComponentModel.DataAnnotations;

namespace Domain.Entities.others;

public class TermsAndConditions
{
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string TitleA { get; set; } = string.Empty;
    public string TitleE { get; set; } = string.Empty;

    [Required]
    public string DescriptionA { get; set; } = string.Empty;
    public string DescriptionE { get; set; } = string.Empty;



    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int DisplayOrder { get; set; }
}
