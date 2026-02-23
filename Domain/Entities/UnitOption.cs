using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Domain.Entities;


public class UnitOption
{
    public int Id { get; set; }

    public int UnitId { get; set; }

    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    public OptionInputType InputType { get; set; }

    public bool IsRequired { get; set; } = false;

    public int DisplayOrder { get; set; } = 0;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation
    public Unit Unit { get; set; } = default!;

    /// <summary>
    /// Only populated when InputType is Select or MultiSelect
    /// </summary>
    public ICollection<UnitOptionSelection> Selections { get; set; } = [];
}

public class UnitOptionSelection
{
    public int Id { get; set; }

    public int UnitOptionId { get; set; }

    [Required, MaxLength(200)]
    public string Value { get; set; } = string.Empty;

    public int DisplayOrder { get; set; } = 0;

    // Navigation
    public UnitOption UnitOption { get; set; } = default!;
}

public enum OptionInputType
{
    Text = 1,
    Number = 2,
    TextArea = 3,
    Checkbox = 4,
    Select = 5,
    MultiSelect = 6
}