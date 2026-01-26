namespace Domain;


public enum BookingStatus
{
    Pending = 1,
    Confirmed = 2,
    CheckedIn = 3,
    CheckedOut = 4,
    Completed = 5,
    Cancelled = 6,
    NoShow = 7
}

public enum PaymentStatus
{
    Pending = 1,
    Paid = 2,
    PartiallyPaid = 3,
    Refunded = 4,
    Failed = 5
}

public enum PaymentMethod
{
    CreditCard = 1,
    DebitCard = 2,
    PayPal = 3,
    BankTransfer = 4,
    Cash = 5,
    Other = 6
}

public enum NotificationType
{
    General = 1,
    BookingConfirmation = 2,
    BookingReminder = 3,
    PaymentReceived = 4,
    ReviewRequest = 5,
    SpecialOffer = 6,
    SystemUpdate = 7,
    AdminAnnouncement = 8
}

public enum NotificationPriority
{
    Low = 1,
    Normal = 2,
    High = 3,
    Urgent = 4
}

public enum NotificationTarget
{
    AllUsers = 1,
    SpecificUser = 2,
    City = 3,
    Unit = 4,
    Role = 5,
    HotelAdmins = 6
}

public enum CouponType
{
    FixedAmount = 0,
    Percentage = 1,
    FreeNight = 2
}

public enum PricingRuleType
{
    EarlyBird = 0,      // Book X days in advance
    LastMinute = 1,     // Book within X days
    LongStay = 2,       // Booking X+ nights
    Weekend = 3,        // Fri-Sun pricing
    Seasonal = 4,       // Summer, winter, etc.
    Occupancy = 5       // Based on current occupancy
}

public enum PricingAdjustmentType
{
    PercentageIncrease = 0,
    PercentageDecrease = 1,
    FixedIncrease = 2,
    FixedDecrease = 3
}

public enum LoyaltyTier
{
    Bronze = 0,
    Silver = 1,
    Gold = 2,
    Platinum = 3
}

public enum SubUnitType
{

    // Hotels
    StandardRoom,
    DeluxeRoom,
    Suite,
    ExecutiveSuite,
    PenthouseSuite,

    // Villas & Resorts
    Villa,
    Cottage,
    Bungalow,
    Cabin,

    // Alternative
    Apartment,
    Studio,

    // Camping/Outdoor
    TentSite,
    GlamingTent,
    RVSpace,

    // Event spaces
    Hall,
    Chalet,

    // Beds in shared spaces (hostels)
    DormBed,
    PrivateRoom
}

public enum AmenityCategory
{
    // General / Property level
    Basic,
    Services,
    Cleanliness,
    Transportation,

    // Rooms
    Room,
    Bedroom,
    Bathroom,
    Kitchen,

    // Food & Beverage
    FoodAndDrink,

    // Entertainment & Leisure
    Entertainment,
    Wellness,
    Outdoor,
    Activities,

    // Business & Events
    Business,
    Events,

    // Safety & Security
    Safety,

    // Accessibility
    Accessibility,

    // Pets
    Pets
}

public enum AmenityName
{
    // ===== BASIC / GENERAL =====
    Wifi,
    FreeWifi,
    PaidWifi,
    AirConditioning,
    Heating,
    ElectricityBackup,
    Parking,
    FreeParking,
    PaidParking,
    ValetParking,
    StreetParking,
    GarageParking,
    Elevator,
    LuggageStorage,
    Reception24Hours,
    ExpressCheckIn,
    ExpressCheckOut,
    Concierge,
    WakeUpService,
    DailyHousekeeping,
    LaundryService,
    DryCleaning,
    IroningService,
    CurrencyExchange,
    ATMOnSite,
    GiftShop,
    MiniMarket,
    SmokingArea,
    NonSmokingRooms,
    FamilyRooms,
    SoundproofRooms,

    // ===== ROOM FEATURES =====
    TV,
    SmartTV,
    CableTV,
    SatelliteTV,
    StreamingServices,
    Desk,
    SeatingArea,
    Sofa,
    Wardrobe,
    Closet,
    Minibar,
    Refrigerator,
    Microwave,
    ElectricKettle,
    CoffeeMachine,
    TeaMaker,
    DiningTable,
    SafeBox,
    Iron,
    IroningBoard,
    AlarmClock,
    CarpetedFloor,
    HardwoodFloor,
    PrivateEntrance,
    Balcony,
    Terrace,
    Patio,
    GardenView,
    CityView,
    SeaView,
    MountainView,
    PoolView,

    // ===== BEDROOM =====
    ExtraLongBeds,
    SofaBed,
    BabyCot,
    CribsAvailable,
    HypoallergenicBedding,
    BlackoutCurtains,

    // ===== BATHROOM =====
    PrivateBathroom,
    SharedBathroom,
    Shower,
    WalkInShower,
    Bathtub,
    JacuzziBathtub,
    Bidet,
    Toilet,
    ToiletPaper,
    Towels,
    Bathrobes,
    Slippers,
    HairDryer,
    FreeToiletries,
    Shampoo,
    Conditioner,
    BodySoap,

    // ===== KITCHEN =====
    Kitchen,
    Kitchenette,
    Oven,
    Stove,
    Dishwasher,
    WashingMachine,
    Dryer,
    Toaster,
    Blender,
    CookingUtensils,

