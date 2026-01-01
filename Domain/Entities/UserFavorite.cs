using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Entities;

public class UserFavorite
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public int UnitId { get; set; }
    public DateTime AddedAt { get; set; }

    public ApplicationUser User { get; set; } = default!;
    public Unit Unit { get; set; } = default!;
}
