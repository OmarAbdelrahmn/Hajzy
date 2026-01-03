using Application.Service.Booking;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Hajzzy.Controllers;

[Route("api/[controller]")]
[ApiController]
public class BookingController(IBookingService service) : ControllerBase
{
    private readonly IBookingService service = service;

    [HttpGet("{bookingId}")]
    public async Task<IActionResult> GetBookingById(int bookingId)
    {
        var result = await service.GetBookingByIdAsync(bookingId);
        if (result.IsSuccess)
            return Ok(result.Value);

        return result.ToProblem();
    }

    [HttpGet("number/{bookingId}")]
    public async Task<IActionResult> GetBookingByNumberAsync(string bookignNumer)
    {
        var result = await service.GetBookingByNumberAsync(bookignNumer);
        if (result.IsSuccess)
            return Ok(result.Value);

        return result.ToProblem();
    }
    [HttpGet("unit/{UnitId}")]
    public async Task<IActionResult> GetUnitBookingsAsync(int UnitId, BookingFilter filter)
    {
        var result = await service.GetUnitBookingsAsync(UnitId  ,filter);
        if (result.IsSuccess)
            return Ok(result.Value);

        return result.ToProblem();
    }
    [HttpGet("user/{UserId}")]
    public async Task<IActionResult> GetUserBookingsAsync(string UserId, BookingFilter filter)
    {
        var result = await service.GetUserBookingsAsync(UserId, filter);
        if (result.IsSuccess)
            return Ok(result.Value);

        return result.ToProblem();
    }
    [HttpPost("")]
    public async Task<IActionResult> CreateBookingAsync(CreateBookingRequest filter)
    {
        var result = await service.CreateBookingAsync(filter);
        if (result.IsSuccess)
            return Ok(result.Value);

        return result.ToProblem();
    }
    [HttpPut("confirm")]
    public async Task<IActionResult> ConfirmBookingAsync(int bookingId, string adminUserId)
    {
        var result = await service.ConfirmBookingAsync(bookingId , adminUserId);
        if (result.IsSuccess)
            return Ok(new {message = "done"});

        return result.ToProblem();
    }
    [HttpPut("checkin")]
    public async Task<IActionResult> CheckInAsync(int bookingId)
    {
        var result = await service.CheckInAsync(bookingId);
        if (result.IsSuccess)
            return Ok(new {message = "done"});

        return result.ToProblem();
    }
    [HttpPut("checkout")]
    public async Task<IActionResult> CheckOutAsync(int bookingId)
    {
        var result = await service.CheckInAsync(bookingId);
        if (result.IsSuccess)
            return Ok(new {message = "done"});

        return result.ToProblem();
    }
    [HttpPut("cancel")]
    public async Task<IActionResult> CancelBookingAsync(int bookingId, string cancellationReason, string userId)
    {
        var result = await service.CancelBookingAsync(bookingId , cancellationReason , userId);
        if (result.IsSuccess)
            return Ok(new {message = "done"});

        return result.ToProblem();
    }
    [HttpPut("payment")]
    public async Task<IActionResult> ProcessPaymentAsync(int bookingId, ProcessPaymentRequest request)
    {
        var result = await service.ProcessPaymentAsync(bookingId, request);
        if (result.IsSuccess)
            return Ok(new {message = "done"});

        return result.ToProblem();
    }
    [HttpPut("refund")]
    public async Task<IActionResult> RefundPaymentAsync(int bookingId, decimal refundAmount, string reason)
    {
        var result = await service.RefundBookingAsync(bookingId, refundAmount, reason);
        if (result.IsSuccess)
            return Ok(new {message = "done"});

        return result.ToProblem();
    }
    [HttpPut("statistics")]
    public async Task<IActionResult> GetBookingStatisticsAsync(BookingStatisticsFilter filter)
    {
        var result = await service.GetBookingStatisticsAsync(filter);
        if (result.IsSuccess)
            return Ok(result.Value);

        return result.ToProblem();
    }
    [HttpPut("calculateprice")]
    public async Task<IActionResult> CalculateBookingPriceAsync(CalculateBookingPriceRequest request)
    {
        var result = await service.CalculateBookingPriceAsync(request);
        if (result.IsSuccess)
            return Ok(result.Value);

        return result.ToProblem();
    }
}
