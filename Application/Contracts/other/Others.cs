using Domain.Entities.others;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Application.Contracts.other;

internal class Others
{
}

public class ContractDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? ContentEnglish { get; set; }
    public string? ContentArabic { get; set; }
    public string? Url { get; set; }
    public TypeI? Type { get; set; }
    public bool IsActive { get; set; }
}

public class CreateContractDto
{
    public string Title { get; set; } = string.Empty;
    public string? ContentEnglish { get; set; }
    public string? ContentArabic { get; set; }
    public string? Url { get; set; }
    public TypeI? Type { get; set; }
    public bool IsActive { get; set; }
}

public class UpdateContractDto
{
    public string? Title { get; set; }
    public string? ContentEnglish { get; set; }
    public string? ContentArabic { get; set; }
    public string? Url { get; set; }
    public TypeI? Type { get; set; }
    public bool? IsActive { get; set; }
}

// ===== PrivacyPolicy DTOs =====
public class PrivacyPolicyDto
{
    public int Id { get; set; }
    public string TitleA { get; set; } = string.Empty;
    public string TitleE { get; set; } = string.Empty;
    public string DescreptionA { get; set; } = string.Empty;
    public string DescreptionE { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

public class CreatePrivacyPolicyDto
{
    public string TitleA { get; set; } = string.Empty;
    public string TitleE { get; set; } = string.Empty;
    public string DescreptionA { get; set; } = string.Empty;
    public string DescreptionE { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

public class UpdatePrivacyPolicyDto
{
    public string? TitleA { get; set; }
    public string? TitleE { get; set; }
    public string? DescreptionA { get; set; }
    public string? DescreptionE { get; set; }
    public string? Version { get; set; }
    public bool? IsActive { get; set; }
}

// ===== PublicCancelPolicy DTOs =====
public class PublicCancelPolicyDto
{
    public int Id { get; set; }
    public string TitleA { get; set; } = string.Empty;
    public string TitleE { get; set; } = string.Empty;
    public string DescriptionA { get; set; } = string.Empty;
    public string DescriptionE { get; set; } = string.Empty;
}
public class PaymentMethodDto
{
    public int Id { get; set; }
    public string TitleA { get; set; } = string.Empty;
    public string TitleE { get; set; } = string.Empty;
    public string DescriptionA { get; set; } = string.Empty;
    public string DescriptionE { get; set; } = string.Empty;
}

public class CreatePublicCancelPolicyDto
{
    public string TitleA { get; set; } = string.Empty;
    public string TitleE { get; set; } = string.Empty;
    public string DescriptionA { get; set; } = string.Empty;
    public string DescriptionE { get; set; } = string.Empty;
}
public class MethodCreateDto
{
    public string TitleA { get; set; } = string.Empty;
    public string TitleE { get; set; } = string.Empty;
    public string DescriptionA { get; set; } = string.Empty;
    public string DescriptionE { get; set; } = string.Empty;
}

public class UpdatePublicCancelPolicyDto
{
    public string? TitleA { get; set; }
    public string? TitleE { get; set; }
    public string? DescriptionA { get; set; }
    public string? DescriptionE { get; set; }
}

public class updatePaymentMethodDto
{
    public string? TitleA { get; set; }
    public string? TitleE { get; set; }
    public string? DescriptionA { get; set; }
    public string? DescriptionE { get; set; }
}

// ===== TermsAndConditions DTOs =====
public class TermsAndConditionsDto
{
    public int Id { get; set; }
    public string TitleA { get; set; } = string.Empty;
    public string TitleE { get; set; } = string.Empty;
    public string DescriptionA { get; set; } = string.Empty;
    public string DescriptionE { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public int DisplayOrder { get; set; }
}

public class CreateTermsAndConditionsDto
{
    public string TitleA { get; set; } = string.Empty;
    public string TitleE { get; set; } = string.Empty;
    public string DescriptionA { get; set; } = string.Empty;
    public string DescriptionE { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public int DisplayOrder { get; set; }
}

public class UpdateTermsAndConditionsDto
{
    public string? TitleA { get; set; }
    public string? TitleE { get; set; }
    public string? DescriptionA { get; set; }
    public string? DescriptionE { get; set; }
    public bool? IsActive { get; set; }
    public int? DisplayOrder { get; set; }
}

// DTOs for FAQ
public class FAQDto
{
    public int Id { get; set; }
    public string QuestionEnglish { get; set; }
    public string QuestionArabic { get; set; }
    public string AnswerEnglish { get; set; }
    public string AnswerArabic { get; set; }
    public string CategoryA { get; set; }
    public string CategoryE { get; set; }
    public bool IsActive { get; set; }
    public int DisplayOrder { get; set; }
}

public class CreateFAQDto
{
    [Required]
    public string QuestionEnglish { get; set; }
    public string QuestionArabic { get; set; }
    [Required]
    public string AnswerEnglish { get; set; }
    public string AnswerArabic { get; set; }
    public string CategoryA { get; set; }
    public string CategoryE { get; set; }
    public bool IsActive { get; set; } = true;
    public int DisplayOrder { get; set; }
}

public class UpdateFAQDto
{
    public string QuestionEnglish { get; set; }
    public string QuestionArabic { get; set; }
    public string AnswerEnglish { get; set; }
    public string AnswerArabic { get; set; }
    public string CategoryA { get; set; }
    public string CategoryE { get; set; }
    public bool? IsActive { get; set; }
    public int? DisplayOrder { get; set; }
}

public class HowToUseDto
{
    public int Id { get; set; }
    public string Url { get; set; }
}

public class CreateHowToUseDto
{
    [Required]
    [MaxLength(500)]
    public string Url { get; set; }
}

public class UpdateHowToUseDto
{
    [MaxLength(500)]
    public string Url { get; set; }
}

// PublicCancelPolicy DTO

// ===== Contract