using Domain.Entities;

namespace Application.Contracts.Options;

// ─────────────────────────────────────────────────────────────────────────────
// SHARED PRIMITIVES
// ─────────────────────────────────────────────────────────────────────────────

public class TypeOptionSelectionDto
{
    public int? Id { get; set; }          // null on create
    public string Value { get; set; } = string.Empty;
    public int DisplayOrder { get; set; } = 0;
}

// ─────────────────────────────────────────────────────────────────────────────
// UNIT TYPE OPTION MANAGEMENT  (admin / platform)
// ─────────────────────────────────────────────────────────────────────────────

public class UnitTypeOptionResponse
{
    public int Id { get; set; }
    public int UnitTypeId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string InputType { get; set; } = string.Empty;
    public bool IsRequired { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    /// <summary>Only populated for Select / MultiSelect.</summary>
    public List<TypeOptionSelectionDto>? Selections { get; set; }
}

public class CreateUnitTypeOptionRequest
{
    public string Name { get; set; } = string.Empty;
    public OptionInputType InputType { get; set; }
    public bool IsRequired { get; set; } = false;
    public int DisplayOrder { get; set; } = 0;

    /// <summary>Required when InputType is Select or MultiSelect.</summary>
    public List<TypeOptionSelectionDto>? Selections { get; set; }
}

public class UpdateUnitTypeOptionRequest
{
    public string? Name { get; set; }
    public OptionInputType? InputType { get; set; }
    public bool? IsRequired { get; set; }
    public int? DisplayOrder { get; set; }
    public bool? IsActive { get; set; }

    /// <summary>When supplied the entire selections list is replaced atomically.</summary>
    public List<TypeOptionSelectionDto>? Selections { get; set; }
}

// ─────────────────────────────────────────────────────────────────────────────
// SUBUNIT TYPE OPTION MANAGEMENT  (admin / platform)
// ─────────────────────────────────────────────────────────────────────────────

public class SubUnitTypeOptionResponse
{
    public int Id { get; set; }
    public int SubUnitTypeId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string InputType { get; set; } = string.Empty;
    public bool IsRequired { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    /// <summary>Only populated for Select / MultiSelect.</summary>
    public List<TypeOptionSelectionDto>? Selections { get; set; }
}

public class CreateSubUnitTypeOptionRequest
{
    public string Name { get; set; } = string.Empty;
    public OptionInputType InputType { get; set; }
    public bool IsRequired { get; set; } = false;
    public int DisplayOrder { get; set; } = 0;

    /// <summary>Required when InputType is Select or MultiSelect.</summary>
    public List<TypeOptionSelectionDto>? Selections { get; set; }
}

public class UpdateSubUnitTypeOptionRequest
{
    public string? Name { get; set; }
    public OptionInputType? InputType { get; set; }
    public bool? IsRequired { get; set; }
    public int? DisplayOrder { get; set; }
    public bool? IsActive { get; set; }

    /// <summary>When supplied the entire selections list is replaced atomically.</summary>
    public List<TypeOptionSelectionDto>? Selections { get; set; }
}

// ─────────────────────────────────────────────────────────────────────────────
// UNIT OPTION VALUES  (hotel admin sets the actual value for their unit)
// ─────────────────────────────────────────────────────────────────────────────

/// <summary>
/// A single option with its current value(s) as stored for a specific Unit.
/// </summary>
public class UnitOptionValueResponse
{
    public int UnitTypeOptionId { get; set; }
    public string OptionName { get; set; } = string.Empty;
    public string InputType { get; set; } = string.Empty;
    public bool IsRequired { get; set; }

    /// <summary>
    /// For scalar types (Text, Number, TextArea, Checkbox) this has exactly one item.
    /// For MultiSelect it may have many.
    /// Empty list means the hotel admin has not provided a value yet.
    /// </summary>
    public List<string> Values { get; set; } = [];

    /// <summary>Available selections for Select / MultiSelect options.</summary>
    public List<TypeOptionSelectionDto>? AvailableSelections { get; set; }
}

/// <summary>One entry in a batch save operation.</summary>
public class UnitOptionValueInput
{
    public int UnitTypeOptionId { get; set; }

    /// <summary>
    /// For scalar types supply a single-element list.
    /// For MultiSelect supply all chosen values.
    /// Empty list / null clears the value.
    /// </summary>
    public List<string> Values { get; set; } = [];
}

/// <summary>Saves multiple option values for a Unit in one call.</summary>
public class SaveUnitOptionValuesRequest
{
    public List<UnitOptionValueInput> Options { get; set; } = [];
}

// ─────────────────────────────────────────────────────────────────────────────
// SUBUNIT OPTION VALUES  (hotel admin sets the actual value for their subunit)
// ─────────────────────────────────────────────────────────────────────────────

/// <summary>
/// A single option with its current value(s) as stored for a specific SubUnit.
/// </summary>
public class SubUnitOptionValueResponse
{
    public int SubUnitTypeOptionId { get; set; }
    public string OptionName { get; set; } = string.Empty;
    public string InputType { get; set; } = string.Empty;
    public bool IsRequired { get; set; }

    /// <summary>
    /// For scalar types (Text, Number, TextArea, Checkbox) this has exactly one item.
    /// For MultiSelect it may have many.
    /// </summary>
    public List<string> Values { get; set; } = [];

    /// <summary>Available selections for Select / MultiSelect options.</summary>
    public List<TypeOptionSelectionDto>? AvailableSelections { get; set; }
}

/// <summary>One entry in a batch save operation.</summary>
public class SubUnitOptionValueInput
{
    public int SubUnitTypeOptionId { get; set; }

    /// <summary>
    /// For scalar types supply a single-element list.
    /// For MultiSelect supply all chosen values.
    /// Empty list / null clears the value.
    /// </summary>
    public List<string> Values { get; set; } = [];
}

/// <summary>Saves multiple option values for a SubUnit in one call.</summary>
public class SaveSubUnitOptionValuesRequest
{
    public List<SubUnitOptionValueInput> Options { get; set; } = [];
}