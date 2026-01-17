using Domain.Migrations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Domain.Entities;

public class UnitType
{
    public int Id { get; set; }

    [Required, MaxLength(50)]
    public string Name { get; set; } = string.Empty; // Hotel, Apartment, Villa, Resort

    [MaxLength(200)]
    public string? Description { get; set; }


    public bool IsActive { get; set; } = true;

    // Navigation
    //public ICollection<Unit> Units { get; set; } = [];
}

public class SubUnitTypee
{
    public int Id { get; set; }

    [Required, MaxLength(50)]
    public string Name { get; set; } = string.Empty; // Hotel, Apartment, Villa, Resort

    [MaxLength(200)]
    public string? Description { get; set; }


    public bool IsActive { get; set; } = true;


}
