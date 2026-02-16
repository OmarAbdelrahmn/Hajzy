namespace Domain.Entities;

public class BookingCoupon
{
    public int Id { get; set; }
    public int BookingId { get; set; }
    public int CouponId { get; set; }
    public decimal DiscountApplied { get; set; }
    public DateTime AppliedAt { get; set; } = DateTime.UtcNow;

    public Booking Booking { get; set; } = default!;
    public Coupon Coupon { get; set; } = default!;
}