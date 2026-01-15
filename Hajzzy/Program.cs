using Application;
using Application.Notifications;
using Application.Service.S3Image;
using Hangfire;
using Hangfire.Dashboard;
using Hangfire.SqlServer;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();

builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddHangfire(configuration => configuration
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseSqlServerStorage(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        new SqlServerStorageOptions
        {
            CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
            SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
            QueuePollInterval = TimeSpan.Zero,
            UseRecommendedIsolationLevel = true,
            DisableGlobalLocks = true
        }));

builder.Services.AddHangfireServer(options =>
{
    options.WorkerCount = 5; // Number of concurrent jobs
    options.Queues = new[] { "default", "image-processing" }; // Define queues
});

builder.Services.AddScoped<ImageProcessingJob>();


builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 50 * 1024 * 1024; // 50 MB
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
