using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Domain.Entities;


public class SubUnitOption
{
    public int Id { get; set; }

    public int SubUnitId { get; set; }

    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    public OptionInputType InputType { get; set; }

    public bool IsRequired { get; set; } = false;

    public int DisplayOrder { get; set; } = 0;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation
    public SubUnit SubUnit { get; set; } = default!;

    /// <summary>
    /// Only populated when InputType is Select or MultiSelect
    /// </summary>
    public ICollection<SubUnitOptionSelection> Selections { get; set; } = [];
}

public class SubUnitOptionSelection
{
    public int Id { get; set; }

    public int SubUnitOptionId { get; set; }

    [Required, MaxLength(200)]
    public string Value { get; set; } = string.Empty;

    public int DisplayOrder { get; set; } = 0;

    // Navigation
    public SubUnitOption SubUnitOption { get; set; } = default!;
}


