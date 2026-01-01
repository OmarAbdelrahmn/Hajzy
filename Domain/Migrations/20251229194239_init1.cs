using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Domain.Migrations
{
    /// <inheritdoc />
    public partial class init1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "AvatarUploadedAt",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AvatarUrl",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Bio",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "DateOfBirth",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastLoginAt",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Nationality",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Amenities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<int>(type: "int", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Category = table.Column<int>(type: "int", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Amenities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CancellationPolicies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    FullRefundDays = table.Column<int>(type: "int", nullable: false),
                    PartialRefundDays = table.Column<int>(type: "int", nullable: false),
                    PartialRefundPercentage = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CancellationPolicies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Departments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Country = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Latitude = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Longitude = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    TotalUnits = table.Column<int>(type: "int", nullable: false),
                    TotalBookings = table.Column<int>(type: "int", nullable: false),
                    AverageRating = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Departments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LoyaltyPrograms",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    TotalPoints = table.Column<int>(type: "int", nullable: false),
                    LifetimePoints = table.Column<int>(type: "int", nullable: false),
                    Tier = table.Column<int>(type: "int", nullable: false),
                    TierAchievedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NextTierEligibleAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoyaltyPrograms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LoyaltyPrograms_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RoomConfigurations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Bedrooms = table.Column<int>(type: "int", nullable: false),
                    Bathrooms = table.Column<int>(type: "int", nullable: false),
                    Size = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BedType = table.Column<int>(type: "int", nullable: false),
                    BedCount = table.Column<int>(type: "int", nullable: false),
                    HasKitchen = table.Column<bool>(type: "bit", nullable: false),
                    HasBalcony = table.Column<bool>(type: "bit", nullable: false),
                    ViewType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Floor = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoomConfigurations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UnitTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UnitTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DepartmentAdmins",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CityId = table.Column<int>(type: "int", nullable: false),
                    AssignedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DepartmentAdmins", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DepartmentAdmins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DepartmentAdmins_Departments_CityId",
                        column: x => x.CityId,
                        principalTable: "Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DepartmentImages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DepartmentId = table.Column<int>(type: "int", nullable: false),
                    ImageType = table.Column<int>(type: "int", nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    S3Key = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    S3Bucket = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CdnUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FileSizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    MimeType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Width = table.Column<int>(type: "int", nullable: false),
                    Height = table.Column<int>(type: "int", nullable: false),
                    OriginalFileName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ThumbnailUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SmallUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MediumUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LargeUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsPrimary = table.Column<bool>(type: "bit", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    Caption = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    AltText = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ProcessingStatus = table.Column<int>(type: "int", nullable: false),
                    ProcessingError = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProcessedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UploadedByUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedByUserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ViewCount = table.Column<int>(type: "int", nullable: false),
                    LastViewedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedById = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DepartmentImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DepartmentImages_AspNetUsers_DeletedById",
                        column: x => x.DeletedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DepartmentImages_AspNetUsers_UploadedByUserId",
                        column: x => x.UploadedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DepartmentImages_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Units",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Latitude = table.Column<decimal>(type: "decimal(10,8)", precision: 10, scale: 8, nullable: false),
                    Longitude = table.Column<decimal>(type: "decimal(11,8)", precision: 11, scale: 8, nullable: false),
                    CityId = table.Column<int>(type: "int", nullable: false),
                    UnitTypeId = table.Column<int>(type: "int", nullable: false),
                    BasePrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    MaxGuests = table.Column<int>(type: "int", nullable: true),
                    Bedrooms = table.Column<int>(type: "int", nullable: true),
                    Bathrooms = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsVerified = table.Column<bool>(type: "bit", nullable: false),
                    AverageRating = table.Column<decimal>(type: "decimal(3,2)", precision: 3, scale: 2, nullable: false),
                    TotalReviews = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CancellationPolicyId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Units", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Units_CancellationPolicies_CancellationPolicyId",
                        column: x => x.CancellationPolicyId,
                        principalTable: "CancellationPolicies",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Units_Departments_CityId",
                        column: x => x.CityId,
                        principalTable: "Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Units_UnitTypes_UnitTypeId",
                        column: x => x.UnitTypeId,
                        principalTable: "UnitTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Bookings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BookingNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    UnitId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CheckInDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CheckOutDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NumberOfGuests = table.Column<int>(type: "int", nullable: false),
                    NumberOfNights = table.Column<int>(type: "int", nullable: false),
                    TotalPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PaidAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    PaymentStatus = table.Column<int>(type: "int", nullable: false),
                    SpecialRequests = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CancellationReason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CancelledAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bookings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Bookings_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Bookings_Units_UnitId",
                        column: x => x.UnitId,
                        principalTable: "Units",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Coupons",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Type = table.Column<int>(type: "int", nullable: false),
                    DiscountAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MinimumSpend = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    MaximumDiscount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    MaxUsageCount = table.Column<int>(type: "int", nullable: true),
                    CurrentUsageCount = table.Column<int>(type: "int", nullable: false),
                    MaxUsagePerUser = table.Column<int>(type: "int", nullable: true),
                    ValidFrom = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ValidUntil = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    TargetUnitId = table.Column<int>(type: "int", nullable: true),
                    TargetCityId = table.Column<int>(type: "int", nullable: true),
                    TargetUnitTypeId = table.Column<int>(type: "int", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Coupons", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Coupons_Departments_TargetCityId",
                        column: x => x.TargetCityId,
                        principalTable: "Departments",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Coupons_UnitTypes_TargetUnitTypeId",
                        column: x => x.TargetUnitTypeId,
                        principalTable: "UnitTypes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Coupons_Units_TargetUnitId",
                        column: x => x.TargetUnitId,
                        principalTable: "Units",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Message = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    CreatedByRole = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Target = table.Column<int>(type: "int", nullable: false),
                    TargetDepartmentId = table.Column<int>(type: "int", nullable: true),
                    TargetUnitId = table.Column<int>(type: "int", nullable: true),
                    IsScheduled = table.Column<bool>(type: "bit", nullable: false),
                    ScheduledFor = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsSent = table.Column<bool>(type: "bit", nullable: false),
                    SentAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TotalRecipients = table.Column<int>(type: "int", nullable: false),
                    DeliveredCount = table.Column<int>(type: "int", nullable: false),
                    FailedCount = table.Column<int>(type: "int", nullable: false),
                    ReadCount = table.Column<int>(type: "int", nullable: false),
                    RetryCount = table.Column<int>(type: "int", nullable: false),
                    MaxRetries = table.Column<int>(type: "int", nullable: false),
                    LastRetryAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notifications_AspNetUsers_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Notifications_Departments_TargetDepartmentId",
                        column: x => x.TargetDepartmentId,
                        principalTable: "Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Notifications_Units_TargetUnitId",
                        column: x => x.TargetUnitId,
                        principalTable: "Units",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "PricingRules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UnitId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    MinDaysBeforeCheckIn = table.Column<int>(type: "int", nullable: true),
                    MaxDaysBeforeCheckIn = table.Column<int>(type: "int", nullable: true),
                    MinNights = table.Column<int>(type: "int", nullable: true),
                    DayOfWeek = table.Column<int>(type: "int", nullable: true),
                    SeasonStart = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SeasonEnd = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AdjustmentType = table.Column<int>(type: "int", nullable: false),
                    AdjustmentValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PricingRules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PricingRules_Units_UnitId",
                        column: x => x.UnitId,
                        principalTable: "Units",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SubUnits",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UnitId = table.Column<int>(type: "int", nullable: false),
                    RoomNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Type = table.Column<int>(type: "int", maxLength: 100, nullable: false),
                    RoomConfigId = table.Column<int>(type: "int", nullable: true),
                    PricePerNight = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    MaxOccupancy = table.Column<int>(type: "int", nullable: false),
                    Bedrooms = table.Column<int>(type: "int", nullable: true),
                    Bathrooms = table.Column<int>(type: "int", nullable: true),
                    Size = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    IsAvailable = table.Column<bool>(type: "bit", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubUnits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SubUnits_RoomConfigurations_RoomConfigId",
                        column: x => x.RoomConfigId,
                        principalTable: "RoomConfigurations",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SubUnits_Units_UnitId",
                        column: x => x.UnitId,
                        principalTable: "Units",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UnitAmenities",
                columns: table => new
                {
                    UnitId = table.Column<int>(type: "int", nullable: false),
                    AmenityId = table.Column<int>(type: "int", nullable: false),
                    Id = table.Column<int>(type: "int", nullable: false),
                    IsAvailable = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UnitAmenities", x => new { x.UnitId, x.AmenityId });
                    table.ForeignKey(
                        name: "FK_UnitAmenities_Amenities_AmenityId",
                        column: x => x.AmenityId,
                        principalTable: "Amenities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UnitAmenities_Units_UnitId",
                        column: x => x.UnitId,
                        principalTable: "Units",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UnitAvailabilities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UnitId = table.Column<int>(type: "int", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsAvailable = table.Column<bool>(type: "bit", nullable: false),
                    Reason = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByUserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedById = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UnitAvailabilities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UnitAvailabilities_AspNetUsers_UpdatedById",
                        column: x => x.UpdatedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_UnitAvailabilities_Units_UnitId",
                        column: x => x.UnitId,
                        principalTable: "Units",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UniteAdmin",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UnitId = table.Column<int>(type: "int", nullable: false),
                    AssignedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UniteAdmin", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UniteAdmin_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UniteAdmin_Units_UnitId",
                        column: x => x.UnitId,
                        principalTable: "Units",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UnitImages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UnitId = table.Column<int>(type: "int", nullable: false),
                    ImageType = table.Column<int>(type: "int", nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    S3Key = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    S3Bucket = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CdnUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FileSizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    MimeType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Width = table.Column<int>(type: "int", nullable: false),
                    Height = table.Column<int>(type: "int", nullable: false),
                    OriginalFileName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ThumbnailUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SmallUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MediumUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LargeUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsPrimary = table.Column<bool>(type: "bit", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    Caption = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    AltText = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ProcessingStatus = table.Column<int>(type: "int", nullable: false),
                    ProcessingError = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProcessedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UploadedByUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedByUserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    ViewCount = table.Column<int>(type: "int", nullable: false),
                    LastViewedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UnitImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UnitImages_AspNetUsers_DeletedByUserId",
                        column: x => x.DeletedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UnitImages_AspNetUsers_UploadedByUserId",
                        column: x => x.UploadedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UnitImages_Units_UnitId",
                        column: x => x.UnitId,
                        principalTable: "Units",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserFavorites",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UnitId = table.Column<int>(type: "int", nullable: false),
                    AddedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserFavorites", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserFavorites_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserFavorites_Units_UnitId",
                        column: x => x.UnitId,
                        principalTable: "Units",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LoyaltyTransactions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LoyaltyProgramId = table.Column<int>(type: "int", nullable: false),
                    BookingId = table.Column<int>(type: "int", nullable: true),
                    PointsEarned = table.Column<int>(type: "int", nullable: false),
                    PointsRedeemed = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoyaltyTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LoyaltyTransactions_Bookings_BookingId",
                        column: x => x.BookingId,
                        principalTable: "Bookings",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_LoyaltyTransactions_LoyaltyPrograms_LoyaltyProgramId",
                        column: x => x.LoyaltyProgramId,
                        principalTable: "LoyaltyPrograms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Payments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BookingId = table.Column<int>(type: "int", nullable: false),
                    TransactionId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PaymentMethod = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    PaymentDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Payments_Bookings_BookingId",
                        column: x => x.BookingId,
                        principalTable: "Bookings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Reviews",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UnitId = table.Column<int>(type: "int", nullable: false),
                    BookingId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Rating = table.Column<int>(type: "int", nullable: false),
                    CleanlinessRating = table.Column<int>(type: "int", nullable: false),
                    LocationRating = table.Column<int>(type: "int", nullable: false),
                    ServiceRating = table.Column<int>(type: "int", nullable: false),
                    ValueRating = table.Column<int>(type: "int", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    OwnerResponse = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    OwnerResponseDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsVerified = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reviews", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Reviews_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Reviews_Bookings_BookingId",
                        column: x => x.BookingId,
                        principalTable: "Bookings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Reviews_Units_UnitId",
                        column: x => x.UnitId,
                        principalTable: "Units",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BookingCoupons",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BookingId = table.Column<int>(type: "int", nullable: false),
                    CouponId = table.Column<int>(type: "int", nullable: false),
                    DiscountApplied = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AppliedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookingCoupons", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BookingCoupons_Bookings_BookingId",
                        column: x => x.BookingId,
                        principalTable: "Bookings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BookingCoupons_Coupons_CouponId",
                        column: x => x.CouponId,
                        principalTable: "Coupons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserNotifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NotificationId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    IsRead = table.Column<bool>(type: "bit", nullable: false),
                    ReadAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReceivedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserNotifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserNotifications_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserNotifications_Notifications_NotificationId",
                        column: x => x.NotificationId,
                        principalTable: "Notifications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BookingRoom",
                columns: table => new
                {
                    BookingId = table.Column<int>(type: "int", nullable: false),
                    RoomId = table.Column<int>(type: "int", nullable: false),
                    Id = table.Column<int>(type: "int", nullable: false),
                    PricePerNight = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    NumberOfNights = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookingRoom", x => new { x.BookingId, x.RoomId });
                    table.ForeignKey(
                        name: "FK_BookingRoom_Bookings_BookingId",
                        column: x => x.BookingId,
                        principalTable: "Bookings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BookingRoom_SubUnits_RoomId",
                        column: x => x.RoomId,
                        principalTable: "SubUnits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GeneralPolicies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", maxLength: 5000, nullable: false),
                    PolicyType = table.Column<int>(type: "int", nullable: false),
                    PolicyCategory = table.Column<int>(type: "int", nullable: true),
                    CustomPolicyName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CancellationPolicyId = table.Column<int>(type: "int", nullable: true),
                    IsMandatory = table.Column<bool>(type: "bit", nullable: false),
                    IsHighlighted = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    UnitId = table.Column<int>(type: "int", nullable: true),
                    SubUnitId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GeneralPolicies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GeneralPolicies_CancellationPolicies_CancellationPolicyId",
                        column: x => x.CancellationPolicyId,
                        principalTable: "CancellationPolicies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_GeneralPolicies_SubUnits_SubUnitId",
                        column: x => x.SubUnitId,
                        principalTable: "SubUnits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GeneralPolicies_Units_UnitId",
                        column: x => x.UnitId,
                        principalTable: "Units",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SubUnitAvailabilities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsAvailable = table.Column<bool>(type: "bit", nullable: false),
                    Reason = table.Column<int>(type: "int", nullable: true),
                    SpecialPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    WeekendPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByUserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    SubUnitId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubUnitAvailabilities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SubUnitAvailabilities_AspNetUsers_UpdatedByUserId",
                        column: x => x.UpdatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_SubUnitAvailabilities_SubUnits_SubUnitId",
                        column: x => x.SubUnitId,
                        principalTable: "SubUnits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SubUniteAmenities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SubUnitId = table.Column<int>(type: "int", nullable: false),
                    AmenityId = table.Column<int>(type: "int", nullable: false),
                    IsAvailable = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubUniteAmenities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SubUniteAmenities_Amenities_AmenityId",
                        column: x => x.AmenityId,
                        principalTable: "Amenities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SubUniteAmenities_SubUnits_SubUnitId",
                        column: x => x.SubUnitId,
                        principalTable: "SubUnits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SubUnitImages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SubUnitId = table.Column<int>(type: "int", nullable: false),
                    ImageType = table.Column<int>(type: "int", nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    S3Key = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    S3Bucket = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CdnUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FileSizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    MimeType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Width = table.Column<int>(type: "int", nullable: false),
                    Height = table.Column<int>(type: "int", nullable: false),
                    OriginalFileName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ThumbnailUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SmallUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MediumUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LargeUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsPrimary = table.Column<bool>(type: "bit", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    Caption = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    AltText = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ProcessingStatus = table.Column<int>(type: "int", nullable: false),
                    ProcessingError = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProcessedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UploadedByUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedByUserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ViewCount = table.Column<int>(type: "int", nullable: false),
                    LastViewedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedById = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubUnitImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SubUnitImages_AspNetUsers_DeletedById",
                        column: x => x.DeletedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SubUnitImages_AspNetUsers_UploadedByUserId",
                        column: x => x.UploadedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SubUnitImages_SubUnits_SubUnitId",
                        column: x => x.SubUnitId,
                        principalTable: "SubUnits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ReviewImages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReviewId = table.Column<int>(type: "int", nullable: false),
                    ImageType = table.Column<int>(type: "int", nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    S3Key = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    S3Bucket = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CdnUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FileSizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    MimeType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Width = table.Column<int>(type: "int", nullable: false),
                    Height = table.Column<int>(type: "int", nullable: false),
                    OriginalFileName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ThumbnailUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SmallUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MediumUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LargeUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsPrimary = table.Column<bool>(type: "bit", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    Caption = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    AltText = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ProcessingStatus = table.Column<int>(type: "int", nullable: false),
                    ProcessingError = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProcessedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UploadedByUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedByUserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ViewCount = table.Column<int>(type: "int", nullable: false),
                    LastViewedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedById = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReviewImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReviewImages_AspNetUsers_DeletedById",
                        column: x => x.DeletedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ReviewImages_AspNetUsers_UploadedByUserId",
                        column: x => x.UploadedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReviewImages_Reviews_ReviewId",
                        column: x => x.ReviewId,
                        principalTable: "Reviews",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "59724D2D-E2B5-4C67-AB6F-D93478147B03",
                columns: new[] { "AvatarUploadedAt", "AvatarUrl", "Bio", "CreatedAt", "DateOfBirth", "LastLoginAt", "Nationality", "PasswordHash" },
                values: new object[] { null, null, null, new DateTime(2025, 12, 29, 19, 42, 38, 266, DateTimeKind.Utc).AddTicks(8619), null, null, null, "AQAAAAIAAYagAAAAEO1LonCHYH6rZhtrH1HvM9K5Zm9wPDWTE7+uUqhRnjK/bsl0BG2R9js0sONFpPiPaQ==" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "59726D2D-E2B5-4C67-AB6F-D93878317B03",
                columns: new[] { "AvatarUploadedAt", "AvatarUrl", "Bio", "CreatedAt", "DateOfBirth", "LastLoginAt", "Nationality", "PasswordHash" },
                values: new object[] { null, null, null, new DateTime(2025, 12, 29, 19, 42, 38, 352, DateTimeKind.Utc).AddTicks(7759), null, null, null, "AQAAAAIAAYagAAAAEMDMbfLKk+E2IFIruwCPMkGASW6QERSmJbdLC2SysJtrQfsEmHScB9MiYb09YAG5ow==" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "59726D2D-E2B5-4C67-AB6F-D99478317B03",
                columns: new[] { "AvatarUploadedAt", "AvatarUrl", "Bio", "CreatedAt", "DateOfBirth", "LastLoginAt", "Nationality", "PasswordHash" },
                values: new object[] { null, null, null, new DateTime(2025, 12, 29, 19, 42, 38, 429, DateTimeKind.Utc).AddTicks(8319), null, null, null, "AQAAAAIAAYagAAAAEAWmtxwDYswRDnPDwP5oapQ6xQatVu2qYGrOJHnlhRzF3mL435kzt1QNj+Y6mKYSSA==" });

            migrationBuilder.InsertData(
                table: "Departments",
                columns: new[] { "Id", "AverageRating", "Country", "CreatedAt", "DeletedAt", "Description", "ImageUrl", "IsActive", "IsDeleted", "Latitude", "Longitude", "Name", "TotalBookings", "TotalUnits" },
                values: new object[,]
                {
                    { 1, null, "Yemen", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Governorate in southern Yemen", null, true, false, 0m, 0m, "Abyan", 0, 0 },
                    { 2, null, "Yemen", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Port city and temporary capital of Yemen", null, true, false, 0m, 0m, "Aden", 0, 0 },
                    { 3, null, "Yemen", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Central Yemeni governorate", null, true, false, 0m, 0m, "Al Bayda", 0, 0 },
                    { 4, null, "Yemen", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Southern highlands governorate", null, true, false, 0m, 0m, "Al Dhale'e", 0, 0 },
                    { 5, null, "Yemen", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Red Sea coastal governorate", null, true, false, 0m, 0m, "Al Hudaydah", 0, 0 },
                    { 6, null, "Yemen", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Northeastern desert governorate", null, true, false, 0m, 0m, "Al Jawf", 0, 0 },
                    { 7, null, "Yemen", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Easternmost governorate of Yemen", null, true, false, 0m, 0m, "Al Mahrah", 0, 0 },
                    { 8, null, "Yemen", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Mountainous northwestern governorate", null, true, false, 0m, 0m, "Al Mahwit", 0, 0 },
                    { 9, null, "Yemen", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Capital city area (Sana'a City)", null, true, false, 0m, 0m, "Amanat Al Asimah", 0, 0 },
                    { 10, null, "Yemen", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Northern governorate near Sana'a", null, true, false, 0m, 0m, "Amran", 0, 0 },
                    { 11, null, "Yemen", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Highland agricultural governorate", null, true, false, 0m, 0m, "Dhamar", 0, 0 },
                    { 12, null, "Yemen", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Largest governorate by area", null, true, false, 0m, 0m, "Hadhramaut", 0, 0 },
                    { 13, null, "Yemen", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Northwestern mountainous governorate", null, true, false, 0m, 0m, "Hajjah", 0, 0 },
                    { 14, null, "Yemen", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "One of the greenest regions in Yemen", null, true, false, 0m, 0m, "Ibb", 0, 0 },
                    { 15, null, "Yemen", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Agricultural governorate near Aden", null, true, false, 0m, 0m, "Lahij", 0, 0 },
                    { 16, null, "Yemen", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Historic governorate with ancient ruins", null, true, false, 0m, 0m, "Ma'rib", 0, 0 },
                    { 17, null, "Yemen", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Mountainous agricultural governorate", null, true, false, 0m, 0m, "Raymah", 0, 0 },
                    { 18, null, "Yemen", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Northernmost governorate", null, true, false, 0m, 0m, "Saada", 0, 0 },
                    { 19, null, "Yemen", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Oil-rich governorate in southeastern Yemen", null, true, false, 0m, 0m, "Shabwah", 0, 0 },
                    { 20, null, "Yemen", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "UNESCO World Heritage archipelago", null, true, false, 0m, 0m, "Socotra", 0, 0 },
                    { 21, null, "Yemen", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Cultural and industrial center of Yemen", null, true, false, 0m, 0m, "Taiz", 0, 0 }
                });

            migrationBuilder.InsertData(
                table: "UnitTypes",
                columns: new[] { "Id", "Description", "IsActive", "Name" },
                values: new object[,]
                {
                    { 1, "Traditional hotel accommodation with various amenities", true, "Hotel" },
                    { 2, "Self-contained apartment for longer stays", true, "Apartment" },
                    { 3, "Luxury villa with private amenities", true, "Villa" },
                    { 4, "Full-service resort with comprehensive facilities", true, "Resort" },
                    { 5, "Budget-friendly hostel accommodation", true, "Hostel" },
                    { 6, "Cozy guest house with homely atmosphere", true, "Guest House" },
                    { 7, "A wedding Hall", true, "Hall" },
                    { 8, "A Camp", true, "A Camp" },
                    { 9, "A Chalet", true, "Chalet" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Amenities_Category",
                table: "Amenities",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_Amenities_Name",
                table: "Amenities",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_BookingCoupons_BookingId",
                table: "BookingCoupons",
                column: "BookingId");

            migrationBuilder.CreateIndex(
                name: "IX_BookingCoupons_CouponId",
                table: "BookingCoupons",
                column: "CouponId");

            migrationBuilder.CreateIndex(
                name: "IX_BookingRoom_RoomId_BookingId",
                table: "BookingRoom",
                columns: new[] { "RoomId", "BookingId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_BookingNumber",
                table: "Bookings",
                column: "BookingNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_CheckInDate",
                table: "Bookings",
                column: "CheckInDate");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_CheckInDate_CheckOutDate",
                table: "Bookings",
                columns: new[] { "CheckInDate", "CheckOutDate" });

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_CheckOutDate",
                table: "Bookings",
                column: "CheckOutDate");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_CreatedAt",
                table: "Bookings",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_PaymentStatus",
                table: "Bookings",
                column: "PaymentStatus");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_Status",
                table: "Bookings",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_UnitId",
                table: "Bookings",
                column: "UnitId");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_UnitId_CheckInDate_CheckOutDate",
                table: "Bookings",
                columns: new[] { "UnitId", "CheckInDate", "CheckOutDate" },
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_UserId",
                table: "Bookings",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_UserId_Status_CheckInDate",
                table: "Bookings",
                columns: new[] { "UserId", "Status", "CheckInDate" },
                filter: "[IsDeleted] = 0")
                .Annotation("SqlServer:Include", new[] { "TotalPrice", "PaymentStatus" });

            migrationBuilder.CreateIndex(
                name: "IX_Coupons_TargetCityId",
                table: "Coupons",
                column: "TargetCityId");

            migrationBuilder.CreateIndex(
                name: "IX_Coupons_TargetUnitId",
                table: "Coupons",
                column: "TargetUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_Coupons_TargetUnitTypeId",
                table: "Coupons",
                column: "TargetUnitTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_DepartmentAdmins_CityId",
                table: "DepartmentAdmins",
                column: "CityId",
                unique: true,
                filter: "[IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_DepartmentAdmins_UserId_CityId",
                table: "DepartmentAdmins",
                columns: new[] { "UserId", "CityId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DepartmentImage_Dept_DisplayOrder",
                table: "DepartmentImages",
                columns: new[] { "DepartmentId", "DisplayOrder", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_DepartmentImage_Dept_Primary",
                table: "DepartmentImages",
                columns: new[] { "DepartmentId", "IsPrimary", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_DepartmentImage_S3Key",
                table: "DepartmentImages",
                column: "S3Key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DepartmentImages_DeletedById",
                table: "DepartmentImages",
                column: "DeletedById");

            migrationBuilder.CreateIndex(
                name: "IX_DepartmentImages_UploadedByUserId",
                table: "DepartmentImages",
                column: "UploadedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Departments_IsActive",
                table: "Departments",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Departments_Name",
                table: "Departments",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_GeneralPolicies_CancellationPolicyId",
                table: "GeneralPolicies",
                column: "CancellationPolicyId");

            migrationBuilder.CreateIndex(
                name: "IX_GeneralPolicies_SubUnitId",
                table: "GeneralPolicies",
                column: "SubUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_GeneralPolicies_UnitId",
                table: "GeneralPolicies",
                column: "UnitId");

            migrationBuilder.CreateIndex(
                name: "IX_LoyaltyPrograms_UserId",
                table: "LoyaltyPrograms",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_LoyaltyTransactions_BookingId",
                table: "LoyaltyTransactions",
                column: "BookingId");

            migrationBuilder.CreateIndex(
                name: "IX_LoyaltyTransactions_LoyaltyProgramId",
                table: "LoyaltyTransactions",
                column: "LoyaltyProgramId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_CreatedAt",
                table: "Notifications",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_CreatedByUserId",
                table: "Notifications",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_ExpiresAt",
                table: "Notifications",
                column: "ExpiresAt",
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_IsScheduled",
                table: "Notifications",
                column: "IsScheduled");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_IsScheduled_ScheduledFor_IsSent",
                table: "Notifications",
                columns: new[] { "IsScheduled", "ScheduledFor", "IsSent" },
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_IsSent",
                table: "Notifications",
                column: "IsSent");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_IsSent_RetryCount_LastRetryAt",
                table: "Notifications",
                columns: new[] { "IsSent", "RetryCount", "LastRetryAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_Priority",
                table: "Notifications",
                column: "Priority");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_ScheduledFor",
                table: "Notifications",
                column: "ScheduledFor");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_Target",
                table: "Notifications",
                column: "Target");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_TargetDepartmentId",
                table: "Notifications",
                column: "TargetDepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_TargetUnitId",
                table: "Notifications",
                column: "TargetUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_Type",
                table: "Notifications",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_BookingId",
                table: "Payments",
                column: "BookingId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_Status",
                table: "Payments",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_TransactionId",
                table: "Payments",
                column: "TransactionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PricingRules_UnitId",
                table: "PricingRules",
                column: "UnitId");

            migrationBuilder.CreateIndex(
                name: "IX_ReviewImage_Review_DisplayOrder",
                table: "ReviewImages",
                columns: new[] { "ReviewId", "DisplayOrder", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_ReviewImage_S3Key",
                table: "ReviewImages",
                column: "S3Key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReviewImages_DeletedById",
                table: "ReviewImages",
                column: "DeletedById");

            migrationBuilder.CreateIndex(
                name: "IX_ReviewImages_UploadedByUserId",
                table: "ReviewImages",
                column: "UploadedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_BookingId",
                table: "Reviews",
                column: "BookingId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_CreatedAt",
                table: "Reviews",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_IsVerified",
                table: "Reviews",
                column: "IsVerified");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_UnitId",
                table: "Reviews",
                column: "UnitId");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_UserId",
                table: "Reviews",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SubUnitAvailabilities_UpdatedByUserId",
                table: "SubUnitAvailabilities",
                column: "UpdatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SubUnitAvailability_DateRange",
                table: "SubUnitAvailabilities",
                columns: new[] { "StartDate", "EndDate", "IsAvailable" })
                .Annotation("SqlServer:Include", new[] { "SubUnitId", "SpecialPrice" });

            migrationBuilder.CreateIndex(
                name: "IX_SubUnitAvailability_Search",
                table: "SubUnitAvailabilities",
                columns: new[] { "SubUnitId", "StartDate", "EndDate", "IsAvailable" });

            migrationBuilder.CreateIndex(
                name: "IX_SubUnitAvailability_SubUnitId_Date",
                table: "SubUnitAvailabilities",
                columns: new[] { "SubUnitId", "StartDate", "EndDate" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SubUniteAmenities_AmenityId",
                table: "SubUniteAmenities",
                column: "AmenityId");

            migrationBuilder.CreateIndex(
                name: "IX_SubUniteAmenities_SubUnitId",
                table: "SubUniteAmenities",
                column: "SubUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_SubUnitImage_S3Key",
                table: "SubUnitImages",
                column: "S3Key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SubUnitImage_SubUnit_DisplayOrder",
                table: "SubUnitImages",
                columns: new[] { "SubUnitId", "DisplayOrder", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_SubUnitImage_SubUnit_Primary",
                table: "SubUnitImages",
                columns: new[] { "SubUnitId", "IsPrimary", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_SubUnitImage_SubUnit_Type",
                table: "SubUnitImages",
                columns: new[] { "SubUnitId", "ImageType", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_SubUnitImages_DeletedById",
                table: "SubUnitImages",
                column: "DeletedById");

            migrationBuilder.CreateIndex(
                name: "IX_SubUnitImages_UploadedByUserId",
                table: "SubUnitImages",
                column: "UploadedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SubUnits_RoomConfigId",
                table: "SubUnits",
                column: "RoomConfigId");

            migrationBuilder.CreateIndex(
                name: "IX_SubUnits_UnitId_IsAvailable",
                table: "SubUnits",
                columns: new[] { "UnitId", "IsAvailable" },
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_SubUnits_UnitId_RoomNumber",
                table: "SubUnits",
                columns: new[] { "UnitId", "RoomNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UnitAmenities_AmenityId",
                table: "UnitAmenities",
                column: "AmenityId");

            migrationBuilder.CreateIndex(
                name: "IX_UnitAvailabilities_UnitId",
                table: "UnitAvailabilities",
                column: "UnitId");

            migrationBuilder.CreateIndex(
                name: "IX_UnitAvailabilities_UpdatedById",
                table: "UnitAvailabilities",
                column: "UpdatedById");

            migrationBuilder.CreateIndex(
                name: "IX_UniteAdmin_UnitId",
                table: "UniteAdmin",
                column: "UnitId");

            migrationBuilder.CreateIndex(
                name: "IX_UniteAdmin_UserId",
                table: "UniteAdmin",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UnitImage_ProcessingStatus",
                table: "UnitImages",
                column: "ProcessingStatus",
                filter: "[ProcessingStatus] != 2");

            migrationBuilder.CreateIndex(
                name: "IX_UnitImage_S3Key",
                table: "UnitImages",
                column: "S3Key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UnitImage_Unit_DisplayOrder",
                table: "UnitImages",
                columns: new[] { "UnitId", "DisplayOrder", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_UnitImage_Unit_Primary",
                table: "UnitImages",
                columns: new[] { "UnitId", "IsPrimary", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_UnitImage_Unit_Type",
                table: "UnitImages",
                columns: new[] { "UnitId", "ImageType", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_UnitImages_DeletedByUserId",
                table: "UnitImages",
                column: "DeletedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_UnitImages_UploadedByUserId",
                table: "UnitImages",
                column: "UploadedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Units_AverageRating",
                table: "Units",
                column: "AverageRating");

            migrationBuilder.CreateIndex(
                name: "IX_Units_CancellationPolicyId",
                table: "Units",
                column: "CancellationPolicyId");

            migrationBuilder.CreateIndex(
                name: "IX_Units_CityId",
                table: "Units",
                column: "CityId");

            migrationBuilder.CreateIndex(
                name: "IX_Units_CityId_IsActive_IsVerified",
                table: "Units",
                columns: new[] { "CityId", "IsActive", "IsVerified" },
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Units_IsActive",
                table: "Units",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Units_IsVerified",
                table: "Units",
                column: "IsVerified");

            migrationBuilder.CreateIndex(
                name: "IX_Units_Latitude_Longitude",
                table: "Units",
                columns: new[] { "Latitude", "Longitude" });

            migrationBuilder.CreateIndex(
                name: "IX_Units_Name",
                table: "Units",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Units_UnitTypeId",
                table: "Units",
                column: "UnitTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_UnitTypes_IsActive",
                table: "UnitTypes",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_UnitTypes_Name",
                table: "UnitTypes",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_UserFavorites_UnitId",
                table: "UserFavorites",
                column: "UnitId");

            migrationBuilder.CreateIndex(
                name: "IX_UserFavorites_UserId",
                table: "UserFavorites",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserNotifications_NotificationId",
                table: "UserNotifications",
                column: "NotificationId");

            migrationBuilder.CreateIndex(
                name: "IX_UserNotifications_ReceivedAt",
                table: "UserNotifications",
                column: "ReceivedAt");

            migrationBuilder.CreateIndex(
                name: "IX_UserNotifications_UserId_IsRead",
                table: "UserNotifications",
                columns: new[] { "UserId", "IsRead" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BookingCoupons");

            migrationBuilder.DropTable(
                name: "BookingRoom");

            migrationBuilder.DropTable(
                name: "DepartmentAdmins");

            migrationBuilder.DropTable(
                name: "DepartmentImages");

            migrationBuilder.DropTable(
                name: "GeneralPolicies");

            migrationBuilder.DropTable(
                name: "LoyaltyTransactions");

            migrationBuilder.DropTable(
                name: "Payments");

            migrationBuilder.DropTable(
                name: "PricingRules");

            migrationBuilder.DropTable(
                name: "ReviewImages");

            migrationBuilder.DropTable(
                name: "SubUnitAvailabilities");

            migrationBuilder.DropTable(
                name: "SubUniteAmenities");

            migrationBuilder.DropTable(
                name: "SubUnitImages");

            migrationBuilder.DropTable(
                name: "UnitAmenities");

            migrationBuilder.DropTable(
                name: "UnitAvailabilities");

            migrationBuilder.DropTable(
                name: "UniteAdmin");

            migrationBuilder.DropTable(
                name: "UnitImages");

            migrationBuilder.DropTable(
                name: "UserFavorites");

            migrationBuilder.DropTable(
                name: "UserNotifications");

            migrationBuilder.DropTable(
                name: "Coupons");

            migrationBuilder.DropTable(
                name: "LoyaltyPrograms");

            migrationBuilder.DropTable(
                name: "Reviews");

            migrationBuilder.DropTable(
                name: "SubUnits");

            migrationBuilder.DropTable(
                name: "Amenities");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "Bookings");

            migrationBuilder.DropTable(
                name: "RoomConfigurations");

            migrationBuilder.DropTable(
                name: "Units");

            migrationBuilder.DropTable(
                name: "CancellationPolicies");

            migrationBuilder.DropTable(
                name: "Departments");

            migrationBuilder.DropTable(
                name: "UnitTypes");

            migrationBuilder.DropColumn(
                name: "AvatarUploadedAt",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "AvatarUrl",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Bio",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "DateOfBirth",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "LastLoginAt",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Nationality",
                table: "AspNetUsers");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "59724D2D-E2B5-4C67-AB6F-D93478147B03",
                column: "PasswordHash",
                value: "AQAAAAIAAYagAAAAENrvU5zX448CSlTe9YOQygtdS7fnp2nUzi8bbad+QWNXRYEOY1i0M2Pg+82rjDezxQ==");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "59726D2D-E2B5-4C67-AB6F-D93878317B03",
                column: "PasswordHash",
                value: "AQAAAAIAAYagAAAAED2i7SFahYiDm5ok8UJgLO7PTDA6JEtkECvXaEfofoEl8dLUkNzRH9HNTI4TSzB7Ww==");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "59726D2D-E2B5-4C67-AB6F-D99478317B03",
                column: "PasswordHash",
                value: "AQAAAAIAAYagAAAAEIENitpeJg021gN1s6nu9sWnVD9X42S4hYNnJ6IB+mWqPkq9BrMtlfRj9Gg61aPDQg==");
        }
    }
}
