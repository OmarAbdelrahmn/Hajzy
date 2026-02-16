using Application.Contracts.Policy;
using Domain;
using Domain.Entities;

namespace Application.Service.Policy;

public class EnumService : IEnumService
{
    public AllEnumsResponse GetAllEnums()
    {
        return new AllEnumsResponse(
            GetBookingStatuses(),
            GetPaymentStatuses(),
            GetPaymentMethods(),
            GetNotificationTypes(),
            GetNotificationPriorities(),
            GetNotificationTargets(),
            GetCouponTypes(),
            GetPricingRuleTypes(),
            GetPricingAdjustmentTypes(),
            GetLoyaltyTiers(),
            GetAmenityCategories(),
            GetAmenityNames(),
            GetGeneralPolicyCategories(),
            GetGeneralPolicyNames(),
            GetUnavailabilityReasons(),
            GetBedTypes(),
            GetUnitImageTypes(),
            GetSubUnitImageTypes(),
            GetReviewImageTypes(),
            GetDepartmentImageTypes()
        );
    }

    public EnumGroupResponse GetBookingStatuses()
    {
        return new EnumGroupResponse(
            "BookingStatus",
            Enum.GetValues(typeof(BookingStatus))
                .Cast<BookingStatus>()
                .Select(e => new EnumValueResponse(
                    (int)e,
                    e.ToString(),
                    FormatEnumName(e.ToString())))
                .ToList()
        );
    }

    public EnumGroupResponse GetPaymentStatuses()
    {
        return new EnumGroupResponse(
            "PaymentStatus",
            Enum.GetValues(typeof(PaymentStatus))
                .Cast<PaymentStatus>()
                .Select(e => new EnumValueResponse(
                    (int)e,
                    e.ToString(),
                    FormatEnumName(e.ToString())))
                .ToList()
        );
    }

    public EnumGroupResponse GetPaymentMethods()
    {
        return new EnumGroupResponse(
            "PaymentMethod",
            Enum.GetValues(typeof(PaymentMethod))
                .Cast<PaymentMethod>()
                .Select(e => new EnumValueResponse(
                    (int)e,
                    e.ToString(),
                    FormatEnumName(e.ToString())))
                .ToList()
        );
    }

    public EnumGroupResponse GetNotificationTypes()
    {
        return new EnumGroupResponse(
            "NotificationType",
            Enum.GetValues(typeof(NotificationType))
                .Cast<NotificationType>()
                .Select(e => new EnumValueResponse(
                    (int)e,
                    e.ToString(),
                    FormatEnumName(e.ToString())))
                .ToList()
        );
    }

    public EnumGroupResponse GetNotificationPriorities()
    {
        return new EnumGroupResponse(
            "NotificationPriority",
            Enum.GetValues(typeof(NotificationPriority))
                .Cast<NotificationPriority>()
                .Select(e => new EnumValueResponse(
                    (int)e,
                    e.ToString(),
                    FormatEnumName(e.ToString())))
                .ToList()
        );
    }

    public EnumGroupResponse GetNotificationTargets()
    {
        return new EnumGroupResponse(
            "NotificationTarget",
            Enum.GetValues(typeof(NotificationTarget))
                .Cast<NotificationTarget>()
                .Select(e => new EnumValueResponse(
                    (int)e,
                    e.ToString(),
                    FormatEnumName(e.ToString())))
                .ToList()
        );
    }

    public EnumGroupResponse GetCouponTypes()
    {
        return new EnumGroupResponse(
            "CouponType",
            Enum.GetValues(typeof(CouponType))
                .Cast<CouponType>()
                .Select(e => new EnumValueResponse(
                    (int)e,
                    e.ToString(),
                    FormatEnumName(e.ToString())))
                .ToList()
        );
    }

    public EnumGroupResponse GetPricingRuleTypes()
    {
        return new EnumGroupResponse(
            "PricingRuleType",
            Enum.GetValues(typeof(PricingRuleType))
                .Cast<PricingRuleType>()
                .Select(e => new EnumValueResponse(
                    (int)e,
                    e.ToString(),
                    FormatEnumName(e.ToString())))
                .ToList()
        );
    }

    public EnumGroupResponse GetPricingAdjustmentTypes()
    {
        return new EnumGroupResponse(
            "PricingAdjustmentType",
            Enum.GetValues(typeof(PricingAdjustmentType))
                .Cast<PricingAdjustmentType>()
                .Select(e => new EnumValueResponse(
                    (int)e,
                    e.ToString(),
                    FormatEnumName(e.ToString())))
                .ToList()
        );
    }

