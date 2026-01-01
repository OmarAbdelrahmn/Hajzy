
using Application.Helpers;
using Domain;
using Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;

namespace Application.Notifications;

public class NotinficationService(
    ApplicationDbcontext dbcontext,
    UserManager<ApplicationUser> userManager,
    IHttpContextAccessor httpContextAccessor,
    IEmailSender emailSender) : INotinficationService
{
    private readonly ApplicationDbcontext dbcontext = dbcontext;
    private readonly UserManager<ApplicationUser> userManager = userManager;
    private readonly IHttpContextAccessor httpContextAccessor = httpContextAccessor;
    private readonly IEmailSender emailSender = emailSender;


    public async Task SendPharmacyNotification()
    {
        IEnumerable<string> News = [
            "hello"
            ];



        var users = await userManager.Users.ToListAsync();

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
}
