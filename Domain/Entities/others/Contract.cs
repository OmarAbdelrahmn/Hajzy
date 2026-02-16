using System.ComponentModel.DataAnnotations;

namespace Domain.Entities.others;

public class Contract
{
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    public string? ContentEnglish { get; set; } = string.Empty;

    public string? ContentArabic { get; set; } = string.Empty;

    public string? Url { get; set; } = string.Empty;

    public TypeI? Type { get; set; }
    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

}

public enum TypeI
{
    Email,
    Facebook,
    Instagram,
    Twitter,
    LinkedIn,
    TikTok,
    YouTube
}