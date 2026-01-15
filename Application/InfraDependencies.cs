using Amazon.S3;
using Application;
using Application.Admin;
using Application.Auth;
using Application.Authentication;
using Application.Helpers;
using Application.Notifications;
using Application.Roles;
using Application.Service.AdService;
using Application.Service.Amenity;
using Application.Service.Availability;
using Application.Service.Booking;
using Application.Service.Department;
using Application.Service.fav;
using Application.Service.OfferService;
using Application.Service.Policy;
using Application.Service.publicuser;
using Application.Service.Report;
using Application.Service.S3Image;
using Application.Service.SubUnit;
using Application.Service.SubUnitAmenity;
using Application.Service.SubUnitImage;
using Application.Service.Unit;
using Application.Service.UnitAmenity;
using Application.Service.UnitImage;
using Application.Service.UnitRegistration;
using Application.Service.UnitType;
using Application.Setting;
using Application.User;
using Domain;
using Domain.Entities;
using FluentValidation;
using Hangfire;
using Mapster;
using MapsterMapper;
using Medical_E_Commerce.Service.Roles;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Reflection;
using System.Text;

namespace Application;
public static class InfraDependencies
{

    public static IServiceCollection AddInfrastructure(this IServiceCollection Services, IConfiguration configuration)
    {
        Services.AddControllers();

        Services.AddEndpointsApiExplorer();
        Services.AddHttpContextAccessor();
        Services.AddScoped<IJwtProvider, JwtProvider>();
        Services.AddScoped<IUserService, UserServices>();
        Services.AddScoped<IAuthService, AuthService>();
        Services.AddScoped<IEmailSender, EmailService>();
        Services.AddScoped<INotinficationService, NotinficationService>();
        Services.AddScoped<IRoleService, RoleService>();
        Services.AddScoped<IAdminService, AdminService>();
        Services.AddScoped<IDepartmanetService, DepartmanetService>();
        Services.AddScoped<IGenerallPolicyService, GenerallPolicyService>();
        Services.AddScoped<ICancelPolicyService, CancelPolicyService>();
        Services.AddScoped<IEnumService, EnumService>();
        Services.AddScoped<IUnitRegistrationService,UnitRegistrationService>();
        Services.AddScoped<IS3ImageService,S3ImageService>();
        Services.AddScoped<IUnitService,UnitService>();
        Services.AddScoped<ISubUnitService,SubUnitService>();
        Services.AddScoped<IUnitAmenityService,UnitAmenityService>();
        Services.AddScoped<ISubUnitAmenityService,SubUnitAmenityService>();
        Services.AddScoped<IUnitImageService, UnitImageService>();
        Services.AddScoped<ISubUnitImageService, SubUnitImageService>();
        Services.AddScoped<IAvailabilityService, AvailabilityService>();
        Services.AddScoped<IAmenityService, AmenityService>();
        Services.AddScoped<IUnitTypeService, UnitTypeService>();
        Services.AddScoped<IBookingService, BookingService>();
        Services.AddScoped<IReportService,ReportService>();
        Services.AddScoped<IPublicServise,PublicService>();
        Services.AddScoped<IFavService,FavService>();
        Services.AddScoped<IAdService,AdService>();
        Services.AddScoped<IOfferService, OfferService>();
        Services.AddScoped<AdExpirationJob>();
        Services.AddScoped<OfferExpirationJob>();

        Services.AddProblemDetails();



        Services.AddAuth(configuration)
                .AddMappester()
                .AddFluentValidation()
                .AddDatabase(configuration)
                .AddCORS()
                .AddHangfire(configuration)
                .AddFluentswagger()
                .AddAWS(configuration)
                ;


        return Services;
    }

