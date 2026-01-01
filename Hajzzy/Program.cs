using Application;
using Application.Notifications;
using Hangfire;
using Hangfire.Dashboard;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();

builder.Services.AddInfrastructure(builder.Configuration);


builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 50 * 1024 * 1024; // 150 MB
});


var app = builder.Build();

//if (app.Environment.IsDevelopment())
//{
    app.UseSwagger();
    app.UseSwaggerUI();
//}

app.UseHangfireDashboard("/jobs", new DashboardOptions
{
    Authorization = [] // Empty array = no authorization
});



var scopeFactory = app.Services.GetRequiredService<IServiceScopeFactory>();
using var scope = scopeFactory.CreateScope();
var notificationService = scope.ServiceProvider.GetRequiredService<INotinficationService>();

RecurringJob.AddOrUpdate<INotinficationService>(
    "daily-news",
    x => x.SendPharmacyNotification(),
    Cron.Daily);

RecurringJob.AddOrUpdate<INotinficationService>(
    "weekly-news",
    x => x.SendPharmacyNotification(),
    Cron.Weekly);

RecurringJob.AddOrUpdate<INotinficationService>(
    "monthly-news",
    x => x.SendPharmacyNotification(),
    Cron.Monthly);




app.UseCors();

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
