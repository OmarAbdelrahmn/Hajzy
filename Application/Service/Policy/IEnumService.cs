using Application.Contracts.Policy;

namespace Application.Service.Policy;

public interface IEnumService
{
    AllEnumsResponse GetAllEnums();
    EnumGroupResponse GetBookingStatuses();
    EnumGroupResponse GetPaymentStatuses();
    EnumGroupResponse GetPaymentMethods();
    EnumGroupResponse GetNotificationTypes();
    EnumGroupResponse GetNotificationPriorities();
    EnumGroupResponse GetNotificationTargets();
    EnumGroupResponse GetCouponTypes();
    EnumGroupResponse GetPricingRuleTypes();
    EnumGroupResponse GetPricingAdjustmentTypes();
    EnumGroupResponse GetLoyaltyTiers();
    EnumGroupResponse GetSubUnitTypes();
    EnumGroupResponse GetAmenityCategories();
    EnumGroupResponse GetAmenityNames();
    EnumGroupResponse GetGeneralPolicyCategories();
    EnumGroupResponse GetGeneralPolicyNames();
    EnumGroupResponse GetUnavailabilityReasons();
    EnumGroupResponse GetBedTypes();
    EnumGroupResponse GetUnitImageTypes();
    EnumGroupResponse GetSubUnitImageTypes();
    EnumGroupResponse GetReviewImageTypes();
    EnumGroupResponse GetDepartmentImageTypes();
}
