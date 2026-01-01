using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Entities;

[Owned]
public class RoomConfiguration
{
    public int Id { get; set; }
    public int Bedrooms { get; set; }
    public int Bathrooms { get; set; }
    public decimal Size { get; set; }
    public BedType BedType { get; set; }
    public int BedCount { get; set; }
    public bool HasKitchen { get; set; }
    public bool HasBalcony { get; set; }
    public string? ViewType { get; set; } // "Ocean", "Mountain", "City"
    public int? Floor { get; set; }
}

public enum BedType
{
    Single,
    Double,
    Queen,
    King,
    Bunk,
    Sofa
}
