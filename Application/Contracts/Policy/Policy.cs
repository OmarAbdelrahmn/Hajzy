using Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Contracts.Policy;

internal class Policy
{
}
public record CancellationPolicyResponse(
    int Id,
    string Name,
    string Description,
    int FullRefundDays,
    int PartialRefundDays,
    decimal PartialRefundPercentage,
    bool IsActive,
    bool IsDefault,
    DateTime CreatedAt,
    int UnitsCount
);

public record CancellationPolicyDetailsResponse(
    int Id,
    string Name,
    string Description,
    int FullRefundDays,
    int PartialRefundDays,
    decimal PartialRefundPercentage,
    bool IsActive,
    bool IsDefault,
    DateTime CreatedAt,
    List<UnitBasicInfo> AssignedUnits
);

public record UnitBasicInfo(
    int Id,
    string Name,
    string Address
);

// Request DTOs
public record CreateCancellationPolicyRequest(
    string Name,
    string Description,
    int FullRefundDays,
    int PartialRefundDays,
    decimal PartialRefundPercentage,
    bool IsActive = true,
    bool IsDefault = false
);

public record UpdateCancellationPolicyRequest(
    string? Name,
    string? Description,
    int? FullRefundDays,
    int? PartialRefundDays,
    decimal? PartialRefundPercentage,
    bool? IsActive,
    bool? IsDefault
);


// Response DTOs
public record GeneralPolicyResponse(
    int Id,
    string Title,
    string Description,
    GeneralPolicyName PolicyType,
    GeneralPolicyCategory? PolicyCategory,
    string? CustomPolicyName,
    int? CancellationPolicyId,
    string? CancellationPolicyName,
    bool IsMandatory,
    bool IsHighlighted,
    bool IsActive,
    int? UnitId,
    string? UnitName,
    int? SubUnitId,
    string? SubUnitRoomNumber,
    string Scope // "Global", "Unit", "SubUnit"
);

public record GeneralPolicyDetailsResponse(
    int Id,
    string Title,
    string Description,
    GeneralPolicyName PolicyType,
    GeneralPolicyCategory? PolicyCategory,
    string? CustomPolicyName,
    CancellationPolicyBasicInfo? CancellationPolicy,
    bool IsMandatory,
    bool IsHighlighted,
    bool IsActive,
    UnitBasicInfo? Unit,
    SubUnitBasicInfo? SubUnit,
    string Scope
);

public record CancellationPolicyBasicInfo(
    int Id,
    string Name,
    int FullRefundDays,
    int PartialRefundDays,
    decimal PartialRefundPercentage
);

public record SubUnitBasicInfo(
    int Id,
    string RoomNumber,
    string Type
);

// Request DTOs
public record CreateGeneralPolicyRequest(
    string Title,
    string Description,
    GeneralPolicyName PolicyType,
    GeneralPolicyCategory? PolicyCategory = null,
    string? CustomPolicyName = null,
    int? CancellationPolicyId = null,
    bool IsMandatory = false,
    bool IsHighlighted = false,
    bool IsActive = true,
    int? UnitId = null,
    int? SubUnitId = null
);

public record UpdateGeneralPolicyRequest(
    string? Title,
    string? Description,
    GeneralPolicyName? PolicyType,
    GeneralPolicyCategory? PolicyCategory,
    string? CustomPolicyName,
    int? CancellationPolicyId,
    bool? IsMandatory,
    bool? IsHighlighted,
    bool? IsActive
);

public record AttachPolicyToUnitRequest(
    int PolicyId,
    int UnitId
);

public record AttachPolicyToSubUnitRequest(
    int PolicyId,
    int SubUnitId
);

public record CreateCustomPolicyForUnitRequest(
    int UnitId,
    string Title,
    string Description,
    GeneralPolicyName PolicyType,
    GeneralPolicyCategory? PolicyCategory = null,
    string? CustomPolicyName = null,
    int? CancellationPolicyId = null,
    bool IsMandatory = false,
    bool IsHighlighted = false,
    bool IsActive = true
);

public record PolicyFilterRequest(
    GeneralPolicyName? PolicyType = null,
    GeneralPolicyCategory? PolicyCategory = null,
    bool? IsActive = null,
    bool? IsMandatory = null,
    int? UnitId = null,
    int? SubUnitId = null,
    string? Scope = null // "Global", "Unit", "SubUnit"
);


public record EnumValueResponse(
    int Value,
    string Name,
    string DisplayName
);

public record EnumGroupResponse(
    string EnumName,
    List<EnumValueResponse> Values
);

public record AllEnumsResponse(
    EnumGroupResponse BookingStatuses,
    EnumGroupResponse PaymentStatuses,
    EnumGroupResponse PaymentMethods,
    EnumGroupResponse NotificationTypes,
    EnumGroupResponse NotificationPriorities,
    EnumGroupResponse NotificationTargets,
    EnumGroupResponse CouponTypes,
    EnumGroupResponse PricingRuleTypes,
    EnumGroupResponse PricingAdjustmentTypes,
    EnumGroupResponse LoyaltyTiers,
    EnumGroupResponse SubUnitTypes,
    EnumGroupResponse AmenityCategories,
    EnumGroupResponse AmenityNames,
    EnumGroupResponse GeneralPolicyCategories,
    EnumGroupResponse GeneralPolicyNames,
    EnumGroupResponse UnavailabilityReasons,
    EnumGroupResponse BedTypes,
    EnumGroupResponse UnitImageTypes,
    EnumGroupResponse SubUnitImageTypes,
    EnumGroupResponse ReviewImageTypes,
    EnumGroupResponse DepartmentImageTypes
);