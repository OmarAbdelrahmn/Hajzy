using Application.Service.Policy;
using Microsoft.AspNetCore.Mvc;

namespace Hajzzy.Controllers;

[Route("api/[controller]")]
[ApiController]
public class EnumController(IEnumService service) : ControllerBase
{
    private readonly IEnumService _service = service;

    /// <summary>
    /// Get all available enums in the system
    /// </summary>
    [HttpGet("all")]

    public IActionResult GetAll()
    {
        var result = _service.GetAllEnums();
        return Ok(result);
    }

    /// <summary>
    /// Get booking status enum values
    /// </summary>
    [HttpGet("booking-statuses")]

    public IActionResult GetBookingStatuses()
    {
        var result = _service.GetBookingStatuses();
        return Ok(result);
    }

    /// <summary>
    /// Get payment status enum values
    /// </summary>
    [HttpGet("payment-statuses")]

    public IActionResult GetPaymentStatuses()
    {
        var result = _service.GetPaymentStatuses();
        return Ok(result);
    }

    /// <summary>
    /// Get payment method enum values
    /// </summary>
    [HttpGet("payment-methods")]

    public IActionResult GetPaymentMethods()
    {
        var result = _service.GetPaymentMethods();
        return Ok(result);
    }

    /// <summary>
    /// Get notification type enum values
    /// </summary>
    [HttpGet("notification-types")]

    public IActionResult GetNotificationTypes()
    {
        var result = _service.GetNotificationTypes();
        return Ok(result);
    }

    /// <summary>
    /// Get notification priority enum values
    /// </summary>
    [HttpGet("notification-priorities")]

    public IActionResult GetNotificationPriorities()
    {
        var result = _service.GetNotificationPriorities();
        return Ok(result);
    }

    /// <summary>
    /// Get notification target enum values
    /// </summary>
    [HttpGet("notification-targets")]

    public IActionResult GetNotificationTargets()
    {
        var result = _service.GetNotificationTargets();
        return Ok(result);
    }

    /// <summary>
    /// Get coupon type enum values
    /// </summary>
    [HttpGet("coupon-types")]

    public IActionResult GetCouponTypes()
    {
        var result = _service.GetCouponTypes();
        return Ok(result);
    }

    /// <summary>
    /// Get pricing rule type enum values
    /// </summary>
    [HttpGet("pricing-rule-types")]

    public IActionResult GetPricingRuleTypes()
    {
        var result = _service.GetPricingRuleTypes();
        return Ok(result);
    }

    /// <summary>
    /// Get pricing adjustment type enum values
    /// </summary>
    [HttpGet("pricing-adjustment-types")]

    public IActionResult GetPricingAdjustmentTypes()
    {
        var result = _service.GetPricingAdjustmentTypes();
        return Ok(result);
    }

    /// <summary>
    /// Get loyalty tier enum values
    /// </summary>
    [HttpGet("loyalty-tiers")]

    public IActionResult GetLoyaltyTiers()
    {
        var result = _service.GetLoyaltyTiers();
        return Ok(result);
    }

    /// <summary>
    /// Get sub unit type enum values (Room types)
    /// </summary>
    [HttpGet("subunit-types")]

    public IActionResult GetSubUnitTypes()
    {
        var result = _service.GetSubUnitTypes();
        return Ok(result);
    }

    /// <summary>
    /// Get amenity category enum values
    /// </summary>
    [HttpGet("amenity-categories")]

    public IActionResult GetAmenityCategories()
    {
        var result = _service.GetAmenityCategories();
        return Ok(result);
    }

    /// <summary>
    /// Get amenity name enum values
    /// </summary>
    [HttpGet("amenity-names")]

    public IActionResult GetAmenityNames()
    {
        var result = _service.GetAmenityNames();
        return Ok(result);
    }

    /// <summary>
    /// Get general policy category enum values
    /// </summary>
    [HttpGet("general-policy-categories")]

    public IActionResult GetGeneralPolicyCategories()
    {
        var result = _service.GetGeneralPolicyCategories();
        return Ok(result);
    }

    /// <summary>
    /// Get general policy name enum values
    /// </summary>
    [HttpGet("general-policy-names")]

    public IActionResult GetGeneralPolicyNames()
    {
        var result = _service.GetGeneralPolicyNames();
        return Ok(result);
    }

    /// <summary>
    /// Get unavailability reason enum values
    /// </summary>
    [HttpGet("unavailability-reasons")]

    public IActionResult GetUnavailabilityReasons()
    {
        var result = _service.GetUnavailabilityReasons();
        return Ok(result);
    }

    /// <summary>
    /// Get bed type enum values
    /// </summary>
    [HttpGet("bed-types")]

    public IActionResult GetBedTypes()
    {
        var result = _service.GetBedTypes();
        return Ok(result);
    }

    /// <summary>
    /// Get unit image type enum values
    /// </summary>
    [HttpGet("unit-image-types")]

    public IActionResult GetUnitImageTypes()
    {
        var result = _service.GetUnitImageTypes();
        return Ok(result);
    }

    /// <summary>
    /// Get sub unit image type enum values
    /// </summary>
    [HttpGet("subunit-image-types")]

    public IActionResult GetSubUnitImageTypes()
    {
        var result = _service.GetSubUnitImageTypes();
        return Ok(result);
    }

    /// <summary>
    /// Get review image type enum values
    /// </summary>
    [HttpGet("review-image-types")]

    public IActionResult GetReviewImageTypes()
    {
        var result = _service.GetReviewImageTypes();
        return Ok(result);
    }

    /// <summary>
    /// Get department image type enum values
    /// </summary>
    [HttpGet("department-image-types")]

    public IActionResult GetDepartmentImageTypes()
    {
        var result = _service.GetDepartmentImageTypes();
        return Ok(result);
    }
}
