using Microsoft.AspNetCore.Mvc;

namespace Hajzzy.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DestinationsController : ControllerBase
    {
        private static readonly List<Destination> _destinations = new()
        {
            new Destination
            {
                Id = 1,
                Name = "Alley Palace",
                Location = "Aspen, USA",
                Rating = 4.1,
                ReviewCount = 245,
                Price = 199,
                ImageUrl = "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcSzFRCTv9icRYlk7EOy07wnFqpd4Wl2ZiKZAA&s",
                Description = "Aspen is as close as one can get to a storybook alpine town in America. The choose-your-own-adventure possibilities—skiing, hiking, dining shopping and more—are endless.",
                Facilities = new List<string> { "Internet", "Dinner", "Bathroom", "Pool" },
                Category = "Popular",
                IsRecommended = false
            },
            new Destination
            {
                Id = 2,
                Name = "Coeurdes Alpes",
                Location = "Aspen, USA",
                Rating = 4.5,
                ReviewCount = 365,
                Price = 199,
                ImageUrl = "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcRNRtWjDgvuJ3zgQwPL7n7PIUjjYdo_E3vnPQ&s",
                Description = "Aspen is as close as one can get to a storybook alpine town in America. The choose-your-own-adventure possibilities—skiing, hiking, dining shopping and more—are endless.",
                Facilities = new List<string> { "Internet", "Dinner", "Bathroom", "Pool" },
                Category = "Popular",
                IsRecommended = false
            },
            new Destination
            {
                Id = 3,
                Name = "Explore Aspen",
                Location = "Aspen, USA",
                Rating = 4.3,
                ReviewCount = 189,
                Price = 249,
                ImageUrl = "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcQEzTSt5fcsrX2p1l3nOHgrZiB20YVc-LzqhQ&s",
                Description = "Experience the breathtaking mountain views and luxury accommodations in the heart of Aspen's downtown area.",
                Facilities = new List<string> { "Internet", "Dinner", "Bathroom", "Pool", "Spa" },
                Category = "Recommended",
                IsRecommended = true
            },
            new Destination
            {
                Id = 4,
                Name = "Luxurious Aspen",
                Location = "Aspen, USA",
                Rating = 4.8,
                ReviewCount = 512,
                Price = 399,
                ImageUrl = "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTtDQETso6CaCvPgUYmUE1F_2GQXkRutP7qiA&s",
                Description = "Indulge in the finest luxury resort with world-class amenities and stunning panoramic views of the Rocky Mountains.",
                Facilities = new List<string> { "Internet", "Dinner", "Bathroom", "Pool", "Spa", "Gym" },
                Category = "Recommended",
                IsRecommended = true
            },
            new Destination
            {
                Id = 5,
                Name = "Mountain View Lodge",
                Location = "Aspen, USA",
                Rating = 4.6,
                ReviewCount = 298,
                Price = 329,
                ImageUrl = "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcSwpyJdAt_7L5U0AtNEIdsGqPwugDuCDKtonw&s",
                Description = "Cozy lodge nestled in the mountains offering authentic alpine experience with modern comfort and amenities.",
                Facilities = new List<string> { "Internet", "Dinner", "Bathroom", "Pool" },
                Category = "Popular",
                IsRecommended = false
            },
            new Destination
            {
                Id = 6,
                Name = "Alpine Retreat",
                Location = "Aspen, USA",
                Rating = 4.4,
                ReviewCount = 423,
                Price = 275,
                ImageUrl = "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcQXin9rS018iLcPfI7p4Wzt5fAcoiyc0l3EhQ&s",
                Description = "Perfect getaway for families and adventure seekers with easy access to ski slopes and hiking trails.",
                Facilities = new List<string> { "Internet", "Dinner", "Bathroom", "Pool", "Kids Club" },
                Category = "Popular",
                IsRecommended = true
            },
            new Destination
            {
                Id = 7,
                Name = "Aspen Grand Hotel",
                Location = "Aspen, USA",
                Rating = 4.7,
                ReviewCount = 634,
                Price = 449,
                ImageUrl = "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTOa0cT7P9jzFBqkWSw9Z6AYmAhJZTjECWMzA&s",
                Description = "Historic grand hotel offering timeless elegance, exceptional service, and unmatched views of Aspen's iconic peaks.",
                Facilities = new List<string> { "Internet", "Dinner", "Bathroom", "Pool", "Spa", "Gym", "Concierge" },
                Category = "Recommended",
                IsRecommended = true
            },
            new Destination
            {
                Id = 8,
                Name = "Snowy Peaks Resort",
                Location = "Aspen, USA",
                Rating = 4.2,
                ReviewCount = 287,
                Price = 219,
                ImageUrl = "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcQzpGUrXxInRt918JyzBahU1P-xdU_EcyAGyw&s",
                Description = "Budget-friendly resort with great amenities and direct access to the best skiing and snowboarding in Aspen.",
                Facilities = new List<string> { "Internet", "Dinner", "Bathroom", "Pool" },
                Category = "Popular",
                IsRecommended = false
            },
            new Destination
            {
                Id = 9,
                Name = "Riverside Chalets",
                Location = "Aspen, USA",
                Rating = 4.5,
                ReviewCount = 356,
                Price = 289,
                ImageUrl = "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTUAv0Nbi_S7vCxy2CwDqWYbYOWm36VKATwiQ&s",
                Description = "Charming chalets situated along the pristine Aspen river, offering tranquility and natural beauty.",
                Facilities = new List<string> { "Internet", "Dinner", "Bathroom", "Fireplace" },
                Category = "Recommended",
                IsRecommended = true
            },
            new Destination
            {
                Id = 10,
                Name = "Summit Lodge & Spa",
                Location = "Aspen, USA",
                Rating = 4.9,
                ReviewCount = 789,
                Price = 549,
                ImageUrl = "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcSdDVK_VJDBDlbBhZVSRjbaEoMfYI5n-qyllA&s",
                Description = "Premier destination for wellness and relaxation with award-winning spa services and gourmet dining experiences.",
                Facilities = new List<string> { "Internet", "Dinner", "Bathroom", "Pool", "Spa", "Gym", "Restaurant", "Bar" },
                Category = "Recommended",
                IsRecommended = true
            },
            new Destination
            {
                Id = 11,
                Name = "Winter Wonderland Inn",
                Location = "Aspen, USA",
                Rating = 4.3,
                ReviewCount = 412,
                Price = 259,
                ImageUrl = "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTtYQ8xZJlH7S2kZ3zxWGr0lQqhZwMnVKjKyA&s",
                Description = "Charming inn with traditional Alpine architecture and cozy fireplaces, perfect for a romantic winter getaway.",
                Facilities = new List<string> { "Internet", "Dinner", "Bathroom", "Fireplace", "Hot Tub" },
                Category = "Popular",
                IsRecommended = false
            },
            new Destination
            {
                Id = 12,
                Name = "Aspen Skyline Suites",
                Location = "Aspen, USA",
                Rating = 4.6,
                ReviewCount = 523,
                Price = 379,
                ImageUrl = "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcSYJxPQl2vHKFwAHp3KH9oBw7DqVPZmlMnvpg&s",
                Description = "Modern suites with floor-to-ceiling windows offering spectacular mountain views and contemporary luxury.",
                Facilities = new List<string> { "Internet", "Dinner", "Bathroom", "Pool", "Spa", "Gym", "Balcony" },
                Category = "Recommended",
                IsRecommended = true
            },
            new Destination
            {
                Id = 13,
                Name = "Pine Ridge Resort",
                Location = "Aspen, USA",
                Rating = 4.0,
                ReviewCount = 198,
                Price = 189,
                ImageUrl = "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcR3gJbKGJ9z3xJBYDKOdh0nMUcCZkVcz9oH7Q&s",
                Description = "Affordable mountain resort with friendly staff and comfortable rooms, ideal for budget-conscious travelers.",
                Facilities = new List<string> { "Internet", "Dinner", "Bathroom", "Parking" },
                Category = "Popular",
                IsRecommended = false
            },
            new Destination
            {
                Id = 14,
                Name = "Crystal Peak Lodge",
                Location = "Aspen, USA",
                Rating = 4.8,
                ReviewCount = 697,
                Price = 469,
                ImageUrl = "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcQJxv9MwTjCFNBH5p3LhHBVZS7zSKPvRGhQ9A&s",
                Description = "Award-winning lodge featuring gourmet dining, private ski access, and personalized concierge services.",
                Facilities = new List<string> { "Internet", "Dinner", "Bathroom", "Pool", "Spa", "Gym", "Ski Access", "Concierge" },
                Category = "Recommended",
                IsRecommended = true
            },
            new Destination
            {
                Id = 15,
                Name = "Mountain Breeze Hotel",
                Location = "Aspen, USA",
                Rating = 4.4,
                ReviewCount = 334,
                Price = 299,
                ImageUrl = "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTk5vZMhJ9gNxPLJYnWzB2j8KxFpHGNUVQHRA&s",
                Description = "Mid-range hotel with excellent location near downtown Aspen and popular hiking trails.",
                Facilities = new List<string> { "Internet", "Dinner", "Bathroom", "Pool", "Shuttle Service" },
                Category = "Popular",
                IsRecommended = false
            },
            new Destination
            {
                Id = 16,
                Name = "Highland Luxury Retreat",
                Location = "Aspen, USA",
                Rating = 4.9,
                ReviewCount = 856,
                Price = 599,
                ImageUrl = "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcR5FkMnBvCH3pTY8gWzX9LqJfK7xVnHrGzTqw&s",
                Description = "Ultra-luxury retreat with private villas, michelin-star dining, and exclusive mountain experiences.",
                Facilities = new List<string> { "Internet", "Dinner", "Bathroom", "Pool", "Spa", "Gym", "Restaurant", "Bar", "Butler Service" },
                Category = "Recommended",
                IsRecommended = true
            },
            new Destination
            {
                Id = 17,
                Name = "Aspen Valley Inn",
                Location = "Aspen, USA",
                Rating = 4.1,
                ReviewCount = 267,
                Price = 209,
                ImageUrl = "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcSJnBhR9MZx7yQwLh3P5nxKGvZT8HmRpGxc5Q&s",
                Description = "Family-friendly inn with spacious rooms and convenient access to year-round outdoor activities.",
                Facilities = new List<string> { "Internet", "Dinner", "Bathroom", "Kids Club", "Game Room" },
                Category = "Popular",
                IsRecommended = false
            },
            new Destination
            {
                Id = 18,
                Name = "Glacier Point Resort",
                Location = "Aspen, USA",
                Rating = 4.7,
                ReviewCount = 578,
                Price = 419,
                ImageUrl = "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcQ7gHKLZFm8xBnHrYvP2MwTh5KJ9NxVgXqW8A&s",
                Description = "Premium resort with heated outdoor pools, wellness center, and breathtaking glacier views.",
                Facilities = new List<string> { "Internet", "Dinner", "Bathroom", "Pool", "Spa", "Gym", "Sauna" },
                Category = "Recommended",
                IsRecommended = true
            },
            new Destination
            {
                Id = 19,
                Name = "Timberline Lodge",
                Location = "Aspen, USA",
                Rating = 4.2,
                ReviewCount = 289,
                Price = 239,
                ImageUrl = "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTxH5gW9nBYpHKJzLqFr3mNvXq8ZVrjTgZCBQ&s",
                Description = "Rustic lodge with authentic mountain charm, offering a peaceful escape from the hustle and bustle.",
                Facilities = new List<string> { "Internet", "Dinner", "Bathroom", "Fireplace", "Library" },
                Category = "Popular",
                IsRecommended = false
            },
            new Destination
            {
                Id = 20,
                Name = "Paradise Peak Hotel",
                Location = "Aspen, USA",
                Rating = 4.8,
                ReviewCount = 712,
                Price = 489,
                ImageUrl = "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcQvXnJ8H0zP2LhKBMQmT5rW9xKJ7VyNpHxQwA&s",
                Description = "Five-star hotel offering unparalleled luxury, fine dining, and world-class ski-in/ski-out access.",
                Facilities = new List<string> { "Internet", "Dinner", "Bathroom", "Pool", "Spa", "Gym", "Restaurant", "Bar", "Ski Valet" },
                Category = "Recommended",
                IsRecommended = true
            }
        };

        // GET: api/destinations?q=search_term
        [HttpGet]
        public ActionResult<IEnumerable<Destination>> GetAll([FromQuery] string? q)
        {
            if (string.IsNullOrWhiteSpace(q))
            {
                return Ok(_destinations);
            }

            var searchTerm = q.ToLower();
            var filteredDestinations = _destinations.Where(d =>
                d.Name.ToLower().Contains(searchTerm) ||
                d.Location.ToLower().Contains(searchTerm) ||
                d.Description.ToLower().Contains(searchTerm) ||
                d.Category.ToLower().Contains(searchTerm) ||
                d.Facilities.Any(f => f.ToLower().Contains(searchTerm)) ||
                d.Price.ToString().Contains(searchTerm) ||
                d.Rating.ToString().Contains(searchTerm)
            ).ToList();

            return Ok(filteredDestinations);
        }

        // GET: api/destinations/{id}
        [HttpGet("{id}")]
        public ActionResult<Destination> GetById(int id)
        {
            var destination = _destinations.FirstOrDefault(d => d.Id == id);

            if (destination == null)
            {
                return NotFound(new { message = $"Destination with ID {id} not found" });
            }

            return Ok(destination);
        }

        // GET: api/destinations/recommended
        [HttpGet("recommended")]
        public ActionResult<IEnumerable<Destination>> GetRecommended()
        {
            var recommended = _destinations
                .Where(d => d.Category == "Recommended" && d.IsRecommended)
                .OrderByDescending(d => d.Rating)
                .ToList();

            return Ok(recommended);
        }

        // GET: api/destinations/popular
        [HttpGet("popular")]
        public ActionResult<IEnumerable<Destination>> GetPopular()
        {
            var popular = _destinations
                .Where(d => d.Category == "Popular")
                .OrderByDescending(d => d.Rating)
                .ToList();

            return Ok(popular);
        }
    }

    public class Destination
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public double Rating { get; set; }
        public int ReviewCount { get; set; }
        public decimal Price { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<string> Facilities { get; set; } = new();
        public string Category { get; set; } = string.Empty;
        public bool IsRecommended { get; set; }
    }
}