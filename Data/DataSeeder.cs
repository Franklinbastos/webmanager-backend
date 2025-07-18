using WebManager.Data;
using WebManager.Models;
using System;
using System.Linq;
using System.Collections.Generic;
using BCrypt.Net;

namespace WebManager.Data
{
    public static class DataSeeder
    {
        public static async Task SeedData(AppDbContext context)
        {
            if (!context.Users.Any())
            {
                // Create a user
                var user = new User
                {
                    Name = "Test User",
                    Email = "test@example.com",
                    Password = BCrypt.Net.BCrypt.HashPassword("password123")
                };
                context.Users.Add(user);
                context.SaveChanges(); // Save user to get an ID
                Console.WriteLine($"DataSeeder: Created user with ID: {user.Id}");

                // Create Goals
                var goals = new List<Goal>
                {
                    new Goal { UserId = user.Id, Item = "Trip to Japan", Value = 10000, Description = "Save for a 2-week trip", Completed = false },
                    new Goal { UserId = user.Id, Item = "New Laptop", Value = 2500, Description = "For work and personal projects", Completed = false },
                    new Goal { UserId = user.Id, Item = "Pay off credit card", Value = 3000, Description = "Clear the balance on the main card", Completed = false }
                };
                context.Goals.AddRange(goals);

                // Create FixedFinances
                var fixedFinances = new List<FixedFinance>
                {
                    // Income repeating for 2 months
                    new FixedFinance { UserId = user.Id, Description = "Freelance Project", Amount = 800, Type = "income", NumberOfMonths = 2, BillingDay = 15, StartDate = DateTime.Now, IsActive = true },
                    // Income repeating for 12 months
                    new FixedFinance { UserId = user.Id, Description = "Rental Income", Amount = 1200, Type = "income", NumberOfMonths = 12, BillingDay = 1, StartDate = DateTime.Now, IsActive = true },
                    // Expense repeating for 6 months
                    new FixedFinance { UserId = user.Id, Description = "Car Loan Payment", Amount = 450, Type = "expense", NumberOfMonths = 6, BillingDay = 20, StartDate = DateTime.Now, IsActive = true }
                };
                context.FixedFinances.AddRange(fixedFinances);
                context.SaveChanges(); // Save fixed finances before FinanceService processes them

                // Now, let's ensure the initial Finance entries are generated with FixedFinanceId
                var financeService = new WebManager.Services.FinanceService(context);
                await financeService.ProcessAllFixedFinancesForUser(user.Id);
            }
            else
            {
                Console.WriteLine("DataSeeder: Users already exist. Skipping data seeding.");
            }
        }
    }
}