    public EnumGroupResponse GetLoyaltyTiers()
    {
        return new EnumGroupResponse(
            "LoyaltyTier",
            Enum.GetValues(typeof(LoyaltyTier))
                .Cast<LoyaltyTier>()
                .Select(e => new EnumValueResponse(
                    (int)e,
                    e.ToString(),
                    FormatEnumName(e.ToString())))
                .ToList()
        );
    }


    public EnumGroupResponse GetAmenityCategories()
    {
        return new EnumGroupResponse(
            "AmenityCategory",
            Enum.GetValues(typeof(AmenityCategory))
                .Cast<AmenityCategory>()
                .Select(e => new EnumValueResponse(
                    (int)e,
                    e.ToString(),
                    FormatEnumName(e.ToString())))
                .ToList()
        );
    }

    public EnumGroupResponse GetAmenityNames()
    {
        return new EnumGroupResponse(
            "AmenityName",
            Enum.GetValues(typeof(AmenityName))
                .Cast<AmenityName>()
                .Select(e => new EnumValueResponse(
                    (int)e,
                    e.ToString(),
                    FormatEnumName(e.ToString())))
                .ToList()
        );
    }

    public EnumGroupResponse GetGeneralPolicyCategories()
    {
        return new EnumGroupResponse(
            "GeneralPolicyCategory",
            Enum.GetValues(typeof(GeneralPolicyCategory))
                .Cast<GeneralPolicyCategory>()
                .Select(e => new EnumValueResponse(
                    (int)e,
                    e.ToString(),
                    FormatEnumName(e.ToString())))
                .ToList()
        );
    }

    public EnumGroupResponse GetGeneralPolicyNames()
    {
        return new EnumGroupResponse(
            "GeneralPolicyName",
            Enum.GetValues(typeof(GeneralPolicyName))
                .Cast<GeneralPolicyName>()
                .Select(e => new EnumValueResponse(
                    (int)e,
                    e.ToString(),
                    FormatEnumName(e.ToString())))
                .ToList()
        );
    }

    public EnumGroupResponse GetUnavailabilityReasons()
    {
        return new EnumGroupResponse(
            "UnavailabilityReason",
            Enum.GetValues(typeof(UnavailabilityReason))
                .Cast<UnavailabilityReason>()
                .Select(e => new EnumValueResponse(
                    (int)e,
                    e.ToString(),
                    FormatEnumName(e.ToString())))
                .ToList()
        );
    }

    public EnumGroupResponse GetBedTypes()
    {
        return new EnumGroupResponse(
            "BedType",
            Enum.GetValues(typeof(BedType))
                .Cast<BedType>()
                .Select(e => new EnumValueResponse(
                    (int)e,
                    e.ToString(),
                    FormatEnumName(e.ToString())))
                .ToList()
        );
    }

    public EnumGroupResponse GetUnitImageTypes()
    {
        return new EnumGroupResponse(
            "UnitImageType",
            Enum.GetValues(typeof(UnitImageType))
                .Cast<UnitImageType>()
                .Select(e => new EnumValueResponse(
                    (int)e,
                    e.ToString(),
                    FormatEnumName(e.ToString())))
                .ToList()
        );
    }

    public EnumGroupResponse GetSubUnitImageTypes()
    {
        return new EnumGroupResponse(
            "SubUnitImageType",
            Enum.GetValues(typeof(SubUnitImageType))
                .Cast<SubUnitImageType>()
                .Select(e => new EnumValueResponse(
                    (int)e,
                    e.ToString(),
                    FormatEnumName(e.ToString())))
                .ToList()
        );
    }

    public EnumGroupResponse GetReviewImageTypes()
    {
        return new EnumGroupResponse(
            "ReviewImageType",
            Enum.GetValues(typeof(ReviewImageType))
                .Cast<ReviewImageType>()
                .Select(e => new EnumValueResponse(
                    (int)e,
                    e.ToString(),
                    FormatEnumName(e.ToString())))
                .ToList()
        );
    }

    public EnumGroupResponse GetDepartmentImageTypes()
    {
        return new EnumGroupResponse(
            "DepartmentImageType",
            Enum.GetValues(typeof(DepartmentImageType))
                .Cast<DepartmentImageType>()
                .Select(e => new EnumValueResponse(
                    (int)e,
                    e.ToString(),
                    FormatEnumName(e.ToString())))
                .ToList()
        );
    }

    // Helper method to format enum names to be more readable
    private static string FormatEnumName(string enumName)
    {
        // Add spaces before capital letters
        var result = System.Text.RegularExpressions.Regex.Replace(
            enumName,
            "(\\B[A-Z])",
            " $1"
        );
        return result;
    }

    public EnumGroupResponse GetSubUnitTypes()
    {
        throw new NotImplementedException();
    }
}
