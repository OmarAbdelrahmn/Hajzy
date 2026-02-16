using Domain.Entities;
using Domain.Entities.others;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Reflection;

namespace Domain;

public class ApplicationDbcontext(DbContextOptions<ApplicationDbcontext> options) : IdentityDbContext<ApplicationUser, ApplicationRole, string>(options)
{

    public required DbSet<ApplicationUser> ApplicationUsers { get; set; }
    public required DbSet<ApplicationRole> ApplicationRoles { get; set; }

    public required DbSet<Department> Departments { get; set; }
    public required DbSet<Unit> Units { get; set; }
    public required DbSet<UnitType> UnitTypes { get; set; }
    public required DbSet<SubUnitTypee> SubUnitTypees { get; set; }
    public required DbSet<SubUnit> SubUnits { get; set; }
    public required DbSet<RoomConfiguration> RoomConfigurations { get; set; }
    public required DbSet<Amenity> Amenities { get; set; }
    public required DbSet<UnitAmenity> UnitAmenities { get; set; }
    public required DbSet<SubUniteAmenity> SubUniteAmenities { get; set; }
    public required DbSet<Booking> Bookings { get; set; }
    public required DbSet<BookingRoom> BookingRooms { get; set; }
    public required DbSet<BookingCoupon> BookingCoupons { get; set; }
    public required DbSet<Review> Reviews { get; set; }
    public required DbSet<Payment> Payments { get; set; }
    public required DbSet<Notification> Notifications { get; set; }
    public required DbSet<UserNotification> UserNotifications { get; set; }
    public required DbSet<DepartmentAdmin> DepartmentAdmins { get; set; }
    public required DbSet<UnitRegistrationRequest> UnitRegistrationRequests { get; set; }

    // Business entities
    public required DbSet<Coupon> Coupons { get; set; }
    public required DbSet<CancellationPolicy> CancellationPolicies { get; set; }
    public required DbSet<PricingRule> PricingRules { get; set; }
    public required DbSet<LoyaltyProgram> LoyaltyPrograms { get; set; }
    public required DbSet<LoyaltyTransaction> LoyaltyTransactions { get; set; }
    public required DbSet<GeneralPolicy> GeneralPolicies { get; set; }

    // Availability
    public required DbSet<SubUnitAvailability> SubUnitAvailabilities { get; set; }
    public required DbSet<UnitAvailability> UnitAvailabilities { get; set; }

    // Images with full metadata
    public required DbSet<UnitImage> UnitImages { get; set; }
    public required DbSet<SubUnitImage> SubUnitImages { get; set; }
    public required DbSet<DepartmentImage> DepartmentImages { get; set; }
    public required DbSet<ReviewImage> ReviewImages { get; set; }

    //fav
    public required DbSet<UserFavorite> UserFavorites { get; set; }