    // ===== FOOD & DRINK =====
    Restaurant,
    BuffetRestaurant,
    ALaCarteRestaurant,
    RoomService,
    BreakfastIncluded,
    BreakfastBuffet,
    ContinentalBreakfast,
    HalalFood,
    VegetarianFood,
    VeganOptions,
    Bar,
    PoolBar,
    SnackBar,
    Cafe,
    CoffeeShop,
    VendingMachines,
    GroceryDelivery,

    // ===== ENTERTAINMENT & LEISURE =====
    SwimmingPool,
    OutdoorPool,
    IndoorPool,
    HeatedPool,
    InfinityPool,
    KidsPool,
    WaterPark,
    Gym,
    FitnessCenter,
    PersonalTrainer,
    Spa,
    WellnessCenter,
    Sauna,
    SteamRoom,
    Hammam,
    Jacuzzi,
    MassageService,
    BeautySalon,
    YogaClasses,
    Aerobics,
    NightClub,
    LiveMusic,
    DJ,
    CinemaRoom,
    GameRoom,
    Billiards,
    TableTennis,
    Bowling,
    Darts,
    Karaoke,
    Library,
    TVLounge,
    KidsPlayArea,
    KidsClub,
    BabysittingService,

    // ===== OUTDOOR & ACTIVITIES =====
    Garden,
    SunTerrace,
    PicnicArea,
    BBQFacilities,
    BeachAccess,
    PrivateBeach,
    BeachUmbrellas,
    BeachChairs,
    WaterSports,
    Diving,
    Snorkeling,
    Canoeing,
    Fishing,
    HorseRiding,
    Hiking,
    Cycling,
    BikeRental,

    // ===== BUSINESS & EVENTS =====
    BusinessCenter,
    MeetingRooms,
    ConferenceHall,
    BanquetHall,
    WeddingServices,
    Fax,
    Photocopying,
    HighSpeedInternet,

    // ===== TRANSPORTATION =====
    AirportShuttle,
    PaidAirportShuttle,
    CarRental,
    TaxiService,
    ShuttleService,
    PublicTransportNearby,
    EVChargingStation,

    // ===== SAFETY & SECURITY =====
    FireExtinguishers,
    SmokeDetectors,
    CarbonMonoxideDetector,
    CCTV,
    Security24Hours,
    KeyCardAccess,
    ElectronicDoorLocks,
    SafeDepositBox,
    FirstAidKit,
    EmergencyExitSigns,

    // ===== ACCESSIBILITY =====
    WheelchairAccessible,
    AccessibleParking,
    AccessibleBathroom,
    GrabRails,
    LoweredSink,
    BrailleSignage,
    VisualAids,
    HearingAccessible,

    // ===== PETS =====
    PetsAllowed,
    PetsNotAllowed,
    PetBowls,
    PetBasket,
    PetSittingService,

    // ===== SERVICES =====
    TourDesk,
    TicketService,
    TourOrganization,
    LocalGuides,
    Lockers,

    // ===== CLEANLINESS & HEALTH =====
    EnhancedCleaning,
    ContactlessCheckIn,
    HandSanitizer,
    TemperatureCheck,
    MedicalAssistance
}

public enum GeneralPolicyCategory
{
    HouseRules,
    CheckInAndCheckOut,
    CancellationAndRefund,
    Payment,
    ChildrenAndBeds,
    Pets,
    Smoking,
    DamageAndLiability,
    SafetyAndSecurity,
    QuietHours,
    Accessibility,
    FacilitiesUsage,
    LegalAndCompliance,
    Other
}

public enum GeneralPolicyName
{
    // ===== CHECK-IN / CHECK-OUT =====
    CheckInTime,
    CheckOutTime,
    EarlyCheckIn,
    LateCheckOut,
    SelfCheckIn,
    IDRequirement,
    MinimumAge,

    // ===== CANCELLATION & PAYMENT =====
    FreeCancellation,
    NonRefundable,
    PartialRefund,
    PaymentMethods,
    PrepaymentRequired,
    DepositRequired,
    TaxesAndFees,
    CityTax,

    // ===== HOUSE RULES =====
    QuietHours,
    PartiesAndEvents,
    VisitorsPolicy,
    Curfew,
    MaximumOccupancy,

    // ===== CHILDREN & BEDS =====
    ChildrenAllowed,
    ChildrenFreeStay,
    ExtraBeds,
    BabyCots,
    AgeRestrictions,

    // ===== PETS =====
    PetsAllowed,
    PetsNotAllowed,
    PetFees,
    PetRestrictions,

    // ===== SMOKING =====
    SmokingAllowed,
    NonSmokingProperty,
    SmokingAreas,

    // ===== DAMAGE & LIABILITY =====
    DamageDeposit,
    LiabilityDisclaimer,
    LostAndFound,
    PropertyDamage,

    // ===== SAFETY & SECURITY =====
    FireSafety,
    EmergencyProcedures,
    CCTVPolicy,
    SecurityPolicy,

    // ===== ACCESSIBILITY =====
    WheelchairAccess,
    AccessibleRooms,
    ServiceAnimals,

    // ===== FACILITY USAGE =====
    PoolRules,
    GymRules,
    SpaRules,
    ParkingRules,
    ElevatorRules,

    // ===== LEGAL & COMPLIANCE =====
    LocalLaws,
    GovernmentID,
    DataPrivacy,
    TermsAndConditions,

    // ===== OTHER =====
    ForceMajeure,
    SpecialRequests,
    Disclaimer
}