    public static IServiceCollection AddFluentValidation(this IServiceCollection Services)
    {
        Services
            .AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        return Services;
    }
    public static IServiceCollection AddFluentswagger(this IServiceCollection Services)
    {
        Services
            .AddEndpointsApiExplorer()
            .AddSwaggerGen();

        return Services;
    }
    public static IServiceCollection AddMappester(this IServiceCollection Services)
    {
        var mappingConfig = TypeAdapterConfig.GlobalSettings;
        mappingConfig.Scan(Assembly.GetExecutingAssembly());

        Services.AddSingleton<IMapper>(new Mapper(mappingConfig));

        return Services;
    }
    public static IServiceCollection AddDatabase(this IServiceCollection Services, IConfiguration c)
    {
        var ConnectionString = c.GetConnectionString("DefaultConnection") ??
            throw new InvalidOperationException("Connection string is not found in the configuration file");

        Services.AddDbContext<ApplicationDbcontext>(options =>
            options.UseSqlServer(ConnectionString));

        return Services;
    }
    public static IServiceCollection AddAuth(this IServiceCollection Services, IConfiguration configuration)
    {


        Services.AddIdentity<ApplicationUser,ApplicationRole>()
            .AddEntityFrameworkStores<ApplicationDbcontext>()
            .AddDefaultTokenProviders();

        Services.Configure<JwtOptions>(configuration.GetSection("Jwt"));

        Services.Configure<MainSettings>(configuration.GetSection(nameof(MainSettings)));

        var Jwtsetting = configuration.GetSection("Jwt").Get<JwtOptions>();

        Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(o =>
        {
            o.SaveToken = true;
            o.TokenValidationParameters = new TokenValidationParameters
            {


                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidAudience = Jwtsetting?.Audience,
                ValidIssuer = Jwtsetting?.Issuer,

                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Jwtsetting?.Key!))
            };
        });
        Services.Configure<IdentityOptions>(options =>
        {
            // Default Lockout settings.
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(10);
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.AllowedForNewUsers = true;
            options.Password.RequiredLength = 8;
            options.SignIn.RequireConfirmedEmail = true;
            options.SignIn.RequireConfirmedPhoneNumber = false;
            options.User.RequireUniqueEmail = true;
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireNonAlphanumeric = true;
            options.Password.RequireUppercase = true;
            options.Password.RequiredUniqueChars = 1;
            //options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
        });

        return Services;
    }
    public static IServiceCollection AddCORS(this IServiceCollection Services)
    {
        Services.AddCors(options =>
        {
            options.AddDefaultPolicy(builder =>
                builder
                        .AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader());
        });
        return Services;
    }
    public static IServiceCollection AddHangfire(this IServiceCollection Services, IConfiguration configuration)
    {
        Services.AddHangfire(config => config
        .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
        .UseSimpleAssemblyNameTypeSerializer()
        .UseRecommendedSerializerSettings()
        .UseSqlServerStorage(configuration.GetConnectionString("DefaultConnection")));

        Services.AddHangfireServer();
        return Services;
    }
    public static IServiceCollection AddAWS(this IServiceCollection services, IConfiguration configuration)
    {
        // ===== Configure Request Size Limits for Image Upload =====
        services.Configure<FormOptions>(options =>
        {
            options.MultipartBodyLengthLimit = 150 * 1024 * 1024;
        });

        // ===== Explicitly Configure AWS Credentials =====
        var awsAccessKey = configuration["AWS:AccessKey"];
        var awsSecretKey = configuration["AWS:SecretKey"];
        var awsRegion = configuration["AWS:Region"];

        if (string.IsNullOrEmpty(awsAccessKey) || string.IsNullOrEmpty(awsSecretKey))
        {
            throw new InvalidOperationException("AWS credentials are not configured properly.");
        }

        var awsOptions = new Amazon.Extensions.NETCore.Setup.AWSOptions
        {
            Credentials = new Amazon.Runtime.BasicAWSCredentials(awsAccessKey, awsSecretKey),
            Region = Amazon.RegionEndpoint.GetBySystemName(awsRegion)
        };

        services.AddDefaultAWSOptions(awsOptions);
        services.AddAWSService<IAmazonS3>();
        // ✅ NO parameters here

        return services;
    }

}
