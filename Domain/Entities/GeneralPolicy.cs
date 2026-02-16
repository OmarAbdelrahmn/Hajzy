using System.ComponentModel.DataAnnotations;

namespace Domain.Entities;

public class GeneralPolicy
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string Description { get; set; } = string.Empty;

    public GeneralPolicyName PolicyType { get; set; }
    public GeneralPolicyCategory? PolicyCategory { get; set; }
    public string? CustomPolicyName { get; set; }


    public int? CancellationPolicyId { get; set; }
    public CancellationPolicy? CancellationPolicy { get; set; }

    public bool IsMandatory { get; set; }
    public bool IsHighlighted { get; set; }
    public bool IsActive { get; set; } = true;


    public int? UnitId { get; set; }
    public int? SubUnitId { get; set; }

    public Unit? Unit { get; set; }
    public SubUnit? SubUnit { get; set; }
}
