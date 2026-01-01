using Domain.Consts;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.EntitiesConfigrations;


public class AmenityConfiguration : IEntityTypeConfiguration<Amenity>
{
    public void Configure(EntityTypeBuilder<Amenity> builder)
    {
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.Description)
            .HasMaxLength(200);

        builder.Property(a => a.Category)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(a => a.Name);
        builder.HasIndex(a => a.Category);


            builder.HasData(
                // ===== BASIC / GENERAL =====
                new Amenity { Id = 1, Name = AmenityName.Wifi, Category = AmenityCategory.Basic, Description = "Wireless internet access" },
                new Amenity { Id = 2, Name = AmenityName.FreeWifi, Category = AmenityCategory.Basic, Description = "Complimentary wireless internet" },
                new Amenity { Id = 3, Name = AmenityName.PaidWifi, Category = AmenityCategory.Basic, Description = "Paid wireless internet service" },
                new Amenity { Id = 4, Name = AmenityName.AirConditioning, Category = AmenityCategory.Basic, Description = "Air conditioning system" },
                new Amenity { Id = 5, Name = AmenityName.Heating, Category = AmenityCategory.Basic, Description = "Heating system" },
                new Amenity { Id = 6, Name = AmenityName.ElectricityBackup, Category = AmenityCategory.Basic, Description = "Backup power generator" },
                new Amenity { Id = 7, Name = AmenityName.Parking, Category = AmenityCategory.Basic, Description = "Parking available" },
                new Amenity { Id = 8, Name = AmenityName.FreeParking, Category = AmenityCategory.Basic, Description = "Complimentary parking" },
                new Amenity { Id = 9, Name = AmenityName.PaidParking, Category = AmenityCategory.Basic, Description = "Paid parking service" },
                new Amenity { Id = 10, Name = AmenityName.ValetParking, Category = AmenityCategory.Basic, Description = "Valet parking service" },
                new Amenity { Id = 11, Name = AmenityName.StreetParking, Category = AmenityCategory.Basic, Description = "Street parking available" },
                new Amenity { Id = 12, Name = AmenityName.GarageParking, Category = AmenityCategory.Basic, Description = "Garage parking facility" },
                new Amenity { Id = 13, Name = AmenityName.Elevator, Category = AmenityCategory.Basic, Description = "Elevator access" },
                new Amenity { Id = 14, Name = AmenityName.LuggageStorage, Category = AmenityCategory.Basic, Description = "Luggage storage facility" },
                new Amenity { Id = 15, Name = AmenityName.Reception24Hours, Category = AmenityCategory.Basic, Description = "24-hour reception desk" },
                new Amenity { Id = 16, Name = AmenityName.ExpressCheckIn, Category = AmenityCategory.Basic, Description = "Express check-in service" },
                new Amenity { Id = 17, Name = AmenityName.ExpressCheckOut, Category = AmenityCategory.Basic, Description = "Express check-out service" },
                new Amenity { Id = 18, Name = AmenityName.Concierge, Category = AmenityCategory.Basic, Description = "Concierge service" },
                new Amenity { Id = 19, Name = AmenityName.WakeUpService, Category = AmenityCategory.Basic, Description = "Wake-up call service" },
                new Amenity { Id = 20, Name = AmenityName.DailyHousekeeping, Category = AmenityCategory.Basic, Description = "Daily housekeeping service" },
                new Amenity { Id = 21, Name = AmenityName.LaundryService, Category = AmenityCategory.Basic, Description = "Laundry service" },
                new Amenity { Id = 22, Name = AmenityName.DryCleaning, Category = AmenityCategory.Basic, Description = "Dry cleaning service" },
                new Amenity { Id = 23, Name = AmenityName.IroningService, Category = AmenityCategory.Basic, Description = "Ironing service" },
                new Amenity { Id = 24, Name = AmenityName.CurrencyExchange, Category = AmenityCategory.Basic, Description = "Currency exchange service" },
                new Amenity { Id = 25, Name = AmenityName.ATMOnSite, Category = AmenityCategory.Basic, Description = "On-site ATM machine" },
                new Amenity { Id = 26, Name = AmenityName.GiftShop, Category = AmenityCategory.Basic, Description = "Gift shop on premises" },
                new Amenity { Id = 27, Name = AmenityName.MiniMarket, Category = AmenityCategory.Basic, Description = "Mini market/shop on premises" },
                new Amenity { Id = 28, Name = AmenityName.SmokingArea, Category = AmenityCategory.Basic, Description = "Designated smoking area" },
                new Amenity { Id = 29, Name = AmenityName.NonSmokingRooms, Category = AmenityCategory.Basic, Description = "Non-smoking rooms available" },
                new Amenity { Id = 30, Name = AmenityName.FamilyRooms, Category = AmenityCategory.Basic, Description = "Family-sized rooms available" },
                new Amenity { Id = 31, Name = AmenityName.SoundproofRooms, Category = AmenityCategory.Basic, Description = "Soundproofed rooms" },

                // ===== ROOM FEATURES =====
                new Amenity { Id = 32, Name = AmenityName.TV, Category = AmenityCategory.Room, Description = "Television" },
                new Amenity { Id = 33, Name = AmenityName.SmartTV, Category = AmenityCategory.Room, Description = "Smart TV with internet connectivity" },
                new Amenity { Id = 34, Name = AmenityName.CableTV, Category = AmenityCategory.Room, Description = "Cable television channels" },
                new Amenity { Id = 35, Name = AmenityName.SatelliteTV, Category = AmenityCategory.Room, Description = "Satellite television channels" },
                new Amenity { Id = 36, Name = AmenityName.StreamingServices, Category = AmenityCategory.Room, Description = "Streaming services access" },
                new Amenity { Id = 37, Name = AmenityName.Desk, Category = AmenityCategory.Room, Description = "Work desk" },
                new Amenity { Id = 38, Name = AmenityName.SeatingArea, Category = AmenityCategory.Room, Description = "Seating area in room" },
                new Amenity { Id = 39, Name = AmenityName.Sofa, Category = AmenityCategory.Room, Description = "Sofa in room" },
                new Amenity { Id = 40, Name = AmenityName.Wardrobe, Category = AmenityCategory.Room, Description = "Wardrobe for clothing" },
                new Amenity { Id = 41, Name = AmenityName.Closet, Category = AmenityCategory.Room, Description = "Closet storage" },
                new Amenity { Id = 42, Name = AmenityName.Minibar, Category = AmenityCategory.Room, Description = "Mini bar in room" },
                new Amenity { Id = 43, Name = AmenityName.Refrigerator, Category = AmenityCategory.Room, Description = "Refrigerator in room" },
                new Amenity { Id = 44, Name = AmenityName.Microwave, Category = AmenityCategory.Room, Description = "Microwave oven" },
                new Amenity { Id = 45, Name = AmenityName.ElectricKettle, Category = AmenityCategory.Room, Description = "Electric kettle" },
                new Amenity { Id = 46, Name = AmenityName.CoffeeMachine, Category = AmenityCategory.Room, Description = "Coffee making machine" },
                new Amenity { Id = 47, Name = AmenityName.TeaMaker, Category = AmenityCategory.Room, Description = "Tea making facilities" },
                new Amenity { Id = 48, Name = AmenityName.DiningTable, Category = AmenityCategory.Room, Description = "Dining table" },
                new Amenity { Id = 49, Name = AmenityName.SafeBox, Category = AmenityCategory.Room, Description = "In-room safe" },
                new Amenity { Id = 50, Name = AmenityName.Iron, Category = AmenityCategory.Room, Description = "Iron provided" },
                new Amenity { Id = 51, Name = AmenityName.IroningBoard, Category = AmenityCategory.Room, Description = "Ironing board" },
                new Amenity { Id = 52, Name = AmenityName.AlarmClock, Category = AmenityCategory.Room, Description = "Alarm clock" },
                new Amenity { Id = 53, Name = AmenityName.CarpetedFloor, Category = AmenityCategory.Room, Description = "Carpeted flooring" },
                new Amenity { Id = 54, Name = AmenityName.HardwoodFloor, Category = AmenityCategory.Room, Description = "Hardwood flooring" },
                new Amenity { Id = 55, Name = AmenityName.PrivateEntrance, Category = AmenityCategory.Room, Description = "Private entrance to room" },
                new Amenity { Id = 56, Name = AmenityName.Balcony, Category = AmenityCategory.Room, Description = "Room with balcony" },
                new Amenity { Id = 57, Name = AmenityName.Terrace, Category = AmenityCategory.Room, Description = "Private terrace" },
                new Amenity { Id = 58, Name = AmenityName.Patio, Category = AmenityCategory.Room, Description = "Private patio" },
                new Amenity { Id = 59, Name = AmenityName.GardenView, Category = AmenityCategory.Room, Description = "Room with garden view" },
                new Amenity { Id = 60, Name = AmenityName.CityView, Category = AmenityCategory.Room, Description = "Room with city view" },
                new Amenity { Id = 61, Name = AmenityName.SeaView, Category = AmenityCategory.Room, Description = "Room with sea view" },
                new Amenity { Id = 62, Name = AmenityName.MountainView, Category = AmenityCategory.Room, Description = "Room with mountain view" },
                new Amenity { Id = 63, Name = AmenityName.PoolView, Category = AmenityCategory.Room, Description = "Room with pool view" },

                // ===== BEDROOM =====
                new Amenity { Id = 64, Name = AmenityName.ExtraLongBeds, Category = AmenityCategory.Bedroom, Description = "Extra long beds" },
                new Amenity { Id = 65, Name = AmenityName.SofaBed, Category = AmenityCategory.Bedroom, Description = "Sofa bed available" },
                new Amenity { Id = 66, Name = AmenityName.BabyCot, Category = AmenityCategory.Bedroom, Description = "Baby cot available" },
                new Amenity { Id = 67, Name = AmenityName.CribsAvailable, Category = AmenityCategory.Bedroom, Description = "Cribs available for infants" },
                new Amenity { Id = 68, Name = AmenityName.HypoallergenicBedding, Category = AmenityCategory.Bedroom, Description = "Hypoallergenic bedding" },
                new Amenity { Id = 69, Name = AmenityName.BlackoutCurtains, Category = AmenityCategory.Bedroom, Description = "Blackout curtains" },

                // ===== BATHROOM =====
                new Amenity { Id = 70, Name = AmenityName.PrivateBathroom, Category = AmenityCategory.Bathroom, Description = "Private bathroom" },
                new Amenity { Id = 71, Name = AmenityName.SharedBathroom, Category = AmenityCategory.Bathroom, Description = "Shared bathroom facilities" },
                new Amenity { Id = 72, Name = AmenityName.Shower, Category = AmenityCategory.Bathroom, Description = "Shower" },
                new Amenity { Id = 73, Name = AmenityName.WalkInShower, Category = AmenityCategory.Bathroom, Description = "Walk-in shower" },
                new Amenity { Id = 74, Name = AmenityName.Bathtub, Category = AmenityCategory.Bathroom, Description = "Bathtub" },
                new Amenity { Id = 75, Name = AmenityName.JacuzziBathtub, Category = AmenityCategory.Bathroom, Description = "Jacuzzi bathtub" },
                new Amenity { Id = 76, Name = AmenityName.Bidet, Category = AmenityCategory.Bathroom, Description = "Bidet" },
                new Amenity { Id = 77, Name = AmenityName.Toilet, Category = AmenityCategory.Bathroom, Description = "Toilet" },
                new Amenity { Id = 78, Name = AmenityName.ToiletPaper, Category = AmenityCategory.Bathroom, Description = "Toilet paper provided" },
                new Amenity { Id = 79, Name = AmenityName.Towels, Category = AmenityCategory.Bathroom, Description = "Towels provided" },
                new Amenity { Id = 80, Name = AmenityName.Bathrobes, Category = AmenityCategory.Bathroom, Description = "Bathrobes provided" },
                new Amenity { Id = 81, Name = AmenityName.Slippers, Category = AmenityCategory.Bathroom, Description = "Slippers provided" },
                new Amenity { Id = 82, Name = AmenityName.HairDryer, Category = AmenityCategory.Bathroom, Description = "Hair dryer" },
                new Amenity { Id = 83, Name = AmenityName.FreeToiletries, Category = AmenityCategory.Bathroom, Description = "Free toiletries" },
                new Amenity { Id = 84, Name = AmenityName.Shampoo, Category = AmenityCategory.Bathroom, Description = "Shampoo provided" },
                new Amenity { Id = 85, Name = AmenityName.Conditioner, Category = AmenityCategory.Bathroom, Description = "Hair conditioner provided" },
                new Amenity { Id = 86, Name = AmenityName.BodySoap, Category = AmenityCategory.Bathroom, Description = "Body soap provided" },

                // ===== KITCHEN =====
                new Amenity { Id = 87, Name = AmenityName.Kitchen, Category = AmenityCategory.Kitchen, Description = "Full kitchen" },
                new Amenity { Id = 88, Name = AmenityName.Kitchenette, Category = AmenityCategory.Kitchen, Description = "Kitchenette with basic appliances" },
                new Amenity { Id = 89, Name = AmenityName.Oven, Category = AmenityCategory.Kitchen, Description = "Oven" },
                new Amenity { Id = 90, Name = AmenityName.Stove, Category = AmenityCategory.Kitchen, Description = "Stove/cooktop" },
                new Amenity { Id = 91, Name = AmenityName.Dishwasher, Category = AmenityCategory.Kitchen, Description = "Dishwasher" },
                new Amenity { Id = 92, Name = AmenityName.WashingMachine, Category = AmenityCategory.Kitchen, Description = "Washing machine" },
                new Amenity { Id = 93, Name = AmenityName.Dryer, Category = AmenityCategory.Kitchen, Description = "Clothes dryer" },
                new Amenity { Id = 94, Name = AmenityName.Toaster, Category = AmenityCategory.Kitchen, Description = "Toaster" },
                new Amenity { Id = 95, Name = AmenityName.Blender, Category = AmenityCategory.Kitchen, Description = "Blender" },
                new Amenity { Id = 96, Name = AmenityName.CookingUtensils, Category = AmenityCategory.Kitchen, Description = "Cooking utensils provided" },

                // ===== FOOD & DRINK =====
                new Amenity { Id = 97, Name = AmenityName.Restaurant, Category = AmenityCategory.FoodAndDrink, Description = "On-site restaurant" },
                new Amenity { Id = 98, Name = AmenityName.BuffetRestaurant, Category = AmenityCategory.FoodAndDrink, Description = "Buffet-style restaurant" },
                new Amenity { Id = 99, Name = AmenityName.ALaCarteRestaurant, Category = AmenityCategory.FoodAndDrink, Description = "À la carte restaurant" },
                new Amenity { Id = 100, Name = AmenityName.RoomService, Category = AmenityCategory.FoodAndDrink, Description = "Room service available" },
                new Amenity { Id = 101, Name = AmenityName.BreakfastIncluded, Category = AmenityCategory.FoodAndDrink, Description = "Breakfast included in rate" },
                new Amenity { Id = 102, Name = AmenityName.BreakfastBuffet, Category = AmenityCategory.FoodAndDrink, Description = "Breakfast buffet" },
                new Amenity { Id = 103, Name = AmenityName.ContinentalBreakfast, Category = AmenityCategory.FoodAndDrink, Description = "Continental breakfast" },
                new Amenity { Id = 104, Name = AmenityName.HalalFood, Category = AmenityCategory.FoodAndDrink, Description = "Halal food options" },
                new Amenity { Id = 105, Name = AmenityName.VegetarianFood, Category = AmenityCategory.FoodAndDrink, Description = "Vegetarian food options" },
                new Amenity { Id = 106, Name = AmenityName.VeganOptions, Category = AmenityCategory.FoodAndDrink, Description = "Vegan food options" },
                new Amenity { Id = 107, Name = AmenityName.Bar, Category = AmenityCategory.FoodAndDrink, Description = "Bar on premises" },
                new Amenity { Id = 108, Name = AmenityName.PoolBar, Category = AmenityCategory.FoodAndDrink, Description = "Poolside bar" },
                new Amenity { Id = 109, Name = AmenityName.SnackBar, Category = AmenityCategory.FoodAndDrink, Description = "Snack bar" },
                new Amenity { Id = 110, Name = AmenityName.Cafe, Category = AmenityCategory.FoodAndDrink, Description = "Café on premises" },
                new Amenity { Id = 111, Name = AmenityName.CoffeeShop, Category = AmenityCategory.FoodAndDrink, Description = "Coffee shop" },
                new Amenity { Id = 112, Name = AmenityName.VendingMachines, Category = AmenityCategory.FoodAndDrink, Description = "Vending machines" },
                new Amenity { Id = 113, Name = AmenityName.GroceryDelivery, Category = AmenityCategory.FoodAndDrink, Description = "Grocery delivery service" },

                // ===== ENTERTAINMENT & LEISURE =====
                new Amenity { Id = 114, Name = AmenityName.SwimmingPool, Category = AmenityCategory.Entertainment, Description = "Swimming pool" },
                new Amenity { Id = 115, Name = AmenityName.OutdoorPool, Category = AmenityCategory.Entertainment, Description = "Outdoor swimming pool" },
                new Amenity { Id = 116, Name = AmenityName.IndoorPool, Category = AmenityCategory.Entertainment, Description = "Indoor swimming pool" },
                new Amenity { Id = 117, Name = AmenityName.HeatedPool, Category = AmenityCategory.Entertainment, Description = "Heated swimming pool" },
                new Amenity { Id = 118, Name = AmenityName.InfinityPool, Category = AmenityCategory.Entertainment, Description = "Infinity pool" },
                new Amenity { Id = 119, Name = AmenityName.KidsPool, Category = AmenityCategory.Entertainment, Description = "Children's pool" },
                new Amenity { Id = 120, Name = AmenityName.WaterPark, Category = AmenityCategory.Entertainment, Description = "Water park" },
                new Amenity { Id = 121, Name = AmenityName.Gym, Category = AmenityCategory.Wellness, Description = "Gym/fitness center" },
                new Amenity { Id = 122, Name = AmenityName.FitnessCenter, Category = AmenityCategory.Wellness, Description = "Fitness center" },
                new Amenity { Id = 123, Name = AmenityName.PersonalTrainer, Category = AmenityCategory.Wellness, Description = "Personal trainer available" },
                new Amenity { Id = 124, Name = AmenityName.Spa, Category = AmenityCategory.Wellness, Description = "Spa facilities" },
                new Amenity { Id = 125, Name = AmenityName.WellnessCenter, Category = AmenityCategory.Wellness, Description = "Wellness center" },
                new Amenity { Id = 126, Name = AmenityName.Sauna, Category = AmenityCategory.Wellness, Description = "Sauna" },
                new Amenity { Id = 127, Name = AmenityName.SteamRoom, Category = AmenityCategory.Wellness, Description = "Steam room" },
                new Amenity { Id = 128, Name = AmenityName.Hammam, Category = AmenityCategory.Wellness, Description = "Turkish bath/hammam" },
                new Amenity { Id = 129, Name = AmenityName.Jacuzzi, Category = AmenityCategory.Wellness, Description = "Jacuzzi/hot tub" },
                new Amenity { Id = 130, Name = AmenityName.MassageService, Category = AmenityCategory.Wellness, Description = "Massage services" },
                new Amenity { Id = 131, Name = AmenityName.BeautySalon, Category = AmenityCategory.Wellness, Description = "Beauty salon" },
                new Amenity { Id = 132, Name = AmenityName.YogaClasses, Category = AmenityCategory.Wellness, Description = "Yoga classes" },
                new Amenity { Id = 133, Name = AmenityName.Aerobics, Category = AmenityCategory.Wellness, Description = "Aerobics classes" },
                new Amenity { Id = 134, Name = AmenityName.NightClub, Category = AmenityCategory.Entertainment, Description = "Night club" },
                new Amenity { Id = 135, Name = AmenityName.LiveMusic, Category = AmenityCategory.Entertainment, Description = "Live music performances" },
                new Amenity { Id = 136, Name = AmenityName.DJ, Category = AmenityCategory.Entertainment, Description = "DJ entertainment" },
                new Amenity { Id = 137, Name = AmenityName.CinemaRoom, Category = AmenityCategory.Entertainment, Description = "Cinema/movie room" },
                new Amenity { Id = 138, Name = AmenityName.GameRoom, Category = AmenityCategory.Entertainment, Description = "Game room" },
                new Amenity { Id = 139, Name = AmenityName.Billiards, Category = AmenityCategory.Entertainment, Description = "Billiards/pool table" },
                new Amenity { Id = 140, Name = AmenityName.TableTennis, Category = AmenityCategory.Entertainment, Description = "Table tennis" },
                new Amenity { Id = 141, Name = AmenityName.Bowling, Category = AmenityCategory.Entertainment, Description = "Bowling alley" },
                new Amenity { Id = 142, Name = AmenityName.Darts, Category = AmenityCategory.Entertainment, Description = "Darts" },
                new Amenity { Id = 143, Name = AmenityName.Karaoke, Category = AmenityCategory.Entertainment, Description = "Karaoke" },
                new Amenity { Id = 144, Name = AmenityName.Library, Category = AmenityCategory.Entertainment, Description = "Library" },
                new Amenity { Id = 145, Name = AmenityName.TVLounge, Category = AmenityCategory.Entertainment, Description = "TV lounge/common area" },
                new Amenity { Id = 146, Name = AmenityName.KidsPlayArea, Category = AmenityCategory.Entertainment, Description = "Children's play area" },
                new Amenity { Id = 147, Name = AmenityName.KidsClub, Category = AmenityCategory.Entertainment, Description = "Kids club" },
                new Amenity { Id = 148, Name = AmenityName.BabysittingService, Category = AmenityCategory.Entertainment, Description = "Babysitting service" },

                // ===== OUTDOOR & ACTIVITIES =====
                new Amenity { Id = 149, Name = AmenityName.Garden, Category = AmenityCategory.Outdoor, Description = "Garden area" },
                new Amenity { Id = 150, Name = AmenityName.SunTerrace, Category = AmenityCategory.Outdoor, Description = "Sun terrace" },
                new Amenity { Id = 151, Name = AmenityName.PicnicArea, Category = AmenityCategory.Outdoor, Description = "Picnic area" },
                new Amenity { Id = 152, Name = AmenityName.BBQFacilities, Category = AmenityCategory.Outdoor, Description = "Barbecue facilities" },
                new Amenity { Id = 153, Name = AmenityName.BeachAccess, Category = AmenityCategory.Outdoor, Description = "Beach access" },
                new Amenity { Id = 154, Name = AmenityName.PrivateBeach, Category = AmenityCategory.Outdoor, Description = "Private beach area" },
                new Amenity { Id = 155, Name = AmenityName.BeachUmbrellas, Category = AmenityCategory.Outdoor, Description = "Beach umbrellas" },
                new Amenity { Id = 156, Name = AmenityName.BeachChairs, Category = AmenityCategory.Outdoor, Description = "Beach chairs" },
                new Amenity { Id = 157, Name = AmenityName.WaterSports, Category = AmenityCategory.Activities, Description = "Water sports activities" },
                new Amenity { Id = 158, Name = AmenityName.Diving, Category = AmenityCategory.Activities, Description = "Diving facilities/lessons" },
                new Amenity { Id = 159, Name = AmenityName.Snorkeling, Category = AmenityCategory.Activities, Description = "Snorkeling equipment/activities" },
                new Amenity { Id = 160, Name = AmenityName.Canoeing, Category = AmenityCategory.Activities, Description = "Canoeing/kayaking" },
                new Amenity { Id = 161, Name = AmenityName.Fishing, Category = AmenityCategory.Activities, Description = "Fishing facilities" },
                new Amenity { Id = 162, Name = AmenityName.HorseRiding, Category = AmenityCategory.Activities, Description = "Horse riding" },
                new Amenity { Id = 163, Name = AmenityName.Hiking, Category = AmenityCategory.Activities, Description = "Hiking trails/guides" },
                new Amenity { Id = 164, Name = AmenityName.Cycling, Category = AmenityCategory.Activities, Description = "Cycling/biking" },
                new Amenity { Id = 165, Name = AmenityName.BikeRental, Category = AmenityCategory.Activities, Description = "Bicycle rental" },

                // ===== BUSINESS & EVENTS =====
                new Amenity { Id = 166, Name = AmenityName.BusinessCenter, Category = AmenityCategory.Business, Description = "Business center" },
                new Amenity { Id = 167, Name = AmenityName.MeetingRooms, Category = AmenityCategory.Business, Description = "Meeting rooms" },
                new Amenity { Id = 168, Name = AmenityName.ConferenceHall, Category = AmenityCategory.Business, Description = "Conference hall" },
                new Amenity { Id = 169, Name = AmenityName.BanquetHall, Category = AmenityCategory.Business, Description = "Banquet hall" },
                new Amenity { Id = 170, Name = AmenityName.WeddingServices, Category = AmenityCategory.Events, Description = "Wedding services" },
                new Amenity { Id = 171, Name = AmenityName.Fax, Category = AmenityCategory.Business, Description = "Fax service" },
                new Amenity { Id = 172, Name = AmenityName.Photocopying, Category = AmenityCategory.Business, Description = "Photocopying service" },
                new Amenity { Id = 173, Name = AmenityName.HighSpeedInternet, Category = AmenityCategory.Business, Description = "High-speed internet" },

                // ===== TRANSPORTATION =====
                new Amenity { Id = 174, Name = AmenityName.AirportShuttle, Category = AmenityCategory.Transportation, Description = "Airport shuttle service" },
                new Amenity { Id = 175, Name = AmenityName.PaidAirportShuttle, Category = AmenityCategory.Transportation, Description = "Paid airport shuttle" },
                new Amenity { Id = 176, Name = AmenityName.CarRental, Category = AmenityCategory.Transportation, Description = "Car rental service" },
                new Amenity { Id = 177, Name = AmenityName.TaxiService, Category = AmenityCategory.Transportation, Description = "Taxi service" },
                new Amenity { Id = 178, Name = AmenityName.ShuttleService, Category = AmenityCategory.Transportation, Description = "Shuttle service" },
                new Amenity { Id = 179, Name = AmenityName.PublicTransportNearby, Category = AmenityCategory.Transportation, Description = "Public transport nearby" },
                new Amenity { Id = 180, Name = AmenityName.EVChargingStation, Category = AmenityCategory.Transportation, Description = "Electric vehicle charging station" },

                // ===== SAFETY & SECURITY =====
                new Amenity { Id = 181, Name = AmenityName.FireExtinguishers, Category = AmenityCategory.Safety, Description = "Fire extinguishers" },
                new Amenity { Id = 182, Name = AmenityName.SmokeDetectors, Category = AmenityCategory.Safety, Description = "Smoke detectors" },
                new Amenity { Id = 183, Name = AmenityName.CarbonMonoxideDetector, Category = AmenityCategory.Safety, Description = "Carbon monoxide detector" },
                new Amenity { Id = 184, Name = AmenityName.CCTV, Category = AmenityCategory.Safety, Description = "CCTV surveillance" },
                new Amenity { Id = 185, Name = AmenityName.Security24Hours, Category = AmenityCategory.Safety, Description = "24-hour security" },
                new Amenity { Id = 186, Name = AmenityName.KeyCardAccess, Category = AmenityCategory.Safety, Description = "Key card access" },
                new Amenity { Id = 187, Name = AmenityName.ElectronicDoorLocks, Category = AmenityCategory.Safety, Description = "Electronic door locks" },
                new Amenity { Id = 188, Name = AmenityName.SafeDepositBox, Category = AmenityCategory.Safety, Description = "Safe deposit box" },
                new Amenity { Id = 189, Name = AmenityName.FirstAidKit, Category = AmenityCategory.Safety, Description = "First aid kit available" },
                new Amenity { Id = 190, Name = AmenityName.EmergencyExitSigns, Category = AmenityCategory.Safety, Description = "Emergency exit signs" },

                // ===== ACCESSIBILITY =====
                new Amenity { Id = 191, Name = AmenityName.WheelchairAccessible, Category = AmenityCategory.Accessibility, Description = "Wheelchair accessible facilities" },
                new Amenity { Id = 192, Name = AmenityName.AccessibleParking, Category = AmenityCategory.Accessibility, Description = "Accessible parking spaces" },
                new Amenity { Id = 193, Name = AmenityName.AccessibleBathroom, Category = AmenityCategory.Accessibility, Description = "Accessible bathroom" },
                new Amenity { Id = 194, Name = AmenityName.GrabRails, Category = AmenityCategory.Accessibility, Description = "Grab rails in bathroom" },
                new Amenity { Id = 195, Name = AmenityName.LoweredSink, Category = AmenityCategory.Accessibility, Description = "Lowered sink" },
                new Amenity { Id = 196, Name = AmenityName.BrailleSignage, Category = AmenityCategory.Accessibility, Description = "Braille signage" },
                new Amenity { Id = 197, Name = AmenityName.VisualAids, Category = AmenityCategory.Accessibility, Description = "Visual aids" },
                new Amenity { Id = 198, Name = AmenityName.HearingAccessible, Category = AmenityCategory.Accessibility, Description = "Hearing accessible features" },

                // ===== PETS =====
                new Amenity { Id = 199, Name = AmenityName.PetsAllowed, Category = AmenityCategory.Pets, Description = "Pets allowed" },
                new Amenity { Id = 200, Name = AmenityName.PetsNotAllowed, Category = AmenityCategory.Pets, Description = "Pets not allowed" },
                new Amenity { Id = 201, Name = AmenityName.PetBowls, Category = AmenityCategory.Pets, Description = "Pet bowls provided" },
                new Amenity { Id = 202, Name = AmenityName.PetBasket, Category = AmenityCategory.Pets, Description = "Pet bed/basket" },
                new Amenity { Id = 203, Name = AmenityName.PetSittingService, Category = AmenityCategory.Pets, Description = "Pet sitting service" },

                // ===== SERVICES =====
                new Amenity { Id = 204, Name = AmenityName.TourDesk, Category = AmenityCategory.Services, Description = "Tour desk/information" },
                new Amenity { Id = 205, Name = AmenityName.TicketService, Category = AmenityCategory.Services, Description = "Ticket booking service" },
                new Amenity { Id = 206, Name = AmenityName.TourOrganization, Category = AmenityCategory.Services, Description = "Tour organization" },
                new Amenity { Id = 207, Name = AmenityName.LocalGuides, Category = AmenityCategory.Services, Description = "Local guides available" },
                new Amenity { Id = 208, Name = AmenityName.Lockers, Category = AmenityCategory.Services, Description = "Lockers/storage lockers" },

                // ===== CLEANLINESS & HEALTH =====
                new Amenity { Id = 209, Name = AmenityName.EnhancedCleaning, Category = AmenityCategory.Cleanliness, Description = "Enhanced cleaning protocols" },
                new Amenity { Id = 210, Name = AmenityName.ContactlessCheckIn, Category = AmenityCategory.Cleanliness, Description = "Contactless check-in" },
                new Amenity { Id = 211, Name = AmenityName.HandSanitizer, Category = AmenityCategory.Cleanliness, Description = "Hand sanitizer available" },
                new Amenity { Id = 212, Name = AmenityName.TemperatureCheck, Category = AmenityCategory.Cleanliness, Description = "Temperature checks" },
                new Amenity { Id = 213, Name = AmenityName.MedicalAssistance, Category = AmenityCategory.Cleanliness, Description = "Medical assistance available" }
            );
        }
    }
