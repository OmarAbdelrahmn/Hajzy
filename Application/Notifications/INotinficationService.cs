using Application.Abstraction;

namespace Application.Notifications;

public interface INotinficationService
{
    Task SendPharmacyNotification();

    /// <summary>
    /// Send booking confirmation email to user
    /// </summary>
    Task<Result> SendBookingConfirmationEmailAsync(int bookingId);

    /// <summary>
    /// Send booking status update email
    /// </summary>
    Task<Result> SendBookingStatusUpdateEmailAsync(int bookingId, string newStatus);

    /// <summary>
    /// Send booking cancellation email
    /// </summary>
    Task<Result> SendBookingCancellationEmailAsync(int bookingId, decimal refundAmount);

    /// <summary>
    /// Send checkout/completion email
    /// </summary>
    Task<Result> SendBookingCheckoutEmailAsync(int bookingId);

    /// <summary>
    /// Send payment confirmation email
    /// </summary>
    Task<Result> SendPaymentConfirmationEmailAsync(int bookingId, decimal amount);

    // ============= REVIEW EMAILS =============

    /// <summary>
    /// Send review request email after checkout
    /// </summary>
    Task<Result> SendReviewRequestEmailAsync(int bookingId);

    /// <summary>
    /// Send thank you email after review submission
    /// </summary>
    Task<Result> SendReviewThankYouEmailAsync(int reviewId);

    /// <summary>
    /// Send notification to property owner about new review
    /// </summary>
    Task<Result> SendNewReviewNotificationToOwnerAsync(int reviewId);

    /// <summary>
    /// Send notification to reviewer about owner response
    /// </summary>
    Task<Result> SendOwnerResponseNotificationAsync(int reviewId);

    // ============= UNIT REGISTRATION EMAILS (existing) =============

    /// <summary>
    /// Send registration confirmation to applicant
    /// </summary>
    Task<Result> SendRegistrationConfirmationEmailAsync(int requestId);

    /// <summary>
    /// Send approval email with credentials
    /// </summary>
    Task<Result> SendRegistrationApprovalEmailAsync(int requestId);

    /// <summary>
    /// Send rejection email
    /// </summary>
    Task<Result> SendRegistrationRejectionEmailAsync(int requestId);

    /// <summary>
    /// Notify admins of new registration
    /// </summary>
    Task<Result> NotifyAdminsOfNewRegistrationAsync(int requestId);
}



