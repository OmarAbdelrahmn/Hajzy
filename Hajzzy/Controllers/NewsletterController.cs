using Application.News;
using Application.Newsletter;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Hajzzy.Controllers;

[Route("api/[controller]")]
[ApiController]
public class NewsletterController(INewsletterService newsletterService) : ControllerBase
{
    private readonly INewsletterService newsletterService = newsletterService;

    // =========================================================================
    // ENDPOINT 1 — Subscribe (any user, no auth required)
    // POST /api/newsletter/subscribe
    // =========================================================================

    /// <summary>
    /// Subscribe an email address to the newsletter.
    /// If the caller is authenticated their user-id is linked to the subscription.
    /// </summary>
    [HttpPost("subscribe")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Subscribe([FromBody] SubscribeRequest request)
{
    if (!ModelState.IsValid)
        return BadRequest(ModelState);

    // Optionally link to an authenticated user
    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

    var result = await newsletterService.SubscribeAsync(request.Email, userId);

    if (!result.IsSuccess)
        return result.ToProblem();

    return Ok(new { message = "You have successfully subscribed to our newsletter." });
}

// =========================================================================
// ENDPOINT 2 — Send newsletter (admin only)
// POST /api/newsletter/send
// =========================================================================

/// <summary>
/// Admin creates a newsletter campaign. Hangfire picks it up immediately
/// and fans the emails out in batches across all active subscribers.
/// </summary>
[HttpPost("send")]
[Authorize(Roles = "SuperAdmin,Admin")]
[ProducesResponseType(typeof(CampaignCreatedResponse), StatusCodes.Status202Accepted)]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public async Task<IActionResult> SendNewsletter([FromBody] SendNewsletterRequest request)
{
    if (!ModelState.IsValid)
        return BadRequest(ModelState);

    var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

    var result = await newsletterService.CreateAndQueueCampaignAsync(
        request.Title,
        request.Description,
        adminId);

    if (!result.IsSuccess)
        return result.ToProblem();

    return Accepted(new CampaignCreatedResponse(
        CampaignId: result.Value,
        Message: "Newsletter campaign queued. Emails are being sent in the background."));
}

// =========================================================================
// ENDPOINT 3 — Unsubscribe via token link (no auth, from email link)
// GET /api/newsletter/unsubscribe/{token}
// =========================================================================

/// <summary>
/// One-click unsubscribe link embedded in every newsletter email.
/// </summary>
[HttpGet("unsubscribe/{token}")]
[AllowAnonymous]
public async Task<IActionResult> Unsubscribe(string token)
{
    var result = await newsletterService.UnsubscribeAsync(token);

    if (!result.IsSuccess)
        return result.ToProblem();

    return Ok(new { message = "You have been unsubscribed successfully." });
}
}