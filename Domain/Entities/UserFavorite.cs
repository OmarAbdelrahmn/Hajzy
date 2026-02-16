using System.ComponentModel.DataAnnotations;

namespace Domain.Entities;

public class UserFavorite
{
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    // Support both Unit and SubUnit favorites
    public int FavId { get; set; }
    public FavoriteType Type { get; set; }

    public DateTime AddedAt { get; set; } = DateTime.UtcNow;


    // Navigation
    public ApplicationUser User { get; set; } = default!;
    public Unit? Unit { get; set; }
    public SubUnit? SubUnit { get; set; }
}

public enum FavoriteType
{
    Unit = 1,
    SubUnit = 2
}