    //other
    public DbSet<Contract> Contracts { get; set; }
    public DbSet<PrivacyPolicy> PrivacyPolicies { get; set; }
    public DbSet<TermsAndConditions> TermsAndConditions { get; set; }
    public DbSet<FAQ> FAQs { get; set; }
    public DbSet<HowToUse> HowToUses { get; set; }
    public DbSet<PublicCancelPolicy> PublicCancelPolicies { get; set; }
    public DbSet<PaymentMethodd> PaymentMethods { get; set; }
    public DbSet<Ad> Ads { get; set; }
    public DbSet<Offer> Offers { get; set; }
    public DbSet<UnitCustomPolicy> UnitCustomPolicies { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        var cascadeFKs = modelBuilder.Model.GetEntityTypes()
            .SelectMany(t => t.GetForeignKeys())
            .Where(fk => !fk.IsOwnership && fk.DeleteBehavior == DeleteBehavior.Cascade);

        foreach (var fk in cascadeFKs)
            fk.DeleteBehavior = DeleteBehavior.Restrict;

        //modelBuilder.Entity<Unit>().HasQueryFilter(u => !u.IsDeleted);
        //modelBuilder.Entity<Booking>().HasQueryFilter(b => !b.IsDeleted);
        //modelBuilder.Entity<ApplicationRole>().HasQueryFilter(r => !r.IsDeleted);
        //modelBuilder.Entity<City>().HasQueryFilter(c => !c.IsDeleted);
        //modelBuilder.Entity<Notification>().HasQueryFilter(n => !n.IsDeleted);
        //modelBuilder.Entity<CityAdmin>().HasQueryFilter(ca => !ca.IsDeleted);

        base.OnModelCreating(modelBuilder);
    }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.ConfigureWarnings(warnings =>
            warnings.Ignore(RelationalEventId.PendingModelChangesWarning));
    }


    //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    //{
    //    // Suppress pending model changes warning
    //    optionsBuilder.ConfigureWarnings(warnings =>
    //        warnings.Ignore(RelationalEventId.PendingModelChangesWarning));

    //    // ADD: Performance optimizations
    //    optionsBuilder
    //        .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking) // Default no-tracking
    //        .EnableSensitiveDataLogging(false) // Disable in production
    //        .EnableDetailedErrors(false); // Disable in production

    //    // ADD: Connection resilience (for cloud environments)
    //    if (Database.IsSqlServer())
    //    {
    //        optionsBuilder.UseSqlServer(options =>
    //        {
    //            options.EnableRetryOnFailure(
    //                maxRetryCount: 5,
    //                maxRetryDelay: TimeSpan.FromSeconds(30),
    //                errorNumbersToAdd: null);

    //            options.CommandTimeout(30); // 30 seconds
    //            options.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
    //        });
    //    }
    //}

    //// ADD: SaveChanges with audit logging
    //public override int SaveChanges()
    //{
    //    AddTimestamps();
    //    AddAuditLogs();
    //    return base.SaveChanges();
    //}

    //public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    //{
    //    AddTimestamps();
    //    AddAuditLogs();
    //    return await base.SaveChangesAsync(cancellationToken);
    //}

    //private void AddTimestamps()
    //{
    //    var entries = ChangeTracker.Entries()
    //        .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

    //    foreach (var entry in entries)
    //    {
    //        if (entry.State == EntityState.Added)
    //        {
    //            // Set CreatedAt for new entities
    //            if (entry.Entity.GetType().GetProperty("CreatedAt") != null)
    //            {
    //                entry.Property("CreatedAt").CurrentValue = DateTime.UtcNow;
    //            }
    //        }

    //        if (entry.State == EntityState.Modified)
    //        {
    //            // Set UpdatedAt for modified entities
    //            if (entry.Entity.GetType().GetProperty("UpdatedAt") != null)
    //            {
    //                entry.Property("UpdatedAt").CurrentValue = DateTime.UtcNow;
    //            }

    //            // Prevent CreatedAt from being modified
    //            if (entry.Entity.GetType().GetProperty("CreatedAt") != null)
    //            {
    //                entry.Property("CreatedAt").IsModified = false;
    //            }
    //        }
    //    }
    //}

    //private void AddAuditLogs()
    //{
    //    // Note: In production, implement proper audit logging
    //    // This is a simplified example
    //    var entries = ChangeTracker.Entries()
    //        .Where(e => e.State == EntityState.Added ||
    //                   e.State == EntityState.Modified ||
    //                   e.State == EntityState.Deleted)
    //        .Where(e => e.Entity.GetType().GetProperty("Id") != null);

    //    foreach (var entry in entries)
    //    {
    //        // Implementation would:
    //        // 1. Serialize old/new values to JSON
    //        // 2. Get current user from HttpContext
    //        // 3. Create AuditLog entry
    //        // 4. Add to AuditLogs DbSet

    //        // Example structure:
    //        // var auditLog = new AuditLog
    //        // {
    //        //     TableName = entry.Metadata.GetTableName(),
    //        //     RecordId = entry.Property("Id").CurrentValue?.ToString(),
    //        //     Action = entry.State == EntityState.Added ? AuditAction.Create : 
    //        //              entry.State == EntityState.Modified ? AuditAction.Update : 
    //        //              AuditAction.Delete,
    //        //     OldValues = SerializeOldValues(entry),
    //        //     NewValues = SerializeNewValues(entry),
    //        //     UserId = GetCurrentUserId(),
    //        //     IpAddress = GetCurrentIpAddress()
    //        // };
    //        // AuditLogs.Add(auditLog);
    //    }
    //}
    //}
}
