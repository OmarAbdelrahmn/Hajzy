using Domain.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Application.Contracts.Options;


public class OptionSelectionRequest
{
    [Required, MaxLength(200)]
    public string Value { get; set; } = string.Empty;

    public int DisplayOrder { get; set; } = 0;
}

public class OptionSelectionResponse
{
    public int Id { get; set; }
    public string Value { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
}

// ─────────────────────────────────────────────
// UNIT OPTION
// ─────────────────────────────────────────────

public class CreateUnitOptionRequest
{
    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public OptionInputType InputType { get; set; }

    public bool IsRequired { get; set; } = false;

    public int DisplayOrder { get; set; } = 0;

    /// <summary>
    /// Required when InputType is Select or MultiSelect.
    /// Must be null / empty for other types.
    /// </summary>
    public List<OptionSelectionRequest>? Selections { get; set; }
}

public class UpdateUnitOptionRequest
{
    [MaxLength(200)]
    public string? Name { get; set; }

    public OptionInputType? InputType { get; set; }

    public bool? IsRequired { get; set; }

    public int? DisplayOrder { get; set; }

    public bool? IsActive { get; set; }

    /// <summary>
    /// When provided the entire selections list is replaced.
    /// Pass an empty list to clear all selections.
    /// </summary>
    public List<OptionSelectionRequest>? Selections { get; set; }
}

public class UnitOptionResponse
{
    public int Id { get; set; }
    public int UnitId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string InputType { get; set; } = string.Empty;
    public bool IsRequired { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    /// <summary>Null unless InputType is Select or MultiSelect.</summary>
    public List<OptionSelectionResponse>? Selections { get; set; }
}

// ─────────────────────────────────────────────
// SUBUNIT OPTION
// ─────────────────────────────────────────────

public class CreateSubUnitOptionRequest
{
    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public OptionInputType InputType { get; set; }

    public bool IsRequired { get; set; } = false;

    public int DisplayOrder { get; set; } = 0;

    /// <summary>
    /// Required when InputType is Select or MultiSelect.
    /// Must be null / empty for other types.
    /// </summary>
    public List<OptionSelectionRequest>? Selections { get; set; }
}

public class UpdateSubUnitOptionRequest
{
    [MaxLength(200)]
    public string? Name { get; set; }

    public OptionInputType? InputType { get; set; }

    public bool? IsRequired { get; set; }

    public int? DisplayOrder { get; set; }

    public bool? IsActive { get; set; }

    /// <summary>
    /// When provided the entire selections list is replaced.
    /// Pass an empty list to clear all selections.
    /// </summary>
    public List<OptionSelectionRequest>? Selections { get; set; }
}

public class SubUnitOptionResponse
{
    public int Id { get; set; }
    public int SubUnitId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string InputType { get; set; } = string.Empty;
    public bool IsRequired { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    /// <summary>Null unless InputType is Select or MultiSelect.</summary>
    public List<OptionSelectionResponse>? Selections { get; set; }
}