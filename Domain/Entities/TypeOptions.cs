using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Domain.Entities;


/// <summary>
/// An option template defined once on a UnitType.
/// Every Unit of that type inherits this option and can supply a value.
/// </summary>
public class UnitTypeOption
{
    public int Id { get; set; }

    public int UnitTypeId { get; set; }

    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    public OptionInputType InputType { get; set; }

    public bool IsRequired { get; set; } = false;

    public int DisplayOrder { get; set; } = 0;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation
    public UnitType UnitType { get; set; } = default!;

    /// <summary>Only populated for Select / MultiSelect input types.</summary>
    public ICollection<UnitTypeOptionSelection> Selections { get; set; } = [];

    /// <summary>Values that individual Units have stored for this option.</summary>
    public ICollection<UnitOptionValue> UnitValues { get; set; } = [];
}

/// <summary>
/// Predefined choice for a UnitTypeOption with InputType = Select | MultiSelect.
/// </summary>
public class UnitTypeOptionSelection
{
    public int Id { get; set; }

    public int UnitTypeOptionId { get; set; }

    [Required, MaxLength(200)]
    public string Value { get; set; } = string.Empty;

    public int DisplayOrder { get; set; } = 0;

    // Navigation
    public UnitTypeOption UnitTypeOption { get; set; } = default!;
}

/// <summary>
/// The value a specific Unit has provided for one UnitTypeOption.
/// One row per (Unit, UnitTypeOption) pair for scalar types;
/// one row per selected choice for MultiSelect.
/// </summary>
public class UnitOptionValue
{
    public int Id { get; set; }

    public int UnitId { get; set; }

    public int UnitTypeOptionId { get; set; }

    /// <summary>
    /// Stored value. For MultiSelect this column holds a single chosen value
    /// (multiple rows exist for the same UnitId + UnitTypeOptionId pair).
    /// For Checkbox: "true"/"false".
    /// </summary>
    [Required, MaxLength(2000)]
    public string Value { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation
    public Unit Unit { get; set; } = default!;
    public UnitTypeOption UnitTypeOption { get; set; } = default!;
}


// ─────────────────────────────────────────────────────────────────────────────
// SUBUNIT TYPE OPTIONS  (option *definitions* live on SubUnitTypee, not SubUnit)
// ─────────────────────────────────────────────────────────────────────────────

/// <summary>
/// An option template defined once on a SubUnitTypee.
/// Every SubUnit of that type inherits this option and can supply a value.
/// </summary>
public class SubUnitTypeOption
{
    public int Id { get; set; }

    public int SubUnitTypeId { get; set; }

    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    public OptionInputType InputType { get; set; }

    public bool IsRequired { get; set; } = false;

    public int DisplayOrder { get; set; } = 0;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation
    public SubUnitTypee SubUnitType { get; set; } = default!;

    /// <summary>Only populated for Select / MultiSelect input types.</summary>
    public ICollection<SubUnitTypeOptionSelection> Selections { get; set; } = [];

    /// <summary>Values that individual SubUnits have stored for this option.</summary>
    public ICollection<SubUnitOptionValue> SubUnitValues { get; set; } = [];
}

/// <summary>
/// Predefined choice for a SubUnitTypeOption with InputType = Select | MultiSelect.
/// </summary>
public class SubUnitTypeOptionSelection
{
    public int Id { get; set; }

    public int SubUnitTypeOptionId { get; set; }

    [Required, MaxLength(200)]
    public string Value { get; set; } = string.Empty;

    public int DisplayOrder { get; set; } = 0;

    // Navigation
    public SubUnitTypeOption SubUnitTypeOption { get; set; } = default!;
}

/// <summary>
/// The value a specific SubUnit has provided for one SubUnitTypeOption.
/// One row per (SubUnit, SubUnitTypeOption) pair for scalar types;
/// one row per selected choice for MultiSelect.
/// </summary>
public class SubUnitOptionValue
{
    public int Id { get; set; }

    public int SubUnitId { get; set; }

    public int SubUnitTypeOptionId { get; set; }

    /// <summary>
    /// Stored value. For MultiSelect this column holds a single chosen value
    /// (multiple rows exist for the same SubUnitId + SubUnitTypeOptionId pair).
    /// For Checkbox: "true"/"false".
    /// </summary>
    [Required, MaxLength(2000)]
    public string Value { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation
    public SubUnit SubUnit { get; set; } = default!;
    public SubUnitTypeOption SubUnitTypeOption { get; set; } = default!;
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