namespace Domain.Entities;

public class UnitAvailability
{
    public int Id { get; set; }
    public int UnitId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsAvailable { get; set; } = true;
    public UnavailabilityReason? Reason { get; set; }


    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }

    // Navigation
    public Unit Unit { get; set; } = default!;
    public ApplicationUser? UpdatedBy { get; set; }
}

