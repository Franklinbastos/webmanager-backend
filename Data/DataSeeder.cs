using WebManager.Data;
using WebManager.Models;
using System;
using System.Linq;
using System.Collections.Generic;

namespace WebManager.Data
{
    public static class DataSeeder
    {
        public static void SeedData(AppDbContext context)
        {
            // Clear existing data for development purposes to ensure fresh seed on each run
            context.Finances.RemoveRange(context.Finances);
            context.Goals.RemoveRange(context.Goals);
            context.FixedFinances.RemoveRange(context.FixedFinances);
            context.Users.RemoveRange(context.Users);
            context.SaveChanges(); // Save changes after clearing to avoid conflicts

            // Create a default user
            var defaultUser = new User
            {
                Name = "Test User",
                Email = "test@example.com",
                Password = BCrypt.Net.BCrypt.HashPassword("password123") // Hash a default password
            };
            context.Users.Add(defaultUser);
            context.SaveChanges();

            // Seed Finances
            var random = new Random();
            for (int i = 0; i < 30; i++)
            {
                context.Finances.Add(new Finance
                {
                    UserId = defaultUser.Id,
                    Date = DateTime.Now.AddDays(-random.Next(1, 365)),
                    Description = i % 2 == 0 ? $"Income {i + 1}" : $"Expense {i + 1}",
                    Amount = (decimal)(random.NextDouble() * 1000 + 50),
                    Type = i % 2 == 0 ? "income" : "expense"
                });
            }
            context.SaveChanges();

            // Seed Goals
            string[] items = { "Laptop", "New Phone", "Vacation", "Car Down Payment", "House Renovation", "Books", "Course", "Bike", "Smartwatch", "Headphones" };
            string[] descriptions = { "For work", "Latest model", "Trip to Europe", "Future investment", "Kitchen remodel", "Learning new skills", "Professional development", "Commuting", "Fitness tracking", "Noise cancelling" };

            for (int i = 0; i < 30; i++)
            {
                context.Goals.Add(new Goal
                {
                    UserId = defaultUser.Id,
                    Item = items[random.Next(items.Length)],
                    Value = (decimal)(random.NextDouble() * 5000 + 100),
                    Description = descriptions[random.Next(descriptions.Length)],
                    Completed = random.Next(2) == 1 // 50% chance of being completed
                });
            }
            context.SaveChanges();

            // Seed FixedFinances
            string[] fixedDescriptions = { "Salary", "Rent", "Car Payment", "Utilities", "Internet", "Phone Bill", "Gym Membership", "Subscription Service" };

            for (int i = 0; i < 30; i++)
            {
                context.FixedFinances.Add(new FixedFinance
                {
                    UserId = defaultUser.Id,
                    Description = fixedDescriptions[random.Next(fixedDescriptions.Length)],
                    Amount = (decimal)(random.NextDouble() * 1000 + 100),
                    Type = i % 2 == 0 ? "income" : "expense",
                    IsActive = random.Next(2) == 1
                });
            }
            context.SaveChanges();
        }
    }
}