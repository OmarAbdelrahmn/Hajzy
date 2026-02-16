using Application.Abstraction;
using Application.Helpers;
using Domain;
using Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Notifications;

public class EmailNotificationService(
    ApplicationDbcontext context,
    IEmailSender emailSender,
    IHttpContextAccessor httpContextAccessor,
    ILogger<EmailNotificationService> logger,
    UserManager<ApplicationUser> manager) : INotinficationService
{
    private readonly ApplicationDbcontext _context = context;
    private readonly IEmailSender _emailSender = emailSender;
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
    private readonly ILogger<EmailNotificationService> _logger = logger;
    private readonly UserManager<ApplicationUser> manager = manager;

    private string GetOriginUrl()
    {
        return _httpContextAccessor.HttpContext?.Request.Headers.Origin.ToString()
            ?? "https://hujjzy.com";
    }

    public async Task SendPharmacyNotification()
    {
        IEnumerable<string> News = [
            "hello"
            ];



        var users = await manager.Users.ToListAsync();

        var origin = httpContextAccessor.HttpContext?.Request.Headers.Origin;

        foreach (var New in News)
        {
            foreach (var user in users)
            {
                var placeholders = new Dictionary<string, string>
                {
                    { "{{name}}", user.FullName?? "hello" },
                    { "{{pollTill}}", New },
                    { "{{endDate}}",$"{DateTime.UtcNow.AddDays(3)} "},
                    { "{{url}}", $"{origin}/pharmacy/start/{New}" }
                };

                var body = EmailBodyBuilder.GenerateEmailBody("hajzzy.Notification", placeholders);

                await emailSender.SendEmailAsync(user.Email!, $"📣 hajzzy: hotel - {New}", body);
            }
        }
    }

    #region BOOKING EMAILS

    public async Task<Result> SendBookingConfirmationEmailAsync(int bookingId)
    {
        try
        {
            var booking = await _context.Bookings
                .Include(b => b.Unit)
                .Include(b => b.User)
                .Include(b => b.BookingRooms)
                    .ThenInclude(br => br.Room)
                .FirstOrDefaultAsync(b => b.Id == bookingId);

            if (booking == null || booking.User == null)
                return Result.Failure(new Error("BookingNotFound", "Booking not found", 404));

            var origin = GetOriginUrl();

            // Build room details string
            var roomDetails = string.Join(", ",
                booking.BookingRooms.Select(br =>
                    $"{br.Room.RoomNumber} (${br.PricePerNight}/night)"));

            if (string.IsNullOrEmpty(roomDetails))
                roomDetails = "Full unit booking";

            var emailBody = EmailBodyBuilder.GenerateEmailBody("BookingConfirmation",
                new Dictionary<string, string>
                {
                    { "{{name}}", booking.User.FullName ?? "Guest" },
                    { "{{booking_number}}", booking.BookingNumber },
                    { "{{status}}", booking.Status.ToString() },
                    { "{{unit_name}}", booking.Unit?.Name ?? "N/A" },
                    { "{{unit_address}}", booking.Unit?.Address ?? "N/A" },
                    { "{{check_in_date}}", booking.CheckInDate.ToString("MMMM dd, yyyy") },
                    { "{{check_out_date}}", booking.CheckOutDate.ToString("MMMM dd, yyyy") },
                    { "{{number_of_nights}}", booking.NumberOfNights.ToString() },
                    { "{{number_of_guests}}", booking.NumberOfGuests.ToString() },
                    { "{{room_details}}", roomDetails },
                    { "{{total_price}}", booking.TotalPrice.ToString("F2") },
                    { "{{dashboard_url}}", $"{origin}/user/bookings/{booking.Id}" }
                });

            await _emailSender.SendEmailAsync(
                booking.User.Email!,
                "Hujjzy: Booking Confirmation - " + booking.BookingNumber,
                emailBody);

            _logger.LogInformation(
                "Booking confirmation email sent for booking {BookingId} to {Email}",
                bookingId, booking.User.Email);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send booking confirmation email for booking {BookingId}", bookingId);
            return Result.Failure(new Error("EmailFailed", "Failed to send email", 500));
        }
    }

    public async Task<Result> SendBookingStatusUpdateEmailAsync(int bookingId, string newStatus)
    {
        try
        {
            var booking = await _context.Bookings
                .Include(b => b.Unit)
                .Include(b => b.User)
                .FirstOrDefaultAsync(b => b.Id == bookingId);

            if (booking == null || booking.User == null)
                return Result.Failure(new Error("BookingNotFound", "Booking not found", 404));

            var origin = GetOriginUrl();

            // Customize message based on status
            var (statusMessage, nextSteps) = newStatus.ToUpper() switch
            {
                "CONFIRMED" => (
                    "Great news! Your booking has been confirmed by the property.",
                    "Please complete the payment if you haven't already. Check-in instructions will be sent 24 hours before your arrival."
                ),
                "CHECKEDIN" => (
                    "Welcome! You've successfully checked in. We hope you enjoy your stay!",
                    "If you need anything during your stay, please contact the property directly."
                ),
                "COMPLETED" => (
                    "Thank you for staying with us! Your booking is now complete.",
                    "We'd love to hear about your experience. Please consider leaving a review."
                ),
                _ => (
                    $"Your booking status has been updated to {newStatus}.",
                    "Please check your booking details for more information."
                )
            };

            var emailBody = EmailBodyBuilder.GenerateEmailBody("BookingStatusUpdate",
                new Dictionary<string, string>
                {
                    { "{{name}}", booking.User.FullName ?? "Guest" },
                    { "{{booking_number}}", booking.BookingNumber },
                    { "{{unit_name}}", booking.Unit?.Name ?? "N/A" },
                    { "{{new_status}}", newStatus },
                    { "{{status_message}}", statusMessage },
                    { "{{next_steps}}", nextSteps },
                    { "{{check_in_date}}", booking.CheckInDate.ToString("MMMM dd, yyyy") },
                    { "{{check_out_date}}", booking.CheckOutDate.ToString("MMMM dd, yyyy") },
                    { "{{dashboard_url}}", $"{origin}/user/bookings/{booking.Id}" }
                });

            await _emailSender.SendEmailAsync(
                booking.User.Email!,
                $"Hujjzy: Booking Update - {booking.BookingNumber}",
                emailBody);

            _logger.LogInformation(
                "Booking status update email sent for booking {BookingId}, status: {Status}",
                bookingId, newStatus);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send booking status email for booking {BookingId}", bookingId);
            return Result.Failure(new Error("EmailFailed", "Failed to send email", 500));
        }
    }

    public async Task<Result> SendBookingCancellationEmailAsync(int bookingId, decimal refundAmount)
    {
        try
        {
            var booking = await _context.Bookings
                .Include(b => b.Unit)
                .Include(b => b.User)
                .FirstOrDefaultAsync(b => b.Id == bookingId);

            if (booking == null || booking.User == null)
                return Result.Failure(new Error("BookingNotFound", "Booking not found", 404));

            var origin = GetOriginUrl();

            var refundMessage = refundAmount > 0
                ? $"A refund of ${refundAmount:F2} will be processed to your original payment method within 5-7 business days."
                : "As per our cancellation policy, no refund is applicable for this cancellation.";

            var emailBody = EmailBodyBuilder.GenerateEmailBody("CancellationEmail",
                new Dictionary<string, string>
                {
                    { "{{name}}", booking.User.FullName ?? "Guest" },
                    { "{{booking_number}}", booking.BookingNumber },
                    { "{{unit_name}}", booking.Unit?.Name ?? "N/A" },
                    { "{{check_in_date}}", booking.CheckInDate.ToString("MMMM dd, yyyy") },
                    { "{{check_out_date}}", booking.CheckOutDate.ToString("MMMM dd, yyyy") },
                    { "{{cancelled_date}}", DateTime.UtcNow.AddHours(3).ToString("MMMM dd, yyyy HH:mm") },
                    { "{{total_price}}", booking.TotalPrice.ToString("F2") },
                    { "{{refund_amount}}", refundAmount.ToString("F2") },
                    { "{{refund_message}}", refundMessage },
                    { "{{cancellation_reason}}", booking.CancellationReason ?? "Not specified" },
                    { "{{dashboard_url}}", $"{origin}/user/bookings/{booking.Id}" }
                });

            await _emailSender.SendEmailAsync(
                booking.User.Email!,
                $"Hujjzy: Booking Cancelled - {booking.BookingNumber}",
                emailBody);

            _logger.LogInformation(
                "Booking cancellation email sent for booking {BookingId}, refund: {Refund}",
                bookingId, refundAmount);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send cancellation email for booking {BookingId}", bookingId);
            return Result.Failure(new Error("EmailFailed", "Failed to send email", 500));
        }
    }

    public async Task<Result> SendBookingCheckoutEmailAsync(int bookingId)
    {
        try
        {
            var booking = await _context.Bookings
                .Include(b => b.Unit)
                .Include(b => b.User)
                .FirstOrDefaultAsync(b => b.Id == bookingId);

            if (booking == null || booking.User == null)
                return Result.Failure(new Error("BookingNotFound", "Booking not found", 404));

            var origin = GetOriginUrl();

            var emailBody = EmailBodyBuilder.GenerateEmailBody("CheckoutEmail",
                new Dictionary<string, string>
                {
                    { "{{name}}", booking.User.FullName ?? "Guest" },
                    { "{{booking_number}}", booking.BookingNumber },
                    { "{{unit_name}}", booking.Unit?.Name ?? "N/A" },
                    { "{{check_in_date}}", booking.CheckInDate.ToString("MMMM dd, yyyy") },
                    { "{{check_out_date}}", booking.CheckOutDate.ToString("MMMM dd, yyyy") },
                    { "{{number_of_nights}}", booking.NumberOfNights.ToString() },
                    { "{{total_paid}}", booking.PaidAmount.ToString("F2") },
                    { "{{review_url}}", $"{origin}/user/bookings/{booking.Id}/review" },
                    { "{{dashboard_url}}", $"{origin}/user/bookings/{booking.Id}" }
                });

            await _emailSender.SendEmailAsync(
                booking.User.Email!,
                "Hujjzy: Thank You for Your Stay!",
                emailBody);

            _logger.LogInformation(
                "Checkout email sent for booking {BookingId}",
                bookingId);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send checkout email for booking {BookingId}", bookingId);
            return Result.Failure(new Error("EmailFailed", "Failed to send email", 500));
        }
    }

    public async Task<Result> SendPaymentConfirmationEmailAsync(int bookingId, decimal amount)
    {
        try
        {
            var booking = await _context.Bookings
                .Include(b => b.Unit)
                .Include(b => b.User)
                .Include(b => b.Payments)
                .FirstOrDefaultAsync(b => b.Id == bookingId);

            if (booking == null || booking.User == null)
                return Result.Failure(new Error("BookingNotFound", "Booking not found", 404));

            var origin = GetOriginUrl();
            var remainingAmount = booking.TotalPrice - booking.PaidAmount;

            var emailBody = EmailBodyBuilder.GenerateEmailBody("PaymentConfirmation",
                new Dictionary<string, string>
                {
                    { "{{name}}", booking.User.FullName ?? "Guest" },
                    { "{{booking_number}}", booking.BookingNumber },
                    { "{{payment_amount}}", amount.ToString("F2") },
                    { "{{payment_date}}", DateTime.UtcNow.AddHours(3).ToString("MMMM dd, yyyy HH:mm") },
                    { "{{payment_status}}", booking.PaymentStatus.ToString() },
                    { "{{unit_name}}", booking.Unit?.Name ?? "N/A" },
                    { "{{check_in_date}}", booking.CheckInDate.ToString("MMMM dd, yyyy") },
                    { "{{total_price}}", booking.TotalPrice.ToString("F2") },
                    { "{{paid_amount}}", booking.PaidAmount.ToString("F2") },
                    { "{{remaining_amount}}", remainingAmount.ToString("F2") },
                    { "{{dashboard_url}}", $"{origin}/user/bookings/{booking.Id}" }
                });

            await _emailSender.SendEmailAsync(
                booking.User.Email!,
                $"Hujjzy: Payment Received - {booking.BookingNumber}",
                emailBody);

            _logger.LogInformation(
                "Payment confirmation email sent for booking {BookingId}, amount: {Amount}",
                bookingId, amount);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send payment confirmation email for booking {BookingId}", bookingId);
            return Result.Failure(new Error("EmailFailed", "Failed to send email", 500));
        }
    }

    #endregion

    #region REVIEW EMAILS

    public async Task<Result> SendReviewRequestEmailAsync(int bookingId)
    {
        try
        {
            var booking = await _context.Bookings
                .Include(b => b.Unit)
                .Include(b => b.User)
                .FirstOrDefaultAsync(b => b.Id == bookingId);

            if (booking == null || booking.User == null)
                return Result.Failure(new Error("BookingNotFound", "Booking not found", 404));

            // Check if review already exists
            var existingReview = await _context.Set<Review>()
                .AnyAsync(r => r.BookingId == bookingId);

            if (existingReview)
                return Result.Success(); // Don't send if already reviewed

            var origin = GetOriginUrl();

            var emailBody = EmailBodyBuilder.GenerateEmailBody("ReviewRequest",
                new Dictionary<string, string>
                {
                    { "{{name}}", booking.User.FullName ?? "Guest" },
                    { "{{unit_name}}", booking.Unit?.Name ?? "N/A" },
                    { "{{check_in_date}}", booking.CheckInDate.ToString("MMMM dd, yyyy") },
                    { "{{check_out_date}}", booking.CheckOutDate.ToString("MMMM dd, yyyy") },
                    { "{{review_url}}", $"{origin}/user/bookings/{booking.Id}/review" },
                    { "{{dashboard_url}}", $"{origin}/user/bookings/{booking.Id}" }
                });

            await _emailSender.SendEmailAsync(
                booking.User.Email!,
                $"Hujjzy: Share Your Experience at {booking.Unit?.Name}",
                emailBody);

            _logger.LogInformation(
                "Review request email sent for booking {BookingId}",
                bookingId);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send review request email for booking {BookingId}", bookingId);
            return Result.Failure(new Error("EmailFailed", "Failed to send email", 500));
        }
    }

    public async Task<Result> SendReviewThankYouEmailAsync(int reviewId)
    {
        try
        {
            var review = await _context.Set<Review>()
                .Include(r => r.Unit)
                .Include(r => r.User)
                .Include(r => r.Booking)
                .FirstOrDefaultAsync(r => r.Id == reviewId);

            if (review == null || review.User == null)
                return Result.Failure(new Error("ReviewNotFound", "Review not found", 404));

            var origin = GetOriginUrl();

            var emailBody = EmailBodyBuilder.GenerateEmailBody("ReviewThankYou",
                new Dictionary<string, string>
                {
                    { "{{name}}", review.User.FullName ?? "Guest" },
                    { "{{unit_name}}", review.Unit?.Name ?? "N/A" },
                    { "{{rating}}", review.Rating.ToString() },
                    { "{{review_url}}", $"{origin}/units/{review.UnitId}#reviews" },
                    { "{{dashboard_url}}", $"{origin}/user/reviews" }
                });

            await _emailSender.SendEmailAsync(
                review.User.Email!,
                "Hujjzy: Thank You for Your Review!",
                emailBody);

            _logger.LogInformation(
                "Review thank you email sent for review {ReviewId}",
                reviewId);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send review thank you email for review {ReviewId}", reviewId);
            return Result.Failure(new Error("EmailFailed", "Failed to send email", 500));
        }
    }

    public async Task<Result> SendNewReviewNotificationToOwnerAsync(int reviewId)
    {
        try
        {
            var review = await _context.Set<Review>()
                .Include(r => r.Unit)
                    .ThenInclude(u => u.Admins)
                        .ThenInclude(a => a.User)
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Id == reviewId);

            if (review == null || review.Unit == null)
                return Result.Failure(new Error("ReviewNotFound", "Review not found", 404));

            var origin = GetOriginUrl();

            // Get all active admins for this unit
            var admins = review.Unit.Admins
                .Where(a => a.IsActive && a.User != null && !string.IsNullOrEmpty(a.User.Email))
                .Select(a => a.User)
                .ToList();

            foreach (var admin in admins)
            {
                var emailBody = EmailBodyBuilder.GenerateEmailBody("NewReviewNotificationToOwner",
                    new Dictionary<string, string>
                    {
                        { "{{owner_name}}", admin.FullName ?? "Property Owner" },
                        { "{{unit_name}}", review.Unit.Name },
                        { "{{reviewer_name}}", review.User?.FullName ?? "A guest" },
                        { "{{rating}}", review.Rating.ToString() },
                        { "{{comment}}", review.Comment ?? "No comment provided" },
                        { "{{review_date}}", review.CreatedAt.ToString("MMMM dd, yyyy") },
                        { "{{review_url}}", $"{origin}/hotel-admin/reviews/{review.Id}" },
                        { "{{respond_url}}", $"{origin}/hotel-admin/reviews/{review.Id}/respond" }
                    });

                await _emailSender.SendEmailAsync(
                    admin.Email!,
                    $"Hujjzy: New Review for {review.Unit.Name}",
                    emailBody);
            }

            _logger.LogInformation(
                "New review notification sent to {Count} owners for review {ReviewId}",
                admins.Count, reviewId);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send new review notification for review {ReviewId}", reviewId);
            return Result.Failure(new Error("EmailFailed", "Failed to send email", 500));
        }
    }

    public async Task<Result> SendOwnerResponseNotificationAsync(int reviewId)
    {
        try
        {
            var review = await _context.Set<Review>()
                .Include(r => r.Unit)
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Id == reviewId);

            if (review == null || review.User == null)
                return Result.Failure(new Error("ReviewNotFound", "Review not found", 404));

            if (string.IsNullOrEmpty(review.OwnerResponse))
                return Result.Success(); // No response to notify about

            var origin = GetOriginUrl();

            var emailBody = EmailBodyBuilder.GenerateEmailBody("OwnerResponseNotification",
                new Dictionary<string, string>
                {
                    { "{{name}}", review.User.FullName ?? "Guest" },
                    { "{{unit_name}}", review.Unit?.Name ?? "N/A" },
                    { "{{owner_response}}", review.OwnerResponse },
                    { "{{review_url}}", $"{origin}/user/reviews/{review.Id}" },
                    { "{{unit_url}}", $"{origin}/units/{review.UnitId}" }
                });

            await _emailSender.SendEmailAsync(
                review.User.Email!,
                $"Hujjzy: Response to Your Review of {review.Unit?.Name}",
                emailBody);

            _logger.LogInformation(
                "Owner response notification sent for review {ReviewId}",
                reviewId);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send owner response notification for review {ReviewId}", reviewId);
            return Result.Failure(new Error("EmailFailed", "Failed to send email", 500));
        }
    }

    #endregion

    #region UNIT REGISTRATION EMAILS (Existing - to be migrated)

    public async Task<Result> SendRegistrationConfirmationEmailAsync(int requestId)
    {
        // Implementation from UnitRegistrationService
        _logger.LogInformation("Sending registration confirmation for request {RequestId}", requestId);
        return Result.Success();
    }

    public async Task<Result> SendRegistrationApprovalEmailAsync(int requestId)
    {
        // Implementation from UnitRegistrationService
        _logger.LogInformation("Sending registration approval for request {RequestId}", requestId);
        return Result.Success();
    }

    public async Task<Result> SendRegistrationRejectionEmailAsync(int requestId)
    {
        // Implementation from UnitRegistrationService
        _logger.LogInformation("Sending registration rejection for request {RequestId}", requestId);
        return Result.Success();
    }

    public async Task<Result> NotifyAdminsOfNewRegistrationAsync(int requestId)
    {
        // Implementation from UnitRegistrationService
        _logger.LogInformation("Notifying admins of new registration {RequestId}", requestId);
        return Result.Success();
    }

    #endregion
}
