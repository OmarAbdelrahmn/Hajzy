using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Domain.Migrations
{
    /// <inheritdoc />
    public partial class amenityseeding : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Amenities",
                columns: new[] { "Id", "Category", "Description", "Name" },
                values: new object[,]
                {
                    { 1, 0, "Wireless internet access", 0 },
                    { 2, 0, "Complimentary wireless internet", 1 },
                    { 3, 0, "Paid wireless internet service", 2 },
                    { 4, 0, "Air conditioning system", 3 },
                    { 5, 0, "Heating system", 4 },
                    { 6, 0, "Backup power generator", 5 },
                    { 7, 0, "Parking available", 6 },
                    { 8, 0, "Complimentary parking", 7 },
                    { 9, 0, "Paid parking service", 8 },
                    { 10, 0, "Valet parking service", 9 },
                    { 11, 0, "Street parking available", 10 },
                    { 12, 0, "Garage parking facility", 11 },
                    { 13, 0, "Elevator access", 12 },
                    { 14, 0, "Luggage storage facility", 13 },
                    { 15, 0, "24-hour reception desk", 14 },
                    { 16, 0, "Express check-in service", 15 },
                    { 17, 0, "Express check-out service", 16 },
                    { 18, 0, "Concierge service", 17 },
                    { 19, 0, "Wake-up call service", 18 },
                    { 20, 0, "Daily housekeeping service", 19 },
                    { 21, 0, "Laundry service", 20 },
                    { 22, 0, "Dry cleaning service", 21 },
                    { 23, 0, "Ironing service", 22 },
                    { 24, 0, "Currency exchange service", 23 },
                    { 25, 0, "On-site ATM machine", 24 },
                    { 26, 0, "Gift shop on premises", 25 },
                    { 27, 0, "Mini market/shop on premises", 26 },
                    { 28, 0, "Designated smoking area", 27 },
                    { 29, 0, "Non-smoking rooms available", 28 },
                    { 30, 0, "Family-sized rooms available", 29 },
                    { 31, 0, "Soundproofed rooms", 30 },
                    { 32, 4, "Television", 31 },
                    { 33, 4, "Smart TV with internet connectivity", 32 },
                    { 34, 4, "Cable television channels", 33 },
                    { 35, 4, "Satellite television channels", 34 },
                    { 36, 4, "Streaming services access", 35 },
                    { 37, 4, "Work desk", 36 },
                    { 38, 4, "Seating area in room", 37 },
                    { 39, 4, "Sofa in room", 38 },
                    { 40, 4, "Wardrobe for clothing", 39 },
                    { 41, 4, "Closet storage", 40 },
                    { 42, 4, "Mini bar in room", 41 },
                    { 43, 4, "Refrigerator in room", 42 },
                    { 44, 4, "Microwave oven", 43 },
                    { 45, 4, "Electric kettle", 44 },
                    { 46, 4, "Coffee making machine", 45 },
                    { 47, 4, "Tea making facilities", 46 },
                    { 48, 4, "Dining table", 47 },
                    { 49, 4, "In-room safe", 48 },
                    { 50, 4, "Iron provided", 49 },
                    { 51, 4, "Ironing board", 50 },
                    { 52, 4, "Alarm clock", 51 },
                    { 53, 4, "Carpeted flooring", 52 },
                    { 54, 4, "Hardwood flooring", 53 },
                    { 55, 4, "Private entrance to room", 54 },
                    { 56, 4, "Room with balcony", 55 },
                    { 57, 4, "Private terrace", 56 },
                    { 58, 4, "Private patio", 57 },
                    { 59, 4, "Room with garden view", 58 },
                    { 60, 4, "Room with city view", 59 },
                    { 61, 4, "Room with sea view", 60 },
                    { 62, 4, "Room with mountain view", 61 },
                    { 63, 4, "Room with pool view", 62 },
                    { 64, 5, "Extra long beds", 63 },
                    { 65, 5, "Sofa bed available", 64 },
                    { 66, 5, "Baby cot available", 65 },
                    { 67, 5, "Cribs available for infants", 66 },
                    { 68, 5, "Hypoallergenic bedding", 67 },
                    { 69, 5, "Blackout curtains", 68 },
                    { 70, 6, "Private bathroom", 69 },
                    { 71, 6, "Shared bathroom facilities", 70 },
                    { 72, 6, "Shower", 71 },
                    { 73, 6, "Walk-in shower", 72 },
                    { 74, 6, "Bathtub", 73 },
                    { 75, 6, "Jacuzzi bathtub", 74 },
                    { 76, 6, "Bidet", 75 },
                    { 77, 6, "Toilet", 76 },
                    { 78, 6, "Toilet paper provided", 77 },
                    { 79, 6, "Towels provided", 78 },
                    { 80, 6, "Bathrobes provided", 79 },
                    { 81, 6, "Slippers provided", 80 },
                    { 82, 6, "Hair dryer", 81 },
                    { 83, 6, "Free toiletries", 82 },
                    { 84, 6, "Shampoo provided", 83 },
                    { 85, 6, "Hair conditioner provided", 84 },
                    { 86, 6, "Body soap provided", 85 },
                    { 87, 7, "Full kitchen", 86 },
                    { 88, 7, "Kitchenette with basic appliances", 87 },
                    { 89, 7, "Oven", 88 },
                    { 90, 7, "Stove/cooktop", 89 },
                    { 91, 7, "Dishwasher", 90 },
                    { 92, 7, "Washing machine", 91 },
                    { 93, 7, "Clothes dryer", 92 },
                    { 94, 7, "Toaster", 93 },
                    { 95, 7, "Blender", 94 },
                    { 96, 7, "Cooking utensils provided", 95 },
                    { 97, 8, "On-site restaurant", 96 },
                    { 98, 8, "Buffet-style restaurant", 97 },
                    { 99, 8, "À la carte restaurant", 98 },
                    { 100, 8, "Room service available", 99 },
                    { 101, 8, "Breakfast included in rate", 100 },
                    { 102, 8, "Breakfast buffet", 101 },
                    { 103, 8, "Continental breakfast", 102 },
                    { 104, 8, "Halal food options", 103 },
                    { 105, 8, "Vegetarian food options", 104 },
                    { 106, 8, "Vegan food options", 105 },
                    { 107, 8, "Bar on premises", 106 },
                    { 108, 8, "Poolside bar", 107 },
                    { 109, 8, "Snack bar", 108 },
                    { 110, 8, "Café on premises", 109 },
                    { 111, 8, "Coffee shop", 110 },
                    { 112, 8, "Vending machines", 111 },
                    { 113, 8, "Grocery delivery service", 112 },
                    { 114, 9, "Swimming pool", 113 },
                    { 115, 9, "Outdoor swimming pool", 114 },
                    { 116, 9, "Indoor swimming pool", 115 },
                    { 117, 9, "Heated swimming pool", 116 },
                    { 118, 9, "Infinity pool", 117 },
                    { 119, 9, "Children's pool", 118 },
                    { 120, 9, "Water park", 119 },
                    { 121, 10, "Gym/fitness center", 120 },
                    { 122, 10, "Fitness center", 121 },
                    { 123, 10, "Personal trainer available", 122 },
                    { 124, 10, "Spa facilities", 123 },
                    { 125, 10, "Wellness center", 124 },
                    { 126, 10, "Sauna", 125 },
                    { 127, 10, "Steam room", 126 },
                    { 128, 10, "Turkish bath/hammam", 127 },
                    { 129, 10, "Jacuzzi/hot tub", 128 },
                    { 130, 10, "Massage services", 129 },
                    { 131, 10, "Beauty salon", 130 },
                    { 132, 10, "Yoga classes", 131 },
                    { 133, 10, "Aerobics classes", 132 },
                    { 134, 9, "Night club", 133 },
                    { 135, 9, "Live music performances", 134 },
                    { 136, 9, "DJ entertainment", 135 },
                    { 137, 9, "Cinema/movie room", 136 },
                    { 138, 9, "Game room", 137 },
                    { 139, 9, "Billiards/pool table", 138 },
                    { 140, 9, "Table tennis", 139 },
                    { 141, 9, "Bowling alley", 140 },
                    { 142, 9, "Darts", 141 },
                    { 143, 9, "Karaoke", 142 },
                    { 144, 9, "Library", 143 },
                    { 145, 9, "TV lounge/common area", 144 },
                    { 146, 9, "Children's play area", 145 },
                    { 147, 9, "Kids club", 146 },
                    { 148, 9, "Babysitting service", 147 },
                    { 149, 11, "Garden area", 148 },
                    { 150, 11, "Sun terrace", 149 },
                    { 151, 11, "Picnic area", 150 },
                    { 152, 11, "Barbecue facilities", 151 },
                    { 153, 11, "Beach access", 152 },
                    { 154, 11, "Private beach area", 153 },
                    { 155, 11, "Beach umbrellas", 154 },
                    { 156, 11, "Beach chairs", 155 },
                    { 157, 12, "Water sports activities", 156 },
                    { 158, 12, "Diving facilities/lessons", 157 },
                    { 159, 12, "Snorkeling equipment/activities", 158 },
                    { 160, 12, "Canoeing/kayaking", 159 },
                    { 161, 12, "Fishing facilities", 160 },
                    { 162, 12, "Horse riding", 161 },
                    { 163, 12, "Hiking trails/guides", 162 },
                    { 164, 12, "Cycling/biking", 163 },
                    { 165, 12, "Bicycle rental", 164 },
                    { 166, 13, "Business center", 165 },
                    { 167, 13, "Meeting rooms", 166 },
                    { 168, 13, "Conference hall", 167 },
                    { 169, 13, "Banquet hall", 168 },
                    { 170, 14, "Wedding services", 169 },
                    { 171, 13, "Fax service", 170 },
                    { 172, 13, "Photocopying service", 171 },
                    { 173, 13, "High-speed internet", 172 },
                    { 174, 3, "Airport shuttle service", 173 },
                    { 175, 3, "Paid airport shuttle", 174 },
                    { 176, 3, "Car rental service", 175 },
                    { 177, 3, "Taxi service", 176 },
                    { 178, 3, "Shuttle service", 177 },
                    { 179, 3, "Public transport nearby", 178 },
                    { 180, 3, "Electric vehicle charging station", 179 },
                    { 181, 15, "Fire extinguishers", 180 },
                    { 182, 15, "Smoke detectors", 181 },
                    { 183, 15, "Carbon monoxide detector", 182 },
                    { 184, 15, "CCTV surveillance", 183 },
                    { 185, 15, "24-hour security", 184 },
                    { 186, 15, "Key card access", 185 },
                    { 187, 15, "Electronic door locks", 186 },
                    { 188, 15, "Safe deposit box", 187 },
                    { 189, 15, "First aid kit available", 188 },
                    { 190, 15, "Emergency exit signs", 189 },
                    { 191, 16, "Wheelchair accessible facilities", 190 },
                    { 192, 16, "Accessible parking spaces", 191 },
                    { 193, 16, "Accessible bathroom", 192 },
                    { 194, 16, "Grab rails in bathroom", 193 },
                    { 195, 16, "Lowered sink", 194 },
                    { 196, 16, "Braille signage", 195 },
                    { 197, 16, "Visual aids", 196 },
                    { 198, 16, "Hearing accessible features", 197 },
                    { 199, 17, "Pets allowed", 198 },
                    { 200, 17, "Pets not allowed", 199 },
                    { 201, 17, "Pet bowls provided", 200 },
                    { 202, 17, "Pet bed/basket", 201 },
                    { 203, 17, "Pet sitting service", 202 },
                    { 204, 1, "Tour desk/information", 203 },
                    { 205, 1, "Ticket booking service", 204 },
                    { 206, 1, "Tour organization", 205 },
                    { 207, 1, "Local guides available", 206 },
                    { 208, 1, "Lockers/storage lockers", 207 },
                    { 209, 2, "Enhanced cleaning protocols", 208 },
                    { 210, 2, "Contactless check-in", 209 },
                    { 211, 2, "Hand sanitizer available", 210 },
                    { 212, 2, "Temperature checks", 211 },
                    { 213, 2, "Medical assistance available", 212 }
                });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "59724D2D-E2B5-4C67-AB6F-D93478147B03",
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 1, 1, 17, 14, 9, 42, DateTimeKind.Utc).AddTicks(4697), "AQAAAAIAAYagAAAAEBLarBvufDRD5NAm9AFguHw3Zj0ErnpP15OFqdl9VSr8E28s5+X9bHCozUkBjF8aug==" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "59726D2D-E2B5-4C67-AB6F-D93878317B03",
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 1, 1, 17, 14, 9, 124, DateTimeKind.Utc).AddTicks(39), "AQAAAAIAAYagAAAAEPfSCzqzy/AaDfMoAbpkKvscL4a0VL8F5AC0twUJ80cc0w3kJqOKwsQ3cCUqu6jUww==" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "59726D2D-E2B5-4C67-AB6F-D99478317B03",
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 1, 1, 17, 14, 9, 206, DateTimeKind.Utc).AddTicks(3190), "AQAAAAIAAYagAAAAEEBcVB6Xgb4aMgzu71zJcDY/oeqIVhOLT3/k86cIl6uNgMgwBxxwrobwVyF+/qI+fQ==" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 17);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 18);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 19);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 20);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 21);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 22);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 23);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 24);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 25);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 26);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 27);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 28);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 29);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 30);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 31);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 32);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 33);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 34);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 35);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 36);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 37);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 38);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 39);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 40);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 41);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 42);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 43);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 44);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 45);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 46);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 47);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 48);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 49);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 50);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 51);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 52);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 53);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 54);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 55);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 56);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 57);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 58);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 59);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 60);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 61);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 62);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 63);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 64);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 65);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 66);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 67);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 68);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 69);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 70);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 71);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 72);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 73);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 74);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 75);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 76);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 77);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 78);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 79);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 80);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 81);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 82);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 83);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 84);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 85);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 86);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 87);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 88);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 89);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 90);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 91);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 92);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 93);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 94);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 95);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 96);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 97);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 98);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 99);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 100);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 101);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 102);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 103);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 104);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 105);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 106);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 107);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 108);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 109);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 110);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 111);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 112);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 113);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 114);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 115);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 116);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 117);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 118);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 119);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 120);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 121);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 122);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 123);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 124);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 125);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 126);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 127);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 128);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 129);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 130);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 131);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 132);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 133);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 134);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 135);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 136);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 137);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 138);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 139);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 140);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 141);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 142);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 143);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 144);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 145);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 146);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 147);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 148);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 149);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 150);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 151);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 152);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 153);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 154);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 155);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 156);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 157);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 158);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 159);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 160);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 161);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 162);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 163);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 164);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 165);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 166);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 167);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 168);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 169);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 170);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 171);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 172);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 173);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 174);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 175);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 176);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 177);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 178);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 179);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 180);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 181);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 182);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 183);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 184);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 185);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 186);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 187);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 188);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 189);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 190);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 191);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 192);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 193);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 194);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 195);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 196);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 197);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 198);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 199);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 200);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 201);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 202);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 203);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 204);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 205);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 206);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 207);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 208);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 209);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 210);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 211);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 212);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 213);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "59724D2D-E2B5-4C67-AB6F-D93478147B03",
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2025, 12, 31, 18, 18, 43, 928, DateTimeKind.Utc).AddTicks(9927), "AQAAAAIAAYagAAAAEKG8edG/eWBPNSyqHbH+6aJ8Zz5aUJWhTp+QOLtc6/spdvproVRvezl+j7LnPbbtkQ==" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "59726D2D-E2B5-4C67-AB6F-D93878317B03",
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2025, 12, 31, 18, 18, 43, 997, DateTimeKind.Utc).AddTicks(7669), "AQAAAAIAAYagAAAAEKTlknBSlozKFUOQTQGhbz2wxokjQHC/EbeVNRoQKa7KXlYdRpX1afnirQ6jyeqLEg==" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "59726D2D-E2B5-4C67-AB6F-D99478317B03",
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2025, 12, 31, 18, 18, 44, 76, DateTimeKind.Utc).AddTicks(5482), "AQAAAAIAAYagAAAAEGue0DqWMjSQBzWj9upxeDlqoYWEc7WMLnhY0Z+gXDidiqsFbquVfDSVcJjaZ+lV6Q==" });
        }
    }
}
