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
                new Amenity { Id = 1, Name = AmenityName.Wifi.ToString(), Category = AmenityCategory.Basic.ToString(), Description = "Wireless internet access" },
                new Amenity { Id = 2, Name = AmenityName.FreeWifi.ToString(), Category = AmenityCategory.Basic.ToString(), Description = "Complimentary wireless internet" },
                new Amenity { Id = 3, Name = AmenityName.PaidWifi.ToString(), Category = AmenityCategory.Basic.ToString(), Description = "Paid wireless internet service" },
                new Amenity { Id = 4, Name = AmenityName.AirConditioning.ToString(), Category = AmenityCategory.Basic.ToString(), Description = "Air conditioning system" },
                new Amenity { Id = 5, Name = AmenityName.Heating.ToString(), Category = AmenityCategory.Basic.ToString(), Description = "Heating system" },
                new Amenity { Id = 6, Name = AmenityName.ElectricityBackup.ToString(), Category = AmenityCategory.Basic.ToString(), Description = "Backup power generator" },
                new Amenity { Id = 7, Name = AmenityName.Parking.ToString(), Category = AmenityCategory.Basic.ToString(), Description = "Parking available" },
                new Amenity { Id = 8, Name = AmenityName.FreeParking.ToString(), Category = AmenityCategory.Basic.ToString(), Description = "Complimentary parking" },
                new Amenity { Id = 9, Name = AmenityName.PaidParking.ToString(), Category = AmenityCategory.Basic.ToString(), Description = "Paid parking service" },
                new Amenity { Id = 10, Name = AmenityName.ValetParking.ToString(), Category = AmenityCategory.Basic.ToString(), Description = "Valet parking service" },
                new Amenity { Id = 11, Name = AmenityName.StreetParking.ToString(), Category = AmenityCategory.Basic.ToString(), Description = "Street parking available" },
                new Amenity { Id = 12, Name = AmenityName.GarageParking.ToString(), Category = AmenityCategory.Basic.ToString(), Description = "Garage parking facility" },
                new Amenity { Id = 13, Name = AmenityName.Elevator.ToString(), Category = AmenityCategory.Basic.ToString(), Description = "Elevator access" },
                new Amenity { Id = 14, Name = AmenityName.LuggageStorage.ToString(), Category = AmenityCategory.Basic.ToString(), Description = "Luggage storage facility" },
                new Amenity { Id = 15, Name = AmenityName.Reception24Hours.ToString(), Category = AmenityCategory.Basic.ToString(), Description = "24-hour reception desk" },
                new Amenity { Id = 16, Name = AmenityName.ExpressCheckIn.ToString(), Category = AmenityCategory.Basic.ToString(), Description = "Express check-in service" },
                new Amenity { Id = 17, Name = AmenityName.ExpressCheckOut.ToString(), Category = AmenityCategory.Basic.ToString(), Description = "Express check-out service" },
                new Amenity { Id = 18, Name = AmenityName.Concierge.ToString(), Category = AmenityCategory.Basic.ToString(), Description = "Concierge service" },
                new Amenity { Id = 19, Name = AmenityName.WakeUpService.ToString(), Category = AmenityCategory.Basic.ToString(), Description = "Wake-up call service" },
                new Amenity { Id = 20, Name = AmenityName.DailyHousekeeping.ToString(), Category = AmenityCategory.Basic.ToString(), Description = "Daily housekeeping service" },
                new Amenity { Id = 21, Name = AmenityName.LaundryService.ToString(), Category = AmenityCategory.Basic.ToString(), Description = "Laundry service" },
                new Amenity { Id = 22, Name = AmenityName.DryCleaning.ToString(), Category = AmenityCategory.Basic.ToString(), Description = "Dry cleaning service" },
                new Amenity { Id = 23, Name = AmenityName.IroningService.ToString(), Category = AmenityCategory.Basic.ToString(), Description = "Ironing service" },
                new Amenity { Id = 24, Name = AmenityName.CurrencyExchange.ToString(), Category = AmenityCategory.Basic.ToString(), Description = "Currency exchange service" },
                new Amenity { Id = 25, Name = AmenityName.ATMOnSite.ToString(), Category = AmenityCategory.Basic.ToString(), Description = "On-site ATM machine" },
                new Amenity { Id = 26, Name = AmenityName.GiftShop.ToString(), Category = AmenityCategory.Basic.ToString(), Description = "Gift shop on premises" },
                new Amenity { Id = 27, Name = AmenityName.MiniMarket.ToString(), Category = AmenityCategory.Basic.ToString(), Description = "Mini market/shop on premises" },
                new Amenity { Id = 28, Name = AmenityName.SmokingArea.ToString(), Category = AmenityCategory.Basic.ToString(), Description = "Designated smoking area" },
                new Amenity { Id = 29, Name = AmenityName.NonSmokingRooms.ToString(), Category = AmenityCategory.Basic.ToString(), Description = "Non-smoking rooms available" },
                new Amenity { Id = 30, Name = AmenityName.FamilyRooms.ToString(), Category = AmenityCategory.Basic.ToString(), Description = "Family-sized rooms available" },
                new Amenity { Id = 31, Name = AmenityName.SoundproofRooms.ToString(), Category = AmenityCategory.Basic.ToString(), Description = "Soundproofed rooms" },

                // ===== ROOM FEATURES =====
                new Amenity { Id = 32, Name = AmenityName.TV.ToString(), Category = AmenityCategory.Room.ToString(), Description = "Television" },
                new Amenity { Id = 33, Name = AmenityName.SmartTV.ToString(), Category = AmenityCategory.Room.ToString(), Description = "Smart TV with internet connectivity" },
                new Amenity { Id = 34, Name = AmenityName.CableTV.ToString(), Category = AmenityCategory.Room.ToString(), Description = "Cable television channels" },
                new Amenity { Id = 35, Name = AmenityName.SatelliteTV.ToString(), Category = AmenityCategory.Room.ToString(), Description = "Satellite television channels" },
                new Amenity { Id = 36, Name = AmenityName.StreamingServices.ToString(), Category = AmenityCategory.Room.ToString(), Description = "Streaming services access" },
                new Amenity { Id = 37, Name = AmenityName.Desk.ToString(), Category = AmenityCategory.Room.ToString(), Description = "Work desk" },
                new Amenity { Id = 38, Name = AmenityName.SeatingArea.ToString(), Category = AmenityCategory.Room.ToString(), Description = "Seating area in room" },
                new Amenity { Id = 39, Name = AmenityName.Sofa.ToString(), Category = AmenityCategory.Room.ToString(), Description = "Sofa in room" },
                new Amenity { Id = 40, Name = AmenityName.Wardrobe.ToString(), Category = AmenityCategory.Room.ToString(), Description = "Wardrobe for clothing" },
                new Amenity { Id = 41, Name = AmenityName.Closet.ToString(), Category = AmenityCategory.Room.ToString(), Description = "Closet storage" },
                new Amenity { Id = 42, Name = AmenityName.Minibar.ToString(), Category = AmenityCategory.Room.ToString(), Description = "Mini bar in room" },
                new Amenity { Id = 43, Name = AmenityName.Refrigerator.ToString(), Category = AmenityCategory.Room.ToString(), Description = "Refrigerator in room" },
                new Amenity { Id = 44, Name = AmenityName.Microwave.ToString(), Category = AmenityCategory.Room.ToString(), Description = "Microwave oven" },
                new Amenity { Id = 45, Name = AmenityName.ElectricKettle.ToString(), Category = AmenityCategory.Room.ToString(), Description = "Electric kettle" },
                new Amenity { Id = 46, Name = AmenityName.CoffeeMachine.ToString(), Category = AmenityCategory.Room.ToString(), Description = "Coffee making machine" },
                new Amenity { Id = 47, Name = AmenityName.TeaMaker.ToString(), Category = AmenityCategory.Room.ToString(), Description = "Tea making facilities" },
                new Amenity { Id = 48, Name = AmenityName.DiningTable.ToString(), Category = AmenityCategory.Room.ToString(), Description = "Dining table" },
                new Amenity { Id = 49, Name = AmenityName.SafeBox.ToString(), Category = AmenityCategory.Room.ToString(), Description = "In-room safe" },
                new Amenity { Id = 50, Name = AmenityName.Iron.ToString(), Category = AmenityCategory.Room.ToString(), Description = "Iron provided" },
                new Amenity { Id = 51, Name = AmenityName.IroningBoard.ToString(), Category = AmenityCategory.Room.ToString(), Description = "Ironing board" },
                new Amenity { Id = 52, Name = AmenityName.AlarmClock.ToString(), Category = AmenityCategory.Room.ToString(), Description = "Alarm clock" },
                new Amenity { Id = 53, Name = AmenityName.CarpetedFloor.ToString(), Category = AmenityCategory.Room.ToString(), Description = "Carpeted flooring" },
                new Amenity { Id = 54, Name = AmenityName.HardwoodFloor.ToString(), Category = AmenityCategory.Room.ToString(), Description = "Hardwood flooring" },
                new Amenity { Id = 55, Name = AmenityName.PrivateEntrance.ToString(), Category = AmenityCategory.Room.ToString(), Description = "Private entrance to room" },
                new Amenity { Id = 56, Name = AmenityName.Balcony.ToString(), Category = AmenityCategory.Room.ToString(), Description = "Room with balcony" },
                new Amenity { Id = 57, Name = AmenityName.Terrace.ToString(), Category = AmenityCategory.Room.ToString(), Description = "Private terrace" },
                new Amenity { Id = 58, Name = AmenityName.Patio.ToString(), Category = AmenityCategory.Room.ToString(), Description = "Private patio" },
                new Amenity { Id = 59, Name = AmenityName.GardenView.ToString(), Category = AmenityCategory.Room.ToString(), Description = "Room with garden view" },
                new Amenity { Id = 60, Name = AmenityName.CityView.ToString(), Category = AmenityCategory.Room.ToString(), Description = "Room with city view" },
                new Amenity { Id = 61, Name = AmenityName.SeaView.ToString(), Category = AmenityCategory.Room.ToString(), Description = "Room with sea view" },
                new Amenity { Id = 62, Name = AmenityName.MountainView.ToString(), Category = AmenityCategory.Room.ToString(), Description = "Room with mountain view" },
                new Amenity { Id = 63, Name = AmenityName.PoolView.ToString(), Category = AmenityCategory.Room.ToString(), Description = "Room with pool view" },

                // ===== BEDROOM =====
                new Amenity { Id = 64, Name = AmenityName.ExtraLongBeds.ToString(), Category = AmenityCategory.Bedroom.ToString(), Description = "Extra long beds" },
                new Amenity { Id = 65, Name = AmenityName.SofaBed.ToString(), Category = AmenityCategory.Bedroom.ToString(), Description = "Sofa bed available" },
                new Amenity { Id = 66, Name = AmenityName.BabyCot.ToString(), Category = AmenityCategory.Bedroom.ToString(), Description = "Baby cot available" },
                new Amenity { Id = 67, Name = AmenityName.CribsAvailable.ToString(), Category = AmenityCategory.Bedroom.ToString(), Description = "Cribs available for infants" },
                new Amenity { Id = 68, Name = AmenityName.HypoallergenicBedding.ToString(), Category = AmenityCategory.Bedroom.ToString(), Description = "Hypoallergenic bedding" },
                new Amenity { Id = 69, Name = AmenityName.BlackoutCurtains.ToString(), Category = AmenityCategory.Bedroom.ToString(), Description = "Blackout curtains" },

                // ===== BATHROOM =====
                new Amenity { Id = 70, Name = AmenityName.PrivateBathroom.ToString(), Category = AmenityCategory.Bathroom.ToString(), Description = "Private bathroom" },
                new Amenity { Id = 71, Name = AmenityName.SharedBathroom.ToString(), Category = AmenityCategory.Bathroom.ToString(), Description = "Shared bathroom facilities" },
                new Amenity { Id = 72, Name = AmenityName.Shower.ToString(), Category = AmenityCategory.Bathroom.ToString(), Description = "Shower" },
                new Amenity { Id = 73, Name = AmenityName.WalkInShower.ToString(), Category = AmenityCategory.Bathroom.ToString(), Description = "Walk-in shower" },
                new Amenity { Id = 74, Name = AmenityName.Bathtub.ToString(), Category = AmenityCategory.Bathroom.ToString(), Description = "Bathtub" },
                new Amenity { Id = 75, Name = AmenityName.JacuzziBathtub.ToString(), Category = AmenityCategory.Bathroom.ToString(), Description = "Jacuzzi bathtub" },
                new Amenity { Id = 76, Name = AmenityName.Bidet.ToString(), Category = AmenityCategory.Bathroom.ToString(), Description = "Bidet" },
                new Amenity { Id = 77, Name = AmenityName.Toilet.ToString(), Category = AmenityCategory.Bathroom.ToString(), Description = "Toilet" },
                new Amenity { Id = 78, Name = AmenityName.ToiletPaper.ToString(), Category = AmenityCategory.Bathroom.ToString(), Description = "Toilet paper provided" },
                new Amenity { Id = 79, Name = AmenityName.Towels.ToString(), Category = AmenityCategory.Bathroom.ToString(), Description = "Towels provided" },
                new Amenity { Id = 80, Name = AmenityName.Bathrobes.ToString(), Category = AmenityCategory.Bathroom.ToString(), Description = "Bathrobes provided" },
                new Amenity { Id = 81, Name = AmenityName.Slippers.ToString(), Category = AmenityCategory.Bathroom.ToString(), Description = "Slippers provided" },
                new Amenity { Id = 82, Name = AmenityName.HairDryer.ToString(), Category = AmenityCategory.Bathroom.ToString(), Description = "Hair dryer" },
                new Amenity { Id = 83, Name = AmenityName.FreeToiletries.ToString(), Category = AmenityCategory.Bathroom.ToString(), Description = "Free toiletries" },
                new Amenity { Id = 84, Name = AmenityName.Shampoo.ToString(), Category = AmenityCategory.Bathroom.ToString(), Description = "Shampoo provided" },
                new Amenity { Id = 85, Name = AmenityName.Conditioner.ToString(), Category = AmenityCategory.Bathroom.ToString(), Description = "Hair conditioner provided" },
                new Amenity { Id = 86, Name = AmenityName.BodySoap.ToString(), Category = AmenityCategory.Bathroom.ToString(), Description = "Body soap provided" },

                // ===== KITCHEN =====
                new Amenity { Id = 87, Name = AmenityName.Kitchen.ToString(), Category = AmenityCategory.Kitchen.ToString(), Description = "Full kitchen" },
                new Amenity { Id = 88, Name = AmenityName.Kitchenette.ToString(), Category = AmenityCategory.Kitchen.ToString(), Description = "Kitchenette with basic appliances" },
                new Amenity { Id = 89, Name = AmenityName.Oven.ToString(), Category = AmenityCategory.Kitchen.ToString(), Description = "Oven" },
                new Amenity { Id = 90, Name = AmenityName.Stove.ToString(), Category = AmenityCategory.Kitchen.ToString(), Description = "Stove/cooktop" },
                new Amenity { Id = 91, Name = AmenityName.Dishwasher.ToString(), Category = AmenityCategory.Kitchen.ToString(), Description = "Dishwasher" },
                new Amenity { Id = 92, Name = AmenityName.WashingMachine.ToString(), Category = AmenityCategory.Kitchen.ToString(), Description = "Washing machine" },
                new Amenity { Id = 93, Name = AmenityName.Dryer.ToString(), Category = AmenityCategory.Kitchen.ToString(), Description = "Clothes dryer" },
                new Amenity { Id = 94, Name = AmenityName.Toaster.ToString(), Category = AmenityCategory.Kitchen.ToString(), Description = "Toaster" },
                new Amenity { Id = 95, Name = AmenityName.Blender.ToString(), Category = AmenityCategory.Kitchen.ToString(), Description = "Blender" },
                new Amenity { Id = 96, Name = AmenityName.CookingUtensils.ToString(), Category = AmenityCategory.Kitchen.ToString(), Description = "Cooking utensils provided" },

                // ===== FOOD & DRINK =====
                new Amenity { Id = 97, Name = AmenityName.Restaurant.ToString(), Category = AmenityCategory.FoodAndDrink.ToString(), Description = "On-site restaurant" },
                new Amenity { Id = 98, Name = AmenityName.BuffetRestaurant.ToString(), Category = AmenityCategory.FoodAndDrink.ToString(), Description = "Buffet-style restaurant" },
                new Amenity { Id = 99, Name = AmenityName.ALaCarteRestaurant.ToString(), Category = AmenityCategory.FoodAndDrink.ToString(), Description = "À la carte restaurant" },
                new Amenity { Id = 100, Name = AmenityName.RoomService.ToString(), Category = AmenityCategory.FoodAndDrink.ToString(), Description = "Room service available" },
                new Amenity { Id = 101, Name = AmenityName.BreakfastIncluded.ToString(), Category = AmenityCategory.FoodAndDrink.ToString(), Description = "Breakfast included in rate" },
                new Amenity { Id = 102, Name = AmenityName.BreakfastBuffet.ToString(), Category = AmenityCategory.FoodAndDrink.ToString(), Description = "Breakfast buffet" },
                new Amenity { Id = 103, Name = AmenityName.ContinentalBreakfast.ToString(), Category = AmenityCategory.FoodAndDrink.ToString(), Description = "Continental breakfast" },
                new Amenity { Id = 104, Name = AmenityName.HalalFood.ToString(), Category = AmenityCategory.FoodAndDrink.ToString(), Description = "Halal food options" },
                new Amenity { Id = 105, Name = AmenityName.VegetarianFood.ToString(), Category = AmenityCategory.FoodAndDrink.ToString(), Description = "Vegetarian food options" },
                new Amenity { Id = 106, Name = AmenityName.VeganOptions.ToString(), Category = AmenityCategory.FoodAndDrink.ToString(), Description = "Vegan food options" },
                new Amenity { Id = 107, Name = AmenityName.Bar.ToString(), Category = AmenityCategory.FoodAndDrink.ToString(), Description = "Bar on premises" },
                new Amenity { Id = 108, Name = AmenityName.PoolBar.ToString(), Category = AmenityCategory.FoodAndDrink.ToString(), Description = "Poolside bar" },
                new Amenity { Id = 109, Name = AmenityName.SnackBar.ToString(), Category = AmenityCategory.FoodAndDrink.ToString(), Description = "Snack bar" },
                new Amenity { Id = 110, Name = AmenityName.Cafe.ToString(), Category = AmenityCategory.FoodAndDrink.ToString(), Description = "Café on premises" },
                new Amenity { Id = 111, Name = AmenityName.CoffeeShop.ToString(), Category = AmenityCategory.FoodAndDrink.ToString(), Description = "Coffee shop" },
                new Amenity { Id = 112, Name = AmenityName.VendingMachines.ToString(), Category = AmenityCategory.FoodAndDrink.ToString(), Description = "Vending machines" },
                new Amenity { Id = 113, Name = AmenityName.GroceryDelivery.ToString(), Category = AmenityCategory.FoodAndDrink.ToString(), Description = "Grocery delivery service" },

                // ===== ENTERTAINMENT & LEISURE =====
                new Amenity { Id = 114, Name = AmenityName.SwimmingPool.ToString(), Category = AmenityCategory.Entertainment.ToString(), Description = "Swimming pool" },
                new Amenity { Id = 115, Name = AmenityName.OutdoorPool.ToString(), Category = AmenityCategory.Entertainment.ToString(), Description = "Outdoor swimming pool" },
                new Amenity { Id = 116, Name = AmenityName.IndoorPool.ToString(), Category = AmenityCategory.Entertainment.ToString(), Description = "Indoor swimming pool" },
                new Amenity { Id = 117, Name = AmenityName.HeatedPool.ToString(), Category = AmenityCategory.Entertainment.ToString(), Description = "Heated swimming pool" },
                new Amenity { Id = 118, Name = AmenityName.InfinityPool.ToString(), Category = AmenityCategory.Entertainment.ToString(), Description = "Infinity pool" },
                new Amenity { Id = 119, Name = AmenityName.KidsPool.ToString(), Category = AmenityCategory.Entertainment.ToString(), Description = "Children's pool" },
                new Amenity { Id = 120, Name = AmenityName.WaterPark.ToString(), Category = AmenityCategory.Entertainment.ToString(), Description = "Water park" },
                new Amenity { Id = 121, Name = AmenityName.Gym.ToString(), Category = AmenityCategory.Wellness.ToString(), Description = "Gym/fitness center" },
                new Amenity { Id = 122, Name = AmenityName.FitnessCenter.ToString(), Category = AmenityCategory.Wellness.ToString(), Description = "Fitness center" },
                new Amenity { Id = 123, Name = AmenityName.PersonalTrainer.ToString(), Category = AmenityCategory.Wellness.ToString(), Description = "Personal trainer available" },
                new Amenity { Id = 124, Name = AmenityName.Spa.ToString(), Category = AmenityCategory.Wellness.ToString(), Description = "Spa facilities" },
                new Amenity { Id = 125, Name = AmenityName.WellnessCenter.ToString(), Category = AmenityCategory.Wellness.ToString(), Description = "Wellness center" },
                new Amenity { Id = 126, Name = AmenityName.Sauna.ToString(), Category = AmenityCategory.Wellness.ToString(), Description = "Sauna" },
                new Amenity { Id = 127, Name = AmenityName.SteamRoom.ToString(), Category = AmenityCategory.Wellness.ToString(), Description = "Steam room" },
                new Amenity { Id = 128, Name = AmenityName.Hammam.ToString(), Category = AmenityCategory.Wellness.ToString(), Description = "Turkish bath/hammam" },
                new Amenity { Id = 129, Name = AmenityName.Jacuzzi.ToString(), Category = AmenityCategory.Wellness.ToString(), Description = "Jacuzzi/hot tub" },
                new Amenity { Id = 130, Name = AmenityName.MassageService.ToString(), Category = AmenityCategory.Wellness.ToString(), Description = "Massage services" },
                new Amenity { Id = 131, Name = AmenityName.BeautySalon.ToString(), Category = AmenityCategory.Wellness.ToString(), Description = "Beauty salon" },
                new Amenity { Id = 132, Name = AmenityName.YogaClasses.ToString(), Category = AmenityCategory.Wellness.ToString(), Description = "Yoga classes" },
                new Amenity { Id = 133, Name = AmenityName.Aerobics.ToString(), Category = AmenityCategory.Wellness.ToString(), Description = "Aerobics classes" },
                new Amenity { Id = 134, Name = AmenityName.NightClub.ToString(), Category = AmenityCategory.Entertainment.ToString(), Description = "Night club" },
                new Amenity { Id = 135, Name = AmenityName.LiveMusic.ToString(), Category = AmenityCategory.Entertainment.ToString(), Description = "Live music performances" },
                new Amenity { Id = 136, Name = AmenityName.DJ.ToString(), Category = AmenityCategory.Entertainment.ToString(), Description = "DJ entertainment" },
                new Amenity { Id = 137, Name = AmenityName.CinemaRoom.ToString(), Category = AmenityCategory.Entertainment.ToString(), Description = "Cinema/movie room" },
                new Amenity { Id = 138, Name = AmenityName.GameRoom.ToString(), Category = AmenityCategory.Entertainment.ToString(), Description = "Game room" },
                new Amenity { Id = 139, Name = AmenityName.Billiards.ToString(), Category = AmenityCategory.Entertainment.ToString(), Description = "Billiards/pool table" },
                new Amenity { Id = 140, Name = AmenityName.TableTennis.ToString(), Category = AmenityCategory.Entertainment.ToString(), Description = "Table tennis" },
                new Amenity { Id = 141, Name = AmenityName.Bowling.ToString(), Category = AmenityCategory.Entertainment.ToString(), Description = "Bowling alley" },
                new Amenity { Id = 142, Name = AmenityName.Darts.ToString(), Category = AmenityCategory.Entertainment.ToString(), Description = "Darts" },
                new Amenity { Id = 143, Name = AmenityName.Karaoke.ToString(), Category = AmenityCategory.Entertainment.ToString(), Description = "Karaoke" },
                new Amenity { Id = 144, Name = AmenityName.Library.ToString(), Category = AmenityCategory.Entertainment.ToString(), Description = "Library" },
                new Amenity { Id = 145, Name = AmenityName.TVLounge.ToString(), Category = AmenityCategory.Entertainment.ToString(), Description = "TV lounge/common area" },
                new Amenity { Id = 146, Name = AmenityName.KidsPlayArea.ToString(), Category = AmenityCategory.Entertainment.ToString(), Description = "Children's play area" },
                new Amenity { Id = 147, Name = AmenityName.KidsClub.ToString(), Category = AmenityCategory.Entertainment.ToString(), Description = "Kids club" },
                new Amenity { Id = 148, Name = AmenityName.BabysittingService.ToString(), Category = AmenityCategory.Entertainment.ToString(), Description = "Babysitting service" },

                // ===== OUTDOOR & ACTIVITIES =====
                new Amenity { Id = 149, Name = AmenityName.Garden.ToString(), Category = AmenityCategory.Outdoor.ToString(), Description = "Garden area" },
                new Amenity { Id = 150, Name = AmenityName.SunTerrace.ToString(), Category = AmenityCategory.Outdoor.ToString(), Description = "Sun terrace" },
                new Amenity { Id = 151, Name = AmenityName.PicnicArea.ToString(), Category = AmenityCategory.Outdoor.ToString(), Description = "Picnic area" },
                new Amenity { Id = 152, Name = AmenityName.BBQFacilities.ToString(), Category = AmenityCategory.Outdoor.ToString(), Description = "Barbecue facilities" },
                new Amenity { Id = 153, Name = AmenityName.BeachAccess.ToString(), Category = AmenityCategory.Outdoor.ToString(), Description = "Beach access" },
                new Amenity { Id = 154, Name = AmenityName.PrivateBeach.ToString(), Category = AmenityCategory.Outdoor.ToString(), Description = "Private beach area" },
                new Amenity { Id = 155, Name = AmenityName.BeachUmbrellas.ToString(), Category = AmenityCategory.Outdoor.ToString(), Description = "Beach umbrellas" },
                new Amenity { Id = 156, Name = AmenityName.BeachChairs.ToString(), Category = AmenityCategory.Outdoor.ToString(), Description = "Beach chairs" },
                new Amenity { Id = 157, Name = AmenityName.WaterSports.ToString(), Category = AmenityCategory.Activities.ToString(), Description = "Water sports activities" },
                new Amenity { Id = 158, Name = AmenityName.Diving.ToString(), Category = AmenityCategory.Activities.ToString(), Description = "Diving facilities/lessons" },
                new Amenity { Id = 159, Name = AmenityName.Snorkeling.ToString(), Category = AmenityCategory.Activities.ToString(), Description = "Snorkeling equipment/activities" },
                new Amenity { Id = 160, Name = AmenityName.Canoeing.ToString(), Category = AmenityCategory.Activities.ToString(), Description = "Canoeing/kayaking" },
                new Amenity { Id = 161, Name = AmenityName.Fishing.ToString(), Category = AmenityCategory.Activities.ToString(), Description = "Fishing facilities" },
                new Amenity { Id = 162, Name = AmenityName.HorseRiding.ToString(), Category = AmenityCategory.Activities.ToString(), Description = "Horse riding" },
                new Amenity { Id = 163, Name = AmenityName.Hiking.ToString(), Category = AmenityCategory.Activities.ToString(), Description = "Hiking trails/guides" },
                new Amenity { Id = 164, Name = AmenityName.Cycling.ToString(), Category = AmenityCategory.Activities.ToString(), Description = "Cycling/biking" },
                new Amenity { Id = 165, Name = AmenityName.BikeRental.ToString(), Category = AmenityCategory.Activities.ToString(), Description = "Bicycle rental" },

                // ===== BUSINESS & EVENTS =====
                new Amenity { Id = 166, Name = AmenityName.BusinessCenter.ToString(), Category = AmenityCategory.Business.ToString(), Description = "Business center" },
                new Amenity { Id = 167, Name = AmenityName.MeetingRooms.ToString(), Category = AmenityCategory.Business.ToString(), Description = "Meeting rooms" },
                new Amenity { Id = 168, Name = AmenityName.ConferenceHall.ToString(), Category = AmenityCategory.Business.ToString(), Description = "Conference hall" },
                new Amenity { Id = 169, Name = AmenityName.BanquetHall.ToString(), Category = AmenityCategory.Business.ToString(), Description = "Banquet hall" },
                new Amenity { Id = 170, Name = AmenityName.WeddingServices.ToString(), Category = AmenityCategory.Events.ToString(), Description = "Wedding services" },
                new Amenity { Id = 171, Name = AmenityName.Fax.ToString(), Category = AmenityCategory.Business.ToString(), Description = "Fax service" },
                new Amenity { Id = 172, Name = AmenityName.Photocopying.ToString(), Category = AmenityCategory.Business.ToString(), Description = "Photocopying service" },
                new Amenity { Id = 173, Name = AmenityName.HighSpeedInternet.ToString(), Category = AmenityCategory.Business.ToString(), Description = "High-speed internet" },

                // ===== TRANSPORTATION =====
                new Amenity { Id = 174, Name = AmenityName.AirportShuttle.ToString(), Category = AmenityCategory.Transportation.ToString(), Description = "Airport shuttle service" },
                new Amenity { Id = 175, Name = AmenityName.PaidAirportShuttle.ToString(), Category = AmenityCategory.Transportation.ToString(), Description = "Paid airport shuttle" },
                new Amenity { Id = 176, Name = AmenityName.CarRental.ToString(), Category = AmenityCategory.Transportation.ToString(), Description = "Car rental service" },
                new Amenity { Id = 177, Name = AmenityName.TaxiService.ToString(), Category = AmenityCategory.Transportation.ToString(), Description = "Taxi service" },
                new Amenity { Id = 178, Name = AmenityName.ShuttleService.ToString(), Category = AmenityCategory.Transportation.ToString(), Description = "Shuttle service" },
                new Amenity { Id = 179, Name = AmenityName.PublicTransportNearby.ToString(), Category = AmenityCategory.Transportation.ToString(), Description = "Public transport nearby" },
                new Amenity { Id = 180, Name = AmenityName.EVChargingStation.ToString(), Category = AmenityCategory.Transportation.ToString(), Description = "Electric vehicle charging station" },

                // ===== SAFETY & SECURITY =====
                new Amenity { Id = 181, Name = AmenityName.FireExtinguishers.ToString(), Category = AmenityCategory.Safety.ToString(), Description = "Fire extinguishers" },
                new Amenity { Id = 182, Name = AmenityName.SmokeDetectors.ToString(), Category = AmenityCategory.Safety.ToString(), Description = "Smoke detectors" },
                new Amenity { Id = 183, Name = AmenityName.CarbonMonoxideDetector.ToString(), Category = AmenityCategory.Safety.ToString(), Description = "Carbon monoxide detector" },
                new Amenity { Id = 184, Name = AmenityName.CCTV.ToString(), Category = AmenityCategory.Safety.ToString(), Description = "CCTV surveillance" },
                new Amenity { Id = 185, Name = AmenityName.Security24Hours.ToString(), Category = AmenityCategory.Safety.ToString(), Description = "24-hour security" },
                new Amenity { Id = 186, Name = AmenityName.KeyCardAccess.ToString(), Category = AmenityCategory.Safety.ToString(), Description = "Key card access" },
                new Amenity { Id = 187, Name = AmenityName.ElectronicDoorLocks.ToString(), Category = AmenityCategory.Safety.ToString(), Description = "Electronic door locks" },
                new Amenity { Id = 188, Name = AmenityName.SafeDepositBox.ToString(), Category = AmenityCategory.Safety.ToString(), Description = "Safe deposit box" },
                new Amenity { Id = 189, Name = AmenityName.FirstAidKit.ToString(), Category = AmenityCategory.Safety.ToString(), Description = "First aid kit available" },
                new Amenity { Id = 190, Name = AmenityName.EmergencyExitSigns.ToString(), Category = AmenityCategory.Safety.ToString(), Description = "Emergency exit signs" },

                // ===== ACCESSIBILITY =====
                new Amenity { Id = 191, Name = AmenityName.WheelchairAccessible.ToString(), Category = AmenityCategory.Accessibility.ToString(), Description = "Wheelchair accessible facilities" },
                new Amenity { Id = 192, Name = AmenityName.AccessibleParking.ToString(), Category = AmenityCategory.Accessibility.ToString(), Description = "Accessible parking spaces" },
                new Amenity { Id = 193, Name = AmenityName.AccessibleBathroom.ToString(), Category = AmenityCategory.Accessibility.ToString(), Description = "Accessible bathroom" },
                new Amenity { Id = 194, Name = AmenityName.GrabRails.ToString(), Category = AmenityCategory.Accessibility.ToString(), Description = "Grab rails in bathroom" },
                new Amenity { Id = 195, Name = AmenityName.LoweredSink.ToString(), Category = AmenityCategory.Accessibility.ToString(), Description = "Lowered sink" },
                new Amenity { Id = 196, Name = AmenityName.BrailleSignage.ToString(), Category = AmenityCategory.Accessibility.ToString(), Description = "Braille signage" },
                new Amenity { Id = 197, Name = AmenityName.VisualAids.ToString(), Category = AmenityCategory.Accessibility.ToString(), Description = "Visual aids" },
                new Amenity { Id = 198, Name = AmenityName.HearingAccessible.ToString(), Category = AmenityCategory.Accessibility.ToString(), Description = "Hearing accessible features" },

                // ===== PETS =====
                new Amenity { Id = 199, Name = AmenityName.PetsAllowed.ToString(), Category = AmenityCategory.Pets.ToString(), Description = "Pets allowed" },
                new Amenity { Id = 200, Name = AmenityName.PetsNotAllowed.ToString(), Category = AmenityCategory.Pets.ToString(), Description = "Pets not allowed" },
                new Amenity { Id = 201, Name = AmenityName.PetBowls.ToString(), Category = AmenityCategory.Pets.ToString(), Description = "Pet bowls provided" },
                new Amenity { Id = 202, Name = AmenityName.PetBasket.ToString(), Category = AmenityCategory.Pets.ToString(), Description = "Pet bed/basket" },
                new Amenity { Id = 203, Name = AmenityName.PetSittingService.ToString(), Category = AmenityCategory.Pets.ToString(), Description = "Pet sitting service" },

                // ===== SERVICES =====
                new Amenity { Id = 204, Name = AmenityName.TourDesk.ToString(), Category = AmenityCategory.Services.ToString(), Description = "Tour desk/information" },
                new Amenity { Id = 205, Name = AmenityName.TicketService.ToString(), Category = AmenityCategory.Services.ToString(), Description = "Ticket booking service" },
                new Amenity { Id = 206, Name = AmenityName.TourOrganization.ToString(), Category = AmenityCategory.Services.ToString(), Description = "Tour organization" },
                new Amenity { Id = 207, Name = AmenityName.LocalGuides.ToString(), Category = AmenityCategory.Services.ToString(), Description = "Local guides available" },
                new Amenity { Id = 208, Name = AmenityName.Lockers.ToString(), Category = AmenityCategory.Services.ToString(), Description = "Lockers/storage lockers" },

                // ===== CLEANLINESS & HEALTH =====
                new Amenity { Id = 209, Name = AmenityName.EnhancedCleaning.ToString(), Category = AmenityCategory.Cleanliness.ToString(), Description = "Enhanced cleaning protocols" },
                new Amenity { Id = 210, Name = AmenityName.ContactlessCheckIn.ToString(), Category = AmenityCategory.Cleanliness.ToString(), Description = "Contactless check-in" },
                new Amenity { Id = 211, Name = AmenityName.HandSanitizer.ToString(), Category = AmenityCategory.Cleanliness.ToString(), Description = "Hand sanitizer available" },
                new Amenity { Id = 212, Name = AmenityName.TemperatureCheck.ToString(), Category = AmenityCategory.Cleanliness.ToString(), Description = "Temperature checks" },
                new Amenity { Id = 213, Name = AmenityName.MedicalAssistance.ToString(), Category = AmenityCategory.Cleanliness.ToString(), Description = "Medical assistance available" }
            );
        }
    }
