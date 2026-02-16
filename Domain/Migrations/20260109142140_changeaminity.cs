using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Domain.Migrations
{
    /// <inheritdoc />
    public partial class changeaminity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Amenities",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Category",
                table: "Amenities",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldMaxLength: 50);

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Basic", "Wifi" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Basic", "FreeWifi" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Basic", "PaidWifi" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Basic", "AirConditioning" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Basic", "Heating" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Basic", "ElectricityBackup" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Basic", "Parking" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Basic", "FreeParking" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Basic", "PaidParking" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 10,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Basic", "ValetParking" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 11,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Basic", "StreetParking" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 12,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Basic", "GarageParking" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 13,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Basic", "Elevator" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 14,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Basic", "LuggageStorage" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 15,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Basic", "Reception24Hours" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 16,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Basic", "ExpressCheckIn" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 17,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Basic", "ExpressCheckOut" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 18,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Basic", "Concierge" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 19,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Basic", "WakeUpService" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 20,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Basic", "DailyHousekeeping" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 21,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Basic", "LaundryService" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 22,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Basic", "DryCleaning" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 23,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Basic", "IroningService" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 24,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Basic", "CurrencyExchange" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 25,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Basic", "ATMOnSite" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 26,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Basic", "GiftShop" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 27,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Basic", "MiniMarket" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 28,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Basic", "SmokingArea" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 29,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Basic", "NonSmokingRooms" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 30,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Basic", "FamilyRooms" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 31,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Basic", "SoundproofRooms" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 32,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Room", "TV" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 33,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Room", "SmartTV" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 34,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Room", "CableTV" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 35,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Room", "SatelliteTV" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 36,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Room", "StreamingServices" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 37,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Room", "Desk" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 38,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Room", "SeatingArea" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 39,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Room", "Sofa" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 40,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Room", "Wardrobe" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 41,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Room", "Closet" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 42,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Room", "Minibar" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 43,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Room", "Refrigerator" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 44,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Room", "Microwave" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 45,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Room", "ElectricKettle" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 46,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Room", "CoffeeMachine" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 47,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Room", "TeaMaker" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 48,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Room", "DiningTable" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 49,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Room", "SafeBox" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 50,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Room", "Iron" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 51,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Room", "IroningBoard" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 52,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Room", "AlarmClock" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 53,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Room", "CarpetedFloor" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 54,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Room", "HardwoodFloor" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 55,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Room", "PrivateEntrance" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 56,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Room", "Balcony" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 57,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Room", "Terrace" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 58,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Room", "Patio" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 59,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Room", "GardenView" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 60,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Room", "CityView" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 61,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Room", "SeaView" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 62,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Room", "MountainView" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 63,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Room", "PoolView" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 64,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Bedroom", "ExtraLongBeds" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 65,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Bedroom", "SofaBed" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 66,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Bedroom", "BabyCot" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 67,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Bedroom", "CribsAvailable" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 68,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Bedroom", "HypoallergenicBedding" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 69,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Bedroom", "BlackoutCurtains" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 70,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Bathroom", "PrivateBathroom" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 71,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Bathroom", "SharedBathroom" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 72,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Bathroom", "Shower" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 73,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Bathroom", "WalkInShower" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 74,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Bathroom", "Bathtub" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 75,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Bathroom", "JacuzziBathtub" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 76,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Bathroom", "Bidet" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 77,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Bathroom", "Toilet" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 78,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Bathroom", "ToiletPaper" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 79,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Bathroom", "Towels" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 80,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Bathroom", "Bathrobes" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 81,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Bathroom", "Slippers" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 82,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Bathroom", "HairDryer" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 83,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Bathroom", "FreeToiletries" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 84,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Bathroom", "Shampoo" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 85,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Bathroom", "Conditioner" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 86,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Bathroom", "BodySoap" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 87,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Kitchen", "Kitchen" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 88,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Kitchen", "Kitchenette" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 89,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Kitchen", "Oven" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 90,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Kitchen", "Stove" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 91,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Kitchen", "Dishwasher" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 92,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Kitchen", "WashingMachine" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 93,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Kitchen", "Dryer" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 94,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Kitchen", "Toaster" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 95,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Kitchen", "Blender" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 96,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Kitchen", "CookingUtensils" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 97,
                columns: new[] { "Category", "Name" },
                values: new object[] { "FoodAndDrink", "Restaurant" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 98,
                columns: new[] { "Category", "Name" },
                values: new object[] { "FoodAndDrink", "BuffetRestaurant" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 99,
                columns: new[] { "Category", "Name" },
                values: new object[] { "FoodAndDrink", "ALaCarteRestaurant" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 100,
                columns: new[] { "Category", "Name" },
                values: new object[] { "FoodAndDrink", "RoomService" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 101,
                columns: new[] { "Category", "Name" },
                values: new object[] { "FoodAndDrink", "BreakfastIncluded" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 102,
                columns: new[] { "Category", "Name" },
                values: new object[] { "FoodAndDrink", "BreakfastBuffet" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 103,
                columns: new[] { "Category", "Name" },
                values: new object[] { "FoodAndDrink", "ContinentalBreakfast" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 104,
                columns: new[] { "Category", "Name" },
                values: new object[] { "FoodAndDrink", "HalalFood" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 105,
                columns: new[] { "Category", "Name" },
                values: new object[] { "FoodAndDrink", "VegetarianFood" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 106,
                columns: new[] { "Category", "Name" },
                values: new object[] { "FoodAndDrink", "VeganOptions" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 107,
                columns: new[] { "Category", "Name" },
                values: new object[] { "FoodAndDrink", "Bar" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 108,
                columns: new[] { "Category", "Name" },
                values: new object[] { "FoodAndDrink", "PoolBar" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 109,
                columns: new[] { "Category", "Name" },
                values: new object[] { "FoodAndDrink", "SnackBar" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 110,
                columns: new[] { "Category", "Name" },
                values: new object[] { "FoodAndDrink", "Cafe" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 111,
                columns: new[] { "Category", "Name" },
                values: new object[] { "FoodAndDrink", "CoffeeShop" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 112,
                columns: new[] { "Category", "Name" },
                values: new object[] { "FoodAndDrink", "VendingMachines" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 113,
                columns: new[] { "Category", "Name" },
                values: new object[] { "FoodAndDrink", "GroceryDelivery" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 114,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Entertainment", "SwimmingPool" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 115,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Entertainment", "OutdoorPool" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 116,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Entertainment", "IndoorPool" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 117,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Entertainment", "HeatedPool" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 118,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Entertainment", "InfinityPool" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 119,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Entertainment", "KidsPool" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 120,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Entertainment", "WaterPark" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 121,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Wellness", "Gym" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 122,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Wellness", "FitnessCenter" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 123,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Wellness", "PersonalTrainer" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 124,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Wellness", "Spa" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 125,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Wellness", "WellnessCenter" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 126,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Wellness", "Sauna" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 127,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Wellness", "SteamRoom" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 128,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Wellness", "Hammam" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 129,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Wellness", "Jacuzzi" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 130,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Wellness", "MassageService" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 131,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Wellness", "BeautySalon" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 132,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Wellness", "YogaClasses" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 133,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Wellness", "Aerobics" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 134,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Entertainment", "NightClub" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 135,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Entertainment", "LiveMusic" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 136,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Entertainment", "DJ" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 137,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Entertainment", "CinemaRoom" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 138,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Entertainment", "GameRoom" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 139,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Entertainment", "Billiards" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 140,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Entertainment", "TableTennis" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 141,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Entertainment", "Bowling" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 142,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Entertainment", "Darts" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 143,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Entertainment", "Karaoke" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 144,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Entertainment", "Library" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 145,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Entertainment", "TVLounge" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 146,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Entertainment", "KidsPlayArea" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 147,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Entertainment", "KidsClub" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 148,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Entertainment", "BabysittingService" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 149,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Outdoor", "Garden" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 150,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Outdoor", "SunTerrace" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 151,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Outdoor", "PicnicArea" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 152,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Outdoor", "BBQFacilities" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 153,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Outdoor", "BeachAccess" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 154,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Outdoor", "PrivateBeach" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 155,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Outdoor", "BeachUmbrellas" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 156,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Outdoor", "BeachChairs" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 157,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Activities", "WaterSports" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 158,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Activities", "Diving" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 159,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Activities", "Snorkeling" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 160,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Activities", "Canoeing" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 161,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Activities", "Fishing" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 162,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Activities", "HorseRiding" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 163,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Activities", "Hiking" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 164,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Activities", "Cycling" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 165,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Activities", "BikeRental" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 166,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Business", "BusinessCenter" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 167,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Business", "MeetingRooms" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 168,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Business", "ConferenceHall" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 169,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Business", "BanquetHall" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 170,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Events", "WeddingServices" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 171,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Business", "Fax" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 172,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Business", "Photocopying" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 173,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Business", "HighSpeedInternet" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 174,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Transportation", "AirportShuttle" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 175,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Transportation", "PaidAirportShuttle" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 176,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Transportation", "CarRental" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 177,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Transportation", "TaxiService" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 178,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Transportation", "ShuttleService" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 179,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Transportation", "PublicTransportNearby" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 180,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Transportation", "EVChargingStation" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 181,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Safety", "FireExtinguishers" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 182,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Safety", "SmokeDetectors" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 183,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Safety", "CarbonMonoxideDetector" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 184,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Safety", "CCTV" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 185,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Safety", "Security24Hours" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 186,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Safety", "KeyCardAccess" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 187,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Safety", "ElectronicDoorLocks" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 188,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Safety", "SafeDepositBox" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 189,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Safety", "FirstAidKit" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 190,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Safety", "EmergencyExitSigns" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 191,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Accessibility", "WheelchairAccessible" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 192,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Accessibility", "AccessibleParking" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 193,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Accessibility", "AccessibleBathroom" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 194,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Accessibility", "GrabRails" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 195,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Accessibility", "LoweredSink" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 196,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Accessibility", "BrailleSignage" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 197,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Accessibility", "VisualAids" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 198,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Accessibility", "HearingAccessible" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 199,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Pets", "PetsAllowed" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 200,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Pets", "PetsNotAllowed" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 201,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Pets", "PetBowls" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 202,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Pets", "PetBasket" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 203,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Pets", "PetSittingService" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 204,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Services", "TourDesk" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 205,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Services", "TicketService" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 206,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Services", "TourOrganization" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 207,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Services", "LocalGuides" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 208,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Services", "Lockers" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 209,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Cleanliness", "EnhancedCleaning" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 210,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Cleanliness", "ContactlessCheckIn" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 211,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Cleanliness", "HandSanitizer" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 212,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Cleanliness", "TemperatureCheck" });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 213,
                columns: new[] { "Category", "Name" },
                values: new object[] { "Cleanliness", "MedicalAssistance" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "59724D2D-E2B5-4C67-AB6F-D93478147B03",
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 1, 9, 14, 21, 38, 799, DateTimeKind.Utc).AddTicks(5012), "AQAAAAIAAYagAAAAEJXFbsHNrWxmEfTXXzc+97oZYNIoF7h5vEkPCUq5R5KwvmCkpUZzU1fGupIrnpVu4A==" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "59726D2D-E2B5-4C67-AB6F-D93878317B03",
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 1, 9, 14, 21, 38, 858, DateTimeKind.Utc).AddTicks(8273), "AQAAAAIAAYagAAAAEDRjA1XrY8Zkxv5VU7AVuL22nltFx32q3H5QXeNhkWvnP8169REWhqoRAMDlvybatg==" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "59726D2D-E2B5-4C67-AB6F-D99478317B03",
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 1, 9, 14, 21, 38, 913, DateTimeKind.Utc).AddTicks(9442), "AQAAAAIAAYagAAAAECX5lALQCuUDEQl7Fa6Kh3zKSoFEnIqpEUa/rzJXgR/3ySsBWt3HyqeUcEbfCrjE+w==" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "Name",
                table: "Amenities",
                type: "int",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<int>(
                name: "Category",
                table: "Amenities",
                type: "int",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Category", "Name" },
                values: new object[] { 0, 0 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Category", "Name" },
                values: new object[] { 0, 1 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Category", "Name" },
                values: new object[] { 0, 2 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "Category", "Name" },
                values: new object[] { 0, 3 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "Category", "Name" },
                values: new object[] { 0, 4 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "Category", "Name" },
                values: new object[] { 0, 5 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "Category", "Name" },
                values: new object[] { 0, 6 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "Category", "Name" },
                values: new object[] { 0, 7 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "Category", "Name" },
                values: new object[] { 0, 8 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 10,
                columns: new[] { "Category", "Name" },
                values: new object[] { 0, 9 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 11,
                columns: new[] { "Category", "Name" },
                values: new object[] { 0, 10 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 12,
                columns: new[] { "Category", "Name" },
                values: new object[] { 0, 11 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 13,
                columns: new[] { "Category", "Name" },
                values: new object[] { 0, 12 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 14,
                columns: new[] { "Category", "Name" },
                values: new object[] { 0, 13 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 15,
                columns: new[] { "Category", "Name" },
                values: new object[] { 0, 14 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 16,
                columns: new[] { "Category", "Name" },
                values: new object[] { 0, 15 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 17,
                columns: new[] { "Category", "Name" },
                values: new object[] { 0, 16 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 18,
                columns: new[] { "Category", "Name" },
                values: new object[] { 0, 17 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 19,
                columns: new[] { "Category", "Name" },
                values: new object[] { 0, 18 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 20,
                columns: new[] { "Category", "Name" },
                values: new object[] { 0, 19 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 21,
                columns: new[] { "Category", "Name" },
                values: new object[] { 0, 20 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 22,
                columns: new[] { "Category", "Name" },
                values: new object[] { 0, 21 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 23,
                columns: new[] { "Category", "Name" },
                values: new object[] { 0, 22 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 24,
                columns: new[] { "Category", "Name" },
                values: new object[] { 0, 23 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 25,
                columns: new[] { "Category", "Name" },
                values: new object[] { 0, 24 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 26,
                columns: new[] { "Category", "Name" },
                values: new object[] { 0, 25 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 27,
                columns: new[] { "Category", "Name" },
                values: new object[] { 0, 26 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 28,
                columns: new[] { "Category", "Name" },
                values: new object[] { 0, 27 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 29,
                columns: new[] { "Category", "Name" },
                values: new object[] { 0, 28 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 30,
                columns: new[] { "Category", "Name" },
                values: new object[] { 0, 29 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 31,
                columns: new[] { "Category", "Name" },
                values: new object[] { 0, 30 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 32,
                columns: new[] { "Category", "Name" },
                values: new object[] { 4, 31 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 33,
                columns: new[] { "Category", "Name" },
                values: new object[] { 4, 32 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 34,
                columns: new[] { "Category", "Name" },
                values: new object[] { 4, 33 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 35,
                columns: new[] { "Category", "Name" },
                values: new object[] { 4, 34 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 36,
                columns: new[] { "Category", "Name" },
                values: new object[] { 4, 35 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 37,
                columns: new[] { "Category", "Name" },
                values: new object[] { 4, 36 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 38,
                columns: new[] { "Category", "Name" },
                values: new object[] { 4, 37 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 39,
                columns: new[] { "Category", "Name" },
                values: new object[] { 4, 38 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 40,
                columns: new[] { "Category", "Name" },
                values: new object[] { 4, 39 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 41,
                columns: new[] { "Category", "Name" },
                values: new object[] { 4, 40 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 42,
                columns: new[] { "Category", "Name" },
                values: new object[] { 4, 41 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 43,
                columns: new[] { "Category", "Name" },
                values: new object[] { 4, 42 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 44,
                columns: new[] { "Category", "Name" },
                values: new object[] { 4, 43 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 45,
                columns: new[] { "Category", "Name" },
                values: new object[] { 4, 44 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 46,
                columns: new[] { "Category", "Name" },
                values: new object[] { 4, 45 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 47,
                columns: new[] { "Category", "Name" },
                values: new object[] { 4, 46 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 48,
                columns: new[] { "Category", "Name" },
                values: new object[] { 4, 47 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 49,
                columns: new[] { "Category", "Name" },
                values: new object[] { 4, 48 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 50,
                columns: new[] { "Category", "Name" },
                values: new object[] { 4, 49 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 51,
                columns: new[] { "Category", "Name" },
                values: new object[] { 4, 50 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 52,
                columns: new[] { "Category", "Name" },
                values: new object[] { 4, 51 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 53,
                columns: new[] { "Category", "Name" },
                values: new object[] { 4, 52 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 54,
                columns: new[] { "Category", "Name" },
                values: new object[] { 4, 53 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 55,
                columns: new[] { "Category", "Name" },
                values: new object[] { 4, 54 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 56,
                columns: new[] { "Category", "Name" },
                values: new object[] { 4, 55 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 57,
                columns: new[] { "Category", "Name" },
                values: new object[] { 4, 56 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 58,
                columns: new[] { "Category", "Name" },
                values: new object[] { 4, 57 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 59,
                columns: new[] { "Category", "Name" },
                values: new object[] { 4, 58 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 60,
                columns: new[] { "Category", "Name" },
                values: new object[] { 4, 59 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 61,
                columns: new[] { "Category", "Name" },
                values: new object[] { 4, 60 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 62,
                columns: new[] { "Category", "Name" },
                values: new object[] { 4, 61 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 63,
                columns: new[] { "Category", "Name" },
                values: new object[] { 4, 62 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 64,
                columns: new[] { "Category", "Name" },
                values: new object[] { 5, 63 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 65,
                columns: new[] { "Category", "Name" },
                values: new object[] { 5, 64 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 66,
                columns: new[] { "Category", "Name" },
                values: new object[] { 5, 65 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 67,
                columns: new[] { "Category", "Name" },
                values: new object[] { 5, 66 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 68,
                columns: new[] { "Category", "Name" },
                values: new object[] { 5, 67 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 69,
                columns: new[] { "Category", "Name" },
                values: new object[] { 5, 68 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 70,
                columns: new[] { "Category", "Name" },
                values: new object[] { 6, 69 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 71,
                columns: new[] { "Category", "Name" },
                values: new object[] { 6, 70 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 72,
                columns: new[] { "Category", "Name" },
                values: new object[] { 6, 71 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 73,
                columns: new[] { "Category", "Name" },
                values: new object[] { 6, 72 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 74,
                columns: new[] { "Category", "Name" },
                values: new object[] { 6, 73 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 75,
                columns: new[] { "Category", "Name" },
                values: new object[] { 6, 74 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 76,
                columns: new[] { "Category", "Name" },
                values: new object[] { 6, 75 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 77,
                columns: new[] { "Category", "Name" },
                values: new object[] { 6, 76 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 78,
                columns: new[] { "Category", "Name" },
                values: new object[] { 6, 77 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 79,
                columns: new[] { "Category", "Name" },
                values: new object[] { 6, 78 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 80,
                columns: new[] { "Category", "Name" },
                values: new object[] { 6, 79 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 81,
                columns: new[] { "Category", "Name" },
                values: new object[] { 6, 80 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 82,
                columns: new[] { "Category", "Name" },
                values: new object[] { 6, 81 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 83,
                columns: new[] { "Category", "Name" },
                values: new object[] { 6, 82 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 84,
                columns: new[] { "Category", "Name" },
                values: new object[] { 6, 83 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 85,
                columns: new[] { "Category", "Name" },
                values: new object[] { 6, 84 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 86,
                columns: new[] { "Category", "Name" },
                values: new object[] { 6, 85 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 87,
                columns: new[] { "Category", "Name" },
                values: new object[] { 7, 86 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 88,
                columns: new[] { "Category", "Name" },
                values: new object[] { 7, 87 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 89,
                columns: new[] { "Category", "Name" },
                values: new object[] { 7, 88 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 90,
                columns: new[] { "Category", "Name" },
                values: new object[] { 7, 89 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 91,
                columns: new[] { "Category", "Name" },
                values: new object[] { 7, 90 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 92,
                columns: new[] { "Category", "Name" },
                values: new object[] { 7, 91 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 93,
                columns: new[] { "Category", "Name" },
                values: new object[] { 7, 92 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 94,
                columns: new[] { "Category", "Name" },
                values: new object[] { 7, 93 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 95,
                columns: new[] { "Category", "Name" },
                values: new object[] { 7, 94 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 96,
                columns: new[] { "Category", "Name" },
                values: new object[] { 7, 95 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 97,
                columns: new[] { "Category", "Name" },
                values: new object[] { 8, 96 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 98,
                columns: new[] { "Category", "Name" },
                values: new object[] { 8, 97 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 99,
                columns: new[] { "Category", "Name" },
                values: new object[] { 8, 98 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 100,
                columns: new[] { "Category", "Name" },
                values: new object[] { 8, 99 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 101,
                columns: new[] { "Category", "Name" },
                values: new object[] { 8, 100 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 102,
                columns: new[] { "Category", "Name" },
                values: new object[] { 8, 101 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 103,
                columns: new[] { "Category", "Name" },
                values: new object[] { 8, 102 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 104,
                columns: new[] { "Category", "Name" },
                values: new object[] { 8, 103 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 105,
                columns: new[] { "Category", "Name" },
                values: new object[] { 8, 104 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 106,
                columns: new[] { "Category", "Name" },
                values: new object[] { 8, 105 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 107,
                columns: new[] { "Category", "Name" },
                values: new object[] { 8, 106 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 108,
                columns: new[] { "Category", "Name" },
                values: new object[] { 8, 107 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 109,
                columns: new[] { "Category", "Name" },
                values: new object[] { 8, 108 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 110,
                columns: new[] { "Category", "Name" },
                values: new object[] { 8, 109 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 111,
                columns: new[] { "Category", "Name" },
                values: new object[] { 8, 110 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 112,
                columns: new[] { "Category", "Name" },
                values: new object[] { 8, 111 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 113,
                columns: new[] { "Category", "Name" },
                values: new object[] { 8, 112 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 114,
                columns: new[] { "Category", "Name" },
                values: new object[] { 9, 113 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 115,
                columns: new[] { "Category", "Name" },
                values: new object[] { 9, 114 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 116,
                columns: new[] { "Category", "Name" },
                values: new object[] { 9, 115 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 117,
                columns: new[] { "Category", "Name" },
                values: new object[] { 9, 116 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 118,
                columns: new[] { "Category", "Name" },
                values: new object[] { 9, 117 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 119,
                columns: new[] { "Category", "Name" },
                values: new object[] { 9, 118 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 120,
                columns: new[] { "Category", "Name" },
                values: new object[] { 9, 119 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 121,
                columns: new[] { "Category", "Name" },
                values: new object[] { 10, 120 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 122,
                columns: new[] { "Category", "Name" },
                values: new object[] { 10, 121 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 123,
                columns: new[] { "Category", "Name" },
                values: new object[] { 10, 122 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 124,
                columns: new[] { "Category", "Name" },
                values: new object[] { 10, 123 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 125,
                columns: new[] { "Category", "Name" },
                values: new object[] { 10, 124 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 126,
                columns: new[] { "Category", "Name" },
                values: new object[] { 10, 125 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 127,
                columns: new[] { "Category", "Name" },
                values: new object[] { 10, 126 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 128,
                columns: new[] { "Category", "Name" },
                values: new object[] { 10, 127 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 129,
                columns: new[] { "Category", "Name" },
                values: new object[] { 10, 128 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 130,
                columns: new[] { "Category", "Name" },
                values: new object[] { 10, 129 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 131,
                columns: new[] { "Category", "Name" },
                values: new object[] { 10, 130 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 132,
                columns: new[] { "Category", "Name" },
                values: new object[] { 10, 131 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 133,
                columns: new[] { "Category", "Name" },
                values: new object[] { 10, 132 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 134,
                columns: new[] { "Category", "Name" },
                values: new object[] { 9, 133 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 135,
                columns: new[] { "Category", "Name" },
                values: new object[] { 9, 134 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 136,
                columns: new[] { "Category", "Name" },
                values: new object[] { 9, 135 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 137,
                columns: new[] { "Category", "Name" },
                values: new object[] { 9, 136 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 138,
                columns: new[] { "Category", "Name" },
                values: new object[] { 9, 137 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 139,
                columns: new[] { "Category", "Name" },
                values: new object[] { 9, 138 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 140,
                columns: new[] { "Category", "Name" },
                values: new object[] { 9, 139 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 141,
                columns: new[] { "Category", "Name" },
                values: new object[] { 9, 140 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 142,
                columns: new[] { "Category", "Name" },
                values: new object[] { 9, 141 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 143,
                columns: new[] { "Category", "Name" },
                values: new object[] { 9, 142 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 144,
                columns: new[] { "Category", "Name" },
                values: new object[] { 9, 143 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 145,
                columns: new[] { "Category", "Name" },
                values: new object[] { 9, 144 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 146,
                columns: new[] { "Category", "Name" },
                values: new object[] { 9, 145 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 147,
                columns: new[] { "Category", "Name" },
                values: new object[] { 9, 146 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 148,
                columns: new[] { "Category", "Name" },
                values: new object[] { 9, 147 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 149,
                columns: new[] { "Category", "Name" },
                values: new object[] { 11, 148 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 150,
                columns: new[] { "Category", "Name" },
                values: new object[] { 11, 149 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 151,
                columns: new[] { "Category", "Name" },
                values: new object[] { 11, 150 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 152,
                columns: new[] { "Category", "Name" },
                values: new object[] { 11, 151 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 153,
                columns: new[] { "Category", "Name" },
                values: new object[] { 11, 152 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 154,
                columns: new[] { "Category", "Name" },
                values: new object[] { 11, 153 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 155,
                columns: new[] { "Category", "Name" },
                values: new object[] { 11, 154 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 156,
                columns: new[] { "Category", "Name" },
                values: new object[] { 11, 155 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 157,
                columns: new[] { "Category", "Name" },
                values: new object[] { 12, 156 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 158,
                columns: new[] { "Category", "Name" },
                values: new object[] { 12, 157 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 159,
                columns: new[] { "Category", "Name" },
                values: new object[] { 12, 158 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 160,
                columns: new[] { "Category", "Name" },
                values: new object[] { 12, 159 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 161,
                columns: new[] { "Category", "Name" },
                values: new object[] { 12, 160 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 162,
                columns: new[] { "Category", "Name" },
                values: new object[] { 12, 161 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 163,
                columns: new[] { "Category", "Name" },
                values: new object[] { 12, 162 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 164,
                columns: new[] { "Category", "Name" },
                values: new object[] { 12, 163 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 165,
                columns: new[] { "Category", "Name" },
                values: new object[] { 12, 164 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 166,
                columns: new[] { "Category", "Name" },
                values: new object[] { 13, 165 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 167,
                columns: new[] { "Category", "Name" },
                values: new object[] { 13, 166 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 168,
                columns: new[] { "Category", "Name" },
                values: new object[] { 13, 167 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 169,
                columns: new[] { "Category", "Name" },
                values: new object[] { 13, 168 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 170,
                columns: new[] { "Category", "Name" },
                values: new object[] { 14, 169 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 171,
                columns: new[] { "Category", "Name" },
                values: new object[] { 13, 170 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 172,
                columns: new[] { "Category", "Name" },
                values: new object[] { 13, 171 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 173,
                columns: new[] { "Category", "Name" },
                values: new object[] { 13, 172 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 174,
                columns: new[] { "Category", "Name" },
                values: new object[] { 3, 173 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 175,
                columns: new[] { "Category", "Name" },
                values: new object[] { 3, 174 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 176,
                columns: new[] { "Category", "Name" },
                values: new object[] { 3, 175 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 177,
                columns: new[] { "Category", "Name" },
                values: new object[] { 3, 176 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 178,
                columns: new[] { "Category", "Name" },
                values: new object[] { 3, 177 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 179,
                columns: new[] { "Category", "Name" },
                values: new object[] { 3, 178 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 180,
                columns: new[] { "Category", "Name" },
                values: new object[] { 3, 179 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 181,
                columns: new[] { "Category", "Name" },
                values: new object[] { 15, 180 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 182,
                columns: new[] { "Category", "Name" },
                values: new object[] { 15, 181 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 183,
                columns: new[] { "Category", "Name" },
                values: new object[] { 15, 182 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 184,
                columns: new[] { "Category", "Name" },
                values: new object[] { 15, 183 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 185,
                columns: new[] { "Category", "Name" },
                values: new object[] { 15, 184 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 186,
                columns: new[] { "Category", "Name" },
                values: new object[] { 15, 185 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 187,
                columns: new[] { "Category", "Name" },
                values: new object[] { 15, 186 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 188,
                columns: new[] { "Category", "Name" },
                values: new object[] { 15, 187 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 189,
                columns: new[] { "Category", "Name" },
                values: new object[] { 15, 188 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 190,
                columns: new[] { "Category", "Name" },
                values: new object[] { 15, 189 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 191,
                columns: new[] { "Category", "Name" },
                values: new object[] { 16, 190 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 192,
                columns: new[] { "Category", "Name" },
                values: new object[] { 16, 191 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 193,
                columns: new[] { "Category", "Name" },
                values: new object[] { 16, 192 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 194,
                columns: new[] { "Category", "Name" },
                values: new object[] { 16, 193 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 195,
                columns: new[] { "Category", "Name" },
                values: new object[] { 16, 194 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 196,
                columns: new[] { "Category", "Name" },
                values: new object[] { 16, 195 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 197,
                columns: new[] { "Category", "Name" },
                values: new object[] { 16, 196 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 198,
                columns: new[] { "Category", "Name" },
                values: new object[] { 16, 197 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 199,
                columns: new[] { "Category", "Name" },
                values: new object[] { 17, 198 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 200,
                columns: new[] { "Category", "Name" },
                values: new object[] { 17, 199 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 201,
                columns: new[] { "Category", "Name" },
                values: new object[] { 17, 200 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 202,
                columns: new[] { "Category", "Name" },
                values: new object[] { 17, 201 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 203,
                columns: new[] { "Category", "Name" },
                values: new object[] { 17, 202 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 204,
                columns: new[] { "Category", "Name" },
                values: new object[] { 1, 203 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 205,
                columns: new[] { "Category", "Name" },
                values: new object[] { 1, 204 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 206,
                columns: new[] { "Category", "Name" },
                values: new object[] { 1, 205 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 207,
                columns: new[] { "Category", "Name" },
                values: new object[] { 1, 206 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 208,
                columns: new[] { "Category", "Name" },
                values: new object[] { 1, 207 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 209,
                columns: new[] { "Category", "Name" },
                values: new object[] { 2, 208 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 210,
                columns: new[] { "Category", "Name" },
                values: new object[] { 2, 209 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 211,
                columns: new[] { "Category", "Name" },
                values: new object[] { 2, 210 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 212,
                columns: new[] { "Category", "Name" },
                values: new object[] { 2, 211 });

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 213,
                columns: new[] { "Category", "Name" },
                values: new object[] { 2, 212 });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "59724D2D-E2B5-4C67-AB6F-D93478147B03",
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 1, 5, 17, 22, 24, 402, DateTimeKind.Utc).AddTicks(9582), "AQAAAAIAAYagAAAAEIfW7U6lNtRC6Yp5m6fqg/n9WFFAGNFO2r6DCujaxhQlngm39lS9W8frvUVV+JXwGQ==" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "59726D2D-E2B5-4C67-AB6F-D93878317B03",
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 1, 5, 17, 22, 24, 464, DateTimeKind.Utc).AddTicks(57), "AQAAAAIAAYagAAAAEDbrmiBc2VeKySwDMH8ULREb14V/yoBZyTpFIRHtjx9YiI6idG3uHwdEZbDk9UENNA==" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "59726D2D-E2B5-4C67-AB6F-D99478317B03",
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 1, 5, 17, 22, 24, 518, DateTimeKind.Utc).AddTicks(6468), "AQAAAAIAAYagAAAAEIV8E6CiX8SjHqqWapdPlmzG7Crt4AsUCRhOg186ncm2h4XbXSEXeRW1S1YiFF5JNQ==" });
        }
    }
}